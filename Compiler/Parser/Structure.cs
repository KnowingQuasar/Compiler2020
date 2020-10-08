using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        /// <summary>
        /// {
        /// </summary>
        /// <returns></returns>
        private bool P_LBrace()
        {
            return CheckToken(TokenType.Lbrace);
        }

        /// <summary>
        /// }
        /// </summary>
        /// <returns></returns>
        private bool P_Rbrace()
        {
            return CheckToken(TokenType.Rbrace);
        }

        /// <summary>
        /// ;
        /// </summary>
        /// <returns></returns>
        private bool P_Semicolon()
        {
            return CheckToken(TokenType.Semicolon);
        }

        /// <summary>
        /// =
        /// </summary>
        /// <returns></returns>
        private bool P_Eq()
        {
            return CheckToken(TokenType.Eq);
        }

        /// <summary>
        /// BEGIN
        /// </summary>
        /// <returns></returns>
        private bool P_Begin()
        {
            return CheckToken(TokenType.Begin);
        }

        /// <summary>
        /// END
        /// </summary>
        /// <returns></returns>
        private bool P_End()
        {
            return CheckToken(TokenType.End);
        }

        /// <summary>
        /// .
        /// </summary>
        /// <returns></returns>
        private bool P_Dot()
        {
            return CheckToken(TokenType.Dot);
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
            return ((P_WriteStmt() || P_ReadStmt() || P_AssignStmt() || P_IfStmt() || P_NumDeclStmt() || P_ArrayStmt() ||
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