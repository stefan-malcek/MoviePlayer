using System.Drawing;
using System.Windows;
using System.Windows.Controls;
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
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Image", typeof(IImage),
              typeof(ImageBoxControl), new UIPropertyMetadata(null, ImagePropertyChanged));

        /// <summary>
        /// Gets or sets the Value which is being displayed
        /// </summary>
        public IImage Image
        {
            get { return (IImage)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        public ImageBoxControl()
        {
            InitializeComponent();
        }

        private static void ImagePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ImageBoxControl)d;
            control.CameraFeedback.Image = (IImage)e.NewValue;
        }
    }
}
