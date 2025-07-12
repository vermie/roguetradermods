using Kingmaker.Blueprints;
using System.Collections.Generic;

namespace OperativeOverseer.Traversal;

public interface IComponentVisitor : IVisitor
{
    bool Visit(Stack<object> ancestors, BlueprintComponent component);
}
