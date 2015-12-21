using System.Collections.Generic;

namespace ProgramVerificationSystems.PlogConverter
{
    public sealed class CsvRow : List<string>
    {
        public string LineText { get; set; }
    }
}