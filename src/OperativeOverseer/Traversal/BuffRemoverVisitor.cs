using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using System.Collections.Generic;
using System.Linq;

namespace OperativeOverseer.Traversal;

public class BuffRemoverVisitor : IActionVisitor
{
    private readonly BlueprintBuff[] _buffBlueprints;
    private readonly List<(ActionList, ContextActionRemoveBuff)> _matches;

    public IReadOnlyCollection<(ActionList, ContextActionRemoveBuff)> Matches => _matches;

    public BuffRemoverVisitor(params BlueprintBuff[] buffBlueprints)
    {
        _buffBlueprints = buffBlueprints;

        _matches = [];
    }

    public bool Visit(Stack<object> ancestors, GameAction action)
    {
        if (action is not ContextActionRemoveBuff remover)
        {
            return false;
        }

        if (!_buffBlueprints.Contains(remover.Buff))
        {
            return false;
        }

        var list = ancestors.OfType<ActionList>().First();
        _matches.Add((list, remover));

        return false;
    }
}
