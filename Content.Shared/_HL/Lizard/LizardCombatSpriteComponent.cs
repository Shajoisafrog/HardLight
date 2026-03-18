using Robust.Shared.GameStates;

namespace Content.Shared._HL.Lizard;

/// <summary>
/// Component for lizards that change sprite based on combat mode
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class LizardCombatSpriteComponent : Component
{
    /// <summary>
    /// Sprite state when combat mode is off
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ClosedState = "lizardheadclosed";

    /// <summary>
    /// Sprite state when combat mode is on
    /// </summary>
    [DataField, AutoNetworkedField]
    public string OpenState = "lizardheadopen";
}
