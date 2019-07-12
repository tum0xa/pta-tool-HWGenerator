using System;
using System.Collections.Generic;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class LibraryCopyViewModel : ObservableObject, IComparable<LibraryCopyViewModel>
    {
        public readonly ILibraryCopyModel Model;

        public LibraryCopyViewModel(ILibraryCopyModel model)
        {
            Model = model;
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

        public string Author
        {
            get { return Model?.Author; }
            set
            {
                if (value == Model.Author)
                    return;
                Model.Author = value;
                RaisePropertyChanged(nameof(Author));
            }
        }

        public DateTime CreationDate
        {
            get { return Model?.CreationDate ?? DateTime.MinValue; }
            set
            {
                if (value == Model.CreationDate)
                    return;
                Model.CreationDate = value;
                RaisePropertyChanged(nameof(CreationDate));
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

        public Dictionary<string, string> Descriptions => Model?.ContentDescriptions ?? new Dictionary<string, string>();

        public int CompareTo(LibraryCopyViewModel other)
        {
            if (other == null)
                return (int)CompareResult.None;
            if (CreationDate > other.CreationDate)
                return (int)CompareResult.Left;
            if (CreationDate < other.CreationDate)
                return (int) CompareResult.Right;
            return (int)CompareResult.Diff;
        }
    }
}
