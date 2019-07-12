using System.ComponentModel;

namespace LibraryCompare.Core.Enums
{
    public enum CompareResult
    {
        None = 0,
        Equal = 1,
        Left = 2,
        Right = 3,
        Diff = 4
    }

    public enum LibraryVersionState
    {
        InWork,
        Committed
    }

    [TypeConverter(typeof(EnumToStringUsingDescription))]
    public enum LibraryItemType
    {
        [Description("None")]
        Unknown,
        [Description("Faceplate")]
        Faceplate,
        [Description("C Script")]
        CScript,
        [Description("VB Script")]
        VbScript,
        [Description("Screen")]
        Screen,
        [Description("HMI Style")]
        HmiStyle,
        [Description("HMI Style Sheet")]
        HmiStyleSheet,
        [Description("HMI UDT")]
        HmiUdt,
        [Description("Block")]
        Block,
        [Description("PLC UDT")]
        PlcUdt
    }
}