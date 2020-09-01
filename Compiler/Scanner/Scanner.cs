using System;
using System.IO;
using Compiler.Token;

namespace Compiler.Scanner
{
    public class Scanner
    {
        private StreamReader StreamReader { get; set; }
        private char CurrChar { get; set; }
        private int Col { get; set; }
        private int Line { get; set; }


        public Scanner()
        {
            CurrChar = '\0';
            Col = 0;
            Line = 1;
        }

        public bool OpenFile(string fileName)
        {
            try
            {
                StreamReader = new StreamReader(fileName);
                return !StreamReader.EndOfStream;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Token.Token GetNextToken()
        {
            return NextChar() ? EvalChar() : new Token.Token(TokenType.Error, "", -1, -1);
        }

        private void MoveForward()
        {
            CurrChar = (char) StreamReader.Read();
            Col++;
        }

        private bool NextChar()
        {
            if (StreamReader.EndOfStream) return false;

            CurrChar = (char) StreamReader.Peek();

            if (char.IsLetter(CurrChar))
            {
                CurrChar = char.ToUpper(CurrChar);
            }
            else if (CurrChar == '\n')
            {
                Line++;
                Col = 0;
            }

            return true;
        }

        private Token.Token EvalChar()
        {
            if (char.IsLetter(CurrChar)) return ProcessWord();
            if (char.IsWhiteSpace(CurrChar))
            {
                MoveForward();
                return GetNextToken();
            }
            return new Token.Token(TokenType.Invalid, CurrChar.ToString(), Line, Col);
        }

        private Token.Token ProcessWord()
        {
            var word = CurrChar.ToString();
            for (;;)
            {
                MoveForward();
                NextChar();
                if (char.IsLetter(CurrChar) || char.IsNumber(CurrChar) || CurrChar == '_') word += CurrChar.ToString();
                else if (char.IsWhiteSpace(CurrChar)) break;
                else
                {
                    Col--;
                    break;
                }
            }

            switch (word)
            {
                case "PROGRAM":
                    return new Token.Token(TokenType.Program, word, Line, Col);
                case "Return":
                    return new Token.Token(TokenType.Return, word, Line, Col);
                case "WRITE":
                    return new Token.Token(TokenType.Write, word, Line, Col);
                case "FOR":
                    return new Token.Token(TokenType.For, word, Line, Col);
                case "TO":
                    return new Token.Token(TokenType.To, word, Line, Col);
                case "STEP":
                    return new Token.Token(TokenType.Step, word, Line, Col);
                case "BEGIN":
                    return new Token.Token(TokenType.Begin, word, Line, Col);
                case "END":
                    return new Token.Token(TokenType.End, word, Line, Col);
                case "NUM":
                    return new Token.Token(TokenType.Num, word, Line, Col);
                case "PROCEDURE":
                    return new Token.Token(TokenType.Proc, word, Line, Col);
                case "IF":
                    return new Token.Token(TokenType.If, word, Line, Col);
                case "THEN":
                    return new Token.Token(TokenType.Then, word, Line, Col);
                case "ELSE":
                    return new Token.Token(TokenType.Else, word, Line, Col);
                case "SWITCH":
                    return new Token.Token(TokenType.Switch, word, Line, Col);
                case "CASE":
                    return new Token.Token(TokenType.Case, word, Line, Col);
                case "DEFAULT":
                    return new Token.Token(TokenType.Default, word, Line, Col);
                default:
                    return new Token.Token(TokenType.Ident, word, Line, Col);
            }
        }
    }
}