using System.Collections;
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

        /// <summary>
        /// CASE [EXP]:{[STATEMENT]} [CASE PART] | DEFAULT:{[STATEMENT]}
        /// </summary>
        /// <param name="switchArg"></param>
        /// <returns></returns>
        private bool P_CasePart(bool isProc, Token.Token? switchArg)
        {
            var tmp = new ArrayList();
            if (P_Case())
            {
                _tmpCtr++;
                var condition = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));
                PerformExpression(isProc, condition);
                _ifCtr++;
                tmp.Add($"mov edi, {DetermineAsmOperand(condition.Lex)}");
                tmp.Add($"cmp {DetermineAsmOperand(switchArg.Lex)}, edi");
                tmp.Add($"jnz _endif_{_ifCtr}");
                var ifCtr = _ifCtr;
                if (P_Colon() && P_LBrace())
                {
                    if (P_Statement())
                    {
                        tmp.Add($"jmp _endswitch_{_switchCtr}");
                        tmp.Add($"_endif_{ifCtr}:");
                        AddToCorrectSection(isProc, tmp);
                        return P_Rbrace() && P_CasePart(isProc, switchArg);
                    }
                }
            }

            if (P_Default())
            {
                _elseCtr++;
                tmp.RemoveAt(_text.Count - 1);
                tmp.Add($"_endif_{_ifCtr}:");
                AddToCorrectSection(isProc, tmp);
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
        
        /// <summary>
        /// SWITCH ([VARIABLE NAME]) { [CASE PART] }
        /// </summary>
        /// <returns></returns>
        private bool P_CaseStmt(bool isProc)
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
                                if (P_CasePart(isProc, switchArg))
                                {
                                    var tmp = new ArrayList {$"_endswitch_{_switchCtr}:"};
                                    AddToCorrectSection(isProc, tmp);
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