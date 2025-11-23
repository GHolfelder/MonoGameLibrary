using MonoGameLibrary.Graphics.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace JsonTilemapExample
{
    /// <summary>
    /// Example demonstrating JSON tilemap loading with proper z-ordering for characters and entities.
    /// </summary>
    public class GameLevelWithZOrdering
    {
        private Tilemap _tilemap;
        private Player _player;
        private List<NPC> _npcs;
        private List<Tree> _trees;
        private Vector2 _cameraPosition;

        public void LoadContent(ContentManager content)
        {
            // Load the tilemap from JSON
            // This assumes you have:
            // - Content/atlas.json (texture atlas sprite definitions)
            // - Content/atlas_animations.json (animation definitions) 
            // - Content/atlas.png (packed texture file)
            // - Content/maps/forest_level.json (tilemap definition)
            _tilemap = Tilemap.FromJson(content, "maps/forest_level.json");

            // Create game entities
            _player = new Player(content);
            _npcs = new List<NPC>
            {
                new NPC(content, new Vector2(200, 150)),
                new NPC(content, new Vector2(400, 300))
            };

            _trees = new List<Tree>
            {
                new Tree(content, new Vector2(250, 200)),
                new Tree(content, new Vector2(350, 250))
            };
        }

        public void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
            
            foreach (var npc in _npcs)
                npc.Update(gameTime);

            // Update camera to follow player
            _cameraPosition = _player.Position - new Vector2(400, 300);
        }

        /// <summary>
        /// Draws the level with proper z-ordering so characters appear behind walls and trees.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            // Calculate tilemap position relative to camera
            Vector2 tilemapPosition = -_cameraPosition;

            // === LAYER 0: GROUND ===
            // Draw ground tiles (grass, dirt, stone floors)
            _tilemap.DrawLayersUpTo(spriteBatch, tilemapPosition, 0);

            // === CHARACTER LEVEL ===
            // Draw all characters and entities that should appear above ground
            // but behind walls and tree canopies

            // Draw trees (trunks only - canopies are in foreground layer)
            foreach (var tree in _trees)
                tree.DrawTrunk(spriteBatch, -_cameraPosition);

            // Draw player
            _player.Draw(spriteBatch, -_cameraPosition);

            // Draw NPCs
            foreach (var npc in _npcs)
                npc.Draw(spriteBatch, -_cameraPosition);

            // === LAYER 1: WALLS AND OBSTACLES ===
            // Draw walls, furniture, and obstacles that characters can walk behind
            TileLayer wallLayer = _tilemap.GetLayerByName("Walls");
            if (wallLayer != null)
                _tilemap.DrawLayer(spriteBatch, wallLayer, tilemapPosition);

            // === LAYER 2+: FOREGROUND ===
            // Draw tree canopies, roofs, and other elements that should appear
            // in front of characters
            _tilemap.DrawLayersFrom(spriteBatch, tilemapPosition, 2);

            // Draw tree canopies
            foreach (var tree in _trees)
                tree.DrawCanopy(spriteBatch, -_cameraPosition);
        }
    }

    /// <summary>
    /// Example player class that demonstrates character positioning.
    /// </summary>
    public class Player
    {
        public Vector2 Position { get; set; } = new Vector2(100, 100);
        private Texture2D _texture;

        public Player(ContentManager content)
        {
            // Load player texture (could also be from atlas)
            _texture = content.Load<Texture2D>("player");
        }

        public void Update(GameTime gameTime)
        {
            // Player movement logic here
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(_texture, Position + offset, Color.White);
        }
    }

    /// <summary>
    /// Example NPC class.
    /// </summary>
    public class NPC
    {
        public Vector2 Position { get; set; }
        private Texture2D _texture;

        public NPC(ContentManager content, Vector2 position)
        {
            Position = position;
            _texture = content.Load<Texture2D>("npc");
        }

        public void Update(GameTime gameTime)
        {
            // NPC AI and movement logic here
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 offset)
        {
            spriteBatch.Draw(_texture, Position + offset, Color.White);
        }
    }

    /// <summary>
    /// Example tree entity with separate trunk and canopy rendering.
    /// </summary>
    public class Tree
    {
        public Vector2 Position { get; set; }
        private Texture2D _trunkTexture;
        private Texture2D _canopyTexture;

        public Tree(ContentManager content, Vector2 position)
        {
            Position = position;
            _trunkTexture = content.Load<Texture2D>("tree_trunk");
            _canopyTexture = content.Load<Texture2D>("tree_canopy");
        }

        /// <summary>
        /// Draw tree trunk - renders at character level.
        /// </summary>
        public void DrawTrunk(SpriteBatch spriteBatch, Vector2 offset)
        {
            Vector2 trunkPosition = Position + offset + new Vector2(0, 32); // Offset trunk down
            spriteBatch.Draw(_trunkTexture, trunkPosition, Color.White);
        }

        /// <summary>
        /// Draw tree canopy - renders in foreground layer.
        /// </summary>
        public void DrawCanopy(SpriteBatch spriteBatch, Vector2 offset)
        {
            Vector2 canopyPosition = Position + offset;
            spriteBatch.Draw(_canopyTexture, canopyPosition, Color.White);
        }
    }

    /// <summary>
    /// Advanced example with Y-sorting for isometric-style depth.
    /// </summary>
    public class IsometricLevelRenderer
    {
        private Tilemap _tilemap;
        private List<IEntity> _entities = new List<IEntity>();

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw background layers
            _tilemap.DrawLayersUpTo(spriteBatch, Vector2.Zero, 0);

            // Sort entities by Y position for isometric depth
            _entities.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));

            // Draw entities in Y-sorted order
            foreach (var entity in _entities)
                entity.Draw(spriteBatch);

            // Draw foreground layers
            _tilemap.DrawLayersFrom(spriteBatch, Vector2.Zero, 1);
        }
    }

    public interface IEntity
    {
        Vector2 Position { get; }
        void Draw(SpriteBatch spriteBatch);
    }
}

