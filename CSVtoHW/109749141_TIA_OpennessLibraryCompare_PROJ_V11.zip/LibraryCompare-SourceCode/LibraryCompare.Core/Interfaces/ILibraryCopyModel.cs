using System;
using System.Collections.Generic;

namespace LibraryCompare.Core.Interfaces
{
    public interface ILibraryCopyModel : ILibraryElement
    {
        DateTime CreationDate { get; set; }
        Dictionary<string, string> ContentDescriptions { get; set; }
    }
}
