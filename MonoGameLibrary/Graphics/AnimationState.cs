namespace MonoGameLibrary.Graphics;

/// <summary>
/// String-based animation state keys used to select animations.
/// These values should match the animation names defined in your atlas/config.
/// </summary>
public static class AnimationState
{
    public const string Idle = "idle";
    public const string Walk = "walk";
    public const string Run = "run";
    public const string Attack = "attack";
    public const string Hurt = "hurt";
    public const string Death = "death";
}
