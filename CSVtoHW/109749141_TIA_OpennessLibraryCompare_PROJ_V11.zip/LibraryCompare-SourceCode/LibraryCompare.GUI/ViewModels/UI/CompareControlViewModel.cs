using System.Windows.Input;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class CompareControlViewModel : ObservableObject
    {
        private readonly ShellViewModel _shellViewModel;

        private bool _filterConflict = true;

        private bool _filterEqual = true;

        private bool _filterNone = true;

        private bool _filterUnEqual = true;

        public CompareControlViewModel(ShellViewModel shellViewModel)
        {
            _shellViewModel = shellViewModel;
        }

        public bool FilterNone
        {
            get { return _filterNone; }
            set
            {
                if (value == _filterNone)
                    return;
                _filterNone = value;
                RaisePropertyChanged(nameof(FilterNone));
            }
        }

        public bool FilterEqual
        {
            get { return _filterEqual; }
            set
            {
                if (value == _filterEqual)
                    return;
                _filterEqual = value;
                RaisePropertyChanged(nameof(FilterEqual));
            }
        }

        public bool FilterConflict
        {
            get { return _filterConflict; }
            set
            {
                if (value == _filterConflict)
                    return;
                _filterConflict = value;
                RaisePropertyChanged(nameof(FilterConflict));
            }
        }

        public bool FilterUnEqual
        {
            get { return _filterUnEqual; }
            set
            {
                if (value == _filterUnEqual)
                    return;
                _filterUnEqual = value;
                RaisePropertyChanged(nameof(FilterUnEqual));
            }
        }

        public ICommand CompareCommand => _shellViewModel.CompareCommand;
    }
}