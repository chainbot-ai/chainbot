using Chainbot.Contracts.Activities;
using Chainbot.Contracts.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chainbot.Contracts.AppDomains;

namespace Chainbot.Cores.Activities
{
    public class RecordingServiceProxy : MarshalByRefServiceProxyBase<IRecordingService>, IRecordingServiceProxy
    {
        public event EventHandler BeginEvent;
        public event EventHandler EndEvent;

        public event EventHandler RecordEvent;
        public event EventHandler SaveEvent;

        public bool IsRecordingWindowOpened
        {
            get
            {
                return InnerService.IsRecordingWindowOpened;
            }

            set
            {
                InnerService.IsRecordingWindowOpened = value;
            }
        }

        public RecordingServiceProxy(IAppDomainControllerService appDomainControllerService) : base(appDomainControllerService)
        {
        }

        protected override void OnAfterConnectToInnerService()
        {
            InnerService.BeginEvent += InnerService_BeginEvent;
            InnerService.EndEvent += InnerService_EndEvent;

            InnerService.RecordEvent += InnerService_RecordEvent;
            InnerService.SaveEvent += InnerService_SaveEvent;
        }

       
        private void InnerService_BeginEvent(object sender, EventArgs e)
        {
            BeginEvent?.Invoke(this, e);
        }

        private void InnerService_EndEvent(object sender, EventArgs e)
        {
            EndEvent?.Invoke(this, e);
        }


        private void InnerService_RecordEvent(object sender, EventArgs e)
        {
            RecordEvent?.Invoke(this, e);
        }

        private void InnerService_SaveEvent(object sender, EventArgs e)
        {
            SaveEvent?.Invoke(this, e);
        }

        public void KeyboardHotKey()
        {
            InnerService.KeyboardHotKey();
        }

        public void KeyboardInput()
        {
            InnerService.KeyboardInput();
        }

        public void MouseDoubleLeftClick()
        {
            InnerService.MouseDoubleLeftClick();
        }

        public void MouseHover()
        {
            InnerService.MouseHover();
        }

        public void MouseLeftClick()
        {
            InnerService.MouseLeftClick();
        }

        public void MouseRightClick()
        {
            InnerService.MouseRightClick();
        }

        public void Save(string path)
        {
            InnerService.Save(path);
        }
    }
}
