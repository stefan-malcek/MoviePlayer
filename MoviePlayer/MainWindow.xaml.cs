using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Emgu.CV;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using MoviePlayer.Annotations;
using MoviePlayer.Models;
using MoviePlayer.ViewModel;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;
using Timer = System.Timers.Timer;

namespace MoviePlayer
{
    public delegate void ProcessFrameCallback();
    // public delegate void 


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly Timer _cameraTimer;
        private readonly Stopwatch _stopwatch;
        private readonly Capture _capture;
        private readonly CascadeClassifier _cascadeClassifier;
        private readonly DispatcherTimer _dispatcherTimer;
        private MainViewModel _mainViewModel;

        private ImageBox _imageBox;
        private bool _startStopwatch;
        //private bool _isStopwatchStarted;
        private string _notification;
        private long _milliseconds;
        private bool _isPlaying;
        private bool _userIsDraggingSlider;

        private bool fullscreen = false;
        private DispatcherTimer DoubleClickTimer = new DispatcherTimer();

        public string Notification
        {
            get { return _notification; }
            set
            {
                if (Equals(value, _notification)) return;
                _notification = value;
                OnPropertyChanged(nameof(Notification));
            }
        }

        public long Milliseconds
        {
            get { return _milliseconds; }
            set
            {
                if (Equals(value, _milliseconds)) return;
                _milliseconds = value;
                OnPropertyChanged(nameof(Milliseconds));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = (MainViewModel)DataContext;
            //this.DataContext = this;

            _capture = new Capture();
            //string path = Path.Combine(Environment.CurrentDirectory, @"haarcascade_frontalface_alt.xml");
            _cascadeClassifier = new CascadeClassifier("haarcascade_eye.xml");

            //_cameraTimer = new Timer(250);
            //_cameraTimer.Elapsed += new ElapsedEventHandler(CameraTick);

            _stopwatch = new Stopwatch();
            //_dispatcherTimer = new DispatcherTimer
            //       (
            //       TimeSpan.FromMinutes(0),
            //       DispatcherPriority.SystemIdle,// Or DispatcherPriority.SystemIdle
            //       (s, e) => ProcessFrame(),
            //       Application.Current.Dispatcher
            //       );
            Messenger.Default.Register<MediaElementCommand>(this, (action) => ReceivePlayVideoMessage(action));

            // _dispatcherTimer.IsEnabled = true;
            // _dispatcherTimer.Tick += DispatcherTimerOnTick;


            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();

            DoubleClickTimer.Interval = TimeSpan.FromMilliseconds(GetDoubleClickTime());
            DoubleClickTimer.Tick += (s, e) => DoubleClickTimer.Stop();

        }

        private void MediaPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!DoubleClickTimer.IsEnabled)
            {
                DoubleClickTimer.Start();
            }
            else
            {
                if (!fullscreen)
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }

