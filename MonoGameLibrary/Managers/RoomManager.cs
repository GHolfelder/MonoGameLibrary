using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Graphics.Tiles;
using MonoGameLibrary.Utilities;

namespace MonoGameLibrary.Managers;

/// <summary>
/// Concrete implementation of RoomManagerBase using enhanced collision detection
/// for exit detection and room transitions.
/// </summary>
public class RoomManager : RoomManagerBase
{
    private Tilemap _currentTilemap;
    private Vector2 _tilemapPosition;
    private QuadTree<ExitWrapper> _exitQuadTree;
    private Dictionary<string, CollisionObject> _exits;
    
    // QuadTree performance tracking
    private int _quadTreeQueryCount;
    private int _linearSearchCount;
    private long _totalQuadTreeQueryTime; // in milliseconds
    private long _totalLinearSearchTime; // in milliseconds
    private System.Diagnostics.Stopwatch _performanceStopwatch = new();
    private DateTime _lastPerformanceReset = DateTime.Now;

    /// <summary>
    /// Wrapper class to make CollisionObject compatible with QuadTree
    /// </summary>
    private class ExitWrapper : ISpatialObject
    {
        public CollisionObject Exit { get; }
        public Rectangle Bounds { get; }
        public Vector2 Position { get; }

        public ExitWrapper(CollisionObject exit, Vector2 tilemapOffset)
        {
            Exit = exit;
            Position = tilemapOffset + exit.Position;
            Bounds = new Rectangle(
                (int)Position.X, 
                (int)Position.Y, 
                exit.Width, 
                exit.Height
            );
        }
    }

    public RoomManager()
    {
        _exits = new Dictionary<string, CollisionObject>();
    }

    /// <summary>
    /// Set the current map and configure spatial partitioning for exits.
    /// </summary>
    /// <param name="mapName">Name of the current map</param>
    /// <param name="tilemap">The tilemap instance</param>
    public override void SetCurrentMap(string mapName, Tilemap tilemap)
    {
        _currentMapName = mapName;
        _currentTilemap = tilemap;
        _tilemapPosition = Vector2.Zero; // Adjust as needed for your tilemap positioning
        
        var config = GetSpatialConfigForMap(mapName, tilemap);
        
        if (config.QuadTreeEnabled)
        {
            SetupQuadTreeForExits(config);
        }
        
        CacheExits();
    }

    /// <summary>
    /// Setup QuadTree spatial partitioning for exit objects.
    /// </summary>
    private void SetupQuadTreeForExits(SpatialConfig config)
    {
        // Create QuadTree bounds based on tilemap size
        var mapBounds = new Rectangle(
            (int)_tilemapPosition.X,
            (int)_tilemapPosition.Y,
            _currentTilemap.Width * _currentTilemap.TileWidth,
            _currentTilemap.Height * _currentTilemap.TileHeight
        );

        _exitQuadTree = new QuadTree<ExitWrapper>(
            mapBounds,
            config.MaxExitsPerNode,
            config.MaxDepth
        );

        // Populate QuadTree with exit objects from "Exits" or "Triggers" layer
        var exits = _currentTilemap.GetCollisionObjects("Exits") ?? new List<CollisionObject>();
        var triggers = _currentTilemap.GetCollisionObjects("Triggers") ?? new List<CollisionObject>();
        
        // Add exits from both layers
        foreach (var exit in exits.Concat(triggers).Where(obj => 
            obj.Name?.StartsWith("Exit", StringComparison.OrdinalIgnoreCase) == true))
        {
            var wrapper = new ExitWrapper(exit, _tilemapPosition);
            _exitQuadTree.Insert(wrapper);
        }
    }

    /// <summary>
    /// Cache all exit objects for quick access by name.
    /// </summary>
    private void CacheExits()
    {
        _exits.Clear();
        
        var allExits = _currentTilemap.GetCollisionObjects("Exits") ?? new List<CollisionObject>();
        var allTriggers = _currentTilemap.GetCollisionObjects("Triggers") ?? new List<CollisionObject>();
        
        foreach (var exit in allExits.Concat(allTriggers).Where(obj => 
            obj.Name?.StartsWith("Exit", StringComparison.OrdinalIgnoreCase) == true))
        {
            if (!string.IsNullOrEmpty(exit.Name))
            {
                _exits[exit.Name] = exit;
            }
        }
    }

