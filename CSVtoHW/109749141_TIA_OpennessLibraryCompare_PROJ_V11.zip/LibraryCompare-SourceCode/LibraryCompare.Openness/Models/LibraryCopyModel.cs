using System;
using System.Collections.Generic;
using LibraryCompare.Core.Interfaces;
using Siemens.Engineering.Library.MasterCopies;

namespace LibraryCompare.Openness.Models
{
    public class LibraryCopyModel : ILibraryCopyModel
    {
        public LibraryCopyModel(MasterCopy copy)
        {
            Name = copy.Name;
            Author = copy.Author;
            CreationDate = copy.CreationDate;
            ContentDescriptions = new Dictionary<string, string>();

            foreach (var description in copy.ContentDescriptions)
            {
                ContentDescriptions.Add(description.ContentName, description.ContentType.ToString());
            }
        }
        public string Name { get; set; }
        public string Author { get; set; }
        public string FolderPath { get; set; }
        public DateTime CreationDate { get; set; }
        public Dictionary<string, string> ContentDescriptions { get; set; }
    }
}
