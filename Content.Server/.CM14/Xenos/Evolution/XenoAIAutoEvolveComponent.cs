using Content.Server.CM14.Xenos.Evolution;

namespace Content.Server.CM14.Xenos.Evolution;

/// <summary>
/// Marks an NPC xeno to automatically evolve when their evolution action is ready.
/// </summary>
[RegisterComponent, Access(typeof(XenoAIAutoEvolveSystem))]
public sealed partial class XenoAIAutoEvolveComponent : Component
{
    /// <summary>
    /// How often to check if evolution is ready (in seconds).
    /// </summary>
    [DataField]
    public float CheckInterval = 30f;

    /// <summary>
    /// Time when the next check should occur.
    /// </summary>
    [DataField]
    public TimeSpan NextCheckTime;
}
