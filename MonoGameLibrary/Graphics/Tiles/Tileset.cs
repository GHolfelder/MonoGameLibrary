namespace MonoGameLibrary.Graphics.Tiles;

/// <summary>
/// Interface for tileset implementations to support both regular and spaced tilesets.
/// </summary>
public interface ITileset
{
    /// <summary>
    /// Gets the width, in pixels, of each tile in this tileset.
    /// </summary>
    int TileWidth { get; }

    /// <summary>
    /// Gets the height, in pixels, of each tile in this tileset.
    /// </summary>
    int TileHeight { get; }

    /// <summary>
    /// Gets the total number of columns in this tileset.
    /// </summary>
    int Columns { get; }

    /// <summary>
    /// Gets the total number of rows in this tileset.
    /// </summary>
    int Rows { get; }

    /// <summary>
    /// Gets the total number of tiles in this tileset.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given index.
    /// </summary>
    /// <param name="index">The index of the texture region in this tile set.</param>
    /// <returns>The texture region for the tile form this tileset at the given index.</returns>
    TextureRegion GetTile(int index);

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given location.
    /// </summary>
    /// <param name="column">The column in this tileset of the texture region.</param>
    /// <param name="row">The row in this tileset of the texture region.</param>
    /// <returns>The texture region for the tile from this tileset at given location.</returns>
    TextureRegion GetTile(int column, int row);
}

/// <summary>
/// The Tileset class will manage a collection of tiles from a texture atlas.
/// Each tile will be represented as a TextureRegion.
/// </summary>
public class Tileset : ITileset
{
    private readonly TextureRegion[] _tiles;

    /// <summary>
    /// Gets the width, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Gets the height, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Gets the total number of columns in this tileset.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the total number of rows in this tileset.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the total number of tiles in this tileset.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Creates a new tileset based on the given texture region with the specified
    /// tile width and height.
    /// </summary>
    /// <param name="textureRegion">The texture region that contains the tiles for the tileset.</param>
    /// <param name="tileWidth">The width of each tile in the tileset.</param>
    /// <param name="tileHeight">The height of each tile in the tileset.</param>
    public Tileset(TextureRegion textureRegion, int tileWidth, int tileHeight)
    {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Columns = textureRegion.Width / tileWidth;
        Rows = textureRegion.Height / tileHeight;
        Count = Columns * Rows;

        // Allocate the array to hold the texture regions that make up each individual tile.
        _tiles = new TextureRegion[Count];
        // Populate the tiles array with individual texture regions for each tile.
        for (int i = 0; i < Count; i++)
        {
            int x = i % Columns * tileWidth;
            int y = i / Columns * tileHeight;
            _tiles[i] = new TextureRegion(textureRegion.Texture, textureRegion.SourceRectangle.X + x, textureRegion.SourceRectangle.Y + y, tileWidth, tileHeight);
        }
    }

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given index.
    /// </summary>
    /// <param name="index">The index of the texture region in this tile set.</param>
    /// <returns>The texture region for the tile form this tileset at the given index.</returns>
    public TextureRegion GetTile(int index) => _tiles[index];

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given location.
    /// </summary>
    /// <param name="column">The column in this tileset of the texture region.</param>
    /// <param name="row">The row in this tileset of the texture region.</param>
    /// <returns>The texture region for the tile from this tileset at given location.</returns>
    public TextureRegion GetTile(int column, int row)
    {
        int index = row * Columns + column;
        return GetTile(index);
    }
}

/// <summary>
/// A tileset that handles spacing and margins between tiles.
/// </summary>
public class SpacedTileset : ITileset
{
    private readonly TextureRegion[] _tiles;

    /// <summary>
    /// Gets the width, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileWidth { get; }

    /// <summary>
    /// Gets the height, in pixels, of each tile in this tileset.
    /// </summary>
    public int TileHeight { get; }

    /// <summary>
    /// Gets the total number of columns in this tileset.
    /// </summary>
    public int Columns { get; }

    /// <summary>
    /// Gets the total number of rows in this tileset.
    /// </summary>
    public int Rows { get; }

    /// <summary>
    /// Gets the total number of tiles in this tileset.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the spacing, in pixels, between tiles in this tileset.
    /// </summary>
    public int Spacing { get; }

    /// <summary>
    /// Gets the margin, in pixels, around the edges of this tileset.
    /// </summary>
    public int Margin { get; }

    /// <summary>
    /// Creates a new spaced tileset with spacing and margins between tiles.
    /// </summary>
    /// <param name="textureRegion">The texture region that contains the tiles for the tileset.</param>
    /// <param name="tileWidth">The width of each tile in the tileset.</param>
    /// <param name="tileHeight">The height of each tile in the tileset.</param>
    /// <param name="spacing">The spacing between tiles in pixels.</param>
    /// <param name="margin">The margin around the tileset in pixels.</param>
    public SpacedTileset(TextureRegion textureRegion, int tileWidth, int tileHeight, int spacing, int margin)
    {
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        Spacing = spacing;
        Margin = margin;

        // Calculate how many complete tiles we can fit with the given spacing and margins
        int availableWidth = textureRegion.Width - (2 * margin);
        int availableHeight = textureRegion.Height - (2 * margin);
        
        Columns = (availableWidth + spacing) / (tileWidth + spacing);
        Rows = (availableHeight + spacing) / (tileHeight + spacing);
        Count = Columns * Rows;

        // Allocate the array to hold the texture regions for each tile
        _tiles = new TextureRegion[Count];
        
        // Populate the tiles array with individual texture regions for each tile
        for (int i = 0; i < Count; i++)
        {
            int column = i % Columns;
            int row = i / Columns;

            int x = textureRegion.SourceRectangle.X + margin + (column * (tileWidth + spacing));
            int y = textureRegion.SourceRectangle.Y + margin + (row * (tileHeight + spacing));

            _tiles[i] = new TextureRegion(textureRegion.Texture, x, y, tileWidth, tileHeight);
        }
    }

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given index.
    /// </summary>
    /// <param name="index">The index of the texture region in this tile set.</param>
    /// <returns>The texture region for the tile form this tileset at the given index.</returns>
    public TextureRegion GetTile(int index) => _tiles[index];

    /// <summary>
    /// Gets the texture region for the tile from this tileset at the given location.
    /// </summary>
    /// <param name="column">The column in this tileset of the texture region.</param>
    /// <param name="row">The row in this tileset of the texture region.</param>
    /// <returns>The texture region for the tile from this tileset at given location.</returns>
    public TextureRegion GetTile(int column, int row)
    {
        int index = row * Columns + column;
        return GetTile(index);
    }
}