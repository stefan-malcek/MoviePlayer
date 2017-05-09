using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Models
{
    public class ProccessedImage
    {
        public Image<Bgr, byte> Image { get; set; }
        public bool IsEyeDetected { get; set; }
    }
}
