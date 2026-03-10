using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ClawCommand.Damage;

/// <summary>
///     A component to mark an entity to be multiple repairable, such as cutting AND prying.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MultiRepairableComponent : Component
{
    [DataField]
    public List<RepairEntry> Entries = new();
}

[DataDefinition, Serializable]
public sealed partial class RepairEntry
{
    [DataField]
    public ProtoId<ToolQualityPrototype> QualityNeeded = "Welding";

    [DataField]
    public float FuelCost = 0f;

    [DataField]
    public float DoAfterDelay = 2f;

    [DataField]
    public bool AllowSelfRepair = true;

    [DataField]
    public DamageSpecifier? Damage;

    /// <summary>
    /// Eye damage to heal, negative = heal
    /// </summary>
    [DataField]
    public int EyeHeal = 0;
}

[Serializable, NetSerializable]
public sealed partial class MultiRepairDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public RepairEntry Entry = default!;
}
