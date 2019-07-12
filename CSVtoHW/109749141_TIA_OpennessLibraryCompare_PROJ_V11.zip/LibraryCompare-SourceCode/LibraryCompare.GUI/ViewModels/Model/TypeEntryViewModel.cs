using System.Collections.ObjectModel;
using System.Linq;
using LibraryCompare.Core.Enums;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class TypeEntryViewModel : ObservableObject
    {
        private LibraryTypeViewModel _left;

        private LibraryTypeViewModel _right;

        private CompareResult _result;

        private ObservableCollection<VersionEntryViewModel> _versions;

        private bool _isSelected;

        public TypeEntryViewModel(LibraryTypeViewModel left, LibraryTypeViewModel right)
        {
            _versions = new ObservableCollection<VersionEntryViewModel>();
            _left = left;
            _right = right;
            _result = (_left == null) ? CompareResult.None : (CompareResult)_left.CompareTo(_right);

            UpdateVersions();
        }

        public LibraryTypeViewModel Left
        {
            get { return _left; }
            set
            {
                if (value == _left)
                    return;
                _left = value;
                UpdateVersions();
                RaisePropertyChanged(nameof(Left));
            }
        }

        public LibraryTypeViewModel Right
        {
            get { return _right; }
            set
            {
                if (value == _right)
                    return;
                _right = value;
                UpdateVersions();
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

        public ObservableCollection<VersionEntryViewModel> Versions
        {
            get { return _versions; }
            set
            {
                if (value == _versions)
                    return;
                _versions = value;
                RaisePropertyChanged(nameof(Versions));
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;
                _isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
        }

        private void UpdateVersions()
        {
            _versions.Clear();
            if (_left == null && _right == null) return;

            if (_left != null)
                foreach (var leftVersion in _left.Versions)
                {
                    var rightVersion = _right?.Versions.SingleOrDefault(o => o.Version == leftVersion.Version);

                    var entry = new VersionEntryViewModel
                    {
                        Left = leftVersion,
                        Right = rightVersion,
                        Result = (CompareResult)leftVersion.CompareTo(rightVersion)
                    };
                    _versions.Add(entry);
                }

            if (_right == null) return;
            foreach (var rightVersion in _right.Versions)
            {
                var leftVersion = _left?.Versions.SingleOrDefault(o => o.Version == rightVersion.Version);
                if (leftVersion != null) continue;
                var entry = new VersionEntryViewModel
                {
                    Left = null,
                    Right = rightVersion,
                    Result = CompareResult.None
                };
                _versions.Add(entry);
            }
        }
    }
}