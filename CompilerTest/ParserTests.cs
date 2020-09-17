using System.Diagnostics;
using Compiler.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTest
{
    [TestClass]
    public class ParserTests
    {
        private Parser _parser;
        
        [TestInitialize]
        public void InitParserTest()
        {
            _parser = new Parser();
        }

        [TestMethod]
        public void TestParseBasicFile()
        {
            const string testFile = @"C:\basics.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\basicsexample.asm -o C:\\Compiler\\basicsexample.o";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\MinGW\bin\ld.exe";
                p.StartInfo.Arguments = "C:\\Compiler\\basicsexample.o -lmsvcrt -lkernel32 -o C:\\Compiler\\basicsexample.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Compiler\basicsexample.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestParseArrayFile()
        {
            const string testFile = @"C:\Users\Ian\Documents\Array_input.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\arrayfun.asm -o C:\\Compiler\\arrayfun.o";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\MinGW\bin\ld.exe";
                p.StartInfo.Arguments = "C:\\Compiler\\arrayfun.o -lmsvcrt -lkernel32 -o C:\\Compiler\\arrayfun.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            
            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Compiler\arrayfun.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
    }
}