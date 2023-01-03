using System;
using System.Activities;
using System.Activities.Expressions;

namespace Chainbot.Cores.CodeAnalysis
{
    public static class ExpressionHelper
    {
        internal static ArgumentDirection GetArgumentDirectionFromExpression(this ITextExpression expression)
        {
            Type c = typeof(Location<>).MakeGenericType(new Type[]
            {
                expression.GetType().GetGenericArguments()[0]
            });

            if (expression.GetType().BaseType.GetGenericArguments()[0].IsAssignableFrom(c))
            {
                return ArgumentDirection.Out;
            }

            return ArgumentDirection.In;
        }
    }
}
