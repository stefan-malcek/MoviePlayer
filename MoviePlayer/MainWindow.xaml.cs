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
    // public delegate void 


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DurationPattern = @"hh\:mm\:ss";
        //private readonly Timer _cameraTimer;
        //private readonly Stopwatch _stopwatch;
        //private readonly Capture _capture;
        //private readonly CascadeClassifier _cascadeClassifier;
        //private readonly DispatcherTimer _dispatcherTimer;
        private readonly MainViewModel _mainViewModel;
        private readonly DispatcherTimer _progressSliderTimer;


        //private ImageBox _imageBox;
        //private bool _startStopwatch;
        ////private bool _isStopwatchStarted;
        //private string _notification;
        //private long _milliseconds;
        //private bool _isPlaying;
        private bool _userIsDraggingSlider;
        private double _movieLength;

        public MainWindow()
        {
            InitializeComponent();

            _mainViewModel = (MainViewModel)DataContext;
            Messenger.Default.Register<MediaElementCommand>(this, ReceiveMediaElementCommand);


            _progressSliderTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _progressSliderTimer.Tick += timer_Tick;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan && !_userIsDraggingSlider)
            {
                ProgressSlider.Value = Player.Position.TotalSeconds;
            }
        }

        private void ReceiveMediaElementCommand(MediaElementCommand command)
        {
            switch (command)
            {
                case MediaElementCommand.Play:
                    PlayMovie();
                    break;
                case MediaElementCommand.Pause:
                    PauseMovie();
                    break;
                case MediaElementCommand.Stop:
                    StopMovie();
                    break;
                default:
                    Debug.WriteLine("Default");
                    break; // throw new InvalidOperationException("Invalid operation");
            }
        }


        //private void ProcessFrame()
        //{
        //    Debug.WriteLine("Procces Frame");
        //    using (var imageFrame = _capture.QueryFrame().ToImage<Bgr, byte>())
        //    {
        //        if (imageFrame != null)
        //        {
        //            var grayframe = imageFrame.Convert<Gray, byte>();
        //            var eyes = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty);
        //            //the actual eye detection happens here
        //            foreach (var eye in eyes)
        //            {
        //                imageFrame.Draw(eye, new Bgr(Color.BurlyWood), 3);
        //                //the detected eye(s) is highlighted here using a box that is drawn around it/them
        //            }

        //            Debug.WriteLine("Elapsed time: {0}", _stopwatch.ElapsedMilliseconds);
        //            _startStopwatch = eyes.Length < 1;

        //            //Seconds.Dispatcher.Invoke(() => Warning.Content = _stopwatch.ElapsedMilliseconds);
        //        }

        //        // ImageBoxControl.Image = imageFrame?.Resize((int)MainWindowElement.ActualWidth / 4, (int)MainWindowElement.ActualHeight / 4, Inter.Linear);
        //    }


        //}

        private void sliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            _userIsDraggingSlider = true;
        }

        private void sliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _userIsDraggingSlider = false;
            Player.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var sliderValue = ProgressSlider.Value;
            LabelProgressStatus.Text = TimeSpan.FromSeconds(sliderValue).ToString(DurationPattern);
            LabelProgressLeftStatus.Text = $"-{TimeSpan.FromSeconds(_movieLength - sliderValue).ToString(DurationPattern)}";
        }

        private void PlayMovie()
        {
            Dispatcher.Invoke(() =>
            {
                Player.Play();
            });
            //setup Slider
            
        }

        private void PauseMovie()
        {
            _progressSliderTimer.Stop();
            Dispatcher.Invoke(() =>
            {
                Player.Pause();
            });
        }

        private void StopMovie()
        {
            _progressSliderTimer.Stop();
            ProgressSlider.Value = 0;
            LabelProgressLeftStatus.Text = "00:00:00";
            Dispatcher.Invoke(() =>
            {
                Player.Stop();
            });
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _mainViewModel.SetWindowsSize(MainWindowElement.ActualWidth, MainWindowElement.ActualHeight);
        }

        private void Player_OnMediaEnded(object sender, RoutedEventArgs e)
        {
            StopMovie();
            _mainViewModel.IsPlaying = false;
        }

        private void Player_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Minimum = 0;
                ProgressSlider.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;
                _movieLength = Player.NaturalDuration.TimeSpan.TotalSeconds;
            }

            _progressSliderTimer.Start();
        }
    }
}
