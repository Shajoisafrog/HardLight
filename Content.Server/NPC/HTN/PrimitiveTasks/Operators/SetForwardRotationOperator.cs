using System.Threading;
using System.Threading.Tasks;

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

/// <summary>
/// Sets the rotation target to forward-facing (0 degrees)
/// </summary>
public sealed partial class SetForwardRotationOperator : HTNOperator
{
    [DataField("targetKey")]
    public string TargetKey = "RotateTarget";

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
        CancellationToken cancelToken)
    {
        return (true, new Dictionary<string, object>()
        {
            {TargetKey, new Angle(global::System.Math.PI)}
        });
    }
}
