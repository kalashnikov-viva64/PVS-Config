using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using ProgramVerificationSystems.PVSStudio.CommonTypes;

namespace ProgramVerificationSystems.PlogConverter
{
    /// <summary>
    ///     Utilities
    /// </summary>
    internal static class Utils
    {
        public const string SourceTreeRootMarker = "|?|";
        private static readonly Encoding DefaultEncoding = Encoding.UTF8;

        public static string GetDescription<T>(T genObject)
        {
            var memberInfos = typeof(T).GetMember(genObject.ToString());
            if (memberInfos.Length < 1)
                return string.Empty;

            var descriptionAttributes =
                (DescriptionAttribute[])memberInfos[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descriptionAttributes.Length == 1 ? descriptionAttributes[0].Description : string.Empty;
        }

        public static T[] GetEnumValues<T>()
        {
            return typeof(T).IsEnum ? (T[])Enum.GetValues(typeof(T)) : new T[0];
        }

        public static IEnumerable<ErrorInfoAdapter> GetErrors(string plogFilename)
        {
            var plogXmlDocument = new XmlDocument();
            plogXmlDocument.LoadXml(File.ReadAllText(plogFilename, DefaultEncoding));
            var messagesElements = plogXmlDocument.GetElementsByTagName(DataTableNames.MessageTableName);
            return
                messagesElements.Cast<object>().Select((o, elIndex) => GetErrorInfo(messagesElements, elIndex)).ToList();
        }

        private static ErrorInfoAdapter GetErrorInfo(XmlNodeList messagesElements, int elIndex)
        {
            var messageNodes = messagesElements[elIndex].ChildNodes;
            var errorInfo = new ErrorInfoAdapter();

            for (var j = 0; j < messageNodes.Count; j++)
            {
                var nodeName = messageNodes[j].Name;
                var firstChildValue = messageNodes[j].HasChildNodes ? messageNodes[j].FirstChild.Value : string.Empty;
                SetErrorValue(nodeName, errorInfo, firstChildValue);
            }

            return errorInfo;
        }

        private static void SetErrorValue(string nodeName, ErrorInfoAdapter errorInfo, string firstChildValue)
        {
            switch (nodeName)
            {
                case DataColumnNames.ErrorListAnalyzer:
                    errorInfo.ErrorInfo.AnalyzerType = (AnalyzerType)Enum.Parse(typeof(AnalyzerType), firstChildValue);
                    break;
                case DataColumnNames.ErrorListCodeCurrent:
                    errorInfo.ErrorInfo.CodeCurrent = Convert.ToUInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListCodeNext:
                    errorInfo.ErrorInfo.CodeNext = Convert.ToUInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListCodePrev:
                    errorInfo.ErrorInfo.CodePrev = Convert.ToUInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListErrorCode:
                    errorInfo.ErrorInfo.ErrorCode = firstChildValue;
                    break;
                case DataColumnNames.ErrorListFalseAlarm:
                    errorInfo.ErrorInfo.FalseAlarmMark = Convert.ToBoolean(firstChildValue);
                    break;
                case DataColumnNames.ErrorListFile:
                    errorInfo.ErrorInfo.FileName = firstChildValue;
                    break;
                case DataColumnNames.ErrorListRetired:
                    errorInfo.ErrorInfo.IsRetired = Convert.ToBoolean(firstChildValue);
                    break;
                case DataColumnNames.ErrorListLevel:
                    errorInfo.ErrorInfo.Level = Convert.ToUInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListLine:
                    errorInfo.ErrorInfo.LineNumber = Convert.ToInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListLineExtension:
                    errorInfo.ErrorInfo.LineNumberExtension = string.Empty; // It's not necessary - just surrogate value
                    break;
                case DataColumnNames.ErrorListMessage:
                    errorInfo.ErrorInfo.Message = firstChildValue;
                    break;
                case DataColumnNames.ErrorListTrial:
                    errorInfo.ErrorInfo.TrialMode = Convert.ToBoolean(firstChildValue);
                    break;
                // Additional values
                case DataColumnNames.ErrorListFavIcon:
                    errorInfo.FavIcon = Convert.ToBoolean(firstChildValue);
                    break;
                case DataColumnNames.ErrorListOrder:
                    errorInfo.DefaultOrder = Convert.ToInt32(firstChildValue);
                    break;
                case DataColumnNames.ErrorListProject:
                    errorInfo.Project = firstChildValue;
                    break;
                case DataColumnNames.ErrorListShortFile:
                    errorInfo.ShortFile = firstChildValue;
                    break;
            }
        }

        public static bool TryParseLevelFilters(IEnumerable<string> analyzerLevels,
            IDictionary<AnalyzerType, ISet<uint>> analyzerLevelFilterMap, out string errorMessage)
        {
            foreach (
                var dtcTypeLevel in
                    analyzerLevels.Select(
                        levelFilter => levelFilter.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries)))
            {
                if (dtcTypeLevel.Length != 2)
                {
                    errorMessage = "Level filter was not specified";
                    return false;
                }

                var analyzerTypeString = dtcTypeLevel[0];
                bool success;
                var analyzerType = analyzerTypeString.ShortNameToType(out success);

                if (success)
                {
                    var levels = dtcTypeLevel[1];
                    if (string.IsNullOrWhiteSpace(levels))
                    {
                        errorMessage = "Levels are not set";
                        return false;
                    }

                    var typeLevels = levels.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (typeLevels.Length == 0)
                    {
                        errorMessage = "No levels were specified";
                        return false;
                    }

                    var parsedLevels = new HashSet<uint>();
                    foreach (var typeLevel in typeLevels)
                    {
                        uint parsedLevel;
                        if (uint.TryParse(typeLevel, out parsedLevel))
                        {
                            parsedLevels.Add(parsedLevel);
                        }
                        else
                        {
                            errorMessage = string.Format("Incorrect level: '{0}'. Level must be an integer value",
                                typeLevel);
                            return false;
                        }
                    }

                    analyzerLevelFilterMap.Add(analyzerType, parsedLevels);
                }
                else
                {
                    errorMessage = AvailableShortAnalyzerNames();
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static string AvailableShortAnalyzerNames()
        {
            var shortNamesBuilder = new StringBuilder("Incorrect analyzer type was specified. Possible values are: ");
            shortNamesBuilder.Append(String.Join(", ", GetAllShortNames()));
            return shortNamesBuilder.ToString();
        }

        private static string[] GetAllShortNames()
        {
            return
                Enum.GetValues(typeof(AnalyzerType))
                    .Cast<AnalyzerType>()
                    .Select(availableType => availableType.GetShortName())
                    .ToArray();
        }

        private static string GetShortName(this AnalyzerType analyzerType)
        {
            return
                analyzerType.GetType()
                    .GetField(analyzerType.ToString())
                    .GetCustomAttributes(typeof(XmlEnumAttribute), false)
                    .Cast<XmlEnumAttribute>()
                    .First()
                    .Name;
        }

        private static AnalyzerType ShortNameToType(this string shortName, out bool success)
        {
            if (string.IsNullOrWhiteSpace(shortName))
            {
                success = false;
                return AnalyzerType.Unknown;
            }

            shortName = shortName.ToLower();
            var shortNames = GetAllShortNames().Select(name => name.ToLower()).ToArray();
            var foundIndex = Array.FindIndex(shortNames, currentShortName => currentShortName == shortName);
            if (foundIndex == -1)
            {
                success = false;
                return AnalyzerType.Unknown;
            }

            var analyzerTypes = Enum.GetValues(typeof(AnalyzerType)).Cast<AnalyzerType>().ToArray();
            if (analyzerTypes.Length != shortNames.Length)
                throw new Exception("Not all analyzer types have a short name");

            success = true;
            return analyzerTypes[foundIndex];
        }

        public static bool TryParseRenderFilter(IEnumerable<string> plogRenderTypes, ISet<RenderType> renderTypes,
            out string errorMessage)
        {
            foreach (var unparsedRenderType in plogRenderTypes)
            {
                RenderType renderType;
                if (Enum.TryParse(unparsedRenderType, true, out renderType))
                {
                    renderTypes.Add(renderType);
                }
                else
                {
                    errorMessage = string.Format("Cannot parse render type with a name {0}", unparsedRenderType);
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    public static class ErrorsFilters
    {
        /// <summary>
        ///     Exclude errors marked as false alarms
        /// </summary>
        /// <param name="errors">Errors</param>
        /// <returns>Errors without false alarms</returns>
        public static List<ErrorInfoAdapter> ExcludeFalseAlarms(this IEnumerable<ErrorInfoAdapter> errors)
        {
            return errors.Where(error => !error.ErrorInfo.FalseAlarmMark).ToList();
        }

        /// <summary>
        ///     Filter errors by disabled error codes
        /// </summary>
        /// <param name="errors">Errors</param>
        /// <param name="disabledErrorCodes">Disabled error codes</param>
        /// <returns>Filtered errors</returns>
        public static List<ErrorInfoAdapter> Filter(this IEnumerable<ErrorInfoAdapter> errors,
            IEnumerable<string> disabledErrorCodes)
        {
            var errorCodes = disabledErrorCodes as string[] ?? disabledErrorCodes.ToArray();
            return (from error in errors
                    let errorCode = error.ErrorInfo.ErrorCode
                    let found =
                        errorCodes.Any(
                            disabledErrorCode =>
                                string.Equals(errorCode, disabledErrorCode, StringComparison.CurrentCultureIgnoreCase))
                    where !found
                    select error).ToList();
        }

        /// <summary>
        ///     Filter errors by analyzer types and levels
        /// </summary>
        /// <param name="errors">Errors</param>
        /// <param name="analyzerLevelMap">Filter map</param>
        /// <returns>Filtered errors</returns>
        public static List<ErrorInfoAdapter> Filter(this IList<ErrorInfoAdapter> errors,
            IDictionary<AnalyzerType, ISet<uint>> analyzerLevelMap)
        {
            return
                errors.GroupByAnalyzerType()
                    .FilterByAnalyzerTypes(analyzerLevelMap.Keys.ToArray())
                    .FilterByLevels(analyzerLevelMap)
                    .Transform();
        }

        private static List<ErrorInfoAdapter> Transform(
            this IDictionary<AnalyzerType, IList<ErrorInfoAdapter>> groupedErrors)
        {
            var filtered = new List<ErrorInfoAdapter>();
            foreach (var groupedError in groupedErrors)
            {
                filtered.AddRange(groupedError.Value);
            }

            return filtered;
        }

        private static IDictionary<AnalyzerType, IList<ErrorInfoAdapter>> FilterByLevels(
            this IDictionary<AnalyzerType, IList<ErrorInfoAdapter>> groupedErrors,
            IDictionary<AnalyzerType, ISet<uint>> analyzerLevelMap)
        {
            var filteredByLevels = new Dictionary<AnalyzerType, IList<ErrorInfoAdapter>>();

            foreach (var analyzerType in groupedErrors.Keys.ToArray())
            {
                var acceptedLevels = analyzerLevelMap[analyzerType];
                var analyzerErrors = groupedErrors[analyzerType];
                var filteredErrors =
                    analyzerErrors.Where(analyzerError => acceptedLevels.Contains(analyzerError.ErrorInfo.Level))
                        .ToList();
                if (filteredErrors.Count > 0)
                {
                    filteredByLevels.Add(analyzerType, filteredErrors);
                }
            }

            return filteredByLevels;
        }

        private static Dictionary<AnalyzerType, IList<ErrorInfoAdapter>> GroupByAnalyzerType(
            this IEnumerable<ErrorInfoAdapter> errors)
        {
            var groupedErrors = new Dictionary<AnalyzerType, IList<ErrorInfoAdapter>>();

            foreach (var error in errors)
            {
                var analyzerType = error.ErrorInfo.AnalyzerType;
                if (!groupedErrors.ContainsKey(analyzerType))
                {
                    groupedErrors.Add(analyzerType, new List<ErrorInfoAdapter>());
                }

                groupedErrors[analyzerType].Add(error);
            }

            return groupedErrors;
        }

        private static Dictionary<AnalyzerType, T> FilterByAnalyzerTypes<T>(
            this Dictionary<AnalyzerType, T> groupedErrors,
            AnalyzerType[] acceptedTypes)
        {
            var analyzerTypes = groupedErrors.Keys.ToArray();
            ISet<AnalyzerType> unnecessaryAnalyzerTypes = new HashSet<AnalyzerType>();
            foreach (var analyzerType in from analyzerType in analyzerTypes
                                         let found = acceptedTypes.Any(acceptedType => analyzerType == acceptedType)
                                         where !found
                                         select analyzerType)
            {
                unnecessaryAnalyzerTypes.Add(analyzerType);
            }

            foreach (var unnecessaryAnalyzerType in unnecessaryAnalyzerTypes)
            {
                groupedErrors.Remove(unnecessaryAnalyzerType);
            }

            return groupedErrors;
        }
    }
}