using LibraryCompare.Core.Enums;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class CopyEntryViewModel : ObservableObject
    {
        private LibraryCopyViewModel _left;

        private LibraryCopyViewModel _right;

        private CompareResult _result;
        
        public CopyEntryViewModel(LibraryCopyViewModel left, LibraryCopyViewModel right)
        {
            _left = left;
            _right = right;
            _result = (_left == null) ? CompareResult.None : (CompareResult)_left.CompareTo(_right);
        }

        public LibraryCopyViewModel Left
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

        public LibraryCopyViewModel Right
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
