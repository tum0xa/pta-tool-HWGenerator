using System;
using System.Collections.Generic;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using Siemens.Engineering.Library.Types;

namespace LibraryCompare.Openness.Models
{
    public class LibraryVersionModel : ILibraryVersionModel
    {
        public readonly LibraryTypeVersion VersionObject;
        
        public LibraryVersionModel(ILibraryTypeModel parent, LibraryTypeVersion version)
        {
            VersionObject = version;
            Parent = parent;
            Author = version.Author;
            Guid = version.Guid;

            Comment = new Dictionary<string, string>();
            foreach (var item in version.Comment.Items)
            {
                Comment.Add(item.Language.Culture.Name, item.Text);
            }

            Version = version.VersionNumber;
            LastModified = version.ModifiedDate;
            State = (LibraryVersionState)version.State;
            Dependencies = new List<ILibraryVersionModel>();
            Dependents = new List<ILibraryVersionModel>();
        }

        public ILibraryTypeModel Parent { get; set; }

        public string Author { get; set; }

        public Guid Guid { get; set; }

        public Dictionary<string, string> Comment { get; set; }

        public Version Version { get; set; }

        public DateTime LastModified { get; set; }

        public LibraryVersionState State { get; set; }

        public IList<ILibraryVersionModel> Dependencies { get; set; }

        public IList<ILibraryVersionModel> Dependents { get; set; }
    }
}