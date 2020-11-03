using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_StrConst(out Token.Token? token)
        {
            if (_curr.Type != TokenType.StrConst)
            {
                token = null;
                return false;
            }
            token = _curr;
            _curr = _scanner.GetNextToken();
            return true;
        }
    }
}