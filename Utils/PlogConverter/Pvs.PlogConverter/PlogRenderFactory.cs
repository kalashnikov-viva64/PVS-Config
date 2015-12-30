using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using ProgramVerificationSystems.PlogConverter.Properties;
using ProgramVerificationSystems.PVSStudio.CommonTypes;

namespace ProgramVerificationSystems.PlogConverter
{
    /// <summary>
    ///     Output provider
    /// </summary>
    public sealed class PlogRenderFactory
    {
        private const string NoMessage = "No Messages Generated";

        private static readonly Comparison<ErrorInfoAdapter> DefaultSortStrategy = (first, second) =>
        {
            var anComp = first.ErrorInfo.AnalyzerType.CompareTo(second.ErrorInfo.AnalyzerType);
            var levelComp = first.ErrorInfo.Level.CompareTo(second.ErrorInfo.Level);
            return anComp != 0
                ? anComp
                : (levelComp != 0
                    ? levelComp
                    : string.Compare(first.ErrorInfo.FileName, second.ErrorInfo.FileName,
                        StringComparison.InvariantCultureIgnoreCase));
        };

        private readonly List<ErrorInfoAdapter> _errors;
        private readonly ParsedArguments _parsedArgs;

        public PlogRenderFactory(ParsedArguments parsedArguments)
        {
            _parsedArgs = parsedArguments;
            var errors = new List<ErrorInfoAdapter>(Utils.GetErrors(_parsedArgs.RenderInfo.Plog));

            if (_parsedArgs.LevelMap != null && _parsedArgs.LevelMap.Count > 0)
                errors = errors.Filter(_parsedArgs.LevelMap);

            if (_parsedArgs.DisabledErrorCodes != null && _parsedArgs.DisabledErrorCodes.Count > 0)
                errors = errors.Filter(_parsedArgs.DisabledErrorCodes);

            errors.Sort(DefaultSortStrategy);
            _errors = errors;
        }

        public IPlogRenderer GetRenderService(RenderType renderType, Action<RenderType, string> completedAction = null)
        {
            switch (renderType)
            {
                case RenderType.Html:
                    IPlogRenderer htmlRenderer = new HtmlPlogRenderer(_parsedArgs.RenderInfo, _errors.ExcludeFalseAlarms());
                    if (completedAction != null)
                        htmlRenderer.RenderComplete += (sender, args) => completedAction(renderType, args.OutputFile);
                    return htmlRenderer;

                case RenderType.Totals:
                    IPlogRenderer totalsRenderer = new PlogTotalsRenderer(_parsedArgs.RenderInfo, _errors.ExcludeFalseAlarms());
                    if (completedAction != null)
                        totalsRenderer.RenderComplete += (sender, args) => completedAction(renderType, args.OutputFile);
                    return totalsRenderer;

                case RenderType.Txt:
                    IPlogRenderer txtRenderer = new PlogTxtRenderer(_parsedArgs.RenderInfo, _errors.ExcludeFalseAlarms());
                    if (completedAction != null)
                        txtRenderer.RenderComplete += (sender, args) => completedAction(renderType, args.OutputFile);
                    return txtRenderer;

                case RenderType.Csv:
                    IPlogRenderer csvRenderer = new CsvRenderer(_parsedArgs.RenderInfo, _errors);
                    if (completedAction != null)
                        csvRenderer.RenderComplete += (sender, args) => completedAction(renderType, args.OutputFile);
                    return csvRenderer;

                default:
                    goto case RenderType.Html;
            }
        }

        #region Implementation for CSV Output

        private class CsvRenderer : IPlogRenderer
        {
            private const string CsvExt = "csv";

            public CsvRenderer(RenderInfo renderInfo, IEnumerable<ErrorInfoAdapter> errors)
            {
                RenderInfo = renderInfo;
                Errors = errors;
            }

            public RenderInfo RenderInfo { get; private set; }
            public IEnumerable<ErrorInfoAdapter> Errors { get; private set; }

