using System;
using System.Collections.Generic;

namespace ProgramVerificationSystems.PlogConverter
{
    /// <summary>
    ///     Renderer interface
    /// </summary>
    public interface IPlogRenderer
    {
        /// <summary>
        ///     Render information
        /// </summary>
        RenderInfo RenderInfo { get; }

        /// <summary>
        ///     Errors to render
        /// </summary>
        IEnumerable<ErrorInfoAdapter> Errors { get; }

        /// <summary>
        ///     Renders plog-file
        /// </summary>
        void Render();

        /// <summary>
        ///     Callback handler on rendering completed
        /// </summary>
        event EventHandler<RenderCompleteEventArgs> RenderComplete;
    }

    public class RenderCompleteEventArgs : EventArgs
    {
        public string OutputFile { get; private set; }

        public RenderCompleteEventArgs(string outputFile)
        {
            OutputFile = outputFile;
        }
    }
}