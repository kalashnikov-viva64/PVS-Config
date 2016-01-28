using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace ProgramVerificationSystems.PlogConverter
{
    /// <summary>
    ///     Type for command line options
    /// </summary>
    public class CmdConverterOptions
    {
        /// <summary>
        ///     Absolute path to plog-file
        /// </summary>
        /// <example>--plog=c:\example\your_plog.plog</example>
        [Option('p', "plog", Required = true, HelpText = "Path to plog-file")]
        public string PlogPath { get; set; }

        /// <summary>
        ///     Destination directory for output files
        /// </summary>
        /// <example>--outputDir=c:\dest</example>
        [Option('o', "outputDir", Required = false, HelpText = "Output directory for the generated files")]
        public string OutputPath { get; set; }

        /// <summary>
        ///     Root path for source files
        /// </summary>
        /// <example>--srcRoot=c:\projects\solutionfolder</example>
        [Option('r', "srcRoot", Required = false, HelpText = "Root path for your source files")]
        public string SrcRoot { get; set; }

        /// <summary>
        ///     Filter by analyzer type with a list of levels
        /// </summary>
        /// <example>--analyzer=GA:1,2;64:1,2,3</example>
        [OptionList('a', "analyzer", Separator = ';', Required = false,
            HelpText = "Specifies analyzer(s) and level(s) to be used for filtering, i.e. GA:1,2;64:1;OP:1,2,3")]
        public IList<string> AnalyzerLevelFilter { get; set; }

        /// <summary>
        ///     Render types
        /// </summary>
        /// <example>--renderTypes=Html,Totals,Txt,Csv</example>
        [OptionList('t', "renderTypes", Separator = ',', Required = false,
            HelpText = "Render types for output. i.e. Html,Totals,Txt,Csv")]
        public IList<string> PlogRenderTypes { get; set; }

        /// <summary>
        ///     Error codes to disable
        /// </summary>
        /// <example>--excludedCodes=V101,V102,...V200</example>
        [OptionList('d', "excludedCodes", Separator = ',', Required = false, HelpText = "Error codes to disable, i.e. V101,V102,V103")]
        public IList<string> DisabledErrorCodes { get; set; }


        // email options
        /// <example>--emailList="Emails.lst"</example>
        [Option("emailList", DefaultValue = "Emails.lst")]
        public string EmailList { get; set; }
        /// <example>--header="[auto] PVS-Studio Analysis x64 Results"</example>
        [Option("header", DefaultValue = "")]
        public string Header { get; set; }
        /// <example>--server=smtp-20.1gb.ru</example>
        [Option("server", DefaultValue = "smtp-20.1gb.ru")]
        public string Server { get; set; }
        /// <example>--port=25</example>
        [Option("port", DefaultValue = 25)]
        public int Port { get; set; }
        /// <example>--smtpUser=szeharia</example>
        [Option("smtpUser", DefaultValue = "")]
        public string SmtpUser { get; set; }
        /// <example>--smtpPassword=123</example>
        [Option("smtpPassword", DefaultValue = "")]
        public string SmtpPassword { get; set; }
        /// <example>--fromAddress=szeharia@dalet.com</example>
        [Option("fromAddress", DefaultValue = "")]
        public string FromAddress { get; set; }
        /// <example>--autoEmail</example>
        [Option("autoEmail", DefaultValue = false,
            HelpText = "Turn on the automatic generation of an e-mail address by the name of the author")]
        public bool AutoEmail { get; set; }
        /// <example>--sendEmail</example>
        [Option("sendEmail", DefaultValue = false,
            HelpText = "Turn on the mail sending")]
        public bool SendEmail { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var helper = HelpText.AutoBuild(this);
            return helper;
        }
    }
}