using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Graphics.Tiles;
using MonoGameLibrary.Utilities;

namespace MonoGameLibrary.Managers;

/// <summary>
/// Base class for room management systems providing lifecycle management,
/// spatial partitioning interface, and save/load functionality.
/// </summary>
public abstract class RoomManagerBase
{
    /// <summary>
    /// Configuration for spatial partitioning per map.
    /// </summary>
    public struct SpatialConfig
    {
        public bool QuadTreeEnabled { get; set; }
        public int MaxExitsPerNode { get; set; }
        public int MaxDepth { get; set; }
        public float ExitDetectionRadius { get; set; }
        
        public static SpatialConfig Default => new()
        {
            QuadTreeEnabled = true,
            MaxExitsPerNode = 8,
            MaxDepth = 6,
            ExitDetectionRadius = 32.0f
        };
    }

    /// <summary>
    /// Event raised when a room transition should occur.
    /// </summary>
    public event Action<string, string> OnRoomTransitionRequested;

    protected Dictionary<string, SpatialConfig> _mapConfigs;
    protected string _currentMapName;
    protected float _exitCooldownTimer;
    protected const float EXIT_COOLDOWN_DURATION = 0.5f; // Prevent rapid bouncing

    /// <summary>
    /// Initialize the room manager.
    /// </summary>
    public virtual void Initialize()
    {
        _mapConfigs = new Dictionary<string, SpatialConfig>();
        _exitCooldownTimer = 0f;
    }

