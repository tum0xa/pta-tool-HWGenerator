using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LibraryCompare.Core.Enums;

namespace LibraryCompare.Core.Interfaces
{
    public interface ILibraryTypeModel : ILibraryElement
    {
        Dictionary<string, string> Comment { get; set; }
        Guid Guid { get; set; }
        Version LatestVersion { get; set; }
        LibraryItemType Type { get; set; }
        bool OutDated { get; set; }
        ObservableCollection<ILibraryVersionModel> Versions { get; set; }
    }
}