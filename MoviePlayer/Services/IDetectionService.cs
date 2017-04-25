using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Services
{
    public delegate void ImageWithDetectionChangedEventHandler(object sender, ImageEventArgs e);
    public interface IDetectionService
    {
        event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;

        bool IsDetected { get; }
    }

    public class ImageEventArgs : EventArgs
    {
        public Image<Bgr, byte> Image { get; set; }
        public bool IsDetecting { get; set; }

        public ImageEventArgs(Image<Bgr, byte> image, bool isDetecting)
        {
            Image = image;
            IsDetecting = isDetecting;
        }
    }
}
