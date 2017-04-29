using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace MoviePlayer.Services
{
    public class DetectionService : IDetectionService
    {
        private const string CascadeName = "haarcascade_eye.xml";
        private readonly ICameraService _cameraService;
        private bool isDetecting = false;
        private List<Rectangle> _eyes;

        public event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;
        public bool IsDetected => _eyes.Any();

        public DetectionService(ICameraService cameraService)
        {
            _cameraService = cameraService;
            _cameraService.ImageChanged += CameraServiceOnImageChanged;
            _eyes = new List<Rectangle>();
        }

        private async void CameraServiceOnImageChanged(object sender, Image<Bgr, byte> image)
        {
            Debug.WriteLine("Image changed" + DateTime.Now.ToString("HH:mm:ss.fff"));
            bool isDelayed = false;

            if (!isDetecting)
            {
                isDetecting = true;

                var result = await DetectFacesAsync(image);

                //TODO: fix delaying
                //isDelayed = true;
                _eyes = result;

                isDetecting = false;
            }

            if (!isDelayed) // to prevent displaing delayed image
            {
                Debug.WriteLine("Image changed not delayed" + DateTime.Now.ToString("HH:mm:ss.fff"));
                DrawRectangles(image);
                RaiseImageWithDetectionChangedEvent(image);
            }
            else
            {
                Debug.WriteLine("Image changed delayed" + DateTime.Now.ToString("HH:mm:ss.fff"));
            }
        }

        private void DrawRectangles(Image<Bgr, byte> image)
        {
            foreach (var eye in _eyes)
            {
                image.Draw(eye, new Bgr(Color.BurlyWood), 3);
                //the detected eye(s) is highlighted here using a box that is drawn around it/them
            }
        }

        private Task<List<Rectangle>> DetectFacesAsync(Image<Bgr, byte> image)
        {
            return Task.Run(() =>
            {
                List<Rectangle> faces = new List<Rectangle>();

                DetectFace(image, faces);

                return faces;
            });
        }

        private void DetectFace(Image<Bgr, byte> image, List<Rectangle> faces)
        {

            //Read the HaarCascade objects
            using (CascadeClassifier face = new CascadeClassifier(CascadeName))
            {
                using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                {
                    //normalizes brightness and increases contrast of the image
                    gray._EqualizeHist();

                    //Detect the faces  from the gray scale image and store the locations as rectangle
                    //The first dimensional is the channel
                    //The second dimension is the index of the rectangle in the specific channel
                    Rectangle[] facesDetected = face.DetectMultiScale(
                       gray,
                       1.1,
                       10,
                       new Size(20, 20),
                       Size.Empty);
                    faces.AddRange(facesDetected);
                }
            }

        }


        private void RaiseImageWithDetectionChangedEvent(Image<Bgr, byte> image)
        {
            ImageWithDetectionChanged?.Invoke(this, new ImageEventArgs(image, IsDetected));
        }
    }
}
