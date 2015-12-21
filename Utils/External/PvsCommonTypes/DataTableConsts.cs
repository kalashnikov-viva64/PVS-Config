using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    //VivaMP is no longer supported
    //leaving for compatibility
    public enum AnalyzerType
    {
        [Description("Fail")]
        [XmlEnum(Name = "Fail")]
        Unknown = 0,

        [Description("64 bit (64)")]
        [XmlEnum(Name = "64")]
        Viva64 = 1,

        [Description("Open MP (MP)")]
        [XmlEnum(Name = "MP")]
        VivaMP = 2,

        [Description("General Analysis (GA)")]
        [XmlEnum(Name = "GA")]
        General = 4,

        [Description("Optimization (OP)")]
        [XmlEnum(Name = "OP")]
        Optimization = 5,

        [Description("Customer Specific (CS)")]
        [XmlEnum(Name = "CS")]
        CustomerSpecific = 6
    }

    [Flags]
    public enum AnalyzerMode
    {
        Viva64 = 0x001,
        VivaMP = 0x002,
        GA = 0x004,
        OP = 0x008,
        CS = 0x010
    }

    public static class DataColumnNames
    {
        public const string ErrorListAnalyzer = "Analyzer";
        public const string ErrorListAnalyzerMessage = "AnalyzerMessage";
        public const string ErrorListCategory = "Category";
        public const string ErrorListCodeCurrent = "CodeCurrent";
        public const string ErrorListCodeNext = "CodeNext";
        public const string ErrorListCodePrev = "CodePrev";
        public const string ErrorListErrorCode = "ErrorCode";
        public const string ErrorListFalseAlarm = "FalseAlarm";
        public const string ErrorListFile = "File";
        public const string ErrorListLevel = "Level";
        public const string ErrorListLine = "Line";
        public const string ErrorListMessage = "Message";
        public const string ErrorListOrder = "DefaultOrder";
        public const string ErrorListShortFile = "ShortFile";
        public const string ErrorListTrial = "TrialMessage";
        public const string ErrorListProject = "Project";
        public const string SolutionPath = "SolutionPath";
        public const string SolutionVer = "SolutionVersion";
        public const string PlogVersion = "PlogVersion";
        public const string PlogModificationDate = "ModificationDate";
        public const string ErrorListFavIcon = "FavIcon";
        public const string ErrorListLineExtension = "LineExtension";
        public const string ErrorListRetired = "Retired";

        //Header identifiers
        public const string HeaderFavIcon = "Header_FavIcon";
        public const string HeaderFalseAlarm = "Header_FalseAlarm";
    }

    public static class DataTableNames
    {
        public const string MetaTableName = "Solution_Path";
        public const string MessageTableName = "PVS-Studio_Analysis_Log";
    }

    /// <summary>
    ///     Индексы в таблице m_Errors_Table (типа DataTable)
    /// </summary>
    public static class TableIndexes
    {
        public const int FavIcon = 0;
        public const int Level = 1;
        public const int Order = 2;
        public const int ErrorCode = 3;
        public const int Message = 4;
        public const int Project = 5;
        public const int ShortFile = 6;
        public const int Line = 7;
        public const int FalseAlarm = 8;
        public const int File = 9;
        public const int CodePrev = 10;
        public const int CodeCurrent = 11;
        public const int CodeNext = 12;
        public const int Trial = 13;
        public const int Analyzer = 14;
        public const int LineExtension = 15;
        public const int Retired = 16;
    }
}