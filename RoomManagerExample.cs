using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Graphics.Tiles;

namespace MonoGameLibrary;

/// <summary>
/// Example room manager demonstrating the enhanced collision detection system
/// for detecting Exit triggers and other tile objects in object layers.
/// </summary>
public class RoomManagerExample
{
    private readonly Tilemap _currentMap;
    private readonly Vector2 _mapPosition;
    private readonly Dictionary<string, string> _roomTransitions;

    public RoomManagerExample(Tilemap tilemap, Vector2 mapPosition)
    {
        _currentMap = tilemap;
        _mapPosition = mapPosition;
        _roomTransitions = new Dictionary<string, string>();
    }

    /// <summary>
    /// Checks for room transitions and other trigger interactions.
    /// Call this each frame in your game update loop.
    /// </summary>
    /// <param name="player">The player character sprite.</param>
    /// <param name="playerPosition">The player's current world position.</param>
    public void CheckTriggers(CharacterSprite player, Vector2 playerPosition)
    {
        // Example 1: Check for Exit triggers specifically
        var exitTrigger = _currentMap.GetFirstCollidingTileObject(
            player, 
            playerPosition, 
            "Triggers",     // Object layer name
            "Exit",         // Specific object name (optional)
            _mapPosition
        );

        if (exitTrigger != null)
        {
            HandleExitTrigger(exitTrigger);
        }

        // Example 2: Check for any collision with trigger objects
        var allTriggers = _currentMap.GetAllCollidingTileObjects(
            player,
            playerPosition,
            "Triggers",     // Object layer name
            null,           // Any object name
            _mapPosition
        );

        foreach (var trigger in allTriggers)
        {
            HandleGenericTrigger(trigger);
        }

        // Example 3: Check for specific named objects (like doors)
        var doorTrigger = _currentMap.GetFirstCollidingTileObject(
            player,
            playerPosition,
            "Interactive", 
            "Door_North",   // Specific door name
            _mapPosition
        );

        if (doorTrigger != null)
        {
            HandleDoorInteraction(doorTrigger);
        }
    }

    /// <summary>
    /// Handles room transition when player touches an Exit trigger.
    /// </summary>
    private void HandleExitTrigger(CollisionObject exitTrigger)
    {
        // Get target room from object properties
        if (exitTrigger.Properties.TryGetValue("targetRoom", out var target))
        {
            string targetRoom = target.ToString();
            string spawnPoint = exitTrigger.Properties.GetValueOrDefault("spawnPoint")?.ToString() ?? "default";
            
            LoadRoom(targetRoom, spawnPoint);
        }
    }

    /// <summary>
    /// Handles generic trigger interactions based on object type.
    /// </summary>
    private void HandleGenericTrigger(CollisionObject trigger)
    {
        switch (trigger.Name?.ToLower())
        {
            case "exit":
            case string name when name?.StartsWith("exit_") == true:
                HandleExitTrigger(trigger);
                break;

            case "spawn":
            case string name when name?.StartsWith("spawn_") == true:
                HandleSpawnArea(trigger);
                break;

            case "checkpoint":
                HandleCheckpoint(trigger);
                break;

            default:
                // Handle custom triggers based on object properties
                if (trigger.Properties.ContainsKey("triggerType"))
                {
                    HandleCustomTrigger(trigger);
                }
                break;
        }
    }

    /// <summary>
    /// Handles door interactions requiring specific conditions.
    /// </summary>
    private void HandleDoorInteraction(CollisionObject door)
    {
        // Check if door requires a key
        if (door.Properties.TryGetValue("requiresKey", out var keyRequired))
        {
            string keyName = keyRequired.ToString();
            if (HasKey(keyName))
            {
                OpenDoor(door);
            }
            else
            {
                ShowMessage($"This door requires a {keyName}");
            }
        }
        else
        {
            // Open door immediately
            OpenDoor(door);
        }
    }

    /// <summary>
    /// Handles spawn area triggers for enemy/item spawning.
    /// </summary>
    private void HandleSpawnArea(CollisionObject spawnArea)
    {
        if (spawnArea.Properties.TryGetValue("spawnType", out var spawnType))
        {
            string type = spawnType.ToString();
            int count = int.Parse(spawnArea.Properties.GetValueOrDefault("count", 1).ToString());
            
            SpawnEntities(type, spawnArea.Position, count);
        }
    }

    /// <summary>
    /// Handles checkpoint saves when player touches checkpoint triggers.
    /// </summary>
    private void HandleCheckpoint(CollisionObject checkpoint)
    {
        string checkpointId = checkpoint.Properties.GetValueOrDefault("id", "default").ToString();
        SaveGameProgress(checkpointId, checkpoint.Position);
        ShowMessage("Game Saved!");
    }

    /// <summary>
    /// Handles custom triggers based on properties.
    /// </summary>
    private void HandleCustomTrigger(CollisionObject trigger)
    {
        string triggerType = trigger.Properties["triggerType"].ToString();
        
        switch (triggerType)
        {
            case "teleport":
                var targetPos = ParseVector2(trigger.Properties.GetValueOrDefault("targetPosition", "0,0").ToString());
                TeleportPlayer(targetPos);
                break;

            case "cutscene":
                string cutsceneName = trigger.Properties.GetValueOrDefault("cutscene", "").ToString();
                PlayCutscene(cutsceneName);
                break;

            case "shop":
                string shopType = trigger.Properties.GetValueOrDefault("shopType", "general").ToString();
                OpenShop(shopType);
                break;
        }
    }

    // Example implementation methods (implement these based on your game's needs)
    private void LoadRoom(string roomName, string spawnPoint) { /* Implement room loading */ }
    private bool HasKey(string keyName) => true; // Implement key checking
    private void OpenDoor(CollisionObject door) { /* Implement door opening */ }
    private void ShowMessage(string message) { /* Implement UI message display */ }
    private void SpawnEntities(string type, Vector2 position, int count) { /* Implement entity spawning */ }
    private void SaveGameProgress(string checkpointId, Vector2 position) { /* Implement save system */ }
    private void TeleportPlayer(Vector2 position) { /* Implement player teleportation */ }
    private void PlayCutscene(string cutsceneName) { /* Implement cutscene system */ }
    private void OpenShop(string shopType) { /* Implement shop system */ }
    
    private Vector2 ParseVector2(string value)
    {
        var parts = value.Split(',');
        return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
    }

    /// <summary>
    /// Call this when modifying tilemap objects at runtime to maintain cache consistency.
    /// </summary>
    public void RefreshCollisionCache()
    {
        _currentMap.ClearAllCollisionShapeCache();
    }

    /// <summary>
    /// Call this when modifying specific objects to maintain cache consistency.
    /// </summary>
    public void RefreshCollisionCache(string layerName)
    {
        _currentMap.ClearAllCollisionShapeCache(layerName);
    }
}