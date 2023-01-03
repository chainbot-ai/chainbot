using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chainbot.Contracts.App
{
    public interface IStudioApplication
    {
        string[] Args { get; }

        void Start(string[] args);
        void Shutdown();

        void OnMergedDictionariesAdd(Collection<ResourceDictionary> mergedDictionaries);
        Window OnLoginWindow();
        Window OnSplashWindow();
        Window OnMainWindow();

        void OnException();
    }
}
