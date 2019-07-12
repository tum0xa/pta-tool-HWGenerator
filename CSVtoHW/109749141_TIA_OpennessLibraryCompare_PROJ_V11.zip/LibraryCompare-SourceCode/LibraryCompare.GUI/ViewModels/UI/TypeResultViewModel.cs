using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using LibraryCompare.GUI.Utilities;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class TypeResultViewModel : ObservableObject
    {
        private readonly CompareControlViewModel _compareControlViewModel;

        private ListCollectionView _cvsEntries;

        private DetailVisibility _showDetails;

        private TypeEntryViewModel _selectedItem;

        private LibraryItemType _selectedTypeFlter;

        private string _nameFilter;

        private ResultFilter _selectedResultFilter;

        private List<string> _languages;

        private bool _showOutDated;

        public TypeResultViewModel(CompareControlViewModel compareControlViewModel)
        {
            _compareControlViewModel = compareControlViewModel;
            _compareControlViewModel.PropertyChanged += CompareControlViewModelPropertyChanged;

            ListEntries = new ObservableCollection<TypeEntryViewModel>();
            _cvsEntries = new ListCollectionView(ListEntries);
            _cvsEntries.Filter += CvsEntriesFilter;
        }

        public ObservableCollection<TypeEntryViewModel> ListEntries { get; set; }

        public ICollectionView AllEntries => _cvsEntries;

        public DetailVisibility ShowDetails
        {
            get { return _showDetails; }
            set
            {
                if (value == _showDetails)
                    return;
                _showDetails = value;
                RaisePropertyChanged(nameof(ShowDetails));
            }
        }

        public TypeEntryViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
                RaisePropertyChanged(nameof(SelectedDepents));
                RaisePropertyChanged(nameof(SelectedDepencies));
            }
        }

        public LibraryItemType SelectedTypeFilter
        {
            get { return _selectedTypeFlter; }
            set
            {
                if (value == _selectedTypeFlter)
                    return;
                _selectedTypeFlter = value;
                RaisePropertyChanged(nameof(SelectedTypeFilter));
                OnFilterChanged();
            }
        }

        public string NameFilter
        {
            get { return _nameFilter; }
            set
            {
                if (value == _nameFilter)
                    return;
                _nameFilter = value;
                RaisePropertyChanged(nameof(NameFilter));
                OnFilterChanged();
            }
        }

        public ResultFilter SelectedResultFilter
        {
            get { return _selectedResultFilter; }
            set
            {
                if (value == _selectedResultFilter)
                    return;
                _selectedResultFilter = value;
                RaisePropertyChanged(nameof(SelectedResultFilter));
                OnFilterChanged();
            }
        }

        public List<string> Languages
        {
            get { return _languages; }
            set
            {
                _languages = value;
                RaisePropertyChanged(nameof(Languages));
            }
        }

        public bool ShowOutDated
        {
            get { return _showOutDated; }
            set
            {
                _showOutDated = value;
                RaisePropertyChanged(nameof(ShowOutDated));
            }
        }

        public ObservableCollection<DependentEntryViewModel> SelectedDepents
        {
            get
            {
                var left = SelectedItem?.Left?.Versions?.LastOrDefault();
                var right = SelectedItem?.Right?.Versions?.LastOrDefault();

                if (left == null && right == null) return null;

                var col = new ObservableCollection<DependentEntryViewModel>
                {
                    new DependentEntryViewModel(left, right)
                };
                return col;
            }
        }

        public ObservableCollection<DependencyEntryViewModel> SelectedDepencies
        {
            get
            {
                var left = SelectedItem?.Left?.Versions?.LastOrDefault();
                var right = SelectedItem?.Right?.Versions?.LastOrDefault();

                if (left == null && right == null) return null;

                var col = new ObservableCollection<DependencyEntryViewModel>
                {
                    new DependencyEntryViewModel(left, right)
                };
                return col;
            }
        }

        public List<TypeEntryViewModel> MarkedItems
        {
            get { return ListEntries.Where(o => o.IsSelected).ToList(); }
        }

        private void CompareControlViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Filter"))
                OnFilterChanged();
        }

        private bool CvsEntriesFilter(object sender)
        {
            var vm = (TypeEntryViewModel)sender;

            if (string.IsNullOrEmpty(NameFilter) == false
                && (vm.Left == null || vm.Left.Name.ToLower().Contains(NameFilter.ToLower()) == false)
                && (vm.Right == null || vm.Right.Name.ToLower().Contains(NameFilter.ToLower()) == false))
            {
                return false;
            }

            bool ret;

            switch (vm.Result)
            {
                case CompareResult.None:
                    ret = _compareControlViewModel.FilterNone;
                    break;
                case CompareResult.Equal:
                    ret = _compareControlViewModel.FilterEqual;
                    break;
                case CompareResult.Left:
                    ret = _compareControlViewModel.FilterUnEqual;
                    break;
                case CompareResult.Right:
                    ret = _compareControlViewModel.FilterUnEqual;
                    break;
                case CompareResult.Diff:
                    ret = _compareControlViewModel.FilterConflict;
                    break;
                default:
                    ret = true;
                    break;
            }

            if (ret == false) return false;

            if (SelectedTypeFilter != LibraryItemType.Unknown)
            {
                if (vm.Left?.Type != SelectedTypeFilter && vm.Right?.Type != SelectedTypeFilter)
                {
                    return false;
                }
            }

            switch (SelectedResultFilter)
            {
                case ResultFilter.PendingChanges:
                    if ((vm.Left == null || vm.Left?.Versions.Count(o => o.State == LibraryVersionState.InWork) == 0)
                        && (vm.Right == null || vm.Right?.Versions.Count(o => o.State == LibraryVersionState.InWork) == 0))
                        return false;
                    break;
                case ResultFilter.MultipleVersions:
                    if ((vm.Left == null || vm.Left?.Versions.Count < 2)
                        && (vm.Right == null || vm.Right?.Versions.Count < 2))
                        return false;
                    break;
            }

            return true;
        }

        private void OnFilterChanged()
        {
            try
            {
                _cvsEntries.Refresh();
            }
            catch (ArgumentOutOfRangeException)
            {
                // prevent sporadic ArgumentOutOfRangeException
                _cvsEntries.Refresh();
            }
        }

        public void DisplayResults(IList<Tuple<ILibraryTypeModel, ILibraryTypeModel>> result)
        {
            ListEntries.Clear();

            var set = new HashSet<string>();

            if (result != null)
            {
                foreach (var tuple in result)
                {
                    LibraryTypeViewModel left = null;
                    LibraryTypeViewModel right = null;
                    if (tuple.Item1 != null)
                    {
                        left = new LibraryTypeViewModel(tuple.Item1);
                        foreach (var pair in left.Comment)
                        {
                            set.Add(pair.Key);
                        }
                    }
                    if (tuple.Item2 != null)
                    {
                        right = new LibraryTypeViewModel(tuple.Item2);
                        foreach (var pair in right.Comment)
                        {
                            set.Add(pair.Key);
                        }
                    }

                    ListEntries.Add(new TypeEntryViewModel(left, right));
                }
            }

            Languages = set.OrderBy(o => o).ToList();

            OnFilterChanged();
        }

        public void ClearResults()
        {
            ListEntries.Clear();
            OnFilterChanged();
        }
    }
}