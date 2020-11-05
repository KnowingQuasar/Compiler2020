using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        public enum AsmDataType
        {
            String,
            Num,
            Float,
            FloatLiteral,
            Array,
            Invalid
        }
        
        private Token.Token _curr;
        private readonly Scanner.Scanner _scanner;
        private StreamWriter? _asmFile;
        private StreamWriter? _errFile;
        private HashSet<BssData> _bss;
        private HashSet<PData> _data;
        private ArrayList _text;
        private ArrayList _procs;
        private int _bssCtr;
        private int _dataCtr;
        private int _expCtr;
        private int _arrCtr;
        private string? _asmFileName;
        private List<PArray> _arrs;
        private int _tmpCtr;
        private int _loopCtr;
        private int _switchCtr;
        private int _ifCtr;
        private int _elseCtr;
        private int _strCtr;
        private int _copyCtr;

        public Parser()
        {
            _curr = new Token.Token(TokenType.Error, "", -1, -1);
            _scanner = new Scanner.Scanner();
            _bss = new HashSet<BssData>();
            _data = new HashSet<PData>();
            _arrs = new List<PArray>();
            _text = new ArrayList();
            _procs = new ArrayList();
            _bssCtr = -1;
            _dataCtr = -1;
            _expCtr = -1;
            _arrCtr = -1;
            _tmpCtr = -1;
            _loopCtr = -1;
            _ifCtr = -1;
            _elseCtr = -1;
            _switchCtr = -1;
            _strCtr = -1;
            _copyCtr = -1;
            _asmFileName = null;
        }

        public string? Parse(string fileName)
        {
            if (!_scanner.OpenFile(fileName))
            {
                Console.Error.WriteLine("Could not open file: " + fileName);
                return null;
            }

            _curr = _scanner.GetNextToken();

            if (!P_Program()) return null;
            // LogError("File compiled successfully!");
            return _asmFileName;
        }

        private string DetermineAsmOperand(string? operand)
        {
            if (operand == "edi") return "edi";
            return int.TryParse(operand, out var val) ? val.ToString() : "DWORD[" + FindAsmName(operand) + "]";
        }

        private void AddToCorrectSection(bool isProc, IEnumerable data)
        {
            if (isProc)
            {
                foreach (var t in data)
                {
                    _procs.Add(t);
                }
            }
            else
            {
                foreach (var t in data)
                {
                    _text.Add(t);
                }
            }
        }

        private void LogError(string message)
        {
            Console.Error.WriteLine(message);
            _errFile?.WriteLine(message);
        }

        private string GenerateSection(IEnumerable list)
        {
            return list.Cast<object?>().Aggregate("", (current, item) => current + (item + "\n"));
        }

        private string GenerateBssName(string? lex)
        {
            return $"_{_bssCtr.ToString()}_{lex.ToLower()}";
        }

        private AsmDataType GetTypeOfVar(string? lex)
        {
            var rg = new Regex(@"\d+\.\d+");
            if (rg.IsMatch(lex)) return AsmDataType.FloatLiteral;
            var bssSym = _bss.FirstOrDefault(item =>
                string.Equals(item.ActualName, lex, StringComparison.CurrentCultureIgnoreCase));
            if (bssSym == null)
            {
                var dataSym = _data.FirstOrDefault(item => string.Equals(item.ActualName, lex, StringComparison.CurrentCultureIgnoreCase));
                if (dataSym != null) return dataSym.AsmDataType;
            }

            return bssSym?.AsmDataType ?? AsmDataType.Invalid;
        }

        private bool DoesVarExist(string? lex)
        {
            return _bss.Any(item => string.Equals(item.ActualName, lex, StringComparison.CurrentCultureIgnoreCase)) ||
                   _data.Any(item => string.Equals(item.ActualName, lex, StringComparison.CurrentCultureIgnoreCase));
        }

        private string GenerateDataName(string lex)
        {
            return $"_{_dataCtr.ToString()}_{lex.ToLower()}";
        }

        private string BuildData()
        {
            var text = _data.Aggregate("section .data\n",
                (current, item) => current + $"{item.AsmName} {item.DataType} {item.Value}\n");

            text += "stringPrinter db \"%s\",0\n";
            text += "numberPrinter db \"%d\",0x0d,0x0a,0\n";
            text += "int_format db \"%i\",0\n";
            text += "floatPrinter db \"%f\",0x0d,0x0a,0\n";

            return text;
        }

        private string BuildBss()
        {
           // if (_bss.Count == 0) return "";
            return _bss.Aggregate("section .bss\n",
                (current, item) => current + $"{item.AsmName} {item.DataType} {item.Size}\n");
        }

        private string? FindAsmName(string? realName)
        {
            foreach (var item in _bss.Where(item =>
                string.Equals(item.ActualName, realName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return item.AsmName;
            }

            return (from item in _data
                where string.Equals(item.ActualName, realName, StringComparison.CurrentCultureIgnoreCase)
                select item.AsmName).FirstOrDefault() ?? null;
        }

        private void LogError(TokenType actualToken, TokenType expectedToken)
        {
            LogError($"Error: Unexpected {actualToken}. Expected a {expectedToken} token.");
        }

        private bool CheckToken(TokenType tokenType)
        {
            if (_curr.Type != tokenType) return false;
            _curr = _scanner.GetNextToken();
            return true;
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
                    _asmFile = new StreamWriter("C:\\Compiler\\" + _asmFileName);
                    _errFile = new StreamWriter("C:\\Compiler\\" + $"{_curr.Lex.ToLower()}.err");
                    
                    _asmFile.WriteLine("extern _printf");
                    _asmFile.WriteLine("extern _scanf");
                    _asmFile.WriteLine("extern _ExitProcess@4");
                    _asmFile.WriteLine("global Start");

                    _curr = _scanner.GetNextToken();

                    if (P_Semicolon() && P_Begin() && P_Statement() && P_End() && P_Dot())
                    {
                        _text.Add("exit:");
                        _text.Add("push 0");
                        _text.Add("call _ExitProcess@4");

                        _asmFile.Write(BuildData());
                        _asmFile.Write(BuildBss());

                        _asmFile.WriteLine("section .text");
                        _asmFile.WriteLine("Start:");
                        if (_procs.Count > 0)
                        {
                            _asmFile.WriteLine("jmp afterProcedures");
                            _asmFile.Write(GenerateSection(_procs));
                            _asmFile.WriteLine("afterProcedures:");
                        }

                        _asmFile.Write(GenerateSection(_text));

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