            public void Render()
            {
                var destFolder = RenderInfo.OutputDir;
                var plogFilename = RenderInfo.Plog;
                var csvPath = Path.Combine(destFolder, string.Format("{0}.{1}", Path.GetFileName(plogFilename), CsvExt));

                using (var csvWriter = new CsvFileWriter(csvPath))
                {
                    var headerRow = new CsvRow();
                    headerRow.AddRange(new List<string>
                    {
                        "FavIcon",
                        "Default order",
                        "Level",
                        "Error code",
                        "Message",
                        "Project",
                        "Short file",
                        "Line",
                        "False alarm",
                        "File",
                        "Analyzer"
                    });
                    csvWriter.WriteRow(headerRow);

                    foreach (var error in Errors)
                    {
                        var messageRow = new CsvRow();
                        messageRow.AddRange(new List<string>
                        {
                            error.FavIcon.ToString(),
                            error.DefaultOrder.ToString(),
                            error.ErrorInfo.Level.ToString(),
                            error.ErrorInfo.ErrorCode,
                            error.ErrorInfo.Message,
                            error.Project,
                            error.ShortFile,
                            error.ErrorInfo.LineNumber.ToString(),
                            error.ErrorInfo.FalseAlarmMark.ToString(),
                            error.ErrorInfo.FileName,
                            error.ErrorInfo.AnalyzerType.ToString()
                        });

                        csvWriter.WriteRow(messageRow);
                    }
                }

                OnRenderComplete(new RenderCompleteEventArgs(csvPath));
            }

            public event EventHandler<RenderCompleteEventArgs> RenderComplete;

            private void OnRenderComplete(RenderCompleteEventArgs renderCompleteArgs)
            {
                var handler = RenderComplete;
                if (handler != null) handler(this, renderCompleteArgs);
            }
        }

        #endregion

        #region Implementation for Html Output

        private class HtmlWriter
        {
            public static readonly string _htmlExt = "html";
            public static readonly string _htmlFoot = Resources.HtmlFoot;
            public static readonly string _htmlHead = Resources.HtmlHead;

            public string _author;
            public string _htmlPath;
            public StreamWriter _writer;
            public bool _withHeader;
            public bool _withMsgType;

            public HtmlWriter(RenderInfo renderInfo, string author)
            {
                _author = author;
                _htmlPath = Path.Combine(renderInfo.OutputDir,
                    string.Format("{0}{1}.{2}", Path.GetFileName(renderInfo.Plog), (author != "all") ? "_" + author : "", _htmlExt));
                _writer = new StreamWriter(_htmlPath, false);
                _withHeader = false;
                _withMsgType = false;
            }

            public void AddHeader()
            {
                _writer.WriteLine(_htmlHead);
                _withHeader = true;
            }

            public void AddMsgType(AnalyzerType analyzerType)
            {
                _writer.WriteLine("<tr style='background: lightcyan;'>");
                _writer.WriteLine(
                    "<td colspan='5' style='color: red; text-align: center; font-size: 1.2em;'>{0}</td>",
                    Utils.GetDescription(analyzerType));
                _writer.WriteLine("</tr>");
                _withMsgType = true;
            }

            public void Close()
            {
                if (_writer.BaseStream != null)
                {
                    if (_withHeader)
                        _writer.WriteLine(_htmlFoot);
                    _writer.Close();
                }
            }

            ~HtmlWriter()
            {
                Close();
            }
        };

        private sealed class HtmlPlogRenderer : IPlogRenderer
        {
            public static readonly string _revisionLink = "http://gfn-portal/trac/changeset/{0}";
            public static readonly string _repoLink = "http://gfn-portal/trac/browser/branches/builds/4.0/src/{0}?rev={1}#L{2}";
            private Dictionary<string, HtmlWriter> _writers = new Dictionary<string, HtmlWriter>();

            public HtmlPlogRenderer(RenderInfo renderInfo, IEnumerable<ErrorInfoAdapter> errors)
            {
                RenderInfo = renderInfo;
                Errors = errors;
            }

            public RenderInfo RenderInfo { get; private set; }
            public IEnumerable<ErrorInfoAdapter> Errors { get; private set; }

            private void OpenFile(string author, bool withHeader, bool withMsgType, AnalyzerType analyzerType = AnalyzerType.Unknown)
            {
                if (_writers.ContainsKey(author))
                    return;
                try
                {
                    HtmlWriter htmlWriter = new HtmlWriter(RenderInfo, author);
                    if (withHeader)
                        htmlWriter.AddHeader();
                    if (withMsgType)
                        htmlWriter.AddMsgType(analyzerType);
                    _writers.Add(author, htmlWriter);
                }
                catch (Exception)
                {
                    ;
                }
            }

            private void CloseAll()
            {
                foreach(var pair in _writers)
                {
                    pair.Value.Close();
                }
            }

