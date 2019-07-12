using LibraryCompare.Core.Enums;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class VersionEntryViewModel : ObservableObject
    {
        private LibraryVersionViewModel _left;

        private CompareResult _result;

        private LibraryVersionViewModel _right;

        public LibraryVersionViewModel Left
        {
            get { return _left; }
            set
            {
                if (value == _left)
                    return;
                _left = value;
                RaisePropertyChanged(nameof(Left));
            }
        }

        public LibraryVersionViewModel Right
        {
            get { return _right; }
            set
            {
                if (value == _right)
                    return;
                _right = value;
                RaisePropertyChanged(nameof(Right));
            }
        }

        public CompareResult Result
        {
            get { return _result; }
            set
            {
                if (value == _result)
                    return;
                _result = value;
                RaisePropertyChanged(nameof(Result));
            }
        }
    }
}