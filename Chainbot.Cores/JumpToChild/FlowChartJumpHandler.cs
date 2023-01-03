using Chainbot.Contracts.JumpToChild;
using System.Activities.Statements;

namespace Chainbot.Cores.JumpToChild
{
    public class FlowChartJumpHandler : IJumpToChildHandler
    {
        private Flowchart flowchart;

        public FlowChartJumpHandler(Flowchart flowchart)
        {
            this.flowchart = flowchart;
        }

        public void SetStartActivity(string childId)
        {
            
        }
    }
}
