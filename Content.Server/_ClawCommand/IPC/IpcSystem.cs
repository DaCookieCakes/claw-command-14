using Content.Shared._ClawCommand.IPC;
using Content.Shared.Actions;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Server._ClawCommand.IPC;

public sealed class IpcSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IpcComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<IpcComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<IpcComponent, InteractHandEvent>(OnInteractHand);

        SubscribeLocalEvent<IpcComponent, GetVerbsEvent<Verb>>(OnGetVerbs);
        SubscribeLocalEvent<IpcComponent, IpcRebootDoAfterEvent>(OnReboot);
        SubscribeLocalEvent<IpcComponent, PowerCellChangedEvent>(OnPowerCellChanged);
    }

    private void OnInteractHand(EntityUid uid, IpcComponent component, InteractHandEvent args)
    {
        if (!TryComp<ItemSlotsComponent>(uid, out _))
            return;

        if (!_itemSlots.TryGetSlot(uid, "cell_slot", out var slot))
            return;

        _itemSlots.SetLock(uid, "cell_slot", !slot.Locked);

        _popup.PopupEntity(slot.Locked
                ? Loc.GetString("lock-comp-do-lock-success", ("entityname", args.Target))
                : Loc.GetString("lock-comp-do-unlock-success", ("entityname", args.Target)),
            uid,
            args.User);

        args.Handled = true;
    }

    private void OnShutdown(EntityUid uid, IpcComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.ActionEntity);
    }

    private void OnStartup(EntityUid uid, IpcComponent component, ComponentStartup args)
    {
        _actions.AddAction(uid, ref component.ActionEntity, component.Action);
    }

    private static readonly TimeSpan PowerUpdateDelay = TimeSpan.FromSeconds(1f);
    private TimeSpan _nextPowerUpdate = TimeSpan.Zero;

    private static readonly TimeSpan CritSoundUpdateDelay = TimeSpan.FromSeconds(30f);
    private TimeSpan _nextCritUpdate = TimeSpan.Zero;

    private void OnPowerCellChanged(EntityUid uid, IpcComponent component, PowerCellChangedEvent args)
    {
        UpdatePowerState(uid, component);
    }

    private void UpdatePowerState(EntityUid uid, IpcComponent component)
    {
        if (!_powerCell.TryGetBatteryFromSlot(uid, out var battery))
        {
            if (_mobState.IsCritical(uid) || _mobState.IsDead(uid))
                return;

            component.PowerLossCrit = true;
            _audio.PlayPvs(component.DyingSound, Transform(uid).Coordinates);
            _mobState.ChangeMobState(uid, MobState.Critical);
            return;
        }

        if (_battery.GetCharge(battery.Value.Owner) > 0)
        {
            if (component.PowerLossCrit)
            {
                component.PowerLossCrit = false;
                _mobState.ChangeMobState(uid, MobState.Alive);
            }
            return;
        }

        if (_mobState.IsCritical(uid) || _mobState.IsDead(uid))
            return;

        component.PowerLossCrit = true;
        _audio.PlayPvs(component.DyingSound, Transform(uid).Coordinates);
        _mobState.ChangeMobState(uid, MobState.Critical);
    }

    private void OnGetVerbs(EntityUid uid, IpcComponent comp, GetVerbsEvent<Verb> args)
    {
        if (!_mobState.IsDead(uid))
            return;

        args.Verbs.Add(new Verb
        {
            Text = Loc.GetString("cc14-ipc-reboot-verb"),
            Act = () =>
            {
                _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.RebootTime, new IpcRebootDoAfterEvent(), uid)
                {
                    BreakOnMove = true,
                    NeedHand = true,
                });
            }
        });
    }

    private void OnReboot(EntityUid uid, IpcComponent comp, IpcRebootDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        _mobState.ChangeMobState(uid, MobState.Alive);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        if (curTime < _nextPowerUpdate)
            return;

        _nextPowerUpdate = curTime + PowerUpdateDelay;

        var query = EntityQueryEnumerator<IpcComponent>();
        while (query.MoveNext(out var ipcUid, out var ipcComp))
        {
            UpdatePowerState(ipcUid, ipcComp);
        }

        if (curTime >= _nextCritUpdate)
        {
            _nextCritUpdate = curTime + CritSoundUpdateDelay;

            var critQuery = EntityQueryEnumerator<IpcComponent>();
            while (critQuery.MoveNext(out var ipcUid, out var ipcComp))
            {
                if (!_mobState.IsCritical(ipcUid))
                    continue;

                _audio.PlayPvs(ipcComp.DyingSound, Transform(ipcUid).Coordinates);
            }
        }
    }
}
