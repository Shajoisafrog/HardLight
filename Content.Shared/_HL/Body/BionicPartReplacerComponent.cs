using Content.Shared.Body.Part;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._HL.Body;

[RegisterComponent, NetworkedComponent]
public sealed partial class BionicPartReplacerComponent : Component
{
    [DataField]
    public BodyPartType TargetType = BodyPartType.Leg;

    [DataField]
    public BodyPartSymmetry Symmetry = BodyPartSymmetry.None;

    [DataField(required: true)]
    public EntProtoId ReplacementProto;

    [DataField]
    public bool ReplaceIfPresent = true;
}
