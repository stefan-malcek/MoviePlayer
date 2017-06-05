using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight.Messaging;
using MoviePlayer.Models;
using MoviePlayer.Services.Interfaces;

namespace MoviePlayer.Services
{
    public class DetectionService : IDetectionService
    {
        private CascadeClassifier _classifier;
        private bool _isDetecting = false;
        private List<Rectangle> _detectedObjects;

        public bool IsObjectDetected => _detectedObjects.Any();

        public DetectionService()
        {
            _detectedObjects = new List<Rectangle>();

            Messenger.Default.Register<Image<Bgr, byte>>(this, ReceiveImage);
        }

        ~DetectionService()
        {
            _classifier?.Dispose();
        }

        public void SetFilter(string filter)
        {
            _classifier?.Dispose();
            _classifier = new CascadeClassifier(filter);
        }

        private async void ReceiveImage(Image<Bgr, byte> image)
        {
            if (!_isDetecting)
            {
                _isDetecting = true;
                var result = await DetectFacesAsync(image);
                _detectedObjects = result;
                _isDetecting = false;
            }

            DrawRectangles(image);
            Messenger.Default.Send(new ProccessedImage { Image = image, IsEyeDetected = IsObjectDetected });
        }

        private Task<List<Rectangle>> DetectFacesAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                var faces = new List<Rectangle>();

                //Read the HaarCascade objects
                using (var gray = image.Convert<Gray, byte>()) //Convert it to Grayscale
                {
                    //normalizes brightness and increases contrast of the image
                    gray._EqualizeHist();

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel  new Size(20, 20)
                    var facesDetected = _classifier.DetectMultiScale(gray, 1.1, 10, Size.Empty);
                    faces.AddRange(facesDetected);
                }

                return faces;
            });
        }

        private void DrawRectangles(Image<Bgr, byte> image)
        {
            foreach (var eye in _detectedObjects)
            {
                //the detected eye(s) is highlighted here using a box that is drawn around it/them
                image.Draw(eye, new Bgr(Color.BurlyWood), 3);
            }
        }
    }
}
