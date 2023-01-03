using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.Contracts
{
    public interface IFFmpegService
    {
        void Init(string screenCaptureSavePath, string fps, string quality);

        void StartCaptureScreen();

        bool IsRunning();

        void StopCaptureScreen();
    }
}
