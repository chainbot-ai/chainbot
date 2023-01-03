using System;
using System.IO;
using System.Net;

namespace Plugins.Shared.Library.Librarys
{
    public class HttpDownloadFile
    {
        public delegate void OnRunningChangedDelegate(HttpDownloadFile obj);
        public OnRunningChangedDelegate OnRunningChanged { get; set; }

        public delegate void OnDownloadFinishedDelegate(HttpDownloadFile obj);
        public OnDownloadFinishedDelegate OnDownloadFinished { get; set; }

        public delegate void OnDownloadingDelegate(HttpDownloadFile obj);
        public OnDownloadingDelegate OnDownloading { get; set; }


        public long FileTotalBytes { get; set; }
        public long FileDownloadedBytes { get; set; }

        public bool IsDownloadSuccess { get; set; }

        public string Url { get; set; }
        public string SaveFilePath { get; set; }

        private bool _isStop;
        private bool _isRunning;

        public void Stop()
        {
            _isStop = true;
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }

            set
            {
                if(_isRunning == value)
                {
                    return;
                }

                _isRunning = value;

                OnRunningChanged?.Invoke(this);

            }
        }

        public bool Download(string url, string localfile)
        {
            Url = url;
            SaveFilePath = localfile;

            long startPosition = 0;
            FileStream writeStream = null;

            try
            {
                if (File.Exists(localfile))
                {
                    writeStream = File.OpenWrite(localfile);             
                    startPosition = writeStream.Length;                  
                    writeStream.Seek(startPosition, SeekOrigin.Current); 
                }
                else
                {
                    writeStream = new FileStream(localfile, FileMode.Create);
                    startPosition = 0;
                }


                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);
                }

                var myResponse = (HttpWebResponse)myRequest.GetResponse();
                FileTotalBytes = myResponse.ContentLength;

                FileDownloadedBytes = startPosition;

                Stream readStream = myRequest.GetResponse().GetResponseStream();

                byte[] btArray = new byte[512];
                int contentSize = readStream.Read(btArray, 0, btArray.Length);

                IsRunning = true;

                while (contentSize > 0)
                {
                    if(_isStop)
                    {
                        break;
                    }

                    writeStream.Write(btArray, 0, contentSize);
                    FileDownloadedBytes += contentSize;
                    OnDownloading?.Invoke(this);

                    contentSize = readStream.Read(btArray, 0, btArray.Length);
                }

                writeStream.Close();
                readStream.Close();

                IsDownloadSuccess = true;        
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                if (writeStream != null)
                {
                    writeStream.Close();
                }
                
                IsDownloadSuccess = false;       
            }

            IsRunning = false;

            OnDownloadFinished?.Invoke(this);

            return IsDownloadSuccess;
        }

    }
}
