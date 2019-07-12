using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LibraryCompare.Core.Enums;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class DependencyEntryViewModel : ObservableObject
    {
        private readonly LibraryVersionViewModel _left;
        private readonly LibraryVersionViewModel _right;

        private readonly ObservableCollection<DependencyEntryViewModel> _dependencies;

        public DependencyEntryViewModel(LibraryVersionViewModel left, LibraryVersionViewModel right)
        {
            _left = left;
            _right = right;
            LeftLatestVersion = _left?.Model.Parent?.LatestVersion;
            RightLatestVersion = _right?.Model.Parent?.LatestVersion;

            _dependencies = new ObservableCollection<DependencyEntryViewModel>();

            if (left != null && right != null)
            {
                _dependencies = GetDependencies(left.Dependencies.OrderBy(o => o.TypeName).ToList(), right.Dependencies.OrderBy(o => o.TypeName).ToList());
            }
            else if (right != null)
            {
                foreach (var dependency in right.Dependencies)
                {
                    _dependencies.Add(new DependencyEntryViewModel(null, dependency));
                }
            }
            else if (left != null)
            {
                foreach (var dependency in left.Dependencies)
                {
                    _dependencies.Add(new DependencyEntryViewModel(dependency, null));
                }
            }
        }

        public LibraryItemType LeftType => _left?.Type ?? LibraryItemType.Unknown;
        public LibraryItemType RightType => _right?.Type ?? LibraryItemType.Unknown;

        public string LeftTypeName => _left?.TypeName;
        public string RightTypeName => _right?.TypeName;

        public Version LeftVersion => _left?.Version;
        public Version RightVersion => _right?.Version;

        public Version LeftLatestVersion { get; }
        public Version RightLatestVersion { get; }

        public bool LeftOutdated
        {
            get
            {
                if (LeftVersion == null)
                    return false;
                return LeftLatestVersion > LeftVersion;
            }
        }
        public bool RightOutdated
        {
            get
            {
                if (RightVersion == null)
                    return false;
                return RightLatestVersion > RightVersion;
            }
        }

        public CompareResult Result
        {
            get
            {
                var result = _left?.CompareTo(_right);
                if (result != null) return (CompareResult)result;
                return CompareResult.None;
            }
        }

        public ObservableCollection<DependencyEntryViewModel> Dependencies => _dependencies;

        private static ObservableCollection<DependencyEntryViewModel> GetDependencies(IReadOnlyList<LibraryVersionViewModel> leftDependencies, IReadOnlyList<LibraryVersionViewModel> rightDependencies)
        {
            var ret = new ObservableCollection<DependencyEntryViewModel>();
            int i;
            int diff = 0;

            for (i = 0; i < leftDependencies.Count; i++)
            {
                if (i + diff >= rightDependencies.Count)
                {
                    ret.Add(new DependencyEntryViewModel(leftDependencies[i], null));
                    continue;
                }

                var order = string.Compare(leftDependencies[i].TypeName, rightDependencies[i + diff].TypeName, StringComparison.OrdinalIgnoreCase);
                if (order == 0)
                {
                    // both types were used
                    ret.Add(new DependencyEntryViewModel(leftDependencies[i], rightDependencies[i + diff]));
                }
                else if (order > 0)
                {
                    ret.Add(new DependencyEntryViewModel(null, rightDependencies[i + diff]));
                    // rightType was used in the list
                    diff++;
                    // repeat loop index
                    i--;
                }
                else
                {
                    // LeftType was used in the list
                    ret.Add(new DependencyEntryViewModel(leftDependencies[i], null));
                    diff--;
                }
            }

            for (var j = i + diff; j < rightDependencies.Count; j++)
            {
                // add all rightTypes that weren't added in the first loop
                ret.Add(new DependencyEntryViewModel(null, rightDependencies[j]));
            }

            return ret;
        }
    }
}
