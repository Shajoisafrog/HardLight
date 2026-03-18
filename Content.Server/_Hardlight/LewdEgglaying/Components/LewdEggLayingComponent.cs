using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Server.Animals.Components;

/// <summary>
///     This component handles egg laying for the Egg layer trait
/// </summary>

[RegisterComponent]
public sealed partial class LewdEggLayingComponent : Component
{
    [DataField]
    public EntProtoId ActionPrototype = "ActionLewdLayEgg";

    [DataField]
    public EntityUid? Action;

    /// <summary>
    ///     Messages while producing eggs
    /// </summary>
    [DataField]
    public IReadOnlyList<string> FlavorMessages = new[]
    {
        "action-popup-lay-egg-flavor-1",
        "action-popup-lay-egg-flavor-2",
        "action-popup-lay-egg-flavor-3",
        "action-popup-lay-egg-flavor-4"
    };

    /// <summary>
    ///     The item that gets laid/spawned, retrieved from animal prototype.
    /// </summary>
    [DataField(required: true)]
    public List<EntitySpawnEntry> EggSpawn = new();

    /// <summary>
    ///     The sound played when egg pops out
    /// </summary>
    [DataField]
    public SoundSpecifier EggLaySound = new SoundPathSpecifier("/Audio/Effects/pop.ogg");

    /// <summary>
    ///     How many eggs produced per unit of cum
    /// </summary>
    [DataField]
    public float ProductionMult = 0.2f;

    /// <summary>
    ///     How many eggs between each flavor text
    /// </summary>
    [DataField]
    public float FlavorFreq = 6.0f;

    /// <summary>
    ///     The number of eggs when movespeed is slowed
    /// </summary>
    [DataField]
    public float EggSlowThreshold = 10;

    /// <summary>
    ///     The max number of eggs you can hold
    /// </summary>
    [DataField]
    public float MaxEggs = 24;

    /// <summary>
    ///     How much the user is slowed by eggs
    /// </summary>
    [DataField]
    public float EggSlowMult = 0.5f;

    /// <summary>
    ///     How long it takes for the egg to come out
    /// </summary>
    [DataField]
    public float EggLayDelay = 5.0f;

    /// <summary>
    /// The number of eggs in your belly
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float eggs = 0;

    /// <summary>
    /// The number of eggs produced since last flavor text
    /// </summary>
    public float eggsFlavorAccum = 0;

    /// <summary>
    /// The number of eggs produced since last flavor text
    /// </summary>
    public bool Temporary = false;

    public void addEggs(float amt)
    {
        eggs = Math.Clamp(eggs + amt, 0, MaxEggs);
        if(amt > 0)
        {
            eggsFlavorAccum += amt;
        }
    }
    public bool hasEggs()
    {
        return eggs >= 1.0f;
    }
    public bool isHeavyOfEggs()
    {
        return eggs >= EggSlowThreshold;
    }
    public bool isFullOfEggs()
    {
        return eggs >= MaxEggs;
    }
    public bool doFlavor()
    {
        if(eggsFlavorAccum >= FlavorFreq)
        {
            eggsFlavorAccum -= FlavorFreq;
            return true;
        }
        return false;
    }
    public void makeTempFrom(LewdEggLayingComponent other)
    {
        FlavorMessages = other.FlavorMessages;
        EggSpawn = other.EggSpawn;
        EggLaySound = other.EggLaySound;
        EggSlowThreshold = other.EggSlowThreshold;
        MaxEggs = other.MaxEggs;
        EggSlowMult = other.EggSlowMult;
        EggLayDelay = other.EggLayDelay;
        Temporary = true;
    }
}
