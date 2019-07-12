using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class LibraryTypeViewModel : ObservableObject, IComparable<LibraryTypeViewModel>
    {
        public readonly ILibraryTypeModel Model;

        private ObservableCollection<LibraryVersionViewModel> _versions;

        public LibraryTypeViewModel(ILibraryTypeModel model)
        {
            Model = model;
            _versions = new ObservableCollection<LibraryVersionViewModel>();
            if (model != null)
                foreach (var version in model.Versions)
                    _versions.Add(new LibraryVersionViewModel(version));
        }

        public string Name
        {
            get { return Model?.Name; }
            private set
            {
                if (value == Model.Name)
                    return;
                Model.Name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public LibraryItemType Type
        {
            get { return Model?.Type ?? LibraryItemType.Unknown; }
            private set
            {
                if (value == Model.Type)
                    return;
                Model.Type = value;
                RaisePropertyChanged(nameof(Type));
            }
        }

        public Guid Guid
        {
            get { return Model?.Guid ?? Guid.Empty; }
            private set
            {
                if (value == Model.Guid)
                    return;
                Model.Guid = value;
                RaisePropertyChanged(nameof(Guid));
            }
        }

        public Dictionary<string, string> Comment
        {
            get { return Model?.Comment; }
            private set
            {
                if (value == Model.Comment)
                    return;
                Model.Comment = value;
                RaisePropertyChanged(nameof(Comment));
            }
        }

        public Version LatestVersion
        {
            get { return Model?.LatestVersion; }
            private set
            {
                if (value == Model.LatestVersion)
                    return;
                Model.LatestVersion = value;
                RaisePropertyChanged(nameof(LatestVersion));
            }
        }

        public bool OutDated
        {
            get { return Model?.OutDated ?? false; }
            set
            {
                if (Model == null || value == Model.OutDated)
                    return;
                Model.OutDated = value;
                RaisePropertyChanged(nameof(OutDated));
            }
        }

        public ObservableCollection<LibraryVersionViewModel> Versions
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

        public string FolderPath
        {
            get { return Model?.FolderPath; }
            set
            {
                if (value == Model.FolderPath)
                    return;
                Model.FolderPath = value;
                RaisePropertyChanged(nameof(FolderPath));
            }
        }

        public int CompareTo(LibraryTypeViewModel other)
        {
            if (other == null)
                return (int)CompareResult.None;
            if (LatestVersion == other.LatestVersion && Versions.Last().Guid == other.Versions.Last().Guid)
                return (int)CompareResult.Equal;
            if (LatestVersion > other.LatestVersion)
                return (int)CompareResult.Left;
            if (LatestVersion < other.LatestVersion)
                return (int)CompareResult.Right;
            return (int)CompareResult.Diff;
        }
    }
}