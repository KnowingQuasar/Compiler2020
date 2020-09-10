using System.Diagnostics;
using Compiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTest
{
    [TestClass]
    public class ParserTests
    {
        private Parser _parser;
        private const string TestFile = @"C:\basics.txt";
        
        [TestInitialize]
        public void InitParserTest()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void TestParseBasicFile()
        {
            var asmName = _parser.Parse(TestFile);
            Assert.IsNotNull(asmName);
            
//            using (var p = new Process())
//            {
//                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
//                p.StartInfo.Arguments = $"-f win32 ";
//            }
//            
//            using (var p = new Process())
//            {
//                p.StartInfo.FileName = @"C:\MinGW\bin\ld.exe";
//                p.StartInfo.Arguments = "-";
//            }
        }
    }
}