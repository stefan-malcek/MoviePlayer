using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using MoviePlayer.Models.Enums;
using MoviePlayer.ViewModel;

namespace MoviePlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DurationPattern = @"hh\:mm\:ss";
        private readonly MainViewModel _mainViewModel;
        private readonly DispatcherTimer _progressSliderTimer;
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

        private void timer_Tick(object sender, EventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan && !_userIsDraggingSlider)
            {
                ProgressSlider.Value = Player.Position.TotalSeconds;
            }
        }

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
