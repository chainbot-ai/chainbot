using ActiproSoftware.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.ExpressionEditor
{
    public interface ISyntaxService
    {
        void AddAssemblyReferences(IEnumerable<string> assemblyNames);

        void AddAssemblyReferences(IEnumerable<Assembly> assemblies);

        Task<ISyntaxLanguage> GetLanguageAsync(ExpressionLanguage expresionLanguage);
    }
}
