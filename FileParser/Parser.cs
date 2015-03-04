using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileParser
{
    /// <summary>
    /// Parses files to strip out comments and break the files into batches. In
    /// configuration file type mode each line is a separate batch, and in SQL
    /// script mode each block seperated by the GO keyword is a batch.
    /// 
    /// In a configuration file comments are single line and start with #. In a
    /// SQL script file comments can be single line starting with --, or block
    /// comments using /* */. Comments must start and end on their own lines and
    /// have noting but whitespace before or after them.
    /// </summary>
    public class Parser
    {
        private FileType File { get; set; }
        private List<string> FileBatches { get; set; }

        /// <summary>
        /// An array containing batches from the parsed file. This will be empty
        /// until the file has been parsed.
        /// </summary>
        public string[] Batches
        {
            get { return this.FileBatches.ToArray(); }
        }
        
        /// <summary>
        /// Build a file parser for the specified type of file.
        /// </summary>
        /// <param name="fileType">Type of file.</param>
        public Parser(FileType fileType)
        {
            this.File = fileType;
            this.FileBatches = new List<string>();
        }

        /// <summary>
        /// Parses the text from the supplied stream and stores the resulting batches
        /// in the Batches property.
        /// </summary>
        /// <param name="stream">Text stream to parse.</param>
        public void Parse(Stream stream)
        {
            // We're starting a parse of the file, clear the batches
            this.FileBatches.Clear();

            var currentBatch = new StringBuilder();
            var inBlockComment = false;
            var tokens = new TokenList(this.File);

            using (var reader = new StreamReader(stream))
            {
                // Process the file by line
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    // If the line is blank and we don't want to preserve it then move on
                    if (!tokens.PreserveBlankLines && line.Trim().Length == 0)
                    {
                        continue;
                    }

                    // Remove single line comments
                    if (tokens.HasLineComment && !inBlockComment)
                    {
                        if (line.TrimStart().StartsWith(tokens.LineComment, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // This is a single line comment, skip it
                            continue;
                        }
                    }

                    // Block comments up next
                    if (tokens.HasBlockComment)
                    {
                        if (line.TrimStart().StartsWith(tokens.BlockCommentStart, StringComparison.CurrentCultureIgnoreCase) && !inBlockComment)
                        {
                            // This is the start of a block comment, we don't skip here because this might be a single line block comment
                            inBlockComment = true;
                        }

                        if (line.TrimEnd().EndsWith(tokens.BlockCommentEnd, StringComparison.CurrentCultureIgnoreCase) && inBlockComment)
                        {
                            // End of a block comment, skip this line
                            inBlockComment = false;
                            continue;
                        }

                        if (inBlockComment)
                        {
                            continue;
                        }
                    }

                    // We have an interesting line
                    if (tokens.BatchSplitMode == TokenList.SplitMode.Line)
                    {
                        this.FileBatches.Add(line);
                    }
                    else if (tokens.BatchSplitMode == TokenList.SplitMode.Token &&
                        line.TrimStart().StartsWith(tokens.BatchSplit, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // End of the batch
                        this.FileBatches.Add(currentBatch.ToString());
                        currentBatch.Clear();
                    }
                    else
                    {
                        currentBatch.AppendFormat("{0}{1}", currentBatch.Length > 0 ? Environment.NewLine : string.Empty, line);
                    }
                
                }

                if (currentBatch.Length > 0)
                {
                    this.FileBatches.Add(currentBatch.ToString());
                }
            }
        }
    }
}
