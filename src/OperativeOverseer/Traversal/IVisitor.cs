using Kingmaker.Blueprints;

namespace OperativeOverseer.Traversal;

public interface IVisitor
{
}

public static class VisitorExtensions
{
    public static void Visit(this IVisitor visitor, BlueprintScriptableObject blueprint)
    {
        Visitor.Visit(blueprint, visitor);
    }
}
