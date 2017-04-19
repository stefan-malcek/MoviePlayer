using GalaSoft.MvvmLight;

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
        private bool _isPlaying;
        private string _notification;

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

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}