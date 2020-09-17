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
            return NextChar() ? EvalChar() : new Token.Token(TokenType.Eof, "", -1, -1);
        }

        private void MoveForward()
        {
            CurrChar = (char) StreamReader.Read();
            switch (CurrChar)
            {
                case '\n':
                    Line++;
                    Col = 0;
                    break;
                case '\t':
                    Col += 8;
                    break;
                default:
                    Col++;
                    break;
            }
        }

        private bool NextChar()
        {
            if (StreamReader.EndOfStream) return false;

            CurrChar = (char) StreamReader.Peek();

            if (char.IsLetter(CurrChar))
            {
                CurrChar = char.ToUpper(CurrChar);
            }

            return true;
        }

        private Token.Token EvalChar()
        {
            while (true)
            {
                if (char.IsLetter(CurrChar))
                {
                    return ProcessWord();
                }

                if (char.IsWhiteSpace(CurrChar))
                {
                    for (;;)
                    {
                        MoveForward();
                        if (!NextChar()) return new Token.Token(TokenType.Eof, "", -1, -1);
                        if (!char.IsWhiteSpace(CurrChar)) break;
                    }

                    continue;
                }

                if (char.IsNumber(CurrChar))
                {
                    return ProcessNum();
                }

                switch (CurrChar)
                {
                    case ';':
                        return ReturnTokenAndAdvance(TokenType.Semicolon);
                    case '/':
                        return ProcessSlash();
                    case '=':
                        return ReturnTokenAndAdvance(TokenType.Eq);
                    case '+':
                        return ReturnTokenAndAdvance(TokenType.Plus);
                    case '-':
                        return ReturnTokenAndAdvance(TokenType.Minus);
                    case '*':
                        return ReturnTokenAndAdvance(TokenType.Asterisk);
                    case '^':
                        return ReturnTokenAndAdvance(TokenType.Pow);
                    case '"':
                        return ProcessStr();
                    case '.':
                        return ReturnTokenAndAdvance(TokenType.Dot);
                    case '[':
                        return ReturnTokenAndAdvance(TokenType.Lbrack);
                    case ']':
                        return ReturnTokenAndAdvance(TokenType.Rbrack);
                    case ',':
                        return ReturnTokenAndAdvance(TokenType.Comma);
                    default:
                        return ReturnTokenAndAdvance(TokenType.Invalid);
                }
            }
        }

        private Token.Token ReturnTokenAndAdvance(TokenType type)
        {
            MoveForward();
            return new Token.Token(type, CurrChar.ToString(), Line, Col);
        }

        private Token.Token ProcessStr()
        {
            var word = CurrChar.ToString();
            MoveForward();
            NextChar();
            for (;;)
            {
                MoveForward();

                if (CurrChar == '"')
                {
                    word += CurrChar;
                    return new Token.Token(TokenType.StrConst, word, Line, Col);
                }

                if (StreamReader.EndOfStream)
                {
                    return new Token.Token(TokenType.Invalid, word, Line, Col);
                }

                word += CurrChar;
            }
        }

        private Token.Token ProcessWord()
        {
            var word = "";
            for (;;)
            {
                if (char.IsLetter(CurrChar) || char.IsNumber(CurrChar) || CurrChar == '_') word += CurrChar.ToString();
                else if (char.IsWhiteSpace(CurrChar)) break;
                else
                {
                    break;
                }
                MoveForward();
                NextChar();
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
                case "ARRAY":
                    return new Token.Token(TokenType.Array, word, Line, Col);
                default:
                    return new Token.Token(TokenType.VarName, word, Line, Col);
            }
        }

        private Token.Token ProcessNum()
        {
            var word = "";
            for (;;)
            {
                NextChar();
                if (!char.IsNumber(CurrChar))
                {
                    break;
                }
                word += CurrChar;
                MoveForward();
            }

            if (word.Length > 1 && word[0] == '0')
            {
                return new Token.Token(TokenType.Invalid, word, Line, Col);
            }
            
            return new Token.Token(TokenType.IntConst, word, Line, Col);
        }

        private Token.Token ProcessSlash()
        {
            MoveForward();
            NextChar();
            if (CurrChar == '/')
            {
                for (;;)
                {
                    MoveForward();
                    if (StreamReader.EndOfStream || CurrChar == '\n')
                    {
                        break;
                    }
                }

                return EvalChar();
            }

            if (CurrChar != '*') return new Token.Token(TokenType.Slash, CurrChar.ToString(), Line, Col);
            for (;;)
            {
                MoveForward();
                if (CurrChar != '*') continue;
                MoveForward();
                if (CurrChar != '/') continue;
                MoveForward();
                break;
            }

            return EvalChar();
        }
    }
}