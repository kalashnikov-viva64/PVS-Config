using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ProgramVerificationSystems.PVSStudio.CommonTypes;
using CmdParser = CommandLine.Parser;

namespace ProgramVerificationSystems.PlogConverter
{
    internal static class Program
    {
        private static readonly TextWriter DefaultWriter = Console.Out;
        private static readonly string DefaultOutputFolder = Environment.CurrentDirectory;
        private static readonly string NewLine = Environment.NewLine;
        
        private static int Main(string[] args)
        {
            try
            {
                // Accepting command-line arguments
                var parsedArgs = new ParsedArguments { RenderInfo = new RenderInfo() };
                string errorMessage;
                var success = AcceptArguments(args, ref parsedArgs, out errorMessage);
                if (!success)
                {
                    Log(DefaultWriter, errorMessage);
                    return 1;
                }

                var renderFactory = new PlogRenderFactory(parsedArgs);

                var acceptedRenderTypes = parsedArgs.RenderTypes != null && parsedArgs.RenderTypes.Count > 0
                    ? parsedArgs.RenderTypes.ToArray()
                    : Utils.GetEnumValues<RenderType>();
                var renderTasks = new Task[acceptedRenderTypes.Length];
                for (var index = 0; index < renderTasks.Length; index++)
                {
                    var closureIndex = index;
                    renderTasks[index] =
                        Task.Factory.StartNew(
                            () =>
                                renderFactory.GetRenderService(acceptedRenderTypes[closureIndex],
                                    (renderType, path) =>
                                        DefaultWriter.WriteLine("{0} output was saved to {1}",
                                            Enum.GetName(typeof (RenderType), renderType), path)).Render());
                }

                Task.WaitAll(renderTasks);
                return 0;
            }
            catch (AggregateException aggrEx)
            {
                var baseEx = aggrEx.GetBaseException();
                Log(DefaultWriter, baseEx.ToString());

                return 1;
            }
            catch (Exception ex)
            {
                Log(DefaultWriter, ex.ToString());

                return 1;
            }
        }

        private static void Log(TextWriter textWriter, string message)
        {
            textWriter.WriteLine(message);
        }

        private static bool AcceptArguments(string[] args, ref ParsedArguments parsedArgs, out string errorMessage)
        {
            var converterOptions = new CmdConverterOptions();
            var parser = new CmdParser(parsingSettings =>
            {
                parsingSettings.HelpWriter = Console.Error;
                parsingSettings.IgnoreUnknownArguments = false;
            });

            if (!parser.ParseArgumentsStrict(args, converterOptions))
            {
                errorMessage = converterOptions.GetUsage();
                return false;
            }

            if (!File.Exists(converterOptions.PlogPath))
            {
                errorMessage = string.Format("File '{0}' does not exist{1}{2}", converterOptions.PlogPath, NewLine,
                    converterOptions.GetUsage());
                return false;
            }

            Reporter.Instance.Header = converterOptions.Header;
            Reporter.Instance.Server = converterOptions.Server;
            Reporter.Instance.Port = converterOptions.Port;
            Reporter.Instance.SmtpUser = converterOptions.SmtpUser;
            Reporter.Instance.SmtpPassword = converterOptions.SmtpPassword;
            Reporter.Instance.FromAddress = converterOptions.FromAddress;
            Reporter.Instance.SendEmail = converterOptions.SendEmail;
            SvnInfo.Instance.AutoEmail = converterOptions.AutoEmail;

            parsedArgs.RenderInfo.Plog = converterOptions.PlogPath;
            parsedArgs.RenderInfo.OutputDir = converterOptions.OutputPath ?? DefaultOutputFolder;

            if (!Directory.Exists(parsedArgs.RenderInfo.OutputDir))
            {
                errorMessage = string.Format("Output directory '{0}' does not exist{1}{2}", converterOptions.OutputPath,
                    NewLine, converterOptions.GetUsage());
                return false;
            }            
            
            parsedArgs.RenderInfo.SrcRoot = converterOptions.SrcRoot;

            // Getting a map for levels by the analyzer type
            IDictionary<AnalyzerType, ISet<uint>> analyzerLevelFilterMap = new Dictionary<AnalyzerType, ISet<uint>>();
            if (converterOptions.AnalyzerLevelFilter != null && converterOptions.AnalyzerLevelFilter.Count > 0 &&
                !Utils.TryParseLevelFilters(converterOptions.AnalyzerLevelFilter, analyzerLevelFilterMap,
                    out errorMessage))
            {
                return false;
            }

            parsedArgs.LevelMap = analyzerLevelFilterMap;

            // Getting render types
            ISet<RenderType> renderTypes = new HashSet<RenderType>();
            if (converterOptions.PlogRenderTypes != null && converterOptions.PlogRenderTypes.Count > 0 &&
                !Utils.TryParseRenderFilter(converterOptions.PlogRenderTypes, renderTypes, out errorMessage))
            {
                return false;
            }

            parsedArgs.RenderTypes = renderTypes;
            parsedArgs.DisabledErrorCodes = converterOptions.DisabledErrorCodes;

            errorMessage = string.Empty;
            return true;
        }
    }

    public class ParsedArguments
    {
        public RenderInfo RenderInfo { get; set; }
        public IDictionary<AnalyzerType, ISet<uint>> LevelMap { get; set; }
        public ISet<RenderType> RenderTypes { get; set; }
        public IList<string> DisabledErrorCodes { get; set; }
    }

    public class RenderInfo
    {
        public string Plog { get; set; }
        public string OutputDir { get; set; }
        public string SrcRoot { get; set; }
    }
}