    /// <summary>
    /// Update the room manager - handle cooldowns and exit detection.
    /// </summary>
    /// <param name="gameTime">The game time</param>
    public virtual void Update(GameTime gameTime)
    {
        if (_exitCooldownTimer > 0f)
        {
            _exitCooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
    }

    /// <summary>
    /// Set the current map and configure spatial partitioning.
    /// </summary>
    /// <param name="mapName">Name of the current map</param>
    /// <param name="tilemap">The tilemap instance</param>
    public abstract void SetCurrentMap(string mapName, Tilemap tilemap);

    /// <summary>
    /// Check for exit collisions with the player using enhanced tile object collision detection.
    /// </summary>
    /// <param name="player">The character sprite to check collision for</param>
    /// <returns>Exit object if collision detected and valid, null otherwise</returns>
    public abstract CollisionObject CheckExitCollisions(CharacterSprite player);

    /// <summary>
    /// Get spatial configuration for a specific map.
    /// Reads from map custom properties if available, otherwise uses defaults.
    /// </summary>
    /// <param name="mapName">Name of the map</param>
    /// <param name="tilemap">The tilemap to read properties from</param>
    /// <returns>Spatial configuration for the map</returns>
    protected virtual SpatialConfig GetSpatialConfigForMap(string mapName, Tilemap tilemap)
    {
        if (_mapConfigs.TryGetValue(mapName, out var cached))
        {
            return cached;
        }

        var config = SpatialConfig.Default;

        // TODO: Read map properties when Tilemap.Properties API is available
        // For now, use default configuration
        // Note: This assumes Tilemap has a way to access custom properties
        // You may need to adjust based on actual MonoGameLibrary API
        /*
        if (tilemap?.Properties != null)
        {
            if (tilemap.Properties.TryGetValue("QuadTreeEnabled", out var quadTreeEnabled))
                config.QuadTreeEnabled = bool.Parse(quadTreeEnabled);

            if (tilemap.Properties.TryGetValue("MaxExitsPerNode", out var maxExits))
                config.MaxExitsPerNode = int.Parse(maxExits);

            if (tilemap.Properties.TryGetValue("MaxQuadTreeDepth", out var maxDepth))
                config.MaxDepth = int.Parse(maxDepth);

            if (tilemap.Properties.TryGetValue("ExitDetectionRadius", out var detectionRadius))
                config.ExitDetectionRadius = float.Parse(detectionRadius);
        }
        */

        _mapConfigs[mapName] = config;
        return config;
    }

    /// <summary>
    /// Validate if player is moving in the correct direction to use an exit.
    /// Uses dot product of player velocity and exit direction to determine validity.
    /// </summary>
    /// <param name="player">The character sprite</param>
    /// <param name="exitPosition">Position of the exit</param>
    /// <returns>True if player is moving toward the exit</returns>
    protected virtual bool ValidateExitDirection(CharacterSprite player, Vector2 exitPosition)
    {
        // If player isn't moving, don't allow exit
        if (player.Velocity.LengthSquared() < 0.1f)
        {
            return false;
        }

        // Calculate direction from player to exit
        Vector2 exitDirection = Vector2.Normalize(exitPosition - player.Position);
        
        // Calculate dot product - positive means moving toward exit
        float dotProduct = Vector2.Dot(Vector2.Normalize(player.Velocity), exitDirection);
        
        // Require player to be moving roughly toward the exit (dot product > 0.5 = within 60 degrees)
        return dotProduct > 0.5f;
    }

    /// <summary>
    /// Trigger a room transition if cooldown has expired.
    /// </summary>
    /// <param name="targetMapName">Name of the target map</param>
    /// <param name="entranceExitName">Name of the entrance exit in the target map</param>
    protected virtual void TriggerRoomTransition(string targetMapName, string entranceExitName)
    {
        if (_exitCooldownTimer <= 0f)
        {
            _exitCooldownTimer = EXIT_COOLDOWN_DURATION;
            OnRoomTransitionRequested?.Invoke(targetMapName, entranceExitName);
        }
    }

    /// <summary>
    /// Get all exit objects from the current tilemap that match the filter criteria.
    /// </summary>
    /// <param name="tilemap">The tilemap to search</param>
    /// <param name="layerName">Object layer name (e.g., "Exits", "Triggers")</param>
    /// <param name="nameFilter">Optional name filter for exits</param>
    /// <returns>List of exit collision objects</returns>
    protected virtual List<CollisionObject> GetExitObjects(Tilemap tilemap, string layerName = "Exits", string nameFilter = null)
    {
        if (tilemap == null) return new List<CollisionObject>();

        var allObjects = tilemap.GetCollisionObjects(layerName) ?? new List<CollisionObject>();
        
        // Filter for exit objects
        var exits = allObjects.Where(obj => 
            obj.Name?.StartsWith("Exit", StringComparison.OrdinalIgnoreCase) == true);

        if (!string.IsNullOrEmpty(nameFilter))
        {
            exits = exits.Where(obj => 
                obj.Name?.Contains(nameFilter, StringComparison.OrdinalIgnoreCase) == true);
        }

        return exits.ToList();
    }

    /// <summary>
    /// Check if an object is a valid exit trigger.
    /// </summary>
    /// <param name="obj">The collision object to check</param>
    /// <returns>True if the object is an exit trigger</returns>
    protected virtual bool IsExitTrigger(CollisionObject obj)
    {
        if (obj?.Name == null) return false;

        return obj.Name.StartsWith("Exit", StringComparison.OrdinalIgnoreCase) ||
               (obj.Properties.ContainsKey("type") && 
                obj.Properties["type"].ToString().Equals("exit", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get the target room name from an exit object's properties.
    /// </summary>
    /// <param name="exitObject">The exit collision object</param>
    /// <returns>Target room name or null if not specified</returns>
    protected virtual string GetTargetRoom(CollisionObject exitObject)
    {
        if (exitObject.Properties.TryGetValue("targetRoom", out var target))
        {
            return target.ToString();
        }
        return null;
    }

    /// <summary>
    /// Get the entrance exit name from an exit object's properties.
    /// </summary>
    /// <param name="exitObject">The exit collision object</param>
    /// <returns>Entrance exit name or "default" if not specified</returns>
    protected virtual string GetEntranceExit(CollisionObject exitObject)
    {
        if (exitObject.Properties.TryGetValue("entranceExit", out var entrance))
        {
            return entrance.ToString();
        }
        return "default";
    }

    /// <summary>
    /// Save room manager state to a data structure.
    /// Override in derived classes to implement specific save logic.
    /// </summary>
    /// <returns>Serializable data representing the room manager state</returns>
    public abstract object SaveState();

    /// <summary>
    /// Load room manager state from saved data.
    /// Override in derived classes to implement specific load logic.
    /// </summary>
    /// <param name="savedData">Previously saved state data</param>
    public abstract void LoadState(object savedData);
}