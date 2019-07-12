using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class LibraryVersionViewModel : ObservableObject, IComparable<LibraryVersionViewModel>
    {
        public readonly ILibraryVersionModel Model;

        public LibraryVersionViewModel(ILibraryVersionModel model)
        {
            Model = model;
        }

        public string Author
        {
            get { return Model?.Author; }
            private set
            {
                if (value == Model.Author)
                    return;
                Model.Author = value;
                RaisePropertyChanged(nameof(Author));
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

        public Version Version
        {
            get { return Model?.Version; }
            private set
            {
                if (value == Model.Version)
                    return;
                Model.Version = value;
                RaisePropertyChanged(nameof(Version));
            }
        }

        public DateTime LastModified
        {
            get { return Model?.LastModified ?? new DateTime(); }
            private set
            {
                if (value == Model.LastModified)
                    return;
                Model.LastModified = value;
                RaisePropertyChanged(nameof(LastModified));
            }
        }

        public LibraryVersionState State
        {
            get { return Model?.State ?? LibraryVersionState.Committed; }
            private set
            {
                if (value == Model.State)
                    return;
                Model.State = value;
                RaisePropertyChanged(nameof(State));
            }
        }

        public LibraryItemType Type
        {
            get { return Model.Parent.Type; }
        }

        public string TypeName
        {
            get { return Model.Parent.Name; }
        }

        public ObservableCollection<LibraryVersionViewModel> Dependencies
        {
            get
            {
                var ret = new ObservableCollection<LibraryVersionViewModel>();
                foreach (var modelDependency in Model.Dependencies)
                {
                    ret.Add(new LibraryVersionViewModel(modelDependency));
                }

                return ret;
            }
        }

        public ObservableCollection<LibraryVersionViewModel> Dependents
        {
            get
            {
                var ret = new ObservableCollection<LibraryVersionViewModel>();
                foreach (var modelDependents in Model.Dependents)
                {
                    ret.Add(new LibraryVersionViewModel(modelDependents));
                }

                return ret;
            }
        }

        public int CompareTo(LibraryVersionViewModel other)
        {
            if (other == null)
                return (int)CompareResult.None;
            if (Version == other.Version && Guid == other.Guid)
                return (int)CompareResult.Equal;
            if (Version > other.Version)
                return (int)CompareResult.Left;
            if (Version < other.Version)
                return (int)CompareResult.Right;
            return (int)CompareResult.Diff;
        }
    }
}