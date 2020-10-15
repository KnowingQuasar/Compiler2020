using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        /// <summary>
        /// SWITCH
        /// </summary>
        /// <returns></returns>
        private bool P_Switch()
        {
            return CheckToken(TokenType.Switch);
        }

        /// <summary>
        /// CASE
        /// </summary>
        /// <returns></returns>
        private bool P_Case()
        {
            return CheckToken(TokenType.Case);
        }

        /// <summary>
        /// DEFAULT
        /// </summary>
        /// <returns></returns>
        private bool P_Default()
        {
            return CheckToken(TokenType.Default);
        }

        private bool P_CasePart(Token.Token switchArg)
        {
            if (P_Case())
            {
                _tmpCtr++;
                var condition = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
                PerformExpression(condition);
                _ifCtr++;
                _text.Add($"mov edi, {DetermineAsmOperand(condition.Lex)}");
                _text.Add($"cmp {DetermineAsmOperand(switchArg.Lex)}, edi");
                _text.Add($"jnz _endif_{_ifCtr}");
                var ifCtr = _ifCtr;
                if (P_Colon() && P_LBrace())
                {
                    if (P_Statement())
                    {
                        _text.Add($"jmp _endswitch_{_switchCtr}");
                        _text.Add($"_endif_{ifCtr}:");
                        return P_Rbrace() && P_CasePart(switchArg);
                    }
                }
            }

            if (P_Default())
            {
                _elseCtr++;
                var thisElseCtr = _elseCtr;
                _text.RemoveAt(_text.Count - 1);
                _text.Add($"_endif_{_ifCtr}:");
                if (P_Colon() && P_LBrace())
                {
                    if (P_Statement())
                    {
                        return P_Rbrace();
                    }
                }
            }
            return false;
        }
        
        private bool P_CaseStmt()
        {
            if (P_Switch())
            {
                _switchCtr++;
                if (P_LParen())
                {
                    if (P_VarName(out var switchArg))
                    {
                        if (P_RParen())
                        {
                            if (P_LBrace())
                            {
                                if (P_CasePart(switchArg))
                                {
                                    _text.Add($"_endswitch_{_switchCtr}:");
                                    if (P_Rbrace())
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}