            public void SendEmails()
            {
                List<string> emptyEmails = new List<string>();
                List<string> adminEmails = new List<string>();
                if (SvnInfo.Instance.Emails.ContainsKey("admin"))
                {
                    adminEmails = SvnInfo.Instance.Emails["admin"];
                }
                foreach (var pair in _writers)
                {
                    string author = pair.Key;
                    string file = pair.Value._htmlPath;
                    if (SvnInfo.Instance.Emails.ContainsKey(author))
                    {
                        List<string> emails = SvnInfo.Instance.Emails[author];
                        Reporter.Instance.SendEmails(author, file, emails, (author != "all") ? adminEmails : emptyEmails);
                    }
                }
            }

            public void Render()
            {
                SvnInfo.Instance.ReadConfig();
                SvnInfo.Instance.AddAuthor("all");
                SvnInfo.Instance.AddAuthor("admin");
                if (Errors != null && Errors.Any())
                {
                    OpenFile("all", true, false);
                    WriteHtmlTable();
                }
                else
                {
                    OpenFile("all", false, false);
                    if (_writers.ContainsKey("all"))
                        _writers["all"]._writer.WriteLine("<h3>{0}</h3>", NoMessage);
                }
                CloseAll();
                SvnInfo.Instance.WriteConfig();
                SendEmails();
                _writers.Clear();
                OnRenderComplete(new RenderCompleteEventArgs(_writers.ContainsKey("all") ? _writers["all"]._htmlPath : ""));
            }

            public event EventHandler<RenderCompleteEventArgs> RenderComplete;

            private void WriteHtmlTable()
            {
                var groupedErrorInfoMap = GroupByErrorInfo(Errors);
                var analyzerTypes = groupedErrorInfoMap.Keys;
                foreach (var analyzerType in analyzerTypes)
                {
                    var groupedErrorInfo = groupedErrorInfoMap[analyzerType];

                    //int cnt = 0;
                    foreach (var error in groupedErrorInfo)
                    {
                        //if (++cnt > 13)
                        //    break;

                        SvnInfo.Instance.ParseBlame(error.ErrorInfo.FileName, error.ErrorInfo.LineNumber);
                        OpenFile(SvnInfo.Instance.Author, true, true, analyzerType);
                        
                        foreach (var pair in _writers)
                        {
                            HtmlWriter htmlWriter = pair.Value;
                            if (htmlWriter._author == "all" || htmlWriter._author == SvnInfo.Instance.Author)
                            {
                                WriteTableRow(htmlWriter._writer, error);
                            }
                        }
                    }
                }
            }

            private void OnRenderComplete(RenderCompleteEventArgs renderComplete)
            {
                var handler = RenderComplete;
                if (handler != null) handler(this, renderComplete);
            }

            private void WriteTableRow(TextWriter writer, ErrorInfoAdapter error)
            {
                var errorCode = HttpUtility.HtmlEncode(error.ErrorInfo.ErrorCode);
                var message = HttpUtility.HtmlEncode(error.ErrorInfo.Message);

                writer.WriteLine("<tr>");
                writer.Write("<td style='width: 10%;'><a href='{0}'>{1}</a></td>",
                    String.Format(_revisionLink, SvnInfo.Instance.Revision), SvnInfo.Instance.Revision);
                writer.Write("<td style='width: 10%;'>{0}</td>", SvnInfo.Instance.Author);
                writer.Write("<td style='width: 25%;'>");
                var fileName = error.ErrorInfo.FileName;

                if (fileName.StartsWith(Utils.SourceTreeRootMarker))
                {
                    fileName = fileName.Replace(Utils.SourceTreeRootMarker, RenderInfo.SrcRoot);
                }

                string caseSensFileName = "";
                if (SvnInfo.Instance.CaseSensFileName.IndexOf("s:\\src\\") == 0)
                {
                    caseSensFileName = SvnInfo.Instance.CaseSensFileName.Substring("s:\\src\\".Length);
                    caseSensFileName = caseSensFileName.Replace('\\', '/');
                }
                caseSensFileName = String.Format(_repoLink, caseSensFileName, SvnInfo.Instance.Revision, error.ErrorInfo.LineNumber);
                writer.WriteLine("<a href='{0}'>{1} ({2})</a>", caseSensFileName, Path.GetFileName(SvnInfo.Instance.CaseSensFileName),
                    error.ErrorInfo.LineNumber.ToString(CultureInfo.InvariantCulture));

                writer.WriteLine("</td>");
                writer.WriteLine("<td style='width: 5%;'><a href='http://www.viva64.com/en/{0}'>{0}</a></td>", errorCode);
                writer.WriteLine("<td style='width: 60%;'>{0}</td>", message);
                writer.WriteLine("</tr>");
            }

