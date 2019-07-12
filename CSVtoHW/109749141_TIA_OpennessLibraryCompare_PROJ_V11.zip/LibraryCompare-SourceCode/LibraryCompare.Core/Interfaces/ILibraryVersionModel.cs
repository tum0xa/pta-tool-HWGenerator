using System;
using System.Collections.Generic;
using LibraryCompare.Core.Enums;

namespace LibraryCompare.Core.Interfaces
{
    public interface ILibraryVersionModel
    {
        string Author { get; set; }
        Dictionary<string, string> Comment { get; set; }
        Guid Guid { get; set; }
        DateTime LastModified { get; set; }
        ILibraryTypeModel Parent { get; set; }
        LibraryVersionState State { get; set; }
        Version Version { get; set; }
        IList<ILibraryVersionModel> Dependencies { get; set; }
        IList<ILibraryVersionModel> Dependents { get; set; }
    }
}