using System;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using MoviePlayer.Models;
using MoviePlayer.Models.Enums;
using MoviePlayer.Services;
using MoviePlayer.Services.Interfaces;

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
        private const string CascadeName = "haarcascade_eye.xml";
        private const int TimeToShowWarning = 5000;

        private readonly ICameraService _cameraService;
        private readonly IDetectionService _detectionService;
        private readonly IFileDialogService _fileDialogService;
        private readonly Stopwatch _stopwatch;

        private bool _isPlaying;
        private bool _isPaused;
        private Image<Bgr, byte> _image;
        private double _windowWidth;
        private double _windowHeight;
        private long _milliseconds;
        private Uri _movieUri;
        private bool _isFeedbackActive;
        private bool _isInteractionAllowed;
        private double _volume;
        private string _moviePath;

        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { Set(ref _isPlaying, value); }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
            set { Set(ref _isPaused, value); }
        }

        public long Milliseconds
        {
            get { return _milliseconds; }
            set { Set(ref _milliseconds, value); }
        }

        public Image<Bgr, byte> Image
        {
            get { return _image; }
            set
            {
                _image = value;
                RaisePropertyChanged(() => Bitmap);
            }
        }

        public Bitmap Bitmap => Image.Resize((int)_windowWidth / 4, (int)_windowHeight / 4, Inter.Linear).Bitmap;

        public string MoviePath
        {
            get { return _moviePath; }
            set
            {
                _moviePath = value;
                RaisePropertyChanged(() => CanPlayMovie);
                RaisePropertyChanged(() => Title);
            }
        }

        public bool CanPlayMovie => MoviePath != null;

        public string Title => $"{MoviePath?.Substring(MoviePath.LastIndexOf('\\') + 1)}" +
                               $"{(MoviePath == null ? "" : " - ")}Movie Player";

        public Uri MovieUri
        {
            get { return _movieUri; }
            set { Set(ref _movieUri, value); }
        }

        public double Volume
        {
            get { return _volume; }
            set
            {
                Set(ref _volume, value);
                RaisePropertyChanged(() => VolumeString);
                RaisePropertyChanged(() => VolumeLevel);
            }
        }

        public string VolumeString => $"{Volume * 100:0}%";

        public VolumeLevel VolumeLevel
        {
            get
            {
                if (Volume > 0.7) return VolumeLevel.High;
                if (Volume > 0.3) return VolumeLevel.Medium;
                return Math.Abs(Volume) < 0.001 ? VolumeLevel.None : VolumeLevel.Low;
            }
        }

        public bool IsFeedbackActive
        {
            get { return _isFeedbackActive; }
            set { Set(ref _isFeedbackActive, value); }
        }

        public bool IsInteractionAllowed
        {
            get { return _isInteractionAllowed; }
            set { Set(ref _isInteractionAllowed, value); }
        }

        public RelayCommand BrowseFileCommand { get; private set; }
        public RelayCommand PlayPauseCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public RelayCommand ChangeFeedbackStateCommand { get; set; }
        public RelayCommand ChangeInteractionStateCommand { get; set; }

        public MainViewModel(ICameraService cameraService, IDetectionService detectionService,
            IFileDialogService fileDialogService)
        {
            _cameraService = cameraService;
            _detectionService = detectionService;
            _fileDialogService = fileDialogService;
            _stopwatch = new Stopwatch();

            IsFeedbackActive = true;
            Volume = 1;
            Image = new Image<Bgr, byte>(new Size(0,0));
        
            BrowseFileCommand = new RelayCommand(BrowseMovie);
            PlayPauseCommand = new RelayCommand(PlayPauseMovie, () => CanPlayMovie);
            StopCommand = new RelayCommand(StopMovie, () => IsPlaying);
            ChangeFeedbackStateCommand = new RelayCommand(() => IsFeedbackActive = !IsFeedbackActive);
            ChangeInteractionStateCommand = new RelayCommand(() => IsInteractionAllowed = !IsInteractionAllowed);

            Messenger.Default.Register<ProccessedImage>(this, ReceiveProcessedImage);

            _detectionService.SetFilter(CascadeName);
            _cameraService.Run();
        }

        public override void Cleanup()
        {
            base.Cleanup();
            _cameraService.Cancel();
        }

        public void SetWindowsSize(double width, double height)
        {
            _windowWidth = width;
            _windowHeight = height;
            RaisePropertyChanged(() => Bitmap);
        }

        private void ReceiveProcessedImage(ProccessedImage image)
        {
            Image = image.Image;

            if (!image.IsEyeDetected)
            {
                if (_stopwatch.IsRunning && //stopwatch is running
                    _stopwatch.ElapsedMilliseconds >= TimeToShowWarning && //checks the inverval
                    IsPlaying &&
                    IsInteractionAllowed)
                {
                    PauseMovie();
                    PlayPauseCommand.RaiseCanExecuteChanged();
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

                if (IsPlaying && IsInteractionAllowed)
                {
                    PlayMovie();
                    PlayPauseCommand.RaiseCanExecuteChanged();
                }
            }

            Milliseconds = _stopwatch.ElapsedMilliseconds;
        }

        private void BrowseMovie()
        {
            string path = _fileDialogService.BrowseMovie();

            if (path == null)
            {
                return;
            }

            MoviePath = path;
            PlayMovie();
        }

        private void PlayPauseMovie()
        {
            if (!IsPlaying)
            {
                PlayMovie();
                return;
            }

            if (IsPaused)
            {
                PlayMovie();
            }
            else
            {
                PauseMovie();
            }
        }

        private void PlayMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Play);
            MovieUri = new Uri(MoviePath);
            IsPaused = false;
            IsPlaying = true;
        }

        private void PauseMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Pause);
            IsPaused = true;
        }

        private void StopMovie()
        {
            Messenger.Default.Send(MediaElementCommand.Stop);
            MovieUri = null;
            IsPaused = false;
            IsPlaying = false;
        }
    }
}