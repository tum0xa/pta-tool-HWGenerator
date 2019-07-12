using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibraryCompare.Core;
using LibraryCompare.Core.Interfaces;
using LibraryCompare.Openness.Models;
using Siemens.Engineering;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using Siemens.Engineering.Library;
using Siemens.Engineering.Library.MasterCopies;
using Siemens.Engineering.Library.Types;

namespace LibraryCompare.Openness
{
    public class Openness : IOpenness
    {
        private ProgressUpdate _onProgressUpdate;

        public event ProgressUpdate OnProgressUpdate
        {
            add { _onProgressUpdate += value; }
            // ReSharper disable once DelegateSubtraction
            remove { _onProgressUpdate -= value; }
        }

        public CompareResults Compare(string leftPath, string rightPath)
        {
            if (string.IsNullOrEmpty(leftPath)) throw new ArgumentException("Parameter is null or empty", nameof(leftPath));
            if (string.IsNullOrEmpty(rightPath)) throw new ArgumentException("Parameter is null or empty", nameof(rightPath));

            var leftInfo = new FileInfo(leftPath);
            if (leftInfo.Exists == false) throw new FileNotFoundException("File does not exist", leftInfo.FullName);
            var rightInfo = new FileInfo(rightPath);
            if (rightInfo.Exists == false) throw new FileNotFoundException("File does not exist", rightInfo.FullName);

            if (leftInfo.Extension == ".ap14" && rightInfo.Extension == ".ap14")
                throw new ArgumentException("Comparing 2 projects is not possible");

            Trace.Listeners.Add(new TextWriterTraceListener(@"OpennessTrace.log"));
            Trace.AutoFlush = true;
            
            UpdateProgress(0, "Start Openness");
            using (var portal = new TiaPortal())
            {
                UpdateProgress(0, "Openning Project/Library 1: " + leftInfo.FullName);
                var leftLibrary = OpenFile(portal, leftInfo);
                UpdateProgress(0, "Openning Project/Library 2: " + rightInfo.FullName);
                var rightLibrary = OpenFile(portal, rightInfo);

                UpdateProgress(0, "Browse all Types Library 1");
                var leftTypes = GetAllTypes(leftLibrary.TypeFolder, "").ToList();
                UpdateProgress(0, "Found " + leftTypes.Count + " Types");
                UpdateProgress(0, "Browse all Types Library 2");
                var rightTypes = GetAllTypes(rightLibrary.TypeFolder, "").ToList();
                UpdateProgress(0, "Found " + rightTypes.Count + " Types");

                UpdateProgress(0, "Create Type Relations");
                var typePairs = CreateTypeRelations(leftTypes, rightTypes);
                UpdateProgress(0, "Created " + typePairs.Count + " Type Relations");
                UpdateProgress(0, "Read Dependency structure");
                AnalyzeTypes(typePairs);

                UpdateProgress(0, "Browse all MasterCopies Library 1");
                var leftCopies = GetAllMasterCopies(leftLibrary.MasterCopyFolder, "").ToList();
                UpdateProgress(0, "Found " + leftCopies.Count + " MasterCopies");

                UpdateProgress(0, "Browse all MasterCopies Library 2");
                var rightCopies = GetAllMasterCopies(rightLibrary.MasterCopyFolder, "").ToList();
                UpdateProgress(0, "Found " + rightCopies.Count + " MasterCopies");

                UpdateProgress(0, "Create MasterCopy Relations");
                var copyPairs = CreateMasterCopyRelations(leftCopies, rightCopies);
                UpdateProgress(0, "Created " + copyPairs.Count + " MasterCopy Relations");

                UpdateProgress(0, "Close Library 1");
                CloseLibrary(leftLibrary);
                UpdateProgress(0, "Close Library 2");
                CloseLibrary(rightLibrary);

                UpdateProgress(0, "Done");
                var ret = new CompareResults()
                {
                    TypePairs = typePairs,
                    CopyPairs = copyPairs
                };

                return ret;
            }
        }

