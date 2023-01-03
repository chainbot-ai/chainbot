using Chainbot.Contracts.JumpToChild;
using System.Activities.Statements;

namespace Chainbot.Cores.JumpToChild
{
    public class SequenceJumpHandler : IJumpToChildHandler
    {
        private Sequence sequence;

        public SequenceJumpHandler(Sequence sequence)
        {
            this.sequence = sequence;
        }

        public void SetStartActivity(string childId)
        {
            
        }
    }
}
