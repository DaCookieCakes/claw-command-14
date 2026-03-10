using Content.Client.Power.EntitySystems;
using Content.Shared._ClawCommand.IPC;
using Content.Shared.Alert;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client._ClawCommand.IPC;

public sealed class IpcClientSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;
    [Dependency] private readonly BatterySystem _battery = default!;

    private static readonly TimeSpan AlertUpdateDelay = TimeSpan.FromSeconds(0.5f);
    private TimeSpan _nextAlertUpdate = TimeSpan.Zero;
    private EntityQuery<IpcComponent> _ipcQuery;
    private EntityQuery<PowerCellSlotComponent> _slotQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IpcComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<IpcComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _ipcQuery = GetEntityQuery<IpcComponent>();
        _slotQuery = GetEntityQuery<PowerCellSlotComponent>();
    }

    private void OnPlayerAttached(Entity<IpcComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        UpdateBatteryAlert(ent);
    }

    private void OnPlayerDetached(Entity<IpcComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.BatteryAlert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.NoBatteryAlert);
    }

    private void UpdateBatteryAlert(Entity<IpcComponent> ent)
    {
        if (!_powerCell.TryGetBatteryFromSlot(ent.Owner, out var battery))
        {
            _alerts.ShowAlert(ent.Owner, ent.Comp.NoBatteryAlert);
            return;
        }

        var chargeLevel = (short)MathF.Round(_battery.GetChargeLevel(battery.Value.AsNullable()) * 10f);

        if (chargeLevel == 0 && _powerCell.HasDrawCharge(ent.Owner))
            chargeLevel = 1;

        _alerts.ShowAlert(ent.Owner, ent.Comp.BatteryAlert, chargeLevel);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_player.LocalEntity is not { } localPlayer)
            return;

        var curTime = _timing.CurTime;
        if (curTime < _nextAlertUpdate)
            return;

        _nextAlertUpdate = curTime + AlertUpdateDelay;

        if (!_ipcQuery.TryComp(localPlayer, out var ipc) || !_slotQuery.TryComp(localPlayer, out _))
            return;

        UpdateBatteryAlert((localPlayer, ipc));
    }
}
