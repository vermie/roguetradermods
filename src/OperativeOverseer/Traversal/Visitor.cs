using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Components;
using System.Collections.Generic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.EntitySystem.Properties;
using Code.GameCore.ElementsSystem;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.UnitLogic.Abilities.Components;

namespace OperativeOverseer.Traversal;

public static class Visitor
{
    public static void Visit(BlueprintScriptableObject blueprint, IVisitor visitor)
    {
        var ancestors = new Stack<object>();

        VisitBlueprint(ancestors, blueprint, visitor);
    }

    private static void VisitBlueprint(Stack<object> ancestors, BlueprintScriptableObject blueprint, IVisitor visitor)
    {
        ancestors.Push(blueprint);

        foreach (var component in blueprint.ComponentsArray)
        {
            VisitComponent(ancestors, component, visitor);
        }

        ancestors.Pop();
    }

    private static void VisitComponent(Stack<object> ancestors, BlueprintComponent component, IVisitor visitor)
    {
        if (visitor is IComponentVisitor componentVisitor)
        {
            if (!componentVisitor.Visit(ancestors, component))
            {
                return;
            }
        }

        ancestors.Push(component);

        switch (component)
        {
            case AbilityEffectRunAction aera: VisitActions(ancestors, aera.Actions, visitor); break;
            case WarhammerWeaponHitTriggerInitiator wwhti: VisitComponentType(ancestors, wwhti, visitor); break;
            case WarhammerDamageTriggerInitiator wdti: VisitComponentType(ancestors, wdti, visitor); break;
            case WarhammerDamageTriggerTarget wdtt: VisitComponentType(ancestors, wdtt, visitor); break;
            case PropertyCalculatorComponent pcc: VisitComponentType(ancestors, pcc, visitor); break;
            default: break;
        }

        ancestors.Pop();
    }

    private static void VisitComponentType(Stack<object> ancestors, WarhammerDamageTriggerInitiator component, IVisitor visitor)
    {
        if (component.Restrictions is not null)
        {
            VisitRestrictions(ancestors, component.Restrictions, visitor);
        }

        if (component.Actions is not null)
        {
            VisitActions(ancestors, component.Actions, visitor);
        }
        if (component.ActionsOnAttacker is not null)
        {
            VisitActions(ancestors, component.ActionsOnAttacker, visitor);
        }
    }

    private static void VisitComponentType(Stack<object> ancestors, WarhammerDamageTriggerTarget component, IVisitor visitor)
    {
        if (component.Restrictions is not null)
        {
            VisitRestrictions(ancestors, component.Restrictions, visitor);
        }

        if (component.Actions is not null)
        {
            VisitActions(ancestors, component.Actions, visitor);
        }
        if (component.ActionsOnAttacker is not null)
        {
            VisitActions(ancestors, component.ActionsOnAttacker, visitor);
        }
    }

    private static void VisitComponentType(Stack<object> ancestors, WarhammerWeaponHitTriggerInitiator component, IVisitor visitor)
    {
        if (component.Restrictions is not null)
        {
            VisitRestrictions(ancestors, component.Restrictions, visitor);
        }

        if (component.ActionOnSelfHit is not null)
        {
            VisitActions(ancestors, component.ActionOnSelfHit, visitor);
        }
        if (component.ActionOnSelfMiss is not null)
        {
            VisitActions(ancestors, component.ActionOnSelfMiss, visitor);
        }
        if (component.ActionsOnTargetHit is not null)
        {
            VisitActions(ancestors, component.ActionsOnTargetHit, visitor);
        }
        if (component.ActionsOnTargetMiss is not null)
        {
            VisitActions(ancestors, component.ActionsOnTargetMiss, visitor);
        }
    }

    private static void VisitComponentType(Stack<object> ancestors, PropertyCalculatorComponent component, IVisitor visitor)
    {
        VisitElements(ancestors, component.Value, visitor);
    }

    private static void VisitElements(Stack<object> ancestors, ElementsList elements, IVisitor visitor)
    {
        ancestors.Push(elements);

        foreach (var element in elements.Elements)
        {
            VisitElement(ancestors, element, visitor);
        }

        ancestors.Pop();
    }

    private static void VisitElement(Stack<object> ancestors, Element element, IVisitor visitor)
    {
        if (visitor is IElementVisitor elementVisitor)
        {
            if (!elementVisitor.Visit(ancestors, element))
            {
                return;
            }
        }

        ancestors.Push(element);

        switch (element)
        {
            case GameAction ga: VisitAction(ancestors, ga, visitor); break;
            case PropertyCalculatorGetter pcg: VisitElements(ancestors, pcg.Value, visitor); break;
            default: break;
        }

        ancestors.Pop();
    }

    private static void VisitRestrictions(Stack<object> ancestors, RestrictionCalculator restrictions, IVisitor visitor)
    {
        ancestors.Push(restrictions);

        VisitElements(ancestors, restrictions.Property, visitor);

        ancestors.Pop();
    }

    private static void VisitActions(Stack<object> ancestors, ActionList actions, IVisitor visitor)
    {
        ancestors.Push(actions);

        foreach (var action in actions.Actions)
        {
            VisitAction(ancestors, action, visitor);
        }

        ancestors.Pop();
    }

    private static void VisitAction(Stack<object> ancestors, GameAction action, IVisitor visitor)
    {
        ancestors.Push(action);

        switch (action)
        {
            case Conditional conditional: VisitActionTerminalType(ancestors, conditional, visitor); return;
            default: break;
        }

        ancestors.Pop();

        if (visitor is IActionVisitor actionVisitor)
        {
            if (!actionVisitor.Visit(ancestors, action))
            {
                return;
            }
        }

        ancestors.Push(action);

        switch (action)
        {
            case ContextActionRepeat repeat: VisitActionContinuingType(ancestors, repeat, visitor); break;
            default: break;
        }

        ancestors.Pop();
    }

    private static void VisitActionTerminalType(Stack<object> ancestors, Conditional action, IVisitor visitor)
    {
        VisitElements(ancestors, action.ConditionsChecker, visitor);

        if (action.IfTrue is not null)
        {
            VisitActions(ancestors, action.IfTrue, visitor);
        }
        if (action.IfFalse is not null)
        {
            VisitActions(ancestors, action.IfFalse, visitor);
        }
    }

    private static void VisitActionContinuingType(Stack<object> ancestors, ContextActionRepeat action, IVisitor visitor)
    {
        VisitActions(ancestors, action.Actions, visitor);
    }
}
