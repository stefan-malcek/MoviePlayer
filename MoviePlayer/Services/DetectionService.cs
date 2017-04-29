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
        private readonly CascadeClassifier _classifier;
        private readonly ICameraService _cameraService;
        private bool isDetecting = false;
        private List<Rectangle> _eyes;

        public event ImageWithDetectionChangedEventHandler ImageWithDetectionChanged;
        public bool IsDetected => _eyes.Any();

        public DetectionService(ICameraService cameraService)
        {
            _cameraService = cameraService;
            _cameraService.ImageChanged += CameraServiceOnImageChanged;
            _classifier = new CascadeClassifier(CascadeName);
            _eyes = new List<Rectangle>();
        }

        ~DetectionService()
        {
            _classifier?.Dispose();
        }

        private async void CameraServiceOnImageChanged(object sender, Image<Bgr, byte> image)
        {
            var isDelayed = false;
            Debug.WriteLine($"1. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");

            if (!isDetecting)
            {
                isDetecting = true;
                Debug.WriteLine($"3. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
                var result = await DetectFacesAsync(image);
                Debug.WriteLine($"4. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
                //TODO: fix delaying
               // isDelayed = true;
                _eyes = result;
                Debug.WriteLine($"5. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
                isDetecting = false;
                Debug.WriteLine($"6. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
            }
            Debug.WriteLine($"7. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
            if (!isDelayed) // to prevent displaing delayed image
            {
                Debug.WriteLine($"8. isDelayed: {isDelayed}, isDetecting: {isDetecting}, time: {DateTime.Now.ToString("HH:mm:ss.fff")}");
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
        }


        private void RaiseImageWithDetectionChangedEvent(Image<Bgr, byte> image)
        {
            ImageWithDetectionChanged?.Invoke(this, new ImageEventArgs(image, IsDetected));
        }
    }
}
