using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class CopyResultViewModel : ObservableObject
    {
        private readonly CompareControlViewModel _compareControlViewModel;

        private ListCollectionView _cvsEntries;

        private CopyEntryViewModel _selectedItem;

        private string _nameFilter;

        public CopyResultViewModel(CompareControlViewModel compareControlViewModel)
        {
            _compareControlViewModel = compareControlViewModel;
            _compareControlViewModel.PropertyChanged += CompareControlViewModelPropertyChanged;

            ListEntries = new ObservableCollection<CopyEntryViewModel>();
            _cvsEntries = new ListCollectionView(ListEntries);
            _cvsEntries.Filter += CvsEntriesFilter;
        }

        public ObservableCollection<CopyEntryViewModel> ListEntries { get; set; }

        public ICollectionView AllEntries => _cvsEntries;

        public CopyEntryViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;
                _selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
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

        private void CompareControlViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Filter"))
                OnFilterChanged();
        }

        private bool CvsEntriesFilter(object sender)
        {
            var vm = (CopyEntryViewModel)sender;

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

            return ret;
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

        public void DisplayResults(IList<Tuple<ILibraryCopyModel, ILibraryCopyModel>> result)
        {
            ListEntries.Clear();
            
            if (result != null)
            {
                foreach (var tuple in result)
                {
                    LibraryCopyViewModel left = null;
                    LibraryCopyViewModel right = null;
                    if (tuple.Item1 != null)
                    {
                        left = new LibraryCopyViewModel(tuple.Item1);
                    }
                    if (tuple.Item2 != null)
                    {
                        right = new LibraryCopyViewModel(tuple.Item2);
                    }

                    ListEntries.Add(new CopyEntryViewModel(left, right));
                }
            }

            OnFilterChanged();
        }

        public void ClearResults()
        {
            ListEntries.Clear();
            OnFilterChanged();
        }
    }
}