            private static IDictionary<AnalyzerType, IList<ErrorInfoAdapter>> GroupByErrorInfo(
                IEnumerable<ErrorInfoAdapter> errors)
            {
                IDictionary<AnalyzerType, IList<ErrorInfoAdapter>> groupedErrorInfoMap =
                    new SortedDictionary<AnalyzerType, IList<ErrorInfoAdapter>>(Comparer<AnalyzerType>.Default);
                var types = Utils.GetEnumValues<AnalyzerType>();
                foreach (var analyzerType in types)
                {
                    groupedErrorInfoMap.Add(analyzerType, new List<ErrorInfoAdapter>());
                }

                foreach (var error in errors)
                {
                    groupedErrorInfoMap[error.ErrorInfo.AnalyzerType].Add(error);
                }

                foreach (var analyzerType in types.Where(analyzerType => groupedErrorInfoMap[analyzerType].Count == 0))
                {
                    groupedErrorInfoMap.Remove(analyzerType);
                }

                return groupedErrorInfoMap;
            }
        }

        #endregion

        #region Implementation for Summary Output

        private sealed class PlogTotalsRenderer : IPlogRenderer
        {
            private const string TotalsSuffix = "_totals.txt";

            public PlogTotalsRenderer(RenderInfo renderInfo, IEnumerable<ErrorInfoAdapter> errors)
            {
                Errors = errors;
                RenderInfo = renderInfo;
            }

            public RenderInfo RenderInfo { get; private set; }
            public IEnumerable<ErrorInfoAdapter> Errors { get; private set; }

            public void Render()
            {
                var totalsPath = Path.Combine(RenderInfo.OutputDir,
                    string.Format("{0}{1}", Path.GetFileName(RenderInfo.Plog), TotalsSuffix));
                File.WriteAllText(totalsPath, Errors != null && Errors.Any() ? CalculateSummary() : NoMessage,
                    Encoding.UTF8);

                OnRenderComplete(new RenderCompleteEventArgs(totalsPath));
            }

            public event EventHandler<RenderCompleteEventArgs> RenderComplete;

            private void OnRenderComplete(RenderCompleteEventArgs renderComplete)
            {
                var handler = RenderComplete;
                if (handler != null) handler(this, renderComplete);
            }

            private string CalculateSummary()
            {
                var totalStat = new Dictionary<AnalyzerType, int[]>
                {
                    {AnalyzerType.Unknown, new[] {0, 0, 0}},
                    {AnalyzerType.General, new[] {0, 0, 0}},
                    {AnalyzerType.Optimization, new[] {0, 0, 0}},
                    //VivaMP is no longer supported by PVS-Studio, leaving for compatibility
                    {AnalyzerType.VivaMP, new[] {0, 0, 0}},
                    {AnalyzerType.Viva64, new[] {0, 0, 0}},
                    {AnalyzerType.CustomerSpecific, new[] {0, 0, 0}}
                };

                foreach (
                    var error in
                        Errors.Where(
                            error =>
                                !error.ErrorInfo.FalseAlarmMark && error.ErrorInfo.Level >= 1 &&
                                error.ErrorInfo.Level <= 3))
                {
                    totalStat[error.ErrorInfo.AnalyzerType][error.ErrorInfo.Level - 1]++;
                }

                var gaTotal = 0;
                for (var i = 0; i < totalStat[AnalyzerType.General].Length; i++)
                    gaTotal += totalStat[AnalyzerType.General][i];

                var opTotal = 0;
                for (var i = 0; i < totalStat[AnalyzerType.Optimization].Length; i++)
                    opTotal += totalStat[AnalyzerType.Optimization][i];

                var total64 = 0;
                for (var i = 0; i < totalStat[AnalyzerType.Viva64].Length; i++)
                    total64 += totalStat[AnalyzerType.Viva64][i];

                var csTotal = 0;
                for (var i = 0; i < totalStat[AnalyzerType.CustomerSpecific].Length; i++)
                    csTotal += totalStat[AnalyzerType.CustomerSpecific][i];

                int l1Total = 0, l2Total = 0, l3Total = 0;
                //VivaMP is no longer supported by PVS-Studio, leaving for compatibility
                //Not counting Unknown errors (fails) in total statistics
                foreach (
                    var stat in
                        totalStat.Where(stat => stat.Key != AnalyzerType.Unknown && stat.Key != AnalyzerType.VivaMP))
                {
                    l1Total += stat.Value[0];
                    l2Total += stat.Value[1];
                    l3Total += stat.Value[2];
                }

                var total = l1Total + l2Total + l3Total;
                return
                    string.Format(Resources.CommandLineTotals,
                        totalStat[AnalyzerType.General][0], totalStat[AnalyzerType.General][1],
                        totalStat[AnalyzerType.General][2], gaTotal,
                        totalStat[AnalyzerType.Optimization][0], totalStat[AnalyzerType.Optimization][1],
                        totalStat[AnalyzerType.Optimization][2], opTotal,
                        totalStat[AnalyzerType.Viva64][0], totalStat[AnalyzerType.Viva64][1],
                        totalStat[AnalyzerType.Viva64][2], total64,
                        totalStat[AnalyzerType.CustomerSpecific][0], totalStat[AnalyzerType.CustomerSpecific][1],
                        totalStat[AnalyzerType.CustomerSpecific][2], csTotal,
                        l1Total, l2Total, l3Total, total) + Environment.NewLine;
            }
        }

