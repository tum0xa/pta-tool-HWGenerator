using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using LibraryCompare.Core.Interfaces;
using MvvmFoundation.Wpf;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using LibraryCompare.Core;
using Microsoft.Win32;

namespace LibraryCompare.GUI.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly IOpenness _openness;

        private bool _isRunning;
        private string _workState;


        public ShellViewModel(IOpenness openness)
        {
            _openness = openness;

            CompareInfo = new CompareControlViewModel(this);
            TypeResults = new TypeResultViewModel(CompareInfo);
            CopyResults = new CopyResultViewModel(CompareInfo);
            LibraryLeft = new LibrarySelectViewModel();
            LibraryRight = new LibrarySelectViewModel();

            LibraryLeft.PropertyChanged += OnPathUpdated;
            LibraryRight.PropertyChanged += OnPathUpdated;
            TypeResults.ListEntries.CollectionChanged += OnEntriesChanged;
#if DEBUG
            LibraryLeft.FilePath = @"D:\Temp\ProjCompare\ProjCompare.ap14";
            LibraryRight.FilePath = @"D:\Temp\LibCompare\LibCompare.al14";
#endif
        }

        public TypeResultViewModel TypeResults { get; }

        public CopyResultViewModel CopyResults { get; }

        public LibrarySelectViewModel LibraryLeft { get; }

        public LibrarySelectViewModel LibraryRight { get; }

        public CompareControlViewModel CompareInfo { get; }

        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (value == _isRunning)
                    return;
                _isRunning = value;
                RaisePropertyChanged(nameof(IsRunning));
            }
        }

        public string WorkState
        {
            get { return _workState; }
            set
            {
                if (value == _workState)
                    return;
                _workState = value;
                RaisePropertyChanged(nameof(WorkState));
            }
        }


        public ICommand CompareCommand => new RelayCommand<object>(OnCompare, CanCompare);
        private void OnCompare(object param)
        {
            if (IsRunning) return;
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (sender, e) =>
            {
                IsRunning = true;
                e.Result = _openness.Compare(LibraryLeft.FilePath, LibraryRight.FilePath);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                _openness.OnProgressUpdate -= UpdateProgress;

                if (e.Error != null)
                {
                    if (e.Error is ArgumentException)
                    {
                        MessageBox.Show(e.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(e.Error.Message, "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var result = (CompareResults)e.Result;
                    TypeResults.DisplayResults(result.TypePairs);
                    CopyResults.DisplayResults(result.CopyPairs);

                }
                IsRunning = false;
            };

            _openness.OnProgressUpdate += UpdateProgress;

            bgWorker.RunWorkerAsync();
        }
        private bool CanCompare(object param)
        {
            return string.IsNullOrEmpty(LibraryLeft.FilePath) == false
                && string.IsNullOrEmpty(LibraryRight.FilePath) == false;
        }

        public ICommand DetailCompareCommand => new RelayCommand(OnDetailCompare, CanDetailCompare);
        private void OnDetailCompare()
        {
            if (IsRunning) return;

            var leftModels = TypeResults.MarkedItems.Select(o => o.Left.Model);
            var rightModels = TypeResults.MarkedItems.Select(o => o.Right.Model);

            MessageBox.Show("A TIA project will be created where you can use the offline/offline compare for a code based compare of the selected types.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            _openness.OnProgressUpdate += UpdateProgress;
            
            var bgWorker = new BackgroundWorker();
            bgWorker.DoWork += (sender, e) =>
            {
                IsRunning = true;
                _openness.DetailCompare(LibraryLeft.FilePath, LibraryRight.FilePath, leftModels, rightModels);
            };
            bgWorker.RunWorkerCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    if (e.Error is ArgumentException)
                    {
                        MessageBox.Show(e.Error.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show(e.Error.Message, "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                    MessageBox.Show("Please proceede in the TIA Portal UI.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                IsRunning = false;
            };

            _openness.OnProgressUpdate += UpdateProgress;

            bgWorker.RunWorkerAsync();
        }
        private bool CanDetailCompare()
        {
            return TypeResults.MarkedItems.Count > 0;
        }

        public ICommand SaveResultsCommand => new RelayCommand(OnSaveResults, CanSaveResults);
        private void OnSaveResults()
        {
            if (IsRunning) return;

            var dlg = new SaveFileDialog
            {
                OverwritePrompt = true,
                Filter = "CSV files (*.csv)|*.csv",
                AddExtension = true,
                DefaultExt = ".csv"
            };

            dlg.FileOk += (sender, args) =>
            {
                try
                {
                    var fileName = dlg.FileName;
                    SaveTypeCompareResult(fileName, LibraryLeft.FilePath, LibraryRight.FilePath, TypeResults.ListEntries);
                    MessageBox.Show("Export successfull!", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Unknown Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            dlg.ShowDialog();
        }
        private bool CanSaveResults()
        {
            return string.IsNullOrEmpty(LibraryLeft.FileName) == false
                && string.IsNullOrEmpty(LibraryRight.FileName) == false
                && !_isRunning;
        }

        private static void SaveTypeCompareResult(string fileName, string leftFilePath, string rightFilePath, IEnumerable<TypeEntryViewModel> compareResults)
        {
            var builder = new StringBuilder();
            builder.AppendLine(leftFilePath + " -- " + rightFilePath);
            builder.AppendLine(
                "Left Name;Left Type; Left Version;Left FolderPath;Left OutDated;Left GUID;Compare Result;Right Name;Right Type; Right Version;Right FolderPath;Right OutDated;Right GUID");

            foreach (var entry in compareResults)
            {
                var left = entry.Left;
                var right = entry.Right;

                builder.Append(left?.Name);
                builder.Append(";" + left?.Type);
                builder.Append("; " + left?.LatestVersion);
                builder.Append(";" + left?.FolderPath);
                builder.Append(";" + left?.OutDated);
                builder.Append(";" + left?.Guid);
                builder.Append(";" + entry.Result);
                builder.Append(";" + right?.Name);
                builder.Append(";" + right?.Type);
                builder.Append("; " + right?.LatestVersion);
                builder.Append(";" + right?.FolderPath);
                builder.Append(";" + right?.OutDated);
                builder.Append(";" + right?.Guid);
                builder.AppendLine();
            }

            File.WriteAllText(fileName, builder.ToString());
        }

        private static void SaveCopyCompareResult(string fileName,string leftFilePath, string rightFilePath, IEnumerable<CopyEntryViewModel> compareResults)
        {
            var builder = new StringBuilder();
            builder.AppendLine(leftFilePath + " -- " + rightFilePath);
            builder.AppendLine(
                "Left Name;Left Author;Left FolderPath;Left Creation Date;Compare Result;Right Name;Right Author;Right FolderPath;Right Creation Date");

            foreach (var entry in compareResults)
            {
                var left = entry.Left;
                var right = entry.Right;

                builder.Append(left?.Name);
                builder.Append(";" + left?.Author);
                builder.Append(";" + left?.FolderPath);
                builder.Append(";" + left?.CreationDate);
                builder.Append(";" + entry.Result);
                builder.Append(";" + right?.Name);
                builder.Append(";" + right?.Author);
                builder.Append(";" + right?.FolderPath);
                builder.Append(";" + right?.CreationDate);
                builder.AppendLine();
            }

            File.WriteAllText(fileName, builder.ToString());
        }

        private void OnPathUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Path"))
                TypeResults.ClearResults();
        }

        private void OnEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += OnItemChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnItemChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.NewItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged += OnItemChanged;
                }
                foreach (var item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnItemChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset && e.OldStartingIndex != -1)
            {
                foreach (var item in e.OldItems)
                {
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnItemChanged;
                }
            }
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
                RaisePropertyChanged("");
        }

        private void UpdateProgress(int percent, object userState)
        {
            var state = userState as string;
            if (state != null)
                WorkState = state;
        }
    }
}