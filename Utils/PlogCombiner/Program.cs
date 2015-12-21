// --------------------------------------------------------
// Utility for combining plog-files of PVS-Studio.       
//
// Usage: PlogCombiner.exe <plogFile> <numParts>
// <plogFile> - plog-file of PVS-Studio
// <numParts> - number of parts
// --------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using ProgramVerificationSystems.PVSStudio;
using ProgramVerificationSystems.PVSStudio.CommonTypes;

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    public class ApplicationSettings {
        public String TransformPathToAbsolute(String RelativePath) { return RelativePath; }
    };
}

namespace PlogCombiner
{
    class Program
    {
        static public bool ReadPlog(String filename, DataTable messageTable, DataTable metaTable, out String lastError)
        {
            lastError = "";

            if (!File.Exists(filename))
            {
                lastError = String.Format("Cannot find file: {0}", filename); 
                return false;
            }

            // Чтение файла
            String fullTableFilter = messageTable.DefaultView.RowFilter;
            XmlReadMode readMode = XmlReadMode.Auto;
            DataSet dataset = new DataSet();
            dataset.Tables.Add(metaTable);
            dataset.Tables.Add(messageTable);
            readMode = dataset.ReadXml(filename);
            dataset.Tables.Clear();
            //При добавлении таблицы в DataSet у ней сбрасывается DataView
            messageTable.DefaultView.RowFilter = fullTableFilter;
            messageTable.DefaultView.AllowNew = false;

            if (readMode == XmlReadMode.IgnoreSchema)
            {
                lastError = "Incorrect log format";
                return false;
            }

            // Проверка версии
            int version = 0;
            object plogVersion = metaTable.Rows[0][DataColumnNames.PlogVersion];
            int.TryParse(plogVersion.ToString(), out version);
            if (version <= 0 || version > DataTableUtils.PlogVersion)
            {
                lastError = "Incorrect version of plog-file";
                return false;
            }
            else if (version > 0 && version < DataTableUtils.PlogVersion)
                DataTableUtils.UpgradePlog(messageTable, metaTable, filename);

            return true;
        }

        static public void WritePlog(String fileName, DataTable messageTable, DataTable metaTable)
        {
            // Запись в файл
            String fullTableFilter = messageTable.DefaultView.RowFilter;
            DataSet dataset = new DataSet();
            dataset.Tables.Add(metaTable);
            dataset.Tables.Add(messageTable);
            dataset.WriteXml(fileName, XmlWriteMode.WriteSchema);
            dataset.Tables.Clear();
            //При добавлении таблицы в DataSet у ней сбрасывается DataView
            messageTable.DefaultView.RowFilter = fullTableFilter;
            messageTable.DefaultView.AllowNew = false;
        }

        static public bool SplitSlnPath(String slnPath, out String slnBase, out String slnExt, out int part)
        {
            slnBase = slnExt = "";
            part = 0;

            int extIndex = slnPath.LastIndexOf('.');
            if (extIndex < 0)
                return false;
            slnBase = slnPath.Substring(0, extIndex);
            slnExt = slnPath.Substring(extIndex);
            
            int partIndex = slnBase.LastIndexOf('_');
            if (partIndex < 0 || !int.TryParse(slnBase.Substring(partIndex + 1), out part))
                return false;
            slnBase = slnBase.Substring(0, partIndex);
            
            return true;
        }
        
        static void Main(string[] args)
        {
            String plogFile = "";
            int numParts = 0;
            if (args.Length >= 2)
            {
                plogFile = args[0];
                int.TryParse(args[1], out numParts);
            }

            System.Console.WriteLine("Utility for combining plog-files of PVS-Studio.");
            if (args.Length != 2 || numParts <= 0 || numParts > 10)
            {
                System.Console.WriteLine(
                    "(x) The program was started incorrectly!\n\n" +
                    "Usage: PlogCombiner.exe <plogFile> <numParts>\n" +
                    "<plogFile> - plog-file of PVS-Studio\n" +
                    "<numParts> - number of parts");
                Environment.Exit(1);
            }
            
            DataTable messageTable = new DataTable();
            DataTable metaTable = new DataTable();
            DataTableUtils.CreatePVSDataTable(messageTable, metaTable, messageTable.DefaultView);
            ApplicationSettings settings = new ApplicationSettings();

            bool bHeader = false;
            for (int i = 1; i <= numParts; i++)
            {
                DataTable partMessageTable = new DataTable();
                DataTable partMetaTable = new DataTable();
                DataTableUtils.CreatePVSDataTable(partMessageTable, partMetaTable, partMessageTable.DefaultView);
                String partFile = String.Format(plogFile, "_"+i);
                String lastError;

                if (!File.Exists(partFile))
                    continue;

                if (!ReadPlog(partFile, partMessageTable, partMetaTable, out lastError))
                {
                    System.Console.WriteLine(lastError);
                    Environment.Exit(1);
                }

                if (!bHeader)
                {
                    bHeader = true;
                    metaTable = partMetaTable.Copy();
                    String slnPath = metaTable.Rows[0][DataColumnNames.SolutionPath] as String;
                    String slnBase, slnExt;
                    int part;
                    if (!SplitSlnPath(slnPath, out slnBase, out slnExt, out part))
                    {
                        System.Console.WriteLine(String.Format("Invalid solution path: {0}", slnPath));
                        Environment.Exit(1);
                    }
                    metaTable.Rows[0][DataColumnNames.SolutionPath] = slnBase + slnExt;
                    metaTable.Rows[0][DataColumnNames.PlogModificationDate] = DateTime.UtcNow.ToString(@"yyyy/MM/ddThh:mm:ss.fffffffZ");
                }

                List<ErrorInfo> lstPartMessages = DataTableUtils.TransformTableToList(partMessageTable);
                foreach(ErrorInfo msg in lstPartMessages)
                {
                    DataTableUtils.AppendErrorInfoToDataTable(messageTable, msg, settings);
                }
            }

            // Запись в файл
            String resFile = String.Format(plogFile, "");
            WritePlog(resFile, messageTable, metaTable);
        }
    }
}
