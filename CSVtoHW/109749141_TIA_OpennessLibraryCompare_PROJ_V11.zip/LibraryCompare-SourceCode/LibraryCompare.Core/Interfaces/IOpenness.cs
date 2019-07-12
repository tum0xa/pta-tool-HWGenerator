using System;
using System.Collections.Generic;

namespace LibraryCompare.Core.Interfaces
{
    public delegate void ProgressUpdate(int percent, object userStatus);
    public interface IOpenness
    {
        CompareResults Compare(string leftPath, string rightPath);
        void DetailCompare(string leftPath, string rightPath, IEnumerable<ILibraryTypeModel> leftModels, IEnumerable<ILibraryTypeModel> rightModels);
        
        event ProgressUpdate OnProgressUpdate;
    }
}