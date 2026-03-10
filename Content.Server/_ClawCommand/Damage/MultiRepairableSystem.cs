using Content.Server.Stack;
using Content.Shared._ClawCommand.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.Tools.Systems;

namespace Content.Server._ClawCommand.Damage;

public sealed class MultiRepairableSystem : EntitySystem
{
    [Dependency] private readonly BlindableSystem _blindable = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedToolSystem _tool = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultiRepairableComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<MultiRepairableComponent, MultiRepairDoAfterEvent>(OnDoAfter);
    }

    private void OnInteractUsing(EntityUid uid, MultiRepairableComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        RepairEntry? matchingEntry = null;
        foreach (var entry in comp.Entries)
        {
            if (!_tool.HasQuality(args.Used, entry.QualityNeeded))
                continue;

            matchingEntry = entry;
            break;
        }

        if (matchingEntry == null)
            return;

        if (!matchingEntry.AllowSelfRepair && args.User == uid)
        {
            _popup.PopupEntity(Loc.GetString("multi-repairable-self-repair-denied"), uid, args.User);
            return;
        }

        args.Handled = _tool.UseTool(
            args.Used,
            args.User,
            uid,
            matchingEntry.DoAfterDelay,
            matchingEntry.QualityNeeded,
            new MultiRepairDoAfterEvent { Entry = matchingEntry },
            matchingEntry.FuelCost);
    }

    private void OnDoAfter(EntityUid uid, MultiRepairableComponent comp, MultiRepairDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var entry = args.Entry;

        if (entry.Damage != null)
            _damageable.TryChangeDamage(uid, entry.Damage, ignoreResistances: true);

        if (entry.EyeHeal != 0)
        {
            _blindable.AdjustEyeDamage(uid, entry.EyeHeal);

            // Consume the item if it's a stack.
            if (TryComp<StackComponent>(args.Used!.Value, out var stackComp))
            {
                _stack.ReduceCount((args.Used.Value, stackComp), 1);

                if (_stack.GetCount((args.Used.Value, stackComp)) <= 0)
                    args.Repeat = false;
            }
            else
            {
                QueueDel(args.Used.Value);
            }
        }

        args.Handled = true;
    }
}

