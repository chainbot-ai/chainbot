
namespace Chainbot.Cores.CodeAnalysis
{
    public class GenericFlatExpressionToken : FlatExpressionToken
    {
        public override string StringRepresentation { get; set; }

        public override TokenType Type { get; set; }

        public override int DepthLevel { get; set; }

        public override int StartPos
        {
            get
            {
                return this._startPos;
            }
        }

        public override int EndPos
        {
            get
            {
                return this._endPos;
            }
        }

        public override string[] ArgumentExpressions
        {
            get
            {
                return this._argumentExpressions;
            }
        }

        internal void SetStartPos(int value)
        {
            this._startPos = value;
        }

        internal void SetEndPos(int value)
        {
            this._endPos = value;
        }

        internal void SetArgumentExpressions(string[] value)
        {
            this._argumentExpressions = value;
        }

        protected int _startPos;

        protected int _endPos;

        protected string[] _argumentExpressions;
    }
}
