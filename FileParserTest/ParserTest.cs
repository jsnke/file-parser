using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileParser;
using System.IO;
using System.Text;

namespace FileParserTest
{
    [TestClass]
    public class ParserTest
    {
        [TestMethod]
        public void Parse_ConfigFile_NoComments()
        {
            var parser = new Parser(FileType.Configuration);

            using(var stream = new MemoryStream())
            {
                FillStream(stream, @"key1=A
key2=B
key3=C");
                
                parser.Parse(stream);

                Assert.AreEqual(3, parser.Batches.Length);
                Assert.AreEqual("key1=A", parser.Batches[0]);
                Assert.AreEqual("key2=B", parser.Batches[1]);
                Assert.AreEqual("key3=C", parser.Batches[2]);
            }
        }

        [TestMethod]
        public void Parse_ConfigFile_Comments()
        {
            var parser = new Parser(FileType.Configuration);

            using (var stream = new MemoryStream())
            {
                FillStream(stream, @"# Comment 1
key1=A
 # Comment 2
key2=B
 ## Comment 3 ##
key3=C");

                parser.Parse(stream);

                Assert.AreEqual(3, parser.Batches.Length);
                Assert.AreEqual("key1=A", parser.Batches[0]);
                Assert.AreEqual("key2=B", parser.Batches[1]);
                Assert.AreEqual("key3=C", parser.Batches[2]);
            }
        }

        [TestMethod]
        public void Parse_ConfigFile_BlankLines()
        {
            var parser = new Parser(FileType.Configuration);

            using (var stream = new MemoryStream())
            {
                FillStream(stream, @"

key1=A
key2=B



key3=C

");

                parser.Parse(stream);

                Assert.AreEqual(3, parser.Batches.Length);
                Assert.AreEqual("key1=A", parser.Batches[0]);
                Assert.AreEqual("key2=B", parser.Batches[1]);
                Assert.AreEqual("key3=C", parser.Batches[2]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_NoComments()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"DECLARE @id INT = 32;
SELECT @id;";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual(sql, parser.Batches[0]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_BlankLines()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"DECLARE @id INT = 32;

SELECT @id;";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual(sql, parser.Batches[0]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_LineComments()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"-- A comment
DECLARE @id INT = 32;
-- SELECT @id;";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual("DECLARE @id INT = 32;", parser.Batches[0]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_BlockComments()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"/* A comment */
DECLARE @id INT = 32;
/*
Another comment
SELECT @id;

*/";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual("DECLARE @id INT = 32;", parser.Batches[0]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_Batches()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"SET ANSI_NULLS ON;
GO
SELECT 1";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(2, parser.Batches.Length);
                Assert.AreEqual("SET ANSI_NULLS ON;", parser.Batches[0]);
                Assert.AreEqual("SELECT 1", parser.Batches[1]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_BatchesLineComment()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"SET ANSI_NULLS ON;
-- GO
SELECT 1";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual(@"SET ANSI_NULLS ON;
SELECT 1", parser.Batches[0]);
            }
        }

        [TestMethod]
        public void Parse_SqlFile_BatchesBlockComment()
        {
            var parser = new Parser(FileType.SqlScript);

            using (var stream = new MemoryStream())
            {
                var sql = @"SET ANSI_NULLS ON;
/*
 * GO
 */
SELECT 1";
                FillStream(stream, sql);

                parser.Parse(stream);

                Assert.AreEqual(1, parser.Batches.Length);
                Assert.AreEqual(@"SET ANSI_NULLS ON;
SELECT 1", parser.Batches[0]);
            }
        }

        /// <summary>
        /// Dump the supplied text into a memory stream.
        /// </summary>
        /// <param name="stream">Memory stream to store text in.</param>
        /// <param name="text">Text to store.</param>
        private void FillStream(MemoryStream stream, string text)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.SetLength(0L);
            var data = Encoding.Default.GetBytes(text);
            stream.Write(data, 0, data.Length);
            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