    /// <summary>
    /// Check for exit collisions with the player using enhanced collision detection.
    /// </summary>
    /// <param name="player">The character sprite to check collision for</param>
    /// <returns>Exit object if collision detected and valid, null otherwise</returns>
    public override CollisionObject CheckExitCollisions(CharacterSprite player)
    {
        if (_currentTilemap == null || player.Collision == null)
            return null;

        var config = GetSpatialConfigForMap(_currentMapName, _currentTilemap);

        CollisionObject exitObject = null;

        if (config.QuadTreeEnabled && _exitQuadTree != null)
        {
            // Use QuadTree for spatial optimization
            exitObject = CheckExitCollisionsWithQuadTree(player, config);
        }
        else
        {
            // Use direct collision detection without spatial partitioning
            exitObject = CheckExitCollisionsDirectly(player);
        }

        // Validate exit direction and cooldown before allowing transition
        if (exitObject != null && 
            ValidateExitDirection(player, _tilemapPosition + exitObject.Position) &&
            _exitCooldownTimer <= 0f)
        {
            return exitObject;
        }

        return null;
    }

    /// <summary>
    /// Check exit collisions using QuadTree spatial partitioning.
    /// </summary>
    private CollisionObject CheckExitCollisionsWithQuadTree(CharacterSprite player, SpatialConfig config)
    {
        _performanceStopwatch.Restart();
        
        var searchRadius = config.ExitDetectionRadius;
        var nearbyExits = new List<ExitWrapper>();
        
        _exitQuadTree.Query(player.Position, searchRadius, nearbyExits);
        _quadTreeQueryCount++;

        foreach (var exitWrapper in nearbyExits)
        {
            var collision = _currentTilemap.GetFirstCollidingTileObject(
                player, 
                player.Position, 
                "Exits", 
                exitWrapper.Exit.Name, 
                _tilemapPosition
            );

            if (collision != null)
            {
                _performanceStopwatch.Stop();
                _totalQuadTreeQueryTime += _performanceStopwatch.ElapsedMilliseconds;
                LogQuadTreePerformance(nearbyExits.Count, true);
                return collision;
            }

            // Also check Triggers layer
            collision = _currentTilemap.GetFirstCollidingTileObject(
                player, 
                player.Position, 
                "Triggers", 
                exitWrapper.Exit.Name, 
                _tilemapPosition
            );

            if (collision != null)
            {
                _performanceStopwatch.Stop();
                _totalQuadTreeQueryTime += _performanceStopwatch.ElapsedMilliseconds;
                LogQuadTreePerformance(nearbyExits.Count, true);
                return collision;
            }
        }
        
        _performanceStopwatch.Stop();
        _totalQuadTreeQueryTime += _performanceStopwatch.ElapsedMilliseconds;
        LogQuadTreePerformance(nearbyExits.Count, false);
        return null;
    }

