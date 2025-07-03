// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared._Mono.Radar;
using Content.Shared.Shuttles.Components;

namespace Content.Server._Mono.Radar;

public sealed partial class RadarBlipSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<PhysicsComponent> _physQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestBlipsEvent>(OnBlipsRequested);

        _physQuery = GetEntityQuery<PhysicsComponent>();
    }

    private void OnBlipsRequested(RequestBlipsEvent ev, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(ev.Radar, out var radarUid))
            return;

        if (!TryComp<RadarConsoleComponent>(radarUid, out var radar))
            return;

        var blips = AssembleBlipsReport((EntityUid)radarUid, radar);

        var giveEv = new GiveBlipsEvent(blips);
        RaiseNetworkEvent(giveEv, args.SenderSession);
    }

    private List<(NetCoordinates Position, Vector2 Vel, float Scale, Color Color, RadarBlipShape Shape)> AssembleBlipsReport(EntityUid uid, RadarConsoleComponent? component = null)
    {
        var blips = new List<(NetCoordinates Position, Vector2 Vel, float Scale, Color Color, RadarBlipShape Shape)>();

        if (Resolve(uid, ref component))
        {
            var radarXform = Transform(uid);
            var radarPosition = _xform.GetWorldPosition(uid);
            var radarGrid = _xform.GetGrid(uid);

            var blipQuery = EntityQueryEnumerator<RadarBlipComponent, TransformComponent>();

            while (blipQuery.MoveNext(out var blipUid, out var blip, out var blipXform))
            {
                if (!blip.Enabled)
                    continue;

                // This prevents blips from showing on radars that are on different maps
                if (blipXform.MapID != radarMapId)
                    continue;

                var blipGrid = blipXform.GridUid;

                // if (HasComp<CircularShieldRadarComponent>(blipUid))
                // {
                //     // Skip if in FTL
                //     if (isFtlMap)
                //         continue;
                //
                //     // Skip if no grid
                //     if (blipGrid == null)
                //         continue;
                //
                //     // Ensure the grid is a valid MapGrid
                //     if (!HasComp<MapGridComponent>(blipGrid.Value))
                //         continue;
                //
                //     // Ensure the shield is a direct child of the grid
                //     if (blipXform.ParentUid != blipGrid)
                //         continue;
                // }

                var blipVelocity = _physics.GetMapLinearVelocity(blipUid);

                var distance = (blipXform.WorldPosition - radarPosition).Length();
                if (distance > component.MaxRange)
                    continue;

                var blipGrid = _xform.GetGrid(blipUid);

                // Check if this is a shield radar blip without a grid
                // If so, don't display it (fixes grid-orphaned shield generators)
                if (HasComp<CircularShieldRadarComponent>(blipUid) && blipGrid == null)
                    continue;

                if (blip.RequireNoGrid)
                {
                    if (blipGrid != null)
                        continue;

                    // For free-floating blips without a grid, use world position with null grid
                    blips.Add((null, blipPosition, blip.Scale, blip.RadarColor, blip.Shape));
                }
                else if (blip.VisibleFromOtherGrids)
                {
                    // For blips that should be visible from other grids, add them regardless of grid
                    // If on a grid, use grid-relative coordinates
                    if (blipGrid != null)
                    {
                        // Local position relative to grid
                        var gridMatrix = _xform.GetWorldMatrix(blipGrid.Value);
                        Matrix3x2.Invert(gridMatrix, out var invGridMatrix);
                        var localPos = Vector2.Transform(blipPosition, invGridMatrix);

                        // Add grid-relative blip with grid entity ID
                        blips.Add((GetNetEntity(blipGrid.Value), localPos, blip.Scale, blip.RadarColor, blip.Shape));
                    }
                    else
                    {
                        // Fallback to world position with null grid
                        blips.Add((null, blipPosition, blip.Scale, blip.RadarColor, blip.Shape));
                    }
                }
                else
                {
                    // If we're requiring grid, make sure they're on the same grid
                    if (blipGrid != radarGrid)
                        continue;

                    // For grid-aligned blips, store grid NetEntity and grid-local position
                    if (blipGrid != null)
                    {
                        // Local position relative to grid
                        var gridMatrix = _xform.GetWorldMatrix(blipGrid.Value);
                        Matrix3x2.Invert(gridMatrix, out var invGridMatrix);
                        var localPos = Vector2.Transform(blipPosition, invGridMatrix);

                        // Add grid-relative blip with grid entity ID
                        blips.Add((GetNetEntity(blipGrid.Value), localPos, blip.Scale, blip.RadarColor, blip.Shape));
                    }
                    else
                    {
                        // Fallback to world position with null grid
                        blips.Add((null, blipPosition, blip.Scale, blip.RadarColor, blip.Shape));
                    }
                }
            }
        }

        return blips;
    }

    /// <summary>
    /// Assembles trajectory information for hitscan projectiles to be displayed on radar
    /// </summary>
    private List<(Vector2 Start, Vector2 End, float Thickness, Color Color)> AssembleHitscanReport(EntityUid uid, RadarConsoleComponent? component = null)
    {
        var hitscans = new List<(Vector2 Start, Vector2 End, float Thickness, Color Color)>();

        if (!Resolve(uid, ref component))
            return hitscans;

        var radarXform = Transform(uid);
        var radarPosition = _xform.GetWorldPosition(uid);
        var radarGrid = _xform.GetGrid(uid);
        var radarMapId = radarXform.MapID;

        var hitscanQuery = EntityQueryEnumerator<HitscanRadarComponent>();

        while (hitscanQuery.MoveNext(out var hitscanUid, out var hitscan))
        {
            if (!hitscan.Enabled)
                continue;

            // Check if either the start or end point is within radar range
            var startDistance = (hitscan.StartPosition - radarPosition).Length();
            var endDistance = (hitscan.EndPosition - radarPosition).Length();

            if (startDistance > component.MaxRange && endDistance > component.MaxRange)
                continue;

            hitscans.Add((hitscan.StartPosition, hitscan.EndPosition, hitscan.LineThickness, hitscan.RadarColor));
        }

        return hitscans;
    }
}
