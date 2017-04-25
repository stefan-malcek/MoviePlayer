using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Services
{
    public delegate void ImageWithDetectionChangedEventHandler(object sender, Image<Bgr, byte> image);
    public interface IDetectionService
    {
        event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;

        bool IsDetected { get; }
    }
}
