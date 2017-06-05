using System.ComponentModel;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Messaging;
using MoviePlayer.Services.Interfaces;

namespace MoviePlayer.Services
{
    public class CameraService : ICameraService
    {
        private readonly Capture _capture;
        private readonly BackgroundWorker _worker;

        public bool IsRunning => _worker?.IsBusy ?? false;

        public CameraService()
        {
            _capture = new Capture(0);
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += Worker_CaptureImage;
        }

        ~CameraService()
        {
            _capture?.Dispose();

            if (IsRunning)
            {
                Cancel();
            }

            _worker?.Dispose();
        }

        public void Run()
        {
            Debug.WriteLine("Run called");
            _worker.RunWorkerAsync();
        }

        public void Cancel()
        {
            _worker.CancelAsync();
        }

        private void Worker_CaptureImage(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (!_worker.CancellationPending)
            {
                Messenger.Default.Send(_capture.QuerySmallFrame().ToImage<Bgr, byte>());
            }
        }
    }
}