        public void DetailCompare(string leftPath, string rightPath, IEnumerable<ILibraryTypeModel> leftModels, IEnumerable<ILibraryTypeModel> rightModels)
        {
            if (string.IsNullOrEmpty(leftPath)) throw new ArgumentException("Parameter is null or empty", nameof(leftPath));
            if (string.IsNullOrEmpty(rightPath)) throw new ArgumentException("Parameter is null or empty", nameof(rightPath));

            var leftInfo = new FileInfo(leftPath);
            if (leftInfo.Exists == false) throw new FileNotFoundException("File does not exist", leftInfo.FullName);
            var rightInfo = new FileInfo(rightPath);
            if (rightInfo.Exists == false) throw new FileNotFoundException("File does not exist", rightInfo.FullName);

            if (leftInfo.Extension == ".ap14" && rightInfo.Extension == ".ap14")
                throw new ArgumentException("Comparing 2 projects is not possible");

            UpdateProgress(0, "Start Openness");
            using (var portal = new TiaPortal(TiaPortalMode.WithUserInterface))
            {
                UpdateProgress(0, "Prepare Library 1: " + leftInfo.FullName);
                var leftLibrary = PrepareLibrary(portal, leftInfo);
                UpdateProgress(0, "Prepare Library 2: " + rightInfo.FullName);
                var rightLibrary = PrepareLibrary(portal, rightInfo);

                UpdateProgress(0, "Create Compare project");
                var project = portal.Projects.Create(new DirectoryInfo(Path.GetTempPath()), leftLibrary.Name + "-" + rightLibrary.Name + "_" + DateTime.Now.ToString("yyyyMMdd-hhmmss"));

                UpdateProgress(0, "Prepare project for Library 1");
                var leftTarget = PrepareProject(project, leftLibrary, leftModels);
                UpdateProgress(0, "Prepare project for Library 2");
                var rightTarget = PrepareProject(project, rightLibrary, rightModels);

                UpdateProgress(0, "Start Compare");
                var result = leftTarget.CompareTo(rightTarget).RootElement;
                UpdateProgress(0, result.ComparisonResult + " - " + result.LeftName + "-" + result.RightName + " -- " + result.DetailedInformation);
            }

            UpdateProgress(0, "Finished");
        }


        private void UpdateProgress(int percent, string state)
        {
            Trace.TraceInformation(DateTime.Now + " - " + state);
            _onProgressUpdate?.Invoke(percent, state);
        }

        private static ILibrary OpenFile(TiaPortal portal, FileInfo fileInfo)
        {
            try
            {
                switch (fileInfo.Extension)
                {
                    case ".al14":
                        return portal.GlobalLibraries.Open(fileInfo, OpenMode.ReadOnly);
                    case ".ap14":
                        var project = portal.Projects.Open(fileInfo);
                        return project.ProjectLibrary;
                    default:
                        throw new ArgumentException("Selected File is of the wrong type");
                }
            }
            catch (EngineeringTargetInvocationException e)
            {
                throw new ArgumentException("The File cannot be opened", fileInfo.FullName, e);
            }
        }

        private static UserGlobalLibrary CreateNewLibrary(TiaPortal portal, string libraryName)
        {
            var path = new FileInfo(Path.Combine(Path.GetTempPath(), libraryName, libraryName + ".al14"));

            UserGlobalLibrary lib;

            if (path.Exists)
            {
                lib = portal.GlobalLibraries.Open(path, OpenMode.ReadWrite);

                foreach (var type in lib.TypeFolder.Types)
                {
                    type.Delete();
                }
                foreach (var folder in lib.TypeFolder.Folders)
                {
                    folder.Delete();
                }
            }
            else
            {
                lib = portal.GlobalLibraries.Create<UserGlobalLibrary>(new DirectoryInfo(Path.GetTempPath()),
                    libraryName);
            }
            return lib;
        }

        private static void CloseLibrary(ILibrary library)
        {
            var tmp = library as UserGlobalLibrary;
            if (tmp != null)
            {
                tmp.Close();
                return;
            }
            ((library as ProjectLibrary)?.Parent as Project)?.Close();
        }

        private static UserGlobalLibrary PrepareLibrary(TiaPortal portal, FileInfo fileInfo)
        {
            UserGlobalLibrary library;
            if (fileInfo.Extension == ".ap14")
            {
                // Copy Types to new GlobalLibrary
                var tmpLibrary = OpenFile(portal, fileInfo);

                library = CreateNewLibrary(portal, "New" + Path.GetFileNameWithoutExtension(fileInfo.FullName));

                tmpLibrary.UpdateLibrary(new[] { tmpLibrary.TypeFolder }, library);
                CloseLibrary(tmpLibrary);
            }
            else
            {
                library = OpenFile(portal, fileInfo) as UserGlobalLibrary;
            }

            return library;
        }

        private static PlcSoftware PrepareProject(Project project, UserGlobalLibrary library, IEnumerable<ILibraryTypeModel> models)
        {
            var device = project.Devices.CreateWithItem("OrderNumber:6ES7 518-4AP00-0AB0/V2.1", library.Name, library.Name + "Station");
            var target = device.DeviceItems[1].GetService<SoftwareContainer>().Software as PlcSoftware;

            var list = GetSelectedTypes(library, models);
            foreach (var type in list)
            {
                InstantiateType(target, type);
            }

            return target;
        }

