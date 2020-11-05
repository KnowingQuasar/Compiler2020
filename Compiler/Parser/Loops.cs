using System.Collections;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_For()
        {
            return CheckToken(TokenType.For);
        }

        private bool P_To()
        {
            return CheckToken(TokenType.To);
        }
        
        private bool P_Step()
        {
            return CheckToken(TokenType.Step);
        }
        
        private bool P_Do()
        {
            return CheckToken(TokenType.Do);
        }

        public bool P_ForStatement(bool isProc)
        {
            if (P_For())
            {
                if (P_VarName(out var accumulator))
                {
                    if (P_Eq())
                    {
                        PerformExpression(isProc, accumulator);
                        if (P_To())
                        {
                            _tmpCtr++;
                            var to = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));
                            PerformExpression(isProc, to);
                            if (P_Step())
                            {
                                var tmp = new ArrayList();
                                _tmpCtr++;
                                var step = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));
                                PerformExpression(isProc, step);
                                _loopCtr++;
                                tmp.Add($"_loop_start_{_loopCtr}:");
                                tmp.Add($"mov esi, DWORD[{FindAsmName(to.Lex)}]");
                                tmp.Add($"cmp DWORD[{FindAsmName(accumulator?.Lex)}], esi");
                                tmp.Add($"jg _loop_end_{_loopCtr}");

                                var origLoopCtr = _loopCtr;
                                if (P_Do())
                                {
                                    if (P_LBrace())
                                    {
                                        if (P_Statement())
                                        {
                                            if (P_Rbrace())
                                            {
                                                tmp.Add($"mov edi, DWORD[{FindAsmName(step.Lex)}]");
                                                tmp.Add($"add DWORD[{FindAsmName(accumulator?.Lex)}], edi");
                                                tmp.Add($"jmp _loop_start_{origLoopCtr}");
                                                tmp.Add($"_loop_end_{origLoopCtr}:");
                                                AddToCorrectSection(isProc, tmp);
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                }
                LogError("Invalid syntax around for statement.");
                return false;
            }
            return false;
        }
    }
}