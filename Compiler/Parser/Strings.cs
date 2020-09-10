using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private Token.Token P_StrConst()
        {
            if (_curr.Type != TokenType.StrConst) return null;
            var str = _curr;
            _curr = _scanner.GetNextToken();
            return str;
        }
    }
}