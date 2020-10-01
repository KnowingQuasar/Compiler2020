using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool P_LBrace()
        {
            if (_curr.Type != TokenType.Lbrace) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Rbrace()
        {
            if (_curr.Type != TokenType.Rbrace) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        /// <summary>
        /// ;
        /// </summary>
        /// <returns></returns>
        private bool P_Semicolon()
        {
            if (_curr.Type != TokenType.Semicolon) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        /// <summary>
        /// =
        /// </summary>
        /// <returns></returns>
        private bool P_Eq()
        {
            if (_curr.Type != TokenType.Eq) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Begin()
        {
            if (_curr.Type != TokenType.Begin) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_End()
        {
            if (_curr.Type == TokenType.End)
            {
                _curr = _scanner.GetNextToken();
                return true;
            }

            LogError(_curr.Type, TokenType.End);
            return false;
        }

        private bool P_Dot()
        {
            if (_curr.Type == TokenType.Dot)
            {
                _curr = _scanner.GetNextToken();
                return true;
            }

            LogError(_curr.Type, TokenType.Dot);
            return false;
        }

        /// <summary>
        /// [read statement] [statement] | [write statement] [statement] | [for statement] [statement] |
        /// [if statement] [statement] | [case statement] [statement] | [num assignment statement] [statement] |
        /// [string assignment statement] [statement] | [array statement] [statement] |
        /// [procedure declaration statement] [statement] | [procedure call statement] [statement] |
        /// [return statement] [statement] | [num declaration statement] [statement] |
        /// [string declaration statement] [statement] | [string concatenation] [statement] |
        /// [array assignment statement] [statement] | e
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once FunctionRecursiveOnAllPaths
        private bool P_Statement()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return ((P_WriteStmt() || P_ReadStmt() || P_AssignStmt() || P_NumDeclStmt() || P_ArrayStmt() ||
                     P_ForStatement()) && P_Statement()) || true;
        }

        private bool P_AssignStmt()
        {
            var assignee = P_VarName();
            if (assignee != null)
            {
                return P_NumAssignStmt(assignee) || P_ArrayAssignStmt(assignee);
            }

            return false;
        }

        private Token.Token P_VarName()
        {
            if (_curr.Type != TokenType.VarName) return null;
            var varName = _curr;
            _curr = _scanner.GetNextToken();
            return varName;
        }
    }
}