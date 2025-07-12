using Kingmaker.ElementsSystem;
using System.Collections.Generic;

namespace OperativeOverseer.Traversal;

public interface IElementVisitor : IVisitor
{
    bool Visit(Stack<object> ancestors, Element element);
}
