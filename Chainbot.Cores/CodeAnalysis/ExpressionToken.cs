using System;

namespace Chainbot.Cores.CodeAnalysis
{
    public enum TokenType
    {
        ObjectCreation = 1,

        IdentifierReference,

        LiteralUsage
    }

    public abstract class ExpressionToken
    {
        public virtual string StringRepresentation { get; set; }

        public virtual TokenType Type { get; set; }

        public virtual Type ExpressionType { get; set; }
    }
}
