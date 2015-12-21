using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml.Serialization;

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    public static class DataTableUtils
    {
        public const uint PlogVersion = 3;
        public const char ProjectNameSeparator = '%';
        public const string TrialRestriction = "TRIAL RESTRICTION";

        static public ErrorInfo ConvertDataRowtoErrorInfo(DataRow row)
        {
            ErrorInfo ei = new ErrorInfo();
            ei.FileName = row.ItemArray[TableIndexes.File] as string;
            ei.LineNumber = (int)row.ItemArray[TableIndexes.Line];
            ei.ErrorCode = row.ItemArray[TableIndexes.ErrorCode] as string;
            ei.CodeCurrent = (uint)row.ItemArray[TableIndexes.CodeCurrent];
            ei.CodeNext = (uint)row.ItemArray[TableIndexes.CodeNext];
            ei.CodePrev = (uint)row.ItemArray[TableIndexes.CodePrev];
            //ei.ErrorType = row.ItemArray[TableIndexes.Type] as string; //deprecated
            ei.FalseAlarmMark = (bool)row.ItemArray[TableIndexes.FalseAlarm];
            ei.Level = (uint)row.ItemArray[TableIndexes.Level];
            ei.Message = row.ItemArray[TableIndexes.Message] as string;
            ei.TrialMode = (bool)row.ItemArray[TableIndexes.Trial];
            ei.AnalyzerType = (AnalyzerType)row.ItemArray[TableIndexes.Analyzer];
            ei.LineNumberExtension = row.ItemArray[TableIndexes.LineExtension] as string;
            ei.IsRetired = (bool)row.ItemArray[TableIndexes.Retired];

            String projectNames = row.ItemArray[TableIndexes.Project] as string;
            if (!String.IsNullOrEmpty(projectNames))
                ei.ProjectNames = new List<string>(projectNames.Split(ProjectNameSeparator));
            else
                ei.ProjectNames = new List<string>();

            return ei;
        }

        static public List<ErrorInfo> TransformTableToList(DataTable table)
        {
            return TransformTableToList(table, false);
        }

        static public List<ErrorInfo> TransformTableToList(DataTable table, bool excludeFalseAlarms)
        {
            List<ErrorInfo> infos = new List<ErrorInfo>();
            foreach (DataRow row in table.Rows)
            {
                ErrorInfo ei = ConvertDataRowtoErrorInfo(row);
                if (excludeFalseAlarms && ei.FalseAlarmMark)
                    continue;

                infos.Add(ei);
            }

            return infos;
        }

        static public bool AppendErrorInfoToDataTable(DataTable table, ErrorInfo errorInfo, ApplicationSettings settings)
        {
            object[] Mkeys = { errorInfo.LineNumber, errorInfo.Message, errorInfo.FileName };
            //filters existing messages for duplicates before adding new item
            if (!table.Rows.Contains(Mkeys))
            {
                DataRow rw = table.NewRow();

                rw[TableIndexes.FavIcon] = false;
                rw[TableIndexes.Level] = errorInfo.Level;
                rw[TableIndexes.Order] = table.Rows.Count + 1;
                rw[TableIndexes.Line] = errorInfo.LineNumber;
                if (!errorInfo.TrialMode)
                    rw[TableIndexes.File] = String.Intern(errorInfo.FileName);
                else
                    rw[TableIndexes.File] = table.Rows.Count + 1;

                String projectNames = String.Empty;
                if (errorInfo.ProjectNames != null)
                    foreach (String projectName in errorInfo.ProjectNames)
                        projectNames += projectName + DataTableUtils.ProjectNameSeparator;

                rw[TableIndexes.Project] = String.Intern(projectNames.Trim(DataTableUtils.ProjectNameSeparator));
                //rw[TableIndexes.Type] = errorInfo.ErrorType; //(deprecated)
                rw[TableIndexes.ErrorCode] = String.Intern(errorInfo.ErrorCode);
                rw[TableIndexes.Message] = String.Intern(errorInfo.Message);
                rw[TableIndexes.FalseAlarm] = errorInfo.FalseAlarmMark;
                rw[TableIndexes.CodePrev] = errorInfo.CodePrev;
                rw[TableIndexes.CodeCurrent] = errorInfo.CodeCurrent;
                rw[TableIndexes.CodeNext] = errorInfo.CodeNext;
                rw[TableIndexes.Trial] = errorInfo.TrialMode;
                rw[TableIndexes.FalseAlarm] = errorInfo.FalseAlarmMark;
                rw[TableIndexes.Analyzer] = (int)errorInfo.AnalyzerType;
                rw[TableIndexes.LineExtension] = errorInfo.LineNumberExtension;
                rw[TableIndexes.Retired] = errorInfo.IsRetired;

                if (!errorInfo.TrialMode)
                    rw[TableIndexes.ShortFile] = String.Intern(Path.GetFileName(settings.TransformPathToAbsolute(errorInfo.FileName)));
                else
                    rw[TableIndexes.ShortFile] = TrialRestriction;

                table.Rows.Add(rw);
                return true;
            }
            else
            {
                DataRow existingRow = table.Rows.Find(Mkeys);
                if (existingRow != null)
                {
                    String projectNames = existingRow[TableIndexes.Project] as String;
                    if (!String.IsNullOrEmpty(projectNames))
                    {
                        foreach (String projectNameToAdd in errorInfo.ProjectNames)
                        {
                            bool projectNamePresent = false;
                            foreach (String projectName in projectNames.Split(ProjectNameSeparator))
                                if (projectName.Equals(projectNameToAdd))
                                {
                                    projectNamePresent = true;
                                    break;
                                }

                            if (!projectNamePresent)
                                projectNames += ProjectNameSeparator + projectNameToAdd;
                        }

                        existingRow[TableIndexes.Project] = projectNames;
                    }
                }

                return false;
            }
        }

        static public void CreatePVSDataTable(DataTable messageTable, DataTable metaTable, DataView View)
        {
            metaTable.TableName = DataTableNames.MetaTableName;
            metaTable.Columns.Add(DataColumnNames.SolutionPath, typeof(string));                      //0
            metaTable.Columns.Add(DataColumnNames.SolutionVer, typeof(string));                       //1
            metaTable.Columns.Add(DataColumnNames.PlogVersion, typeof(uint));                         //2
            DataColumn col = new DataColumn(DataColumnNames.PlogModificationDate, typeof(DateTime));//3
            col.DateTimeMode = DataSetDateTime.Utc;
            metaTable.Columns.Add(col);

            messageTable.TableName = DataTableNames.MessageTableName;
            //rows visible in grid
            messageTable.Columns.Add(DataColumnNames.ErrorListFavIcon, typeof(bool));                      //0
            messageTable.Columns.Add(DataColumnNames.ErrorListLevel, typeof(uint));                        //1
            messageTable.Columns.Add(DataColumnNames.ErrorListOrder, typeof(int));                         //2
            messageTable.Columns.Add(DataColumnNames.ErrorListErrorCode, typeof(string));                  //3
            messageTable.Columns.Add(DataColumnNames.ErrorListMessage, typeof(string));                    //4
            messageTable.Columns.Add(DataColumnNames.ErrorListProject, typeof(string));                    //5
            messageTable.Columns.Add(DataColumnNames.ErrorListShortFile, typeof(string));                  //6
            messageTable.Columns.Add(DataColumnNames.ErrorListLine, typeof(int));                          //7
            messageTable.Columns.Add(DataColumnNames.ErrorListFalseAlarm, typeof(bool));                   //8
                                                                                                           //invisible rows
            messageTable.Columns.Add(DataColumnNames.ErrorListFile, typeof(string));                       //9
            messageTable.Columns.Add(DataColumnNames.ErrorListCodePrev, typeof(uint));                     //10
            messageTable.Columns.Add(DataColumnNames.ErrorListCodeCurrent, typeof(uint));                  //11
            messageTable.Columns.Add(DataColumnNames.ErrorListCodeNext, typeof(uint));                     //12
            messageTable.Columns.Add(DataColumnNames.ErrorListTrial, typeof(bool));                        //13
            messageTable.Columns.Add(DataColumnNames.ErrorListAnalyzer, typeof(int));                      //14
            messageTable.Columns.Add(DataColumnNames.ErrorListLineExtension, typeof(string));              //15
            messageTable.Columns.Add(DataColumnNames.ErrorListRetired, typeof(bool)).DefaultValue = false; //16

            SetPrimaryKey(messageTable);

            if (View != null)
                View.AllowNew = false;
        }

        static public void UpgradePlog(DataTable errorsTable, DataTable metaTable, String plogPath)
        {
            uint versionToUpgrade = (uint)metaTable.Rows[0][DataColumnNames.PlogVersion];

            if (versionToUpgrade < 3)
            {
                //в plog'ах до версии 3 отсутствовала информация о дате модификации.

                DateTime modTime = DateTime.UtcNow;
                if (File.Exists(plogPath))
                    modTime = File.GetLastWriteTimeUtc(plogPath);

                metaTable.Rows[0][DataColumnNames.PlogModificationDate] = modTime;
            }

            //устанавливаем новую версию
            metaTable.Rows[0][DataColumnNames.PlogVersion] = PlogVersion;
        }

        static public object[] GetPrimaryKey(DataRow row)
        {
            return new object[] {row[TableIndexes.Line],
                                 row[TableIndexes.Message],
                                 row[TableIndexes.File] };
        }

        static public void SetPrimaryKey(DataTable table)
        {
            DataColumn[] keys = new DataColumn[3];
            keys[0] = table.Columns[TableIndexes.Line];
            keys[1] = table.Columns[TableIndexes.Message];
            keys[2] = table.Columns[TableIndexes.File];
            table.PrimaryKey = keys;
        }
    }
}