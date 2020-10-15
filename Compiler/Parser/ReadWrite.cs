using System.Collections.Generic;
using System.Linq;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_WriteStmt()
        {
            if (_curr.Type != TokenType.Write) return false;
            _curr = _scanner.GetNextToken();
            var alpha = P_Alpha();
            if (alpha.Type == TokenType.StrConst)
            {
                _dataCtr++;
                _data.Add(new PData($"s{_dataCtr}", $"s{_dataCtr}", "db", $"{alpha.Lex},0x0d,0x0a,0"));
                _text.Add($"push s{_dataCtr}");
                _text.Add("push stringPrinter");
            } 
            else
            {
                if (P_LeftBracket())
                {
                    var indices = new List<int>();
                    for (;;)
                    {
                        if (P_PosOrNeg(out var num))
                        {
                            if (int.TryParse(num.Lex, out var idx))
                            {
                                indices.Add(idx);
                                if (!P_Comma()) break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (P_RightBracket())
                    {
                        var arr = _arrs.FirstOrDefault(x => x.Name == alpha.Lex);
                        if (arr == null) return false;
                        _text.Add($"mov edi, {arr.GetRef(indices).ToString()}");
                        _text.Add($"add edi, {FindAsmName(alpha.Lex)}");
                        _text.Add("push DWORD[edi]");
                        _text.Add("push numberPrinter");
                        _text.Add("call _printf");
                        _text.Add("add esp, 0x08");
                        return P_Semicolon();
                    }
                }
                _text.Add($"push DWORD[{FindAsmName(alpha.Lex)}]");
                _text.Add("push numberPrinter");
            }
            _text.Add("call _printf");
            _text.Add("add esp, 0x08");
            return P_Semicolon();
        }

        private bool P_ReadStmt()
        {
            if (_curr.Type != TokenType.Read) return false;
            _curr = _scanner.GetNextToken();
            if (P_VarName(out var assignee))
            {
                _text.Add("pusha");
                _text.Add($"push {FindAsmName(assignee.Lex)}");
                _text.Add("push dword int_format");
                _text.Add("call _scanf");
                _text.Add("add esp, 0x04");
                _text.Add("popa");
                return P_Semicolon();
            }

            return false;
        }

        private Token.Token P_Alpha()
        {
            var alpha = P_StrConst();
            if (alpha == null) P_VarName(out alpha);
            return alpha;
        }
    }
}