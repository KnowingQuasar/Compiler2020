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
            const string testFile = @"C:\Compiler\basics.txt";
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
                p.StartInfo.Arguments =
                    "C:\\Compiler\\basicsexample.o -lmsvcrt -lkernel32 -o C:\\Compiler\\basicsexample.exe";
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

        [TestMethod]
        public void TestArithmeticExp()
        {
            const string testFile = @"C:\Compiler\ae.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\sampleae.asm -o C:\\Compiler\\ae.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\ae.o -lmsvcrt -lkernel32 -o C:\\Compiler\\ae.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Compiler\ae.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }

        [TestMethod]
        public void TestFor()
        {
            const string testFile = @"C:\Compiler\forexample.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\forprogram.asm -o C:\\Compiler\\for.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\for.o -lmsvcrt -lkernel32 -o C:\\Compiler\\for.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Compiler\for.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }

        [TestMethod]
        public void TestRead()
        {
            const string testFile = @"C:\Compiler\read_write.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\read_write.asm -o C:\\Compiler\\read_write.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\read_write.o -lmsvcrt -lkernel32 -o C:\\Compiler\\read_write.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestIf()
        {
            const string testFile = @"C:\Compiler\ifexample.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\ifexample.asm -o C:\\Compiler\\ifexample.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\ifexample.o -lmsvcrt -lkernel32 -o C:\\Compiler\\ifexample.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestCase()
        {
            const string testFile = @"C:\Compiler\case.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\caseexample.asm -o C:\\Compiler\\case.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\case.o -lmsvcrt -lkernel32 -o C:\\Compiler\\case.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestProc()
        {
            const string testFile = @"C:\Compiler\procedureTry.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\proceduretry.asm -o C:\\Compiler\\proc.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\proc.o -lmsvcrt -lkernel32 -o C:\\Compiler\\proc.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestArraySubscripting()
        {
            const string testFile = @"C:\Compiler\subscriptPassoff.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\subscript.asm -o C:\\Compiler\\subscript.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\proc.o -lmsvcrt -lkernel32 -o C:\\Compiler\\subscript.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestStrType()
        {
            const string testFile = @"C:\Compiler\stringsConcat.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\stringsconcat.asm -o C:\\Compiler\\stringsconcat.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\stringsconcat.o -lmsvcrt -lkernel32 -o C:\\Compiler\\stringsconcat.exe";
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
        }
        
        [TestMethod]
        public void TestFloat()
        {
            const string testFile = @"C:\Compiler\floating point arithmetic source.txt";
            var asmName = _parser.Parse(testFile);
            Assert.IsNotNull(asmName);

            using (var p = new Process())
            {
                p.StartInfo.FileName = @"C:\Program Files\NASM\nasm.exe";
                p.StartInfo.Arguments = $"-f win32 C:\\Compiler\\floats.asm -o C:\\Compiler\\float.o";
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
                p.StartInfo.Arguments = "C:\\Compiler\\float.o -lmsvcrt -lkernel32 -o C:\\Compiler\\float.exe";
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