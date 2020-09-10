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
                _text += $"push DWORD[{FindAsmName(alpha.Lex)}]\n";
                _text += "push numberPrinter\n";
            }
            _text += "call _printf\n";
            _text += "add esp, 0x08\n";
            return true;
        }

        private Token.Token P_Alpha()
        {
            var alpha = P_StrConst();
            alpha = alpha ?? P_VarName();
            if (alpha == null) return null;
            return P_Semicolon() ? alpha : null;
        }
    }
}