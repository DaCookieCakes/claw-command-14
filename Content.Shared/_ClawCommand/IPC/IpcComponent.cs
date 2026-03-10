using Content.Shared.Alert;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ClawCommand.IPC;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class IpcComponent : Component
{
    /// <summary>
    ///     Whether the IPC is currently rebooting after death/disable.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Rebooting;

    /// <summary>
    ///     Whether the IPC is critical from power loss or combat.
    /// </summary>
    [DataField]
    public bool PowerLossCrit;

    /// <summary>
    ///     How long it takes to reboot the IPC.
    /// </summary>
    [DataField]
    public float RebootTime = 2f;

        /// <summary>
    /// The battery charge alert.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> BatteryAlert = "BorgBattery";

    /// <summary>
    /// The alert for a missing battery.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> NoBatteryAlert = "BorgBatteryNone";

    [DataField]
    public EntProtoId Action = "ActionPAIPlayMidi";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? ChargeSound = new SoundPathSpecifier("/Audio/Effects/sparks1.ogg")
    {
        Params = new AudioParams
        {
            Variation = 0.250f,
        }
    };

    [DataField, AutoNetworkedField]
    public SoundSpecifier? DyingSound = new SoundPathSpecifier("/Audio/Machines/warning_buzzer_xenoborg.ogg")
    {
        Params = new AudioParams
        {
            Volume = -2,
        }
    };
}

[Serializable, NetSerializable]
public sealed partial class IpcRebootDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class IpcChargeAfterEvent : SimpleDoAfterEvent;
