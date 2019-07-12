using System.IO;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class LibrarySelectViewModel : ObservableObject
    {
        private string _filePath;

        public string FilePath
        {
            get { return _filePath; }
            set
            {
                if (value == _filePath)
                    return;
                _filePath = value;
                RaisePropertyChanged(nameof(FilePath));
                RaisePropertyChanged(nameof(FileType));
                RaisePropertyChanged(nameof(FileName));
            }
        }

        public string FileType
        {
            get
            {
                var ext = Path.GetExtension(_filePath);
                switch (ext)
                {
                    case ".al14":
                        return "Library";
                    case ".ap14":
                        return "Project";
                    default:
                        return "none";
                }
            }
        }

        public string FileName
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath) || File.Exists(_filePath) == false)
                    return "";
                return Path.GetFileNameWithoutExtension(_filePath);
            }
        }
    }
}