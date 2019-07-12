using System.ComponentModel;
using LibraryCompare.Core.Enums;

namespace LibraryCompare.GUI.Utilities
{
    [TypeConverter(typeof(EnumToStringUsingDescription))]
    public enum ResultFilter
    {
        [Description("Show all types")]
        ShowAll = 0,
        [Description("Types with pending changes")]
        PendingChanges = 1,
        [Description("Types with multiple versions")]
        MultipleVersions = 2
    }

    [TypeConverter(typeof(EnumToStringUsingDescription))]
    public enum DetailVisibility
    {
        [Description("No Details")]
        None,
        [Description("Single Line")]
        Single,
        [Description("All Details")]
        All
    }
}
