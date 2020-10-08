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
                _text += $"mov edi, {DetermineAsmOperand(rhand.Lex)}\n";
                _text += $"cmp {DetermineAsmOperand(lhand.Lex)}, edi\n";
                switch (op.Type)
                {
                    case TokenType.EqComp:
                        _text += $"jnz _endif_{_ifCtr}\n";
                        break;
                    case TokenType.Greater:
                        _text += $"jle _endif_{_ifCtr}\n";
                        break;
                    case TokenType.Less:
                        _text += $"jge _endif_{_ifCtr}\n";
                        break;
                    case TokenType.Neq:
                        _text += $"jz _endif_{_ifCtr}\n";
                        break;
                    case TokenType.Geq:
                        _text += $"jl _endif_{_ifCtr}\n";
                        break;
                    case TokenType.Leq:
                        _text += $"jg _endif_{_ifCtr}\n";
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

        private bool P_Condition(out Token.Token lhand, out Token.Token rhand, out Token.Token op)
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
                _text += $"_endif_{ifCtr}:\n";
                return;
            }
            _elseCtr++;
            var thisElseCtr = _elseCtr;
            _text += $"jmp _endelse_{thisElseCtr}\n";
            _text += $"_endif_{ifCtr}:\n";
            if (P_LBrace() && P_Statement() && P_Rbrace())
            {
                _text += $"_endelse_{thisElseCtr}:\n";
                return;
            }
            LogError("Invalid else statement syntax.");
        }
    }
}