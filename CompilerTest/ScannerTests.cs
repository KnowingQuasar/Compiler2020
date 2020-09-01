using System.IO;
using Compiler.Scanner;
using Compiler.Token;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTest
{
    [TestClass]
    public class ScannerTests
    {
        private Scanner _scanner;
        private const string TestFile = @"C:\basics.txt";

        [TestInitialize]
        public void InitScannerTest()
        {
            _scanner = new Scanner();
        }

        [TestMethod]
        public void TestFileRead()
        {
            Assert.IsTrue(_scanner.OpenFile(TestFile));
        }

        [TestMethod]
        public void TestFileBadRead()
        {
            Assert.IsFalse(_scanner.OpenFile(@"fakefile.exe"));
        }

        [TestMethod]
        public void TestEquality()
        {
            Assert.AreEqual(new Token(TokenType.Do, "test", 0, 0), new Token(TokenType.Do, "test", 0, 0));
        }

        [TestMethod]
        public void TestProgramToken()
        {
            Assert.IsTrue(_scanner.OpenFile(TestFile));
            Assert.AreEqual(new Token(TokenType.Program, "PROGRAM", 1, 7), _scanner.GetNextToken());
        }
        
        [TestMethod]
        public void TestBeginToken()
        {
            Assert.IsTrue(_scanner.OpenFile(TestFile));
            Assert.AreEqual(new Token(TokenType.Program, "PROGRAM", 1, 7), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Ident, "IDENT", 1, 21), _scanner.GetNextToken());
        }
    }
}