        private static void InstantiateType(PlcSoftware target, LibraryType type)
        {
            var folderNames = new Stack<string>();

            IEngineeringObject parent = type;
            try
            {
                while (!(parent is LibraryTypeSystemFolder))
                {
                    parent = parent.Parent;
                    folderNames.Push(((LibraryTypeFolder)parent).Name);
                }

                if (type.GetType().Name == "CodeBlockLibraryType")
                {
                    PlcBlockGroup blockGroup = target.BlockGroup;
                    while (folderNames.Any())
                    {
                        var name = folderNames.Pop();
                        blockGroup = blockGroup.Groups.Find(name) ?? blockGroup.Groups.Create(name);
                    }
                    blockGroup.Blocks.CreateFrom((CodeBlockLibraryTypeVersion)type.Versions.LastOrDefault());
                }
                else if (type.GetType().Name == "PlcTypeLibraryType")
                {
                    PlcTypeGroup typeGroup = target.TypeGroup;
                    while (folderNames.Any())
                    {
                        var name = folderNames.Pop();
                        typeGroup = typeGroup.Groups.Find(name) ?? typeGroup.Groups.Create(name);
                    }
                    typeGroup.Types.CreateFrom((PlcTypeLibraryTypeVersion)type.Versions.LastOrDefault());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(DateTime.Now + " - " + ex);
            }
        }

        private static IEnumerable<LibraryType> GetSelectedTypes(ILibrary library, IEnumerable<ILibraryTypeModel> models)
        {
            var dict = models.ToDictionary(model => model.Guid);

            return GetAllTypes(library.TypeFolder).Where(item => dict.ContainsKey(item.Guid));
        }

        private static IEnumerable<LibraryTypeModel> GetAllTypes(LibraryTypeFolder folder, string path)
        {
            path = path + @"\" + folder.Name;

            var ret = folder.Types.Select(o => new LibraryTypeModel(o) { FolderPath = path }).ToList();

            foreach (var subFolder in folder.Folders)
            {
                ret.AddRange(GetAllTypes(subFolder, path));
            }

            return ret;
        }

        private static IEnumerable<LibraryType> GetAllTypes(LibraryTypeFolder folder)
        {
            var ret = folder.Types.ToList();

            foreach (var subFolder in folder.Folders)
            {
                ret.AddRange(GetAllTypes(subFolder));
            }

            return ret;
        }

        private static IList<Tuple<ILibraryTypeModel, ILibraryTypeModel>> CreateTypeRelations(IReadOnlyList<LibraryTypeModel> leftTypes,
            IReadOnlyList<LibraryTypeModel> rightTypes)
        {
            var typePairs = new List<Tuple<ILibraryTypeModel, ILibraryTypeModel>>();
            leftTypes = leftTypes.OrderBy(o => o.Guid).ToList();
            rightTypes = rightTypes.OrderBy(o => o.Guid).ToList();

            var diff = 0;
            int i;

            for (i = 0; i < leftTypes.Count; i++)
            {
                ILibraryTypeModel rightModel = null;
                var leftModel = leftTypes[i];

                if (i + diff < rightTypes.Count)
                {
                    rightModel = rightTypes[i + diff];

                    var order = leftTypes[i].Guid.CompareTo(rightTypes[i + diff].Guid);
                    if (order < 0)
                    {
                        // LeftType was used in the list
                        rightModel = null;
                        diff--;
                    }
                    else if (order > 0)
                    {
                        // rightType was used in the list
                        leftModel = null;
                        diff++;
                        // repeat loop index
                        i--;
                    }
                    else
                    {
                        // both types were used
                    }
                }

                typePairs.Add(new Tuple<ILibraryTypeModel, ILibraryTypeModel>(leftModel, rightModel));
            }

            for (var j = i + diff; j < rightTypes.Count; j++)
            {
                // add all rightTypes that weren't added in the first loop
                typePairs.Add(new Tuple<ILibraryTypeModel, ILibraryTypeModel>(null, rightTypes[j]));
            }

            return typePairs;
        }

        private static void AnalyzeTypes(IEnumerable<Tuple<ILibraryTypeModel, ILibraryTypeModel>> typePairs)
        {
            var leftTypeModels = new List<LibraryTypeModel>();
            var rightTypeModels = new List<LibraryTypeModel>();

            foreach (var typePair in typePairs)
            {
                if (typePair.Item1 != null) leftTypeModels.Add((LibraryTypeModel)typePair.Item1);
                if (typePair.Item2 != null) rightTypeModels.Add((LibraryTypeModel)typePair.Item2);
            }

            BrowseDependencies(leftTypeModels);
            BrowseDependencies(rightTypeModels);

            Trace.TraceInformation(DateTime.Now + " - Check for Updates");
            CheckForUpdates(leftTypeModels, rightTypeModels.ToDictionary(o => o.Guid));
            CheckForUpdates(rightTypeModels, leftTypeModels.ToDictionary(o => o.Guid));
        }

        private static void BrowseDependencies(IReadOnlyList<LibraryTypeModel> typeModels)
        {
            var versionList = (from typeModel in typeModels from versionModel in typeModel.Versions select versionModel as LibraryVersionModel).ToDictionary(version => version.Guid);

            foreach (var typeModel in typeModels)
            {
                var type = typeModel.TypeObject;
                if (type == null) continue;

                foreach (var versionModel in typeModel.Versions)
                {
                    var version = ((LibraryVersionModel)versionModel).VersionObject;

                    foreach (var versionDependency in version.Dependencies)
                    {
                        LibraryVersionModel tmp;
                        if (versionList.TryGetValue(versionDependency.Guid, out tmp))
                            versionModel.Dependencies.Add(tmp);
                    }
                    foreach (var versionDependents in version.Dependents)
                    {
                        LibraryVersionModel tmp;
                        if (versionList.TryGetValue(versionDependents.Guid, out tmp))
                            versionModel.Dependents.Add(tmp);
                    }
                }
            }
        }

        private static void CheckForUpdates(IEnumerable<LibraryTypeModel> toCheck,
            IReadOnlyDictionary<Guid, LibraryTypeModel> referenzes)
        {
            foreach (var typeModel in toCheck)
            {
                var version = typeModel.Versions.Last();

                foreach (var dependency in version.Dependencies)
                {
                    LibraryTypeModel tmp;

                    if (typeModel.OutDated) break;

                    if (dependency.Parent.LatestVersion > dependency.Version
                        || referenzes.TryGetValue(dependency.Parent.Guid, out tmp)
                        && tmp.LatestVersion > dependency.Version)
                        typeModel.OutDated = true;
                }
            }
        }

        private static IEnumerable<LibraryCopyModel> GetAllMasterCopies(MasterCopyFolder folder, string path)
        {
            path = path + @"\" + folder.Name;

            var ret = folder.MasterCopies.Select(o => new LibraryCopyModel(o) { FolderPath = path }).ToList();

            foreach (var subFolder in folder.Folders)
            {
                ret.AddRange(GetAllMasterCopies(subFolder, path));
            }

            return ret;
        }

        private static IList<Tuple<ILibraryCopyModel, ILibraryCopyModel>> CreateMasterCopyRelations(IReadOnlyList<LibraryCopyModel> leftCopies,
            IReadOnlyList<LibraryCopyModel> rightCopies)
        {
            var typePairs = new List<Tuple<ILibraryCopyModel, ILibraryCopyModel>>();
            leftCopies = leftCopies.OrderBy(o => o.FolderPath).ThenBy(o => o.Name).ToList();
            rightCopies = rightCopies.OrderBy(o => o.FolderPath).ThenBy(o => o.Name).ToList();

            var diff = 0;
            int i;

            for (i = 0; i < leftCopies.Count; i++)
            {
                ILibraryCopyModel rightModel = null;
                var leftModel = leftCopies[i];

                if (i + diff < rightCopies.Count)
                {
                    rightModel = rightCopies[i + diff];
                    
                    var order = string.Compare(leftModel.FolderPath, rightModel.FolderPath, StringComparison.OrdinalIgnoreCase);
                    if (order < 0) // leftPath comes before rightpath
                    {
                        // leftCopy was used in the list
                        rightModel = null;
                        diff--;
                    }
                    else if (order > 0) // leftPath comes after rightpath
                    {
                        // rightCopy was used in the list
                        leftModel = null;
                        diff++;
                        // repeat loop index
                        i--;
                    }
                    else // paths are identical
                    {
                        // both copies were used
                        order = string.Compare(leftModel.Name, rightModel.Name, StringComparison.OrdinalIgnoreCase);

                        if (order < 0)
                        {
                            // leftCopy was used in the list
                            rightModel = null;
                            diff--;
                        }
                        else if (order > 0)
                        {
                            // rightCopy was used in the list
                            leftModel = null;
                            diff++;
                            // repeat loop index
                            i--;
                        }
                    }
                }

                typePairs.Add(new Tuple<ILibraryCopyModel, ILibraryCopyModel>(leftModel, rightModel));
            }

            for (var j = i + diff; j < rightCopies.Count; j++)
            {
                // add all rightCopies that weren't added in the first loop
                typePairs.Add(new Tuple<ILibraryCopyModel, ILibraryCopyModel>(null, rightCopies[j]));
            }

            return typePairs;
        }
    }
}