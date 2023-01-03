using Chainbot.Contracts.Log;
using Chainbot.Contracts.Nupkg;
using Chainbot.Contracts.UI;
using log4net;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Cores.Nupkg
{
    public class PackageImportService : IPackageImportService
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ILogService _logService;

        private IMessageBoxService _messageBoxService;

        private ZipPackage _zipPackage;

        public PackageImportService(ILogService logService, IMessageBoxService messageBoxService)
        {
            _logService = logService;
            _messageBoxService = messageBoxService;
        }

        public bool Init(string nupkgFile)
        {
            try
            {
                _zipPackage = new ZipPackage(nupkgFile);
            }
            catch (Exception err)
            {
                _logService.Error(err, _logger);
                return false;
            }
            
            return true;
        }

        public string GetDescription()
        {
            return _zipPackage.Description;
        }

        public string GetId()
        {
            return _zipPackage.Id;
        }

        public string GetVersion()
        {
            return _zipPackage.Version.ToString();
        }


        public bool ExtractToDirectory(string path)
        {
            try
            {
                var files = _zipPackage.GetFiles();
                foreach(var file in files)
                {
                    var effectivePath = file.EffectivePath;
                    var stream = file.GetStream();
                    var outputFilePath = Path.Combine(path, effectivePath);

                    var outputDir = Path.GetDirectoryName(outputFilePath);
                    if (!System.IO.Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }

                    using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                    {
                        stream.CopyTo(outputFileStream);
                    }
                }

            }
            catch (Exception e)
            {
                _logService.Error(e, _logger);

                _messageBoxService.ShowError("导入Nupkg包的过程中发生异常，请检查！");
                return false;
            }

            return true;
        }


    }
}
