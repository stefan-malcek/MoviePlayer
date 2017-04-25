using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

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
