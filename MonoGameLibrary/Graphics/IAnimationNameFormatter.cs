using MonoGameLibrary.Utilities;

namespace MonoGameLibrary.Graphics;

/// <summary>
/// Interface for formatting animation names from state and direction
/// </summary>
public interface IAnimationNameFormatter
{
    /// <summary>
    /// Formats an animation name from the given parameters
    /// </summary>
    /// <param name="characterPrefix">The character prefix (e.g., "player", "npc")</param>
    /// <param name="state">The animation state (e.g., "idle", "walk")</param>
    /// <param name="direction">The direction abbreviation (e.g., "N", "SE")</param>
    /// <returns>The formatted animation name</returns>
    string FormatAnimationName(string characterPrefix, string state, string direction);
}

/// <summary>
/// Default implementation using the pattern: {prefix}_{state}_{direction}
/// Example: "player_walk_N"
/// </summary>
public class DefaultAnimationNameFormatter : IAnimationNameFormatter
{
    public string FormatAnimationName(string characterPrefix, string state, string direction)
    {
        return $"{characterPrefix}_{state}_{direction}";
    }
}
