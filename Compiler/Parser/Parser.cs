using System;
using System.Collections.Generic;
using System.IO;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private Token.Token _curr;
        private readonly Scanner.Scanner _scanner;
        private StreamWriter _asmFile;
        private StreamWriter _errFile;
        private HashSet<BssData> _bss;
        private HashSet<PData> _data;
        private int _bssCtr;
        private int _dataCtr;
        private string _text;
        private int _expCtr;
        private string _asmFileName;
        
        public Parser()
        {
            _curr = new Token.Token(TokenType.Error, "", -1, -1);
            _scanner = new Scanner.Scanner();
            _bss = new HashSet<BssData>();
            _data = new HashSet<PData>();
            _bssCtr = -1;
            _dataCtr = -1;
            _expCtr = -1;
            _asmFileName = null;
        }

        public string Parse(string fileName)
        {
            if (!_scanner.OpenFile(fileName))
            {
                Console.Error.WriteLine("Could not open file: " + fileName);
                return null;
            }

            _curr = _scanner.GetNextToken();

            if (!P_Program()) return null;
            LogError("File compiled successfully!");
            return _asmFileName;
        }

        private void LogError(string message)
        {
            
        }

        private string GenerateBssName(string lex)
        {
            return $"_{_bssCtr.ToString()}_{lex.ToLower()}";
        }

        private string GenerateDataName(string lex)
        {
            return $"_{_dataCtr.ToString()}_{lex.ToLower()}";
        }

        private string BuildData()
        {
            var text = "section .data\n";
            foreach (var item in _data)
            {
                text += $"{item.AsmName} {item.DataType} {item.Value}\n";
            }

            text += "stringPrinter db \"%s\",0\n";
            text += "numberPrinter db \"%d\",0x0d,0x0a,0\n";

            return text;
        }

        private string BuildBss()
        {
            var text = "section .bss\n";
            foreach (var item in _bss)
            {
                text += $"{item.AsmName} {item.DataType} {item.Size}\n";
            }

            return text;
        }

        private string FindAsmName(string realName)
        {
            foreach(var item in _bss)
            {
                if (string.Equals(item.ActualName, realName, StringComparison.CurrentCultureIgnoreCase)) return item.AsmName;
            }

            foreach (var item in _data)
            {
                if (string.Equals(item.ActualName, realName, StringComparison.CurrentCultureIgnoreCase)) return item.AsmName;
            }

            return null;
        }

        private void LogError(TokenType actualToken, TokenType expectedToken)
        {
            LogError($"Error: Unexpected {actualToken}. Expected a {expectedToken} token.");
        }

        /// <summary>
        /// program [variable name] ; begin [statement] end .
        /// </summary>
        /// <returns></returns>
        private bool P_Program()
        {
            if (_curr.Type == TokenType.Program)
            {
                _curr = _scanner.GetNextToken();

                if (_curr.Type == TokenType.VarName)
                {
                    _asmFileName = $"{_curr.Lex.ToLower()}.asm";
                    _asmFile = new StreamWriter(_asmFileName);
                    _errFile = new StreamWriter($"{_curr.Lex.ToLower()}.err");

                    _asmFile.WriteLine("global Start");
                    _asmFile.WriteLine("extern _printf");
                    _asmFile.WriteLine("extern _ExitProcess@4");

                    _text += "section .text\n";
                    _text += "Start:\n";
                    
                    _curr = _scanner.GetNextToken();
                    
                    if (P_Semicolon() && P_Begin() && P_Statement() && P_End() && P_Dot())
                    {
                        _text += "exit:\n";
                        _text += "mov eax, 0x0\n";
                        _text += "call _ExitProcess@4";
                        _asmFile.Write(BuildData());
                        _asmFile.Write(BuildBss());
                        _asmFile.Write(_text);
                        
                        _asmFile.Close();
                        _asmFile.Dispose();
                        _errFile.Close();
                        _errFile.Dispose();
                        return true;
                    }
                    
                    _asmFile.Close();
                    _asmFile.Dispose();
                    _errFile.Close();
                    _errFile.Dispose();
                    return false;
                }

                Console.Error.WriteLine("Could not find an IDENT token. Exiting...");
            }
            else
            {
                Console.Error.WriteLine("Could not find a PROGRAM token. Exiting...");
            }

            return false;
        }
    }
}