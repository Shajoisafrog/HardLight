using Content.Server.Fluids.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Robust.Shared.GameObjects;

namespace Content.Server.Fluids.EntitySystems;

public sealed partial class PuddleSystem
{
    private static readonly TimeSpan PuddleDecayDelay = TimeSpan.FromMinutes(30);
    private const string WaterReagentId = "Water";

    private void OnPuddleInit(Entity<PuddleComponent> entity, ref ComponentInit args)
    {
        var decay = EnsureComp<PuddleDecayComponent>(entity);
        decay.DecayAt = _timing.CurTime + PuddleDecayDelay;
        decay.Decayed = false;
    }

    private void ResetPuddleDecay(EntityUid uid)
    {
        var decay = EnsureComp<PuddleDecayComponent>(uid);
        decay.DecayAt = _timing.CurTime + PuddleDecayDelay;
        decay.Decayed = false;
    }

    private void TickDecay()
    {
        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<PuddleDecayComponent, PuddleComponent>();

        while (query.MoveNext(out var uid, out var decay, out var puddle))
        {
            if (decay.Decayed || decay.DecayAt > curTime)
                continue;

            if (!_solutionContainerSystem.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution, out var solution))
                continue;

            var volume = solution.Volume;
            if (volume == FixedPoint2.Zero)
                continue;

            var waterQuantity = solution.GetTotalPrototypeQuantity(WaterReagentId);
            if (waterQuantity == volume && solution.Contents.Count <= 1)
            {
                decay.Decayed = true;
                continue;
            }

            if (puddle.Solution == null)
                continue;

            _solutionContainerSystem.RemoveAllSolution(puddle.Solution.Value);
            _solutionContainerSystem.TryAddReagent(puddle.Solution.Value, WaterReagentId, volume, out _);

            decay.Decayed = true;
        }
    }
}
