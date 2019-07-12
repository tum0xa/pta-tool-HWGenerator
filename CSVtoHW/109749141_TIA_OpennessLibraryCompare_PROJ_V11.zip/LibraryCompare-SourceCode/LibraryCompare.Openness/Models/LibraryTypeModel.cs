using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LibraryCompare.Core.Enums;
using LibraryCompare.Core.Interfaces;
using Siemens.Engineering.Library.Types;

namespace LibraryCompare.Openness.Models
{
    public class LibraryTypeModel : ILibraryTypeModel
    {
        public readonly LibraryType TypeObject;

        public LibraryTypeModel(LibraryType type)
        {
            TypeObject = type;
            Name = type.Name;
            switch (type.GetType().Name)
            {
                case "FaceplateLibraryType":
                    Type = LibraryItemType.Faceplate;
                    break;
                case "CScriptLibraryType":
                    Type = LibraryItemType.CScript;
                    break;
                case "VBScriptLibraryType":
                    Type = LibraryItemType.VbScript;
                    break;
                case "ScreenLibraryType":
                    Type = LibraryItemType.Screen;
                    break;
                case "StyleLibraryType":
                    Type = LibraryItemType.HmiStyle;
                    break;
                case "StyleSheetLibraryType":
                    Type = LibraryItemType.HmiStyleSheet;
                    break;
                case "HmiUdtLibraryType":
                    Type = LibraryItemType.HmiUdt;
                    break;
                case "CodeBlockLibraryType":
                    Type = LibraryItemType.Block;
                    break;
                case "PlcTypeLibraryType":
                    Type = LibraryItemType.PlcUdt;
                    break;
                default:
                    Type = LibraryItemType.Unknown;
                    break;
            }
            Guid = type.Guid;

            Comment = new Dictionary<string, string>();
            foreach (var item in type.Comment.Items)
            {
                Comment.Add(item.Language.Culture.Name, item.Text);
            }

            Versions = new ObservableCollection<ILibraryVersionModel>();
            foreach (var typeVersion in type.Versions.OrderBy(o => o.VersionNumber))
            {
                Versions.Add(new LibraryVersionModel(this, typeVersion));
            }

            LatestVersion = Versions.LastOrDefault()?.Version;
        }

        public string Name { get; set; }

        public string Author { get; set; }

        public LibraryItemType Type { get; set; }

        public Guid Guid { get; set; }

        public Dictionary<string, string> Comment { get; set; }

        public Version LatestVersion { get; set; }

        public bool OutDated { get; set; }

        public ObservableCollection<ILibraryVersionModel> Versions { get; set; }

        public string FolderPath { get; set; }
    }
}