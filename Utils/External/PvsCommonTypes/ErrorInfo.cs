using System;
using System.Collections.Generic;

namespace ProgramVerificationSystems.PVSStudio.CommonTypes
{
    public sealed class ErrorInfo : ICloneable
    {
        //public String ErrorType; //(deprecated)
        public AnalyzerType AnalyzerType;
        public uint CodeCurrent;
        public uint CodeNext;
        public uint CodePrev;
        public String ErrorCode;
        public bool FalseAlarmMark;
        public String FileName = String.Empty;
        public bool IsRetired;
        public uint Level; // Possible values: 0 (analyzer errors), 1-3 (analyzer warnings)
        public int LineNumber;
        public string LineNumberExtension;
        public String Message = String.Empty;
        public List<String> ProjectNames = new List<string>();
        public bool TrialMode;

        object ICloneable.Clone()
        {
            // simply delegate to our type-safe cousin
            return Clone();
        }

        // Equals should not compare all of the class members
        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            ErrorInfo ei = obj as ErrorInfo;
            if ((Object)ei == null)
                return false;

            return LineNumber == ei.LineNumber &&
                   ErrorCode == ei.ErrorCode &&
                   (String.Compare(FileName, ei.FileName, StringComparison.OrdinalIgnoreCase) == 0) &&
                   (String.Compare(Message, ei.Message, StringComparison.OrdinalIgnoreCase) == 0) &&
                   (FalseAlarmMark == ei.FalseAlarmMark);
        }

        public ErrorInfo Clone()
        {
            // Start with a flat, memberwise copy
            ErrorInfo x = new ErrorInfo();
            x.ErrorCode = ErrorCode;
            x.TrialMode = TrialMode;
            x.LineNumber = LineNumber;
            x.FileName = FileName;
            //x.ErrorType = this.ErrorType; //(deprecated)
            x.Message = Message;
            x.FalseAlarmMark = FalseAlarmMark;
            x.Level = Level;
            x.CodePrev = CodePrev;
            x.CodeCurrent = CodeCurrent;
            x.CodeNext = CodeNext;
            x.AnalyzerType = AnalyzerType;
            x.ProjectNames = new List<string>(ProjectNames);
            x.LineNumberExtension = LineNumberExtension;
            x.IsRetired = IsRetired;

            return x;
        }

        public bool Equals(ErrorInfo ei)
        {
            return (object)ei != null && Equals((object)ei);
        }

        public static bool operator ==(ErrorInfo a, ErrorInfo b)
        {
            return ReferenceEquals(a, b) || ((object)a != null) && ((object)b != null) && a.Equals(b);
        }

        public static bool operator !=(ErrorInfo a, ErrorInfo b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ LineNumber.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", (ErrorCode ?? String.Empty), Message, FileName);
        }

        private static bool IsAnalyzerErrorCode(String errStr)
        {
            // Analyzer keyword mask: 
            // VXXX, where XXX - number. Examples: V101, V110
            // VXXXX, where XXXX - number. Examples: V1001, V1102
            if (errStr.Length != 4 && errStr.Length != 5)
                return false;

            if (errStr[0] != 'V' && errStr[0] != 'v')
                return false;
            for (Int32 i = 1; i <= 3; i++)
            {
                if (!Char.IsDigit(errStr[i]))
                    return false;
            }
            if (errStr.Length == 5)
            {
                if (!Char.IsDigit(errStr[4]))
                    return false;
            }
            return true;
        }

        public static uint GetHashCodePVS(String msg)
        {
            bool continueSearch = true;
            do
            {
                continueSearch = false;
                int index = msg.LastIndexOf("//-");
                if (index != -1)
                {
                    String subStr = msg.Substring(index + 3);
                    subStr = subStr.TrimEnd();
                    continueSearch = IsAnalyzerErrorCode(subStr);
                    if (continueSearch)
                    {
                        msg = msg.Remove(index);
                    }
                }
            } while (continueSearch);

            uint sum = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                char ch = msg[i];
                if (ch != ' ' && ch != '\t')
                {
                    bool hiBit = (sum & 0x80000000u) != 0;
                    sum <<= 1;
                    sum ^= ch;
                    if (hiBit)
                        sum ^= 0x00000001u;
                }
            }
            return sum;
        }
    }
}