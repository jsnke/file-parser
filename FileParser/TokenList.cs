using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser
{
    /// <summary>
    /// List of interesting tokens.
    /// </summary>
    class TokenList
    {
        /// <summary>
        /// Configure this token list according to the supplied file type.
        /// </summary>
        /// <param name="fileType">File type.</param>
        public TokenList(FileType fileType)
        {
            switch(fileType)
            {
                case FileType.Configuration:
                    this.LineComment = "#";
                    this.BlockCommentStart = null;
                    this.BlockCommentEnd = null;
                    this.BatchSplitMode = SplitMode.Line;
                    this.BatchSplit = null;
                    break;
                case FileType.SqlScript:
                    this.LineComment = "--";
                    this.BlockCommentStart = "/*";
                    this.BlockCommentEnd = "*/";
                    this.BatchSplitMode = SplitMode.Token;
                    this.BatchSplit = "GO";
                    break;
            }
        }
        /// <summary>
        /// Token for a line comment. Currently only one supported.
        /// </summary>
        public string LineComment { get; private set; }

        /// <summary>
        /// Flag to indicate if there is a line comment token.
        /// </summary>
        public bool HasLineComment
        {
            get
            {
                return !string.IsNullOrEmpty(this.LineComment);
            }
        }

        /// <summary>
        /// Comment block start token. Currently only one is supported.
        /// </summary>
        public string BlockCommentStart { get; private set; }

        /// <summary>
        /// Comment block end token. Currently only one is supported.
        /// </summary>
        public string BlockCommentEnd { get; private set; }

        /// <summary>
        /// Flag to indicate if there are comment block start and end tokens.
        /// </summary>
        public bool HasBlockComment
        {
            get
            {
                return !(string.IsNullOrEmpty(this.BlockCommentStart) || string.IsNullOrEmpty(this.BlockCommentStart));
            }
        }

        /// <summary>
        /// Token to split batches on.
        /// </summary>
        public string BatchSplit { get; private set; }

        /// <summary>
        /// Split mode to use.
        /// </summary>
        public SplitMode BatchSplitMode { get; private set; }

        /// <summary>
        /// Flag to indicate if blank lines should be preserved
        /// </summary>
        public bool PreserveBlankLines
        {
            get
            {
                return this.BatchSplitMode != SplitMode.Line;
            }
        }

        /// <summary>
        /// Batch split mode.
        /// </summary>
        public enum SplitMode
        {
            None,
            Line,
            Token
        }
    }
}
