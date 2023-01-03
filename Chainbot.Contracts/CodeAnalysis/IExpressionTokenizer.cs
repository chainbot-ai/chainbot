using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.CodeAnalysis
{
    public interface IExpressionTokenizer
    {
        IReadOnlyCollection<string> GetUniqueIdentifiers(string expression);
    }
}
