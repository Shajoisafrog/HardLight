using Content.Shared._HL.Lizard;
using Content.Shared.CombatMode;
using Robust.Client.GameObjects;

namespace Content.Client._HL.Lizard;

/// <summary>
/// Updates lizard sprite based on combat mode state
/// </summary>
public sealed class LizardCombatSpriteSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LizardCombatSpriteComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, LizardCombatSpriteComponent component, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!TryComp<CombatModeComponent>(uid, out var combat))
            return;

        // Get the base layer (should be the first layer with DamageStateVisualLayers.Base)
        if (!sprite.LayerMapTryGet("enum.DamageStateVisualLayers.Base", out var layerIndex))
            return;

        var newState = combat.IsInCombatMode ? component.OpenState : component.ClosedState;
        _sprite.LayerSetRsiState((uid, sprite), layerIndex, newState);
    }
}
