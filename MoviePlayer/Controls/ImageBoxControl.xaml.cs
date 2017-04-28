using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        public static readonly DependencyProperty ImageValueProperty =
            DependencyProperty.Register("Image", typeof(IImage),
              typeof(ImageBoxControl), new UIPropertyMetadata(null, ImagePropertyChanged));

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public IImage Image
        {
            get { return (IImage)GetValue(ImageValueProperty); }
            set { SetValue(ImageValueProperty, value); }
        }

        public static readonly DependencyProperty MillisecondsProperty =
            DependencyProperty.Register("Milliseconds", typeof(long),
                typeof(ImageBoxControl), new UIPropertyMetadata((long)0, MillisecondsPropertyChanged));

        public long Milliseconds
        {
            get { return (long)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        private static void MillisecondsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageBoxControl)d;
            control.MillisecondsLabel.Content = $"Eyes not detected: {e.NewValue:D3}mls";
        }


        public ImageBoxControl()
        {
            InitializeComponent();
        }

        private static void ImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageBoxControl)d;
            using (MemoryStream ms = new MemoryStream())
            {
                ((System.Drawing.Bitmap)((IImage)e.NewValue).Bitmap).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = new MemoryStream(ms.ToArray());
                image.EndInit();

                control.CameraImage.Source = image;
            }

            //  control.CameraFeedback.Image = (IImage)e.NewValue;
        }
    }
}