    /// <summary>
    /// Check exit collisions directly without spatial partitioning.
    /// </summary>
    private CollisionObject CheckExitCollisionsDirectly(CharacterSprite player)
    {
        // Check Exits layer first
        var exit = _currentTilemap.GetFirstCollidingTileObject(
            player, 
            player.Position, 
            "Exits", 
            null, // Any exit object
            _tilemapPosition
        );

        if (exit != null)
        {
            return exit;
        }

        // Check Triggers layer for exit objects
        var triggerExits = _currentTilemap.GetAllCollidingTileObjects(
            player, 
            player.Position, 
            "Triggers", 
            null, 
            _tilemapPosition
        );

        return triggerExits.FirstOrDefault(obj => 
            obj.Name?.StartsWith("Exit", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Get an exit object by name for spawning/positioning.
    /// </summary>
    /// <param name="exitName">Name of the exit to find</param>
    /// <returns>Exit collision object or null if not found</returns>
    public CollisionObject GetExitByName(string exitName)
    {
        _exits.TryGetValue(exitName, out var exit);
        return exit;
    }

    /// <summary>
    /// Get the spawn position for an exit.
    /// </summary>
    /// <param name="exitName">Name of the exit</param>
    /// <returns>World position for spawning at the exit</returns>
    public Vector2? GetExitSpawnPosition(string exitName)
    {
        var exit = GetExitByName(exitName);
        if (exit != null)
        {
            // Spawn at the center of the exit
            return _tilemapPosition + exit.Position + new Vector2(exit.Width / 2f, exit.Height / 2f);
        }
        return null;
    }

    /// <summary>
    /// Trigger a room transition with exit validation.
    /// </summary>
    /// <param name="exitObject">The exit object that was triggered</param>
    public void TriggerExitTransition(CollisionObject exitObject)
    {
        if (exitObject.Properties.TryGetValue("targetRoom", out var targetRoom))
        {
            var entranceExit = exitObject.Properties.GetValueOrDefault("entranceExit")?.ToString() ?? "default";
            TriggerRoomTransition(targetRoom.ToString(), entranceExit);
        }
    }

    /// <summary>
    /// Save room manager state including current map and exit cache.
    /// </summary>
    /// <returns>Serializable state data</returns>
    public override object SaveState()
    {
        return new
        {
            CurrentMapName = _currentMapName,
            TimemapPosition = _tilemapPosition,
            ExitCooldownTimer = _exitCooldownTimer,
            MapConfigs = _mapConfigs
        };
    }

    /// <summary>
    /// Load room manager state from saved data.
    /// </summary>
    /// <param name="savedData">Previously saved state data</param>
    public override void LoadState(object savedData)
    {
        if (savedData is not Dictionary<string, object> state) return;

        if (state.TryGetValue("CurrentMapName", out var mapName))
            _currentMapName = mapName.ToString();

        if (state.TryGetValue("TimemapPosition", out var position) && position is Vector2 pos)
            _tilemapPosition = pos;

        if (state.TryGetValue("ExitCooldownTimer", out var timer) && timer is float cooldown)
            _exitCooldownTimer = cooldown;

        if (state.TryGetValue("MapConfigs", out var configs) && configs is Dictionary<string, SpatialConfig> mapConfigs)
            _mapConfigs = mapConfigs;
    }
    
    /// <summary>
    /// Logs QuadTree performance metrics for debugging.
    /// </summary>
    /// <param name="nearbyExitCount">Number of exits found in QuadTree query</param>
    /// <param name="collisionFound">Whether a collision was detected</param>
    private void LogQuadTreePerformance(int nearbyExitCount, bool collisionFound)
    {
        // Only log periodically to avoid spam
        if (_quadTreeQueryCount % 60 == 0) // Every 60 queries (roughly once per second at 60fps)
        {
            var avgQueryTime = _totalQuadTreeQueryTime / (double)_quadTreeQueryCount;
            Core.AddDebugMessage($"QuadTree: {_quadTreeQueryCount} queries, avg {avgQueryTime:F2}ms, found {nearbyExitCount} nearby exits");
        }
        
        if (collisionFound && nearbyExitCount > 1)
        {
            Core.AddDebugMessage($"QuadTree: Collision found, checked {nearbyExitCount} exits");
        }
    }
    
    /// <summary>
    /// Logs linear search performance metrics for debugging.
    /// </summary>
    /// <param name="collisionFound">Whether a collision was detected</param>
    private void LogLinearSearchPerformance(bool collisionFound)
    {
        // Log periodically
        if (_linearSearchCount % 60 == 0) // Every 60 searches
        {
            var avgSearchTime = _totalLinearSearchTime / (double)_linearSearchCount;
            Core.AddDebugMessage($"Linear Search: {_linearSearchCount} searches, avg {avgSearchTime:F2}ms");
        }
        
        if (collisionFound)
        {
            Core.AddDebugMessage("Linear Search: Collision found, checked all exits");
        }
    }
    
    /// <summary>
    /// Resets performance tracking statistics.
    /// </summary>
    public void ResetPerformanceStats()
    {
        _quadTreeQueryCount = 0;
        _linearSearchCount = 0;
        _totalQuadTreeQueryTime = 0;
        _totalLinearSearchTime = 0;
        _lastPerformanceReset = DateTime.Now;
        Core.AddDebugMessage("RoomManager: Performance statistics reset");
    }
    
    /// <summary>
    /// Gets performance statistics summary for debugging.
    /// </summary>
    /// <returns>Formatted string with performance metrics</returns>
    public string GetPerformanceStats()
    {
        var timeSpan = DateTime.Now - _lastPerformanceReset;
        var avgQuadTreeTime = _quadTreeQueryCount > 0 ? _totalQuadTreeQueryTime / (double)_quadTreeQueryCount : 0;
        var avgLinearTime = _linearSearchCount > 0 ? _totalLinearSearchTime / (double)_linearSearchCount : 0;
        
        return $"RoomManager Performance (last {timeSpan.TotalSeconds:F0}s):\n" +
               $"  QuadTree: {_quadTreeQueryCount} queries, avg {avgQuadTreeTime:F2}ms\n" +
               $"  Linear: {_linearSearchCount} searches, avg {avgLinearTime:F2}ms";
    }
}