using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Timing;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators.Interactions;

/// <summary>
/// Attempts to pull a target entity with a cooldown to prevent spam.
/// </summary>
public sealed partial class PullOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private PullingSystem _pulling = default!;

    /// <summary>
    /// The blackboard key containing the target entity to pull.
    /// </summary>
    [DataField(required: true)]
    public string TargetKey = "Target";

    /// <summary>
    /// Cooldown between pull attempts in seconds.
    /// </summary>
    [DataField]
    public float Cooldown = 2f;

    private const string LastPullAttemptKey = "LastPullAttempt";

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pulling = sysManager.GetEntitySystem<PullingSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager))
            return HTNOperatorStatus.Failed;

        // Check cooldown
        if (blackboard.TryGetValue<TimeSpan>(LastPullAttemptKey, out var lastAttempt, _entManager))
        {
            if ((_timing.CurTime - lastAttempt).TotalSeconds < Cooldown)
                return HTNOperatorStatus.Failed; // Still on cooldown
        }

        // Always try to pull (will succeed if not already pulling, or re-establish if lost)
        blackboard.SetValue(LastPullAttemptKey, _timing.CurTime);
        _pulling.TryStartPull(owner, target);
        
        return HTNOperatorStatus.Finished;
    }
}
