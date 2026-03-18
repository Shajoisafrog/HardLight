using Content.Server.Afk;
using Content.Server.Afk.Events;
using Content.Server.Mind;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Players;

/// <summary>
/// Cleans up player-controlled entities when a player has been AFK or offline for too long.
/// </summary>
public sealed class InactivityCleanupSystem : EntitySystem
{
    [Dependency] private readonly IAfkManager _afkManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    private readonly Dictionary<NetUserId, TimeSpan> _afkSince = new();
    private readonly Dictionary<NetUserId, TimeSpan> _offlineSince = new();

    private TimeSpan _nextCheck;

    private static readonly TimeSpan AfkCleanupDelay = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan OfflineCleanupDelay = TimeSpan.FromHours(1);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(30);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AFKEvent>(OnAfk);
        SubscribeLocalEvent<UnAFKEvent>(OnUnAfk);
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
        _afkSince.Clear();
        _offlineSince.Clear();
    }

    private void OnAfk(ref AFKEvent ev)
    {
        var userId = ev.Session.UserId;
        _afkSince[userId] = _timing.RealTime;
    }

    private void OnUnAfk(ref UnAFKEvent ev)
    {
        var userId = ev.Session.UserId;
        _afkSince.Remove(userId);
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        var userId = e.Session.UserId;
        if (e.NewStatus == SessionStatus.Disconnected)
        {
            _offlineSince[userId] = _timing.RealTime;
            _afkSince.Remove(userId);
            return;
        }

        _offlineSince.Remove(userId);
        _afkSince.Remove(userId);
    }


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.RealTime < _nextCheck)
            return;

        _nextCheck = _timing.RealTime + CheckInterval;

        var now = _timing.RealTime;
        var query = EntityQueryEnumerator<MindComponent>();
        while (query.MoveNext(out var mindId, out var mind))
        {
            if (mind.UserId is not { } userId)
                continue;

            if (_offlineSince.TryGetValue(userId, out var offlineAt) && now - offlineAt >= OfflineCleanupDelay)
            {
                CleanupMindEntity(mindId, mind, "offline");
                _offlineSince.Remove(userId);
                _afkSince.Remove(userId);
                continue;
            }

            if (!_playerManager.TryGetSessionById(userId, out var session) || session.Status == SessionStatus.Disconnected)
                continue;

            if (_afkSince.TryGetValue(userId, out var afkAt))
            {
                if (now - afkAt >= AfkCleanupDelay && _afkManager.IsAfk(session))
                {
                    CleanupMindEntity(mindId, mind, "afk");
                    _afkSince.Remove(userId);
                }

                continue;
            }

            if (_afkManager.IsAfk(session))
            {
                _afkSince[userId] = now;
            }
        }
    }

    private void CleanupMindEntity(EntityUid mindId, MindComponent mind, string reason)
    {
        var target = mind.OwnedEntity ?? mind.CurrentEntity;
        if (target is null)
            return;

        if (TerminatingOrDeleted(target.Value))
            return;

        if (HasComp<GhostComponent>(target.Value))
            return;

        Log.Info($"Inactivity cleanup: deleting {ToPrettyString(target.Value)} for mind {ToPrettyString(mindId)} due to {reason}.");
        QueueDel(target.Value);
    }
}
