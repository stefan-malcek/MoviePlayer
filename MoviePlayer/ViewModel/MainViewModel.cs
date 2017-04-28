using System;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MoviePlayer.Models;
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
        private const int TimeToShowWarning = 3000;

        private readonly ICameraService _cameraService;
        private readonly IDetectionService _detectionService;
        private readonly IFileDialogService _fileDialogService;
        private readonly Stopwatch _stopwatch;
        private bool _isPlaying;
        private string _notification;
        private Image<Bgr, byte> _image;
        private double _windowWidth;
        private double _windowHeight;
        private long _milliseconds;
        private Uri _movieUri;
        private bool _isFeedbackActive;
        private bool _isAutoPlayAllowed;
        private double _volume;
        private string _moviePath;

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

        public long Milliseconds
        {
            get { return _milliseconds; }
            set
            {
                Set(ref _milliseconds, value);
                RaisePropertyChanged(() => Milliseconds);
            }
        }

        public Image<Bgr, byte> Image
        {
            get
            {
                //if (WindowHeight == 0 || WindowWidth == 0) return _image;
                return _image.Resize((int)WindowWidth / 4, (int)WindowHeight / 4, Inter.Linear);
            }
            set
            {
                Set(ref _image, value);
                RaisePropertyChanged(() => Image);
            }
        }

        public string MoviePath
        {
            get { return _moviePath; }
            set
            {
                _moviePath = value;
                RaisePropertyChanged(() => CanPlayMovie);
            }
        }

        public Uri MovieUri
        {
            get { return _movieUri; }
            set
            {
                Set(ref _movieUri, value);
                RaisePropertyChanged(() => MovieUri);
                // RaisePropertyChanged(() => CanPlayMovie);
            }
        }

        public double Volume
        {
            get { return _volume; }
            set
            {
                Set(ref _volume, value);
                RaisePropertyChanged(() => Volume);
            }
        }

        public bool CanPlayMovie => MoviePath != null;

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

        public bool IsFeedbackActive
        {
            get { return _isFeedbackActive; }
            set
            {
                Set(ref _isFeedbackActive, value);
                RaisePropertyChanged(() => IsFeedbackActive);
            }
        }

        public bool IsAutoPlayAllowed
        {
            get { return _isAutoPlayAllowed; }
            set
            {
                Set(ref _isAutoPlayAllowed, value);
                RaisePropertyChanged(() => IsAutoPlayAllowed);
            }
        }

        public RelayCommand BrowseFileCommand { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand PauseCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }

        public MainViewModel()
        {
            _cameraService = new CameraService();
            _detectionService = new DetectionService(_cameraService);
            _fileDialogService = new FileDialogService();
            _detectionService.ImageWithDetectionChanged += _faceDetectionService_ImageChanged;
            _stopwatch = new Stopwatch();
            _cameraService.Run();

            IsFeedbackActive = true;
            Volume = 1;

            BrowseFileCommand = new RelayCommand(BrowseMovie);
            PlayCommand = new RelayCommand(PlayMovie, () => CanPlayMovie);
            PauseCommand = new RelayCommand(PauseMovie, () => IsPlaying);
            StopCommand = new RelayCommand(StopMovie, () => IsPlaying);
        }

        public void BrowseMovie()
        {
            MoviePath = _fileDialogService.BrowseMovie();

            if (MoviePath == null)
            {
                return;
            }

            if (IsAutoPlayAllowed)
            {
                PlayMovie();
            }
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

        private void _faceDetectionService_ImageChanged(object sender, ImageEventArgs e)
        {
            this.Image = e.Image;
            Debug.WriteLine("Detection_ImageChanged");
            if (!e.IsDetecting)
            {
                if (_stopwatch.IsRunning)
                {

                    if (_stopwatch.ElapsedMilliseconds >= TimeToShowWarning)
                    {
                        //_mainViewModel.Notification = "Warning";
                        Debug.WriteLine("WARNING");
                        Notification = "WARNING!";
                        if (CanPlayMovie && IsAutoPlayAllowed)
                        {
                            PauseMovie();
                            PauseCommand.RaiseCanExecuteChanged();
                            StopCommand.RaiseCanExecuteChanged();
                        }

                        //PauseMovie();
                    }
                }
                else
                {
                    _stopwatch.Start();
                }
            }
            else
            {
                if (!_stopwatch.IsRunning) return;
                _stopwatch.Restart();
                //_mainViewModel.Notification = string.Empty;
                Notification = string.Empty;
                if (CanPlayMovie && IsAutoPlayAllowed)
                {
                    PlayMovie();
                    PauseCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                }

                // PlayMovie();
            }

            Milliseconds = _stopwatch.ElapsedMilliseconds;
        }


        private void PlayMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Play);
            MovieUri = new Uri(MoviePath);
            IsPlaying = true;
        }

        private void PauseMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Pause);
        }

        private void StopMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Stop);
            MovieUri = null;
            IsPlaying = false;
        }
    }
}