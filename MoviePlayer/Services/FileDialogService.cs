using Microsoft.Win32;
using MoviePlayer.Services.Interfaces;

namespace MoviePlayer.Services
{
    public class FileDialogService : IFileDialogService
    {
        private const string Filter = "All files (*.*)|*.*";

        public string BrowseMovie()
        {
            var openFileDialog = new OpenFileDialog { Filter = Filter };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }
    }
}
