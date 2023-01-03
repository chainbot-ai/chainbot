using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chainbot.Contracts.Activities
{
    public interface IRecordingService
    {
        event EventHandler BeginEvent;
        event EventHandler EndEvent;

        event EventHandler RecordEvent;
        event EventHandler SaveEvent;

        bool IsRecordingWindowOpened { get; set; }

        void Save(string path);

        void MouseLeftClick();
        void MouseRightClick();
        void MouseDoubleLeftClick();
        void MouseHover();

        void KeyboardInput();
        void KeyboardHotKey();
    }
}