        #endregion

        #region Implementation for Text Output

        private sealed class PlogTxtRenderer : IPlogRenderer
        {
            private const string TxtExt = "txt";
            private static readonly string StringFiller = new string('=', 15);

            public PlogTxtRenderer(RenderInfo renderInfo, IEnumerable<ErrorInfoAdapter> errors)
            {
                RenderInfo = renderInfo;
                Errors = errors;
            }

            public RenderInfo RenderInfo { get; private set; }
            public IEnumerable<ErrorInfoAdapter> Errors { get; private set; }

            public void Render()
            {
                var logName = Path.GetFileName(RenderInfo.Plog);
                var destDir = RenderInfo.OutputDir;
                var txtPath = Path.Combine(destDir, string.Format("{0}.{1}", logName, TxtExt));

                using (
                    TextWriter txtWriter =
                        new StreamWriter(
                            new FileStream(txtPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite),
                            Encoding.Default))
                {
                    if (Errors != null && Errors.Any())
                    {
                        WriteText(txtWriter);
                    }
                    else
                    {
                        txtWriter.WriteLine(NoMessage);
                    }
                }

                OnRenderComplete(new RenderCompleteEventArgs(txtPath));
            }

            public event EventHandler<RenderCompleteEventArgs> RenderComplete;

            private void WriteText(TextWriter txtWriter)
            {
                var outputIndex = 0;
                var currentType = AnalyzerType.Unknown;
                foreach (var error in Errors)
                {
                    if (error.ErrorInfo.AnalyzerType != currentType)
                    {
                        currentType = error.ErrorInfo.AnalyzerType;
                        if (outputIndex != 0)
                        {
                            txtWriter.WriteLine();
                        }

                        txtWriter.WriteLine("{0}{1}{0}", StringFiller, Utils.GetDescription(currentType));
                    }

                    var message = GetOutput(error);
                    if (!message.EndsWith(Environment.NewLine))
                    {
                        message += Environment.NewLine;
                    }

                    txtWriter.Write(message);
                    outputIndex++;
                }
            }

            private void OnRenderComplete(RenderCompleteEventArgs renderComplete)
            {
                var handler = RenderComplete;
                if (handler != null) handler(this, renderComplete);
            }

            private string GetOutput(ErrorInfoAdapter error)
            {
                var fileName = error.ErrorInfo.FileName;
                if (fileName.StartsWith(Utils.SourceTreeRootMarker))
                {
                    fileName = fileName.Replace(Utils.SourceTreeRootMarker, RenderInfo.SrcRoot);
                }

                return error.ErrorInfo.Level >= 1 && error.ErrorInfo.Level <= 3
                    ? string.Format("{0} ({1}): error {2}: {3}{4}", fileName, error.ErrorInfo.LineNumber,
                        error.ErrorInfo.ErrorCode, error.ErrorInfo.Message, Environment.NewLine)
                    : error.ErrorInfo.Message;
            }
        }

        #endregion
    }
}