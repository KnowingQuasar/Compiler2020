using System.Collections;
using System.Collections.Generic;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_Float()
        {
            return CheckToken(TokenType.Float);
        }

        private bool P_FloatAssignStmt(bool isProc, Token.Token? assignee)
        {
            if (P_Eq())
            {
                var postfixStack = new Stack<string>();
                var exp = new Queue<string>();
                if (P_Exp(isProc, postfixStack, exp))
                {
                    for (;;)
                    {
                        if (!postfixStack.TryPop(out var val)) break;
                        exp.Enqueue(val);
                    }

                    var tmp = new ArrayList();
                    if (exp.Count == 1)
                    {
                        var val = exp.Dequeue();
                        if (int.TryParse(val, out var result))
                        {
                            tmp.Add($"mov DWORD[{FindAsmName(assignee.Lex)}], {result.ToString()}");
                            AddToCorrectSection(isProc, tmp);
                            return P_Semicolon();
                        }

                        if (float.TryParse(val, out var fresult))
                        {
                            tmp.Add($"mov DWORD[{FindAsmName(assignee.Lex)}], __float32__({fresult.ToString()})");
                            AddToCorrectSection(isProc, tmp);
                            return P_Semicolon();
                        }

                        tmp.Add($"mov esi, DWORD[{FindAsmName(val)}]");
                        tmp.Add($"mov DWORD[{FindAsmName(assignee.Lex)}], esi");
                        AddToCorrectSection(isProc, tmp);
                        return P_Semicolon();
                    }
                    var reg = ExecutePostfix(isProc, exp);
                    if (P_Semicolon())
                    {
                        tmp.Add($"mov esi, DWORD[{FindAsmName(reg)}]");
                        tmp.Add($"mov DWORD[{FindAsmName(assignee.Lex)}], esi");
                        AddToCorrectSection(isProc, tmp);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool P_FloatDeclStmt(bool isProc)
        {
            if (P_Float())
            {
                if (P_VarName(out var assignee))
                {
                    _bssCtr++;
                    _bss.Add(new BssData(GenerateBssName(assignee?.Lex), assignee?.Lex, "resb", "4", AsmDataType.Float));

                    return P_Delta(isProc, assignee);
                }
            }
            return false;
        }
    }
}