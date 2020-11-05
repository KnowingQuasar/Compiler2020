using System.Collections;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_Str()
        {
            return CheckToken(TokenType.String);
        }

        private bool P_StrConst(out Token.Token? token)
        {
            if (_curr.Type != TokenType.StrConst)
            {
                token = null;
                return false;
            }

            token = _curr;
            _dataCtr++;
            _data.Add(new PData($"s{_dataCtr}", $"s{_dataCtr}", "db", $"{token.Lex},0x0d,0x0a,0", AsmDataType.String));

            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_StrAssignStmt(bool isProc, Token.Token? assignee)
        {
            if (P_Eq())
            {
                if (P_DeathStar(isProc, assignee))
                {
                    return true;
                }
            }

            return false;
        }

        private bool P_DeathStar(bool isProc, Token.Token? assignee)
        {
            if (P_VarName(out var assignment))
            {
                if (P_Alderaan(isProc, assignee, assignment))
                {
                    return true;
                }
            }

            if (P_StrConst(out assignment))
            {
                var tmp = new ArrayList
                {
                    "xor ecx, ecx",
                    "cld",
                    $"mov esi, s{_dataCtr}",
                    $"mov edi, {FindAsmName(assignee?.Lex)}",
                    $"copy{++_copyCtr}:",
                    "mov cl, byte[esi]",
                    "add cl, 1",
                    "movsb",
                    $"loop copy{_copyCtr}"
                };
                AddToCorrectSection(isProc, tmp);
                return P_Semicolon();
            }

            return false;
        }

        private bool P_Alderaan(bool isProc, Token.Token? assignee, Token.Token? assignment)
        {
            var tmp = new ArrayList
            {
                "xor ecx, ecx",
                "cld",
                $"mov esi, {FindAsmName(assignment?.Lex)}",
                $"mov edi, {FindAsmName(assignee?.Lex)}",
                $"copy{++_copyCtr}:",
                "mov cl, byte[esi]",
                "add cl, 1",
                "movsb",
                $"loop copy{_copyCtr}"
            };
            
            if (P_Semicolon())
            {
                AddToCorrectSection(isProc, tmp);
                return true;
            }

            if (P_Plus())
            {
                if (P_VarName(out var concatArg))
                {
                    tmp.Add("xor ecx, ecx");
                    tmp.Add("dec edi");
                    tmp.Add("dec edi");
                    tmp.Add("dec edi");
                    tmp.Add($"mov esi, {FindAsmName(concatArg?.Lex)}");
                    tmp.Add($"concat{++_copyCtr}:");
                    tmp.Add("mov cl, byte[esi]");
                    tmp.Add("add cl, 1");
                    tmp.Add("movsb");
                    tmp.Add($"loop concat{_copyCtr}");
                    AddToCorrectSection(isProc, tmp);
                    return P_Semicolon();
                }
            }

            return false;
        }

        private bool P_StrDeclStmt(bool isProc)
        {
            if (P_Str())
            {
                if (P_VarName(out var str))
                {
                    if (DoesVarExist(str?.Lex)) return false;
                    _strCtr++;
                    _bss.Add(new BssData($"_str_{_strCtr}_{str?.Lex}", str?.Lex, "resb", "128", AsmDataType.String));
                    if (P_DarthVader(isProc, str))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool P_DarthVader(bool isProc, Token.Token? str)
        {
            if (P_Eq())
            {
                if (P_Luke(isProc, str))
                {
                    return true;
                }
            }

            if (P_Semicolon())
            {
                if (!DoesVarExist("___defaultString"))
                {
                    _dataCtr++;
                    _data.Add(new PData("___defaultString", "___defaultString", "db", $"\"\",0x0d,0x0a,0", AsmDataType.String));
                }

                var tmp = new ArrayList
                {
                    "xor ecx, ecx",
                    "cld",
                    "mov esi, ___defaultString",
                    $"mov edi, {FindAsmName(str?.Lex)}",
                    $"copy{++_copyCtr}:",
                    "mov cl, byte[esi]",
                    "add cl, 1",
                    "movsb",
                    $"loop copy{_copyCtr}"
                };
                AddToCorrectSection(isProc, tmp);
                return true;
            }

            return false;
        }

        private bool P_Luke(bool isProc, Token.Token? str)
        {
            var tmp = new ArrayList
            {
                "xor ecx, ecx",
                "cld"
            };
            
            if (P_VarName(out var assignee))
            {
                tmp.Add($"mov esi, {FindAsmName(assignee?.Lex)}");
            }
            else if (P_StrConst(out assignee))
            {
                tmp.Add($"mov esi, s{_dataCtr}");
            }
            else
            {
                return false;
            }
            
            tmp.Add($"mov edi, {FindAsmName(str?.Lex)}");
            tmp.Add($"copy{++_copyCtr}:");
            tmp.Add("mov cl, byte[esi]");
            tmp.Add("add cl, 1");
            tmp.Add("movsb");
            tmp.Add($"loop copy{_copyCtr}");
            AddToCorrectSection(isProc, tmp);
            return P_Semicolon();
        }
    }
}