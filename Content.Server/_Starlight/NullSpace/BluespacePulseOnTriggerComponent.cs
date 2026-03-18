using Robust.Shared.Serialization;

namespace Content.Server._Starlight.NullSpace;

/// <summary>
/// Component that causes a Bluespace pulse when the owner is triggered.
/// Used with TriggerOnProximity to purge NullSpace entities and stun them.
/// </summary>
[RegisterComponent]
public sealed partial class BluespacePulseOnTriggerComponent : Component
{
    [DataField]
    public float Radius = 10f;

    [DataField]
    public float StunSeconds = 4f;
}