                fullscreen = !fullscreen;
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetDoubleClickTime();

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((Player.Source != null) && (Player.NaturalDuration.HasTimeSpan) && (!_userIsDraggingSlider))
            {
                SliderProgress.Minimum = 0;
                SliderProgress.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                SliderProgress.Value = Player.Position.TotalSeconds;
            }
        }

        private void ResetProgressSlider()
        {
            SliderProgress.Value = 0;
        }

        private object ReceivePlayVideoMessage(MediaElementCommand command)
        {
            switch (command)
            {
                case MediaElementCommand.Play:
                    this.Dispatcher.Invoke(() =>
                    {
                        Player.Play();
                    });
                    break;
                case MediaElementCommand.Pause:
                    this.Dispatcher.Invoke(() =>
                    {
                        Player.Pause();
                    });

                    break;
                case MediaElementCommand.Stop:
                    this.Dispatcher.Invoke(() =>
                    {
                        Player.Stop();
                        ResetProgressSlider();
                    });
                    break;
                default:
                    Debug.WriteLine("Default");
                    break; // throw new InvalidOperationException("Invalid operation");
            }

            //if (playVideo != null)
            //{
            //    //Player.Source = new Uri(playVideo, UriKind.Relative);
            //    //this.Visibility = Visibility.Visible;
            //    Player.Play();
            //}
            return null;
        }

        private void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {

        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            // var host = new System.Windows.Forms.Integration.WindowsFormsHost();

            // // Create the ImageBox control.
            // _imageBox = new ImageBox();
            // //TODO: disable scrolling

            // // Assign the ImageBox control as the host control's child.
            // host.Child = _imageBox;

            // // Add the interop host control to the Grid
            // // control's collection of child controls.
            // this.ImageBoxHolder.Children.Add(host);
            //_dispatcherTimer.Start();


            // _cameraTimer.Start();
        }

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            _cameraTimer.Start();
        }

        private void CameraTick(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // ImageBox.Dispatcher.Invoke(new ProcessFrameCallback(ProcessFrame));

            if (_startStopwatch)
            {
                if (_stopwatch.IsRunning)
                {
                    if (_stopwatch.ElapsedMilliseconds >= 3000)
                    {
                        //_mainViewModel.Notification = "Warning";
                        Debug.WriteLine("WARNING");
                        Notification = "WARNING!";
                        PauseMovie();
                    }
                }
                else
                {
                    _stopwatch.Restart();
                }
            }
            else
            {
                if (_stopwatch.IsRunning)
                {
                    _stopwatch.Stop();
                    //_mainViewModel.Notification = string.Empty;
                    Notification = string.Empty;
                    PlayMovie();
                }
            }
        }

        private void ProcessFrame()
        {
            Debug.WriteLine("Procces Frame");
            using (var imageFrame = _capture.QueryFrame().ToImage<Bgr, byte>())
            {
                if (imageFrame != null)
                {
                    var grayframe = imageFrame.Convert<Gray, byte>();
                    var eyes = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty);
                    //the actual eye detection happens here
                    foreach (var eye in eyes)
                    {
                        imageFrame.Draw(eye, new Bgr(Color.BurlyWood), 3);
                        //the detected eye(s) is highlighted here using a box that is drawn around it/them
                    }

                    Debug.WriteLine("Elapsed time: {0}", _stopwatch.ElapsedMilliseconds);
                    _startStopwatch = eyes.Length < 1;
                    Milliseconds = _stopwatch.ElapsedMilliseconds;

                    //Seconds.Dispatcher.Invoke(() => Warning.Content = _stopwatch.ElapsedMilliseconds);
                }

                // ImageBoxControl.Image = imageFrame?.Resize((int)MainWindowElement.ActualWidth / 4, (int)MainWindowElement.ActualHeight / 4, Inter.Linear);
            }


        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {

        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {

        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void sliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void sliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            Player.Position = TimeSpan.FromSeconds(SliderProgress.Value);
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelProgressStatus.Text = TimeSpan.FromSeconds(SliderProgress.Value).ToString(@"hh\:mm\:ss");
        }

        private void Open_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { Filter = "All files (*.*)|*.*" };

            if (openFileDialog.ShowDialog() == true)
            {
                Player.Source = new Uri(openFileDialog.FileName);
                Debug.WriteLine(openFileDialog.FileName);

                // PlayMovie();
            }
        }

        private void Play_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Player?.Source != null;
        }

        private void Play_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            PlayMovie();
        }

        private void PlayMovie()
        {
            this.Dispatcher.Invoke(() =>
            {
                Player.Play();
                _isPlaying = true;
            });


        }

        private void PauseMovie()
        {
            Debug.WriteLine("Pause");

            this.Dispatcher.Invoke(() =>
            {
                Player.Pause();
            });

        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _mainViewModel.SetWindowsSize(MainWindowElement.ActualWidth, MainWindowElement.ActualHeight);
        }

        private void Pause_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = _isPlaying;
        }

        private void Pause_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            PauseMovie();
        }

        private void Stop_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = _isPlaying;
        }

        private void Stop_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                Player.Stop();
            });
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _capture.Dispose();
            //_cameraTimer.Dispose();
            _cascadeClassifier.Dispose();
        }


    }
}
