using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Scenes;

/// <summary>
/// Manages scene transitions with optional caching to preserve scene state
/// </summary>
public class SceneManager
{
    private Dictionary<Type, Scene> _cachedScenes = new Dictionary<Type, Scene>();
    private Scene _currentScene;

    /// <summary>
    /// Gets the currently active scene
    /// </summary>
    public Scene CurrentScene => _currentScene;

    /// <summary>
    /// Transitions to a scene of type T, creating and caching it if it doesn't exist
    /// </summary>
    /// <typeparam name="T">The type of scene to transition to</typeparam>
    public void TransitionTo<T>() where T : Scene, new()
    {
        var sceneType = typeof(T);
        
        // Create scene if not cached
        if (!_cachedScenes.ContainsKey(sceneType))
        {
            _cachedScenes[sceneType] = new T();
            _cachedScenes[sceneType].Initialize(); // Initialize new scenes immediately
        }

        // Pause current scene instead of disposing
        _currentScene?.OnPause();
        
        // Switch to cached scene
        _currentScene = _cachedScenes[sceneType];
        _currentScene.OnResume();
    }

    /// <summary>
    /// Transitions to a specific scene instance and optionally caches it
    /// </summary>
    /// <param name="scene">The scene instance to transition to</param>
    /// <param name="cacheScene">Whether to cache the scene instance for future use</param>
    public void TransitionTo(Scene scene, bool cacheScene = false)
    {
        if (scene == null) return;

        _currentScene?.OnPause();
        
        _currentScene = scene;
        
        // Initialize if not already initialized
        if (!_cachedScenes.ContainsValue(scene))
        {
            scene.Initialize();
        }
        
        // Cache the scene if requested and it's not already cached
        if (cacheScene && !_cachedScenes.ContainsValue(scene))
        {
            var sceneType = scene.GetType();
            _cachedScenes[sceneType] = scene;
        }
        
        scene.OnResume();
    }

    /// <summary>
    /// Updates the current scene
    /// </summary>
    /// <param name="gameTime">Game timing information</param>
    public void Update(GameTime gameTime)
    {
        _currentScene?.Update(gameTime);
    }

    /// <summary>
    /// Draws the current scene
    /// </summary>
    /// <param name="gameTime">Game timing information</param>
    public void Draw(GameTime gameTime)
    {
        _currentScene?.Draw(gameTime);
    }

    /// <summary>
    /// Removes all cached scenes and disposes them
    /// </summary>
    public void ClearCache()
    {
        foreach (var scene in _cachedScenes.Values)
        {
            scene.Dispose();
        }
        _cachedScenes.Clear();
    }

    /// <summary>
    /// Removes a specific scene type from cache and disposes it
    /// </summary>
    /// <typeparam name="T">The type of scene to remove from cache</typeparam>
    public void RemoveFromCache<T>() where T : Scene
    {
        var sceneType = typeof(T);
        if (_cachedScenes.TryGetValue(sceneType, out var scene))
        {
            // If this is the current scene, we need to handle that
            if (_currentScene == scene)
            {
                _currentScene = null;
            }
            
            scene.Dispose();
            _cachedScenes.Remove(sceneType);
        }
    }

    /// <summary>
    /// Gets a cached scene of the specified type, if it exists
    /// </summary>
    /// <typeparam name="T">The type of scene to retrieve</typeparam>
    /// <returns>The cached scene instance, or null if not cached</returns>
    public T GetCachedScene<T>() where T : Scene
    {
        var sceneType = typeof(T);
        return _cachedScenes.TryGetValue(sceneType, out var scene) ? (T)scene : null;
    }

    /// <summary>
    /// Checks if a scene type is currently cached
    /// </summary>
    /// <typeparam name="T">The type of scene to check</typeparam>
    /// <returns>True if the scene is cached, false otherwise</returns>
    public bool IsSceneCached<T>() where T : Scene
    {
        return _cachedScenes.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Gets the number of currently cached scenes
    /// </summary>
    public int CachedSceneCount => _cachedScenes.Count;
    
    /// <summary>
    /// Gets debug information about cached scenes
    /// </summary>
    /// <returns>String describing cached scenes</returns>
    public string GetCachedScenesInfo()
    {
        if (_cachedScenes.Count == 0)
            return "No scenes cached";
            
        var sceneNames = _cachedScenes.Keys.Select(t => t.Name);
        return $"Cached scenes: {string.Join(", ", sceneNames)}";
    }
}