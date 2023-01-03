using Chainbot.Contracts.CodeAnalysis;
using Chainbot.Cores.ExpressionEditor;
using System;

namespace Chainbot.Cores.CodeAnalysis
{
    public class ExpressionAnalyzerFactory
    {
        public IExpressionTokenizer GetIExpressionAnalyzer(ExpressionLanguage language)
        {
            if (language == ExpressionLanguage.VisualBasic)
            {
                return new VisualBasicExpressionTokenizer();
            }

            throw new NotImplementedException(language.ToString("G"));
        }

        public IExpressionTokenizer GetIExpressionAnalyzer(string language)
        {
            return this.GetIExpressionAnalyzer(this.FromLanguageName(language));
        }

        private ExpressionLanguage FromLanguageName(string language)
        {
            if (language == "VB" || language == "VisualBasic")
            {
                return ExpressionLanguage.VisualBasic;
            }

            throw new NotImplementedException(language);
        }

        public const string VisualBasicLanguage = "VB";

        public const string VisualBasicExpressionLanguage = "VisualBasic";
    }
}
