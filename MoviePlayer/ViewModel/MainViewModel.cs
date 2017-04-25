using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using MoviePlayer.Services;

namespace MoviePlayer.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly ICameraService _cameraService;
        private readonly IDetectionService _detectionService;
        private bool _isPlaying;
        private string _notification;
        private Image<Bgr, byte> _image;
        private double _windowWidth;
        private double _windowHeight;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set
            {
                if (Equals(value, _isPlaying)) return;
                Set(ref _isPlaying, value);
                RaisePropertyChanged(() => IsPlaying);
            }
        }

        public string Notification
        {
            get { return _notification; }
            set
            {
                if (Equals(value, _notification)) return;
                Set(ref _notification, value);
                RaisePropertyChanged(() => Notification);
            }
        }

        public Image<Bgr, byte> Image
        {
            get { return _image.Resize((int)WindowWidth / 4, (int)WindowHeight / 4, Inter.Linear); }
            set
            {
                Set(ref _image, value);
                RaisePropertyChanged(() => Image);
            }
        }

        public double WindowWidth
        {
            get { return _windowWidth; }
            set
            {
                _windowWidth = value;
                RaisePropertyChanged(() => Image);
            }
        }

        public double WindowHeight
        {
            get { return _windowHeight; }
            set
            {
                _windowHeight = value;
                RaisePropertyChanged(() => Image);
            }
        }

        public MainViewModel()
        {
            _cameraService = new CameraService();
            _detectionService = new DetectionService(_cameraService);
            _detectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;

            _cameraService.Run();
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        //public MainViewModel(ICameraService cameraService, IDetectionService processService)
        //{
        //    _cameraService = cameraService;
        //    _detectionService = processService;
        //    _detectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;

        //    _cameraService.Run();
        //}

        private void _faceDetectionService_ImageChanged(object sender, Image<Bgr, byte> image)
        {
            this.Image = image;
        }
    }
}