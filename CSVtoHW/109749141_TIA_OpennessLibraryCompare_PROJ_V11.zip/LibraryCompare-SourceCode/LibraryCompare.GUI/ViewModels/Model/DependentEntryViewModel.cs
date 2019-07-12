using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LibraryCompare.Core.Enums;
using MvvmFoundation.Wpf;

namespace LibraryCompare.GUI.ViewModels
{
    public class DependentEntryViewModel : ObservableObject
    {
        private readonly LibraryVersionViewModel _left;
        private readonly LibraryVersionViewModel _right;

        private readonly ObservableCollection<DependentEntryViewModel> _dependents;

        public DependentEntryViewModel(LibraryVersionViewModel left, LibraryVersionViewModel right)
        {
            _left = left;
            _right = right;
            LeftLatestVersion = _left?.Model.Parent?.LatestVersion ?? new Version(0, 0, 0);
            RightLatestVersion = _right?.Model.Parent?.LatestVersion ?? new Version(0, 0, 0);

            _dependents = new ObservableCollection<DependentEntryViewModel>();

            if (left != null && right != null)
            {
                _dependents = GetDependents(left.Dependents.OrderBy(o => o.TypeName).ToList(), right.Dependents.OrderBy(o => o.TypeName).ToList());
            }
            else if (right != null)
            {
                foreach (var dependency in right.Dependents)
                {
                    _dependents.Add(new DependentEntryViewModel(null, dependency));
                }
            }
            else if (left != null)
            {
                foreach (var dependency in left.Dependents)
                {
                    _dependents.Add(new DependentEntryViewModel(dependency, null));
                }
            }
        }

        private static ObservableCollection<DependentEntryViewModel> GetDependents(IReadOnlyList<LibraryVersionViewModel> leftDependencies, IReadOnlyList<LibraryVersionViewModel> rightDependencies)
        {
            var ret = new ObservableCollection<DependentEntryViewModel>();
            int i;
            int diff = 0;

            for (i = 0; i < leftDependencies.Count; i++)
            {
                if (i + diff >= rightDependencies.Count)
                {
                    ret.Add(new DependentEntryViewModel(leftDependencies[i], null));
                    continue;
                }

                var order = string.Compare(leftDependencies[i].TypeName, rightDependencies[i + diff].TypeName, StringComparison.OrdinalIgnoreCase);
                if (order == 0)
                {
                    // both types were used
                    ret.Add(new DependentEntryViewModel(leftDependencies[i], rightDependencies[i + diff]));
                }
                else if (order > 0)
                {
                    ret.Add(new DependentEntryViewModel(null, rightDependencies[i + diff]));
                    // rightType was used in the list
                    diff++;
                    // repeat loop index
                    i--;
                }
                else
                {
                    // LeftType was used in the list
                    ret.Add(new DependentEntryViewModel(leftDependencies[i], null));
                    diff--;
                }
            }

            for (var j = i + diff; j < rightDependencies.Count; j++)
            {
                // add all rightTypes that weren't added in the first loop
                ret.Add(new DependentEntryViewModel(null, rightDependencies[j]));
            }

            return ret;
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

        public ObservableCollection<DependentEntryViewModel> Dependents => _dependents;
    }
}
