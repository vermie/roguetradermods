using Kingmaker.ElementsSystem;
using System.Collections.Generic;

namespace OperativeOverseer.Traversal;

public interface IActionVisitor : IVisitor
{
    bool Visit(Stack<object> ancestors, GameAction action);
}
