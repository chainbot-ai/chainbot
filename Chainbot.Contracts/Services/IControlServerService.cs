using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Services
{
    public interface IControlServerService
    {
        Task<bool> Publish(string projectName, string publishVersion, string publishDescription, string nupkgFilePath);
    }
}
