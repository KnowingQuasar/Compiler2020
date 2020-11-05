using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_WriteStmt(bool isProc)
        {
            if (_curr.Type != TokenType.Write) return false;
            _curr = _scanner.GetNextToken();
            if (!P_Alpha(isProc, out var alpha)) return false;
            var tmp = new ArrayList();
            if (alpha != null && alpha.Type == TokenType.StrConst)
            {
                tmp.Add($"push s{_dataCtr}");
                tmp.Add("push stringPrinter");
            }
            else
            {
                if (P_LeftBracket())
                {
                    var indices = new ArrayList();
                    for (;;)
                    {
                        _tmpCtr++;
                        var idx = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                        _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));

                        if (PerformExpression(isProc, idx))
                        {
                            indices.Add(idx);
                            if (!P_Comma()) break;
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
                        tmp.AddRange(arr.GetRef(indices));
                        tmp.Add($"add eax, {FindAsmName(alpha.Lex)}");
                        tmp.Add("push DWORD[eax]");
                        tmp.Add("push numberPrinter");
                        tmp.Add("call _printf");
                        tmp.Add("add esp, 0x08");
                        AddToCorrectSection(isProc, tmp);
                        return P_Semicolon();
                    }
                }

                if (GetTypeOfVar(alpha?.Lex) == AsmDataType.String)
                {
                    tmp.Add($"push {FindAsmName(alpha?.Lex)}");
                    tmp.Add("push stringPrinter");
                }
                else if (GetTypeOfVar(alpha?.Lex) == AsmDataType.Float)
                {
                    tmp.Add($"fld DWORD[{FindAsmName(alpha?.Lex)}]");
                    tmp.Add("fstp QWORD[esp]");
                    tmp.Add("push floatPrinter");
                }
                else
                {
                    tmp.Add($"push DWORD[{FindAsmName(alpha.Lex)}]");
                    tmp.Add("push numberPrinter");
                }
            }
            tmp.Add("call _printf");
            tmp.Add("add esp, 0x08");
            AddToCorrectSection(isProc, tmp);
            return P_Semicolon();
        }

        private bool P_ReadStmt(bool isProc)
        {
            if (_curr.Type != TokenType.Read) return false;
            _curr = _scanner.GetNextToken();
            if (P_VarName(out var assignee))
            {
                var tmp = new ArrayList
                {
                    "pusha",
                    $"push {FindAsmName(assignee.Lex)}",
                    "push dword int_format",
                    "call _scanf",
                    "add esp, 0x04",
                    "popa"
                };
                AddToCorrectSection(isProc, tmp);
                return P_Semicolon();
            }

            return false;
        }

        private bool P_Alpha(bool isProc, out Token.Token? token)
        {
            if (!(P_StrConst(out token) || P_VarName(out token)))
            {
                _tmpCtr++;
                var writeData = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));
                return PerformExpression(isProc, writeData);
            }
            return true;
        }
    }
}