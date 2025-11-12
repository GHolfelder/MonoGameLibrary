namespace MonoGameLibrary.Utilities;

/// <summary>
/// Represents the 8 cardinal and intercardinal directions
/// </summary>
public enum Direction8
{
    North,
    Northeast, 
    East,
    Southeast,
    South,
    Southwest,
    West,
    Northwest
}

/// <summary>
/// Represents the 4 cardinal directions
/// </summary>
public enum Direction4
{
    North,
    East, 
    South,
    West
}

/// <summary>
/// Specifies whether to use 4-way or 8-way directional movement/animations
/// </summary>
public enum DirectionMode
{
    /// <summary>
    /// Use 4 cardinal directions (N, E, S, W)
    /// </summary>
    FourWay,
    
    /// <summary>
    /// Use 8 directions including diagonals (N, NE, E, SE, S, SW, W, NW)
    /// </summary>
    EightWay
}

/// <summary>
/// Utility methods for working with directions
/// </summary>
public static class DirectionHelper
{
    /// <summary>
    /// Converts an 8-way direction to the nearest 4-way direction
    /// </summary>
    public static Direction4 ToDirection4(Direction8 direction8)
    {
        return direction8 switch
        {
            Direction8.North or Direction8.Northeast or Direction8.Northwest => Direction4.North,
            Direction8.East or Direction8.Southeast => Direction4.East,
            Direction8.South or Direction8.Southwest => Direction4.South,
            Direction8.West => Direction4.West,
            _ => Direction4.South
        };
    }

    /// <summary>
    /// Gets the direction abbreviation for use in animation names
    /// </summary>
    /// <param name="direction">The 8-way direction</param>
    /// <param name="mode">Direction mode (FourWay or EightWay)</param>
    /// <returns>Direction abbreviation string</returns>
    public static string GetDirectionAbbreviation(Direction8 direction, DirectionMode mode = DirectionMode.EightWay)
    {
        if (mode == DirectionMode.EightWay)
        {
            return direction switch
            {
                Direction8.North => "N",
                Direction8.Northeast => "NE",
                Direction8.East => "E",
                Direction8.Southeast => "SE",
                Direction8.South => "S",
                Direction8.Southwest => "SW",
                Direction8.West => "W",
                Direction8.Northwest => "NW",
                _ => "S"
            };
        }
        else
        {
            // Convert to 4-way direction and return abbreviation
            var direction4 = ToDirection4(direction);
            return direction4 switch
            {
                Direction4.North => "N",
                Direction4.East => "E",
                Direction4.South => "S",
                Direction4.West => "W",
                _ => "S"
            };
        }
    }
}
