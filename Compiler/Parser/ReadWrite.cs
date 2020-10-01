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
                _text += $"push s{_dataCtr}\n";
                _text += "push stringPrinter\n";
            } 
            else
            {
                if (P_LeftBracket())
                {
                    var indices = new List<int>();
                    for (;;)
                    {
                        if (int.TryParse(P_PosOrNeg().Lex, out var idx))
                        {
                            indices.Add(idx);
                            if (!P_Comma()) break;
                        }
                    }

                    if (P_RightBracket())
                    {
                        var arr = _arrs.FirstOrDefault(x => x.Name == alpha.Lex);
                        if (arr == null) return false;
                        _text += $"mov edi, {arr.GetRef(indices).ToString()}\n";
                        _text += $"add edi, {FindAsmName(alpha.Lex)}\n";
                        _text += "push DWORD[edi]\n";
                        _text += "push numberPrinter\n";
                        _text += "call _printf\n";
                        _text += "add esp, 0x08\n";
                        return P_Semicolon();
                    }
                }
                _text += $"push DWORD[{FindAsmName(alpha.Lex)}]\n";
                _text += "push numberPrinter\n";
            }
            _text += "call _printf\n";
            _text += "add esp, 0x08\n";
            return P_Semicolon();
        }

        private bool P_ReadStmt()
        {
            if (_curr.Type != TokenType.Read) return false;
            _curr = _scanner.GetNextToken();
            var assignee = P_VarName();
            _text += "pusha\n";
            _text += $"push {FindAsmName(assignee.Lex)}\n";
            _text += "push dword int_format\n";
            _text += "call _scanf\n";
            _text += "add esp, 0x04\n";
            _text += "popa\n";
            return P_Semicolon();
        }

        private Token.Token P_Alpha()
        {
            var alpha = P_StrConst();
            alpha = alpha ?? P_VarName();
            return alpha;
        }
    }
}