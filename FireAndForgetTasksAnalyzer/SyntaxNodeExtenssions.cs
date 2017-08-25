using Microsoft.CodeAnalysis;

namespace FireAndForgetTasksAnalyzer
{
    public static class SyntaxNodeExtenssions
    {
        public static T GetParentOfType<T>(this SyntaxNode node)
            where T : SyntaxNode
        {
            if (node is T)
                return (T)node;

            if (node.Parent == null)
                return null;

            return GetParentOfType<T>(node.Parent);
        }
    }
}
