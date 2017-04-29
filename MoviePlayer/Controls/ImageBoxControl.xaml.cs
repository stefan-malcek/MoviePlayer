using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;

namespace MoviePlayer.Controls
{
    /// <summary>
    /// Interaction logic for ImageBoxControl.xaml
    /// </summary>
    public partial class ImageBoxControl : UserControl
    {
        /// <summary>
        /// Identified the Label dependency property
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource),
              typeof(ImageBoxControl), new UIPropertyMetadata(null, ImagePropertyChanged));

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register("Milliseconds", typeof(long),
                typeof(ImageBoxControl), new UIPropertyMetadata((long)0, MillisecondsPropertyChanged));

        public long Milliseconds
        {
            get { return (long)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        public ImageBoxControl()
        {
            InitializeComponent();
        }

        private static void ImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageBoxControl)d;
            control.CameraImage.Source = (ImageSource)e.NewValue;
        }

        private static void MillisecondsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageBoxControl)d;
            control.MillisecondsLabel.Content = $"Eyes not detected: {e.NewValue:D3}ms";
        }
    }
}
