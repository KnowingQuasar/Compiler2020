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

        public bool P_ForStatement()
        {
            if (P_For())
            {
                var accumulator = P_VarName();
                
                if (accumulator != null)
                {
                    if (P_Eq())
                    {
                        PerformExpression(accumulator);
                        if (P_To())
                        {
                            _tmpCtr++;
                            var to = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
                            PerformExpression(to);
                            if (P_Step())
                            {
                                _tmpCtr++;
                                var step = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
                                PerformExpression(step);
                                _loopCtr++;
                                _text += $"_loop_start_{_loopCtr}:\n";
                                _text += $"mov esi, DWORD[{FindAsmName(to.Lex)}]\n";
                                _text += $"cmp DWORD[{FindAsmName(accumulator.Lex)}], esi\n";
                                _text += $"jg _loop_end_{_loopCtr}\n";

                                var origLoopCtr = _loopCtr;
                                if (P_Do())
                                {
                                    if (P_LBrace())
                                    {
                                        if (P_Statement())
                                        {
                                            if (P_Rbrace())
                                            {
                                                _text += $"mov edi, DWORD[{FindAsmName(step.Lex)}]\n";
                                                _text += $"add DWORD[{FindAsmName(accumulator.Lex)}], edi\n";
                                                _text += $"jmp _loop_start_{origLoopCtr}\n";
                                                _text += $"_loop_end_{origLoopCtr}:\n";
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