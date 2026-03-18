using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Shared.Actions;

namespace Content.Shared.Traits.Events;

public sealed partial class LewdEggLayingActionEvent : InstantActionEvent { }

[Serializable, NetSerializable]
public sealed partial class LewdEggLayingDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class LewdEggLayingInsideDoAfterEvent : SimpleDoAfterEvent { }


