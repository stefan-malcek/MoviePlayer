using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows.Controls.Primitives;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using MoviePlayer.Annotations;
using Color = System.Drawing.Color;
using Path = System.IO.Path;
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
        private ImageBox _imageBox;
        private bool _startStopwatch;
        private bool _isStopwatchStarted;
        private string _warningText;
        private long _milliseconds;

        public string WarningText
        {
            get { return _warningText; }
            set
            {
                if (Equals(value, _warningText)) return;
                _warningText = value;
                OnPropertyChanged(nameof(WarningText));
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

            this.DataContext = this;

            _capture = new Capture();
            //string path = Path.Combine(Environment.CurrentDirectory, @"haarcascade_frontalface_alt.xml");
            _cascadeClassifier = new CascadeClassifier("haarcascade_eye.xml");

            _cameraTimer = new Timer(250);
            _cameraTimer.Elapsed += new ElapsedEventHandler(CameraTick);

            _stopwatch = new Stopwatch();
        }

        private void DistractionTick(object sender, ElapsedEventArgs elapsedEventArgs)
        {

        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            var host = new System.Windows.Forms.Integration.WindowsFormsHost();

            // Create the ImageBox control.
            _imageBox = new ImageBox();
            //TODO: disable scrolling

            // Assign the ImageBox control as the host control's child.
            host.Child = _imageBox;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.ImageBoxHolder.Children.Add(host);

            _cameraTimer.Start();
        }

        private void StartTimer(object sender, RoutedEventArgs e)
        {
            _cameraTimer.Start();
        }

        private void CameraTick(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ImageBoxHolder.Dispatcher.Invoke(new ProcessFrameCallback(ProcessFrame));

            if (_startStopwatch)
            {
                if (_stopwatch.IsRunning)
                {
                    if (_stopwatch.ElapsedMilliseconds >= 3000)
                    {
                        Debug.WriteLine("WARNING");
                        WarningText = "WARNING!";
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
                    WarningText = string.Empty;
                }
            }
        }

        private void ProcessFrame()
        {
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
                    _startStopwatch = eyes.Length < 2;
                    Milliseconds = _stopwatch.ElapsedMilliseconds;

                    //Seconds.Dispatcher.Invoke(() => Warning.Content = _stopwatch.ElapsedMilliseconds);
                }

                _imageBox.Image = imageFrame?.Resize((int)MainWindowElement.ActualWidth / 4, (int)MainWindowElement.ActualHeight / 4, Inter.Linear);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void sliderProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void sliderProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void sliderProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            throw new NotImplementedException();
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ImageBoxHolder.Dispatcher.Invoke();
        }
    }
}
