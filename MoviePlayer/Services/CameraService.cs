using System.ComponentModel;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Services
{
    public class CameraService : ICameraService
    {
        private readonly Capture _capture;
        private readonly BackgroundWorker _worker;

        public bool IsRunning => _worker?.IsBusy ?? false;
        public event ImageChangedEventHandler ImageChanged;

        public CameraService()
        {
            _capture = new Capture();
            _worker = new BackgroundWorker { WorkerSupportsCancellation = true };
            _worker.DoWork += Worker_CaptureImage;
        }

        private void Worker_CaptureImage(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            Debug.WriteLine("DoWork");
            while (!_worker.CancellationPending)
            {
                Debug.WriteLine("DoWork_Loop");
                RaiseImageChangedEvent(_capture.QuerySmallFrame().ToImage<Bgr, byte>());
            }
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
            _worker.RunWorkerAsync();
        }

        public void Cancel()
        {
            _worker.CancelAsync();
        }

        private void RaiseImageChangedEvent(Image<Bgr, byte> image)
        {
            ImageChanged?.Invoke(this, image);
        }
    }
}
