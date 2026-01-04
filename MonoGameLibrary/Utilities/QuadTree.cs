using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameLibrary.Utilities;

/// <summary>
/// Interface for objects that can provide their own spatial bounds and position.
/// </summary>
public interface ISpatialObject
{
    Rectangle Bounds { get; }
    Vector2 Position { get; }
}

/// <summary>
/// QuadTree implementation for spatial partitioning of tilemap objects like exits.
/// Provides efficient collision detection by organizing objects spatially.
/// </summary>
public class QuadTree<T> where T : ISpatialObject
{
    private readonly Rectangle _bounds;
    private readonly int _maxObjectsPerNode;
    private readonly int _maxDepth;
    private readonly int _currentDepth;
    
    private List<QuadTreeItem> _objects;
    private QuadTree<T>[] _children;

    public struct QuadTreeItem
    {
        public T Object;
        public Rectangle Bounds;
        
        public QuadTreeItem(T obj, Rectangle bounds)
        {
            Object = obj;
            Bounds = bounds;
        }
    }

    /// <summary>
    /// Initialize a new QuadTree node.
    /// </summary>
    /// <param name="bounds">Spatial bounds of this node</param>
    /// <param name="maxObjectsPerNode">Maximum objects before subdivision</param>
    /// <param name="maxDepth">Maximum depth of subdivision</param>
    /// <param name="currentDepth">Current depth (0 for root)</param>
    public QuadTree(Rectangle bounds, int maxObjectsPerNode = 8, int maxDepth = 6, int currentDepth = 0)
    {
        _bounds = bounds;
        _maxObjectsPerNode = maxObjectsPerNode;
        _maxDepth = maxDepth;
        _currentDepth = currentDepth;
        _objects = new List<QuadTreeItem>();
        _children = null;
    }

    /// <summary>
    /// Insert an object into the QuadTree.
    /// </summary>
    /// <param name="obj">Object to insert</param>
    /// <param name="bounds">Spatial bounds of the object</param>
    public void Insert(T obj, Rectangle bounds)
    {
        // If object doesn't fit in this node, ignore it
        if (!_bounds.Intersects(bounds))
            return;

        // If we haven't subdivided and have room, add to this node
        if (_children == null && _objects.Count < _maxObjectsPerNode)
        {
            _objects.Add(new QuadTreeItem(obj, bounds));
            return;
        }

        // If we can subdivide and haven't yet, do so
        if (_children == null && _currentDepth < _maxDepth)
        {
            Subdivide();
        }

        // If we have children, try to insert into appropriate child
        if (_children != null)
        {
            foreach (var child in _children)
            {
                child.Insert(obj, bounds);
            }
        }
        else
        {
            // Can't subdivide further, add to this node
            _objects.Add(new QuadTreeItem(obj, bounds));
        }
    }

    /// <summary>
    /// Insert a spatial object into the QuadTree using its own bounds.
    /// </summary>
    /// <param name="spatialObj">Object that implements ISpatialObject</param>
    public void Insert(T spatialObj)
    {
        Insert(spatialObj, spatialObj.Bounds);
    }

    /// <summary>
    /// Retrieve all objects within a given area.
    /// </summary>
    /// <param name="searchBounds">Area to search within</param>
    /// <param name="results">List to add results to</param>
    public void Query(Rectangle searchBounds, List<T> results)
    {
        // If search area doesn't overlap this node, return
        if (!_bounds.Intersects(searchBounds))
            return;

        // Add objects from this node that intersect the search area
        foreach (var item in _objects)
        {
            if (item.Bounds.Intersects(searchBounds))
            {
                results.Add(item.Object);
            }
        }

        // Query children if they exist
        if (_children != null)
        {
            foreach (var child in _children)
            {
                child.Query(searchBounds, results);
            }
        }
    }

    /// <summary>
    /// Retrieve all objects within a circular area.
    /// </summary>
    /// <param name="center">Center of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="results">List to add results to</param>
    /// <param name="getPosition">Function to get position from object for distance calculation</param>
    public void Query(Vector2 center, float radius, List<T> results, Func<T, Vector2> getPosition)
    {
        // Create bounding rectangle for initial broad-phase check
        int radiusInt = (int)Math.Ceiling(radius);
        Rectangle searchBounds = new Rectangle(
            (int)(center.X - radiusInt),
            (int)(center.Y - radiusInt),
            radiusInt * 2,
            radiusInt * 2
        );

        var broadResults = new List<T>();
        Query(searchBounds, broadResults);

        // Narrow-phase: check actual distance
        float radiusSquared = radius * radius;
        foreach (var obj in broadResults)
        {
            Vector2 objPosition = getPosition(obj);
            if (Vector2.DistanceSquared(center, objPosition) <= radiusSquared)
            {
                results.Add(obj);
            }
        }
    }

    /// <summary>
    /// Retrieve all objects within a circular area for objects that implement ISpatialObject.
    /// </summary>
    /// <param name="center">Center of the circle</param>
    /// <param name="radius">Radius of the circle</param>
    /// <param name="results">List to add results to</param>
    public void Query(Vector2 center, float radius, List<T> results)
    {
        Query(center, radius, results, obj => obj.Position);
    }

    /// <summary>
    /// Clear all objects from the QuadTree.
    /// </summary>
    public void Clear()
    {
        _objects.Clear();
        
        if (_children != null)
        {
            foreach (var child in _children)
            {
                child.Clear();
            }
            _children = null;
        }
    }

    /// <summary>
    /// Subdivide this node into four child nodes.
    /// </summary>
    private void Subdivide()
    {
        int halfWidth = _bounds.Width / 2;
        int halfHeight = _bounds.Height / 2;

        _children = new QuadTree<T>[4];
        
        // Top-left
        _children[0] = new QuadTree<T>(
            new Rectangle(_bounds.X, _bounds.Y, halfWidth, halfHeight),
            _maxObjectsPerNode, _maxDepth, _currentDepth + 1);

        // Top-right
        _children[1] = new QuadTree<T>(
            new Rectangle(_bounds.X + halfWidth, _bounds.Y, halfWidth, halfHeight),
            _maxObjectsPerNode, _maxDepth, _currentDepth + 1);

        // Bottom-left
        _children[2] = new QuadTree<T>(
            new Rectangle(_bounds.X, _bounds.Y + halfHeight, halfWidth, halfHeight),
            _maxObjectsPerNode, _maxDepth, _currentDepth + 1);

        // Bottom-right
        _children[3] = new QuadTree<T>(
            new Rectangle(_bounds.X + halfWidth, _bounds.Y + halfHeight, halfWidth, halfHeight),
            _maxObjectsPerNode, _maxDepth, _currentDepth + 1);

        // Move existing objects to children
        var existingObjects = new List<QuadTreeItem>(_objects);
        _objects.Clear();

        foreach (var item in existingObjects)
        {
            foreach (var child in _children)
            {
                child.Insert(item.Object, item.Bounds);
            }
        }
    }

    /// <summary>
    /// Get the total number of objects in this QuadTree.
    /// </summary>
    public int Count
    {
        get
        {
            int count = _objects.Count;
            if (_children != null)
            {
                foreach (var child in _children)
                {
                    count += child.Count;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Get the bounds of this QuadTree node.
    /// </summary>
    public Rectangle Bounds => _bounds;
}