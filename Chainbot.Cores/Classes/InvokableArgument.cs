using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Classes
{
    public class InvokableArgument
    {
        public string Name { get; }

        public Type ArgumentInnerType { get; }

        public ArgumentDirection Direction { get; set; }

        public InvokableArgument(string name, ArgumentDirection direction, Type argumentInnerType)
        {
            this.Name = name;
            this.Direction = direction;
            this.ArgumentInnerType = argumentInnerType;
        }
    }
}