/*
EXAMPLE TILEMAP JSON STRUCTURE FOR Z-ORDERING:

{
  "name": "forest_level",
  "width": 50,
  "height": 40,
  "tileWidth": 32,
  "tileHeight": 32,
  "orientation": "orthogonal",
  "backgroundColor": "#2e3440",
  "atlasFile": "atlas.png",
  "tilesets": [
    {
      "name": "terrain",
      "firstGid": 1,
      "tileWidth": 32,
      "tileHeight": 32,
      "tileCount": 16,
      "columns": 4,
      "margin": 0,
      "spacing": 0,
      "atlasSprite": "terrain_tileset",
      "tiles": [],
      "properties": {}
    }
  ],
  "tileLayers": [
    {
      "id": 1,
      "name": "Ground",
      "width": 50,
      "height": 40,
      "opacity": 1.0,
      "visible": true,
      "offsetX": 0,
      "offsetY": 0,
      "tiles": [1, 1, 2, 2, ...],
      "properties": {}
    },
    {
      "id": 2,
      "name": "Walls",
      "width": 50,
      "height": 40,
      "opacity": 1.0,
      "visible": true,
      "offsetX": 0,
      "offsetY": 0,
      "tiles": [0, 0, 0, 5, ...],
      "properties": {}
    },
    {
      "id": 3,
      "name": "Canopy",
      "width": 50,
      "height": 40,
      "opacity": 1.0,
      "visible": true,
      "offsetX": 0,
      "offsetY": 0,
      "tiles": [0, 0, 9, 0, ...],
      "properties": {}
    }
  ]
}

RENDERING ORDER:
1. Layer 0 "Ground" (grass, dirt)
2. Character entities (player, NPCs, tree trunks)  
3. Layer 1 "Walls" (obstacles at character height)
4. Layer 2 "Canopy" (tree tops, roofs)
5. Flying entities (birds, effects)

This creates proper depth illusion where characters can walk behind walls
and under tree canopies while appearing above the ground.
*/