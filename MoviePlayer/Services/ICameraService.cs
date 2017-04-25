using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Services
{
    public delegate void ImageChangedEventHandler(object sender, Image<Bgr, byte> image);

    public interface ICameraService
    {
        event ImageChangedEventHandler ImageChanged;

        bool IsRunning { get; }

        void Run();
        void Cancel();
    }
}
