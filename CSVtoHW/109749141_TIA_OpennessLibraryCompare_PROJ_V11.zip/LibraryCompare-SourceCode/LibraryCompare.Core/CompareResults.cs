using System;
using System.Collections.Generic;
using LibraryCompare.Core.Interfaces;

namespace LibraryCompare.Core
{
    public class CompareResults
    {
        public IList<Tuple<ILibraryTypeModel, ILibraryTypeModel>> TypePairs;
        public IList<Tuple<ILibraryCopyModel, ILibraryCopyModel>> CopyPairs;
    }
}
