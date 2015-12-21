using System;
using ProgramVerificationSystems.PVSStudio.CommonTypes;

namespace ProgramVerificationSystems.PlogConverter
{
    public sealed class ErrorInfoAdapter : IEquatable<ErrorInfoAdapter>
    {
        public ErrorInfoAdapter()
            : this(new ErrorInfo())
        {
        }

        private ErrorInfoAdapter(ErrorInfo errorInfo)
        {
            ErrorInfo = errorInfo;
        }

        public ErrorInfo ErrorInfo { get; private set; }

        public bool Equals(ErrorInfoAdapter other)
        {
            return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || ErrorInfo.Equals(other.ErrorInfo));
        }

        public override bool Equals(object obj)
        {
            return obj is ErrorInfoAdapter && Equals((ErrorInfoAdapter)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ErrorInfo.GetHashCode();
            }
        }

        public static bool operator ==(ErrorInfoAdapter left, ErrorInfoAdapter right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ErrorInfoAdapter left, ErrorInfoAdapter right)
        {
            return !Equals(left, right);
        }

        #region Additional properties for adoption

        public bool FavIcon { get; set; }
        public int DefaultOrder { get; set; }
        public string Project { get; set; }
        public string ShortFile { get; set; }

        #endregion
    }
}