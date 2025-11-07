using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.RatKing;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._Sunrise.CarpQueen;

public abstract class SharedCarpQueenSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] protected readonly IRobustRandom Random = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CarpQueenComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<CarpQueenComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CarpQueenComponent, RatKingOrderActionEvent>(OnOrderAction);
    }

    protected virtual void OnStartup(EntityUid uid, CarpQueenComponent component, ComponentStartup args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        _actions.AddAction(uid, ref component.ActionSummonEntity, component.ActionSummon, component: comp);
        _actions.AddAction(uid, ref component.ActionOrderStayEntity, component.ActionOrderStay, component: comp);
        _actions.AddAction(uid, ref component.ActionOrderFollowEntity, component.ActionOrderFollow, component: comp);
        _actions.AddAction(uid, ref component.ActionOrderKillEntity, component.ActionOrderKill, component: comp);
        _actions.AddAction(uid, ref component.ActionOrderLooseEntity, component.ActionOrderLoose, component: comp);

        UpdateActions(uid, component);
    }

    private void OnShutdown(EntityUid uid, CarpQueenComponent component, ComponentShutdown args)
    {
        foreach (var servant in component.Servants)
        {
            if (TryComp(servant, out CarpQueenServantComponent? servantComp))
                servantComp.Queen = null;
        }

        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        var actions = new Entity<ActionsComponent?>(uid, comp);
        _actions.RemoveAction(actions, component.ActionSummonEntity);
        _actions.RemoveAction(actions, component.ActionOrderStayEntity);
        _actions.RemoveAction(actions, component.ActionOrderFollowEntity);
        _actions.RemoveAction(actions, component.ActionOrderKillEntity);
        _actions.RemoveAction(actions, component.ActionOrderLooseEntity);
    }

    private void OnOrderAction(EntityUid uid, CarpQueenComponent component, RatKingOrderActionEvent args)
    {
        if (component.CurrentOrder == args.Type)
            return;

        args.Handled = true;
        component.CurrentOrder = args.Type;
        Dirty(uid, component);

        UpdateActions(uid, component);
        UpdateAllServants(uid, component);
        DoCommandCallout(uid, component);
    }

    private void UpdateActions(EntityUid uid, CarpQueenComponent component)
    {
        _actions.SetToggled(component.ActionOrderStayEntity, component.CurrentOrder == RatKingOrderType.Stay);
        _actions.SetToggled(component.ActionOrderFollowEntity, component.CurrentOrder == RatKingOrderType.Follow);
        _actions.SetToggled(component.ActionOrderKillEntity, component.CurrentOrder == RatKingOrderType.CheeseEm);
        _actions.SetToggled(component.ActionOrderLooseEntity, component.CurrentOrder == RatKingOrderType.Loose);
        _actions.StartUseDelay(component.ActionOrderStayEntity);
        _actions.StartUseDelay(component.ActionOrderFollowEntity);
        _actions.StartUseDelay(component.ActionOrderKillEntity);
        _actions.StartUseDelay(component.ActionOrderLooseEntity);
    }

    public void UpdateAllServants(EntityUid uid, CarpQueenComponent component)
    {
        foreach (var servant in component.Servants)
        {
            UpdateServantNpc(servant, component.CurrentOrder);
        }
    }

    public virtual void UpdateServantNpc(EntityUid uid, RatKingOrderType orderType)
    {
    }

    public virtual void DoCommandCallout(EntityUid uid, CarpQueenComponent component)
    {
    }
}


