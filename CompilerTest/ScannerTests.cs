using Compiler.Parser;
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
        public void TestBasicProgram()
        {
            Assert.IsTrue(_scanner.OpenFile(TestFile));
            Assert.AreEqual(new Token(TokenType.Program, "PROGRAM", 1, 7), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "BASICSEXAMPLE", 1, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 1, 22), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Begin, "BEGIN", 2, 13), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Num, "NUM", 14, 19), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM1", 14, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 14, 25), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Num, "NUM", 15, 19), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 15, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 15, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "3", 15, 28), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 15, 29), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Num, "NUM", 27, 19), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 27, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 27, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 27, 31), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 27, 32), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 28, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 28, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 28, 27), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Plus, "+", 28, 29), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "10", 28, 32), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 28, 33), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 29, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 29, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 29, 27), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Plus, "+", 29, 29), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 29, 34), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 29, 35), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM1", 30, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 30, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "2", 30, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Asterisk, "*", 30, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "5", 30, 28), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 30, 29), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM1", 33, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 33, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 33, 27), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Asterisk, "*", 33, 29), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 33, 34), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 33, 35), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 34, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 34, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "8", 34, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Minus, "-", 34, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "5", 34, 28), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 34, 29), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 35, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 35, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "8", 35, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Minus, "-", 35, 23), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "5", 35, 25), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 35, 26), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 37, 20), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Eq, "=", 37, 22), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "8", 37, 24), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Pow, "^", 37, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.IntConst, "6", 37, 28), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 37, 29), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Write, "WRITE", 38, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.StrConst, "\"Basics.txt:\"", 38, 35), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 38, 36), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Write, "WRITE", 39, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM1", 39, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 39, 27), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Write, "WRITE", 40, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM2", 40, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 40, 27), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Write, "WRITE", 41, 21), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.VarName, "NUM3", 41, 26), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Semicolon, ";", 41, 27), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.End, "END", 42, 11), _scanner.GetNextToken());
            Assert.AreEqual(new Token(TokenType.Dot, ".", 42, 12), _scanner.GetNextToken());
            
            Assert.AreEqual(new Token(TokenType.Eof, "", -1, -1), _scanner.GetNextToken());
        }
    }
}