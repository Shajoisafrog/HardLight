using System;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.StatusEffectNew;
using NUnit.Framework;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.Body
{
    [TestFixture]
    [TestOf(typeof(BloodstreamSystem))]
    public sealed class BloodstreamStatusRegressionTest
    {
        [Test]
        public async Task HealingBloodAboveThresholdClearsBloodlossBeforePeriodicUpdate()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;
            var map = await pair.CreateTestMap();

            await server.WaitIdleAsync();

            var entMan = server.ResolveDependency<IEntityManager>();
            var systems = server.ResolveDependency<IEntitySystemManager>();
            var bloodstream = systems.GetEntitySystem<BloodstreamSystem>();
            var status = systems.GetEntitySystem<StatusEffectsSystem>();

            EntityUid mob = default;

            await server.WaitPost(() =>
            {
                mob = entMan.SpawnEntity("MobHuman", map.GridCoords);

                var bloodstreamComp = entMan.GetComponent<BloodstreamComponent>(mob);

                Assert.That(
                    bloodstream.TryModifyBloodLevel((mob, bloodstreamComp), -bloodstreamComp.BloodMaxVolume),
                    Is.True,
                    "Failed to drain blood for the regression setup.");

                Assert.That(
                    status.TrySetStatusEffectDuration(mob, SharedBloodstreamSystem.Bloodloss),
                    Is.True,
                    "Failed to seed the bloodloss status for the regression setup.");
            });

            await server.WaitAssertion(() =>
            {
                Assert.That(
                    status.HasStatusEffect(mob, SharedBloodstreamSystem.Bloodloss),
                    Is.True,
                    "Bloodloss should be present before verifying the cleanup path.");
            });

            await server.WaitPost(() =>
            {
                var bloodstreamComp = entMan.GetComponent<BloodstreamComponent>(mob);

                Assert.That(
                    bloodstream.TryModifyBloodLevel((mob, bloodstreamComp), bloodstreamComp.BloodMaxVolume),
                    Is.True,
                    "Failed to restore blood for the regression check.");
            });

            await server.WaitRunTicks(1);

            await server.WaitAssertion(() =>
            {
                Assert.That(
                    status.HasStatusEffect(mob, SharedBloodstreamSystem.Bloodloss),
                    Is.False,
                    "Restoring blood above the threshold should clear bloodloss immediately instead of waiting for the next bloodstream update.");
            });

            await pair.CleanReturnAsync();
        }
    }
}