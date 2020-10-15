using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_If()
        {
            return CheckToken(TokenType.If);
        }

        private bool P_Else()
        {
            return CheckToken(TokenType.Else);
        }

        private bool P_Then()
        {
            return CheckToken(TokenType.Then);
        }

        private bool P_IfStmt()
        {
            if (!P_If()) return false;
            if(P_Condition(out var lhand, out var rhand, out var op))
            {
                _ifCtr++;
                _text.Add($"mov edi, {DetermineAsmOperand(rhand?.Lex)}");
                _text.Add($"cmp {DetermineAsmOperand(lhand.Lex)}, edi");
                if (op != null)
                    switch (op.Type)
                    {
                        case TokenType.EqComp:
                            _text.Add($"jnz _endif_{_ifCtr}");
                            break;
                        case TokenType.Greater:
                            _text.Add($"jle _endif_{_ifCtr}");
                            break;
                        case TokenType.Less:
                            _text.Add($"jge _endif_{_ifCtr}");
                            break;
                        case TokenType.Neq:
                            _text.Add($"jz _endif_{_ifCtr}");
                            break;
                        case TokenType.Geq:
                            _text.Add($"jl _endif_{_ifCtr}");
                            break;
                        case TokenType.Leq:
                            _text.Add($"jg _endif_{_ifCtr}");
                            break;
                        default:
                            LogError("Invalid if statement syntax.");
                            return false;
                    }

                if (P_Then())
                {
                    if (P_LBrace())
                    {
                        var ifCtr = _ifCtr;
                        if (P_Statement())
                        {
                            if (P_Rbrace())
                            {
                                P_TheForce(ifCtr);
                                return true;
                            }
                        }
                    }
                }
            }
            LogError("Invalid if statement syntax.");

            return false;
        }

        private bool P_Condition(out Token.Token lhand, out Token.Token? rhand, out Token.Token? op)
        {
            rhand = null;
            op = null;
            _tmpCtr++;
            lhand = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
            PerformExpression(lhand);
            if (!P_RelationalOperator(out op)) return false;
            _tmpCtr++;
            rhand = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
            PerformExpression(rhand);
            return true;
        }

        private bool P_RelationalOperator(out Token.Token op)
        {
            op = _curr;
            _curr = _scanner.GetNextToken();
            return op.Type == TokenType.EqComp || op.Type == TokenType.Greater || op.Type == TokenType.Geq ||
                   op.Type == TokenType.Less || op.Type == TokenType.Leq || op.Type == TokenType.Neq;
        }

        private void P_TheForce(int ifCtr)
        {
            if (!P_Else())
            {
                _text.Add($"_endif_{ifCtr}:");
                return;
            }
            _elseCtr++;
            var thisElseCtr = _elseCtr;
            _text.Add($"jmp _endelse_{thisElseCtr}");
            _text.Add($"_endif_{ifCtr}:");
            if (P_LBrace() && P_Statement() && P_Rbrace())
            {
                _text.Add($"_endelse_{thisElseCtr}:");
                return;
            }
            LogError("Invalid else statement syntax.");
        }
    }
}