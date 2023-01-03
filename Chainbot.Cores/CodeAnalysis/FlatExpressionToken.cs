
namespace Chainbot.Cores.CodeAnalysis
{
    public abstract class FlatExpressionToken : ExpressionToken
    {
        public virtual int DepthLevel { get; set; }

        public virtual int StartPos { get; }

        public virtual int EndPos { get; }

        public virtual string[] ArgumentExpressions { get; }
    }
}
