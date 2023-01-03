using log4net;
using Plugins.Shared.Library;
using Chainbot.Contracts.Log;
using ChainbotRobot.Contracts;
using ChainbotRobot.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Cores
{
    public class FFmpegService: IFFmpegService
    {

        private ILogService _logService;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private string _screenCaptureSavePath;

        private CLIManager _cliManager;

        private string _fps;

        private string _crf;

        private string FFmpegPath
        {
            get
            {
                return SharedObject.Instance.ApplicationCurrentDirectory + @"\ffmpeg.exe";
            }
        }

        private string CaptureScreenOption
        {
            get
            {
                return $" -y -rtbufsize 150M -f gdigrab -framerate {_fps}  -draw_mouse 1 -i desktop -c:v libx264 -r {_fps} -preset ultrafast -tune zerolatency -crf {_crf} -pix_fmt yuv420p -movflags +faststart ";
            }
        }


        public FFmpegService(ILogService logService)
        {
            _logService = logService;

        }


        public void Init(string screenCaptureSavePath, string fps, string quality)
        {
            _screenCaptureSavePath = screenCaptureSavePath;
            _fps = fps;
            _crf = ((100 - Convert.ToInt32(quality)) / 2).ToString();
        }

        public void StartCaptureScreen()
        {
            if (_cliManager != null)
            {
                _cliManager.Dispose();
                _cliManager = null;
            }

            _cliManager = new CLIManager();

            var args = $"{CaptureScreenOption} \"{_screenCaptureSavePath}\"";
            _logService.Debug("Start of ffmpeg recording, parameters=" + args, _logger);
            _cliManager.Open(FFmpegPath, args);
        }

        public bool IsRunning()
        {
            if (_cliManager == null)
            {
                return false;
            }

            return _cliManager.IsProcessRunning();
        }

        public void StopCaptureScreen()
        {
            if (_cliManager != null)
            {
                _cliManager.WaitForClose("q");
                _cliManager.Dispose();
                _cliManager = null;

                _logService.Debug("End of ffmpeg recording", _logger);
            }
        }
    }
}
