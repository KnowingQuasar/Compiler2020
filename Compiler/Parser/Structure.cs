using System.Collections;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        /// <summary>
        /// (
        /// </summary>
        /// <returns></returns>
        private bool P_LParen()
        {
            return CheckToken(TokenType.Lparen);
        }

        /// <summary>
        /// :
        /// </summary>
        /// <returns></returns>
        private bool P_Colon()
        {
            return CheckToken(TokenType.Colon);
        }

        /// <summary>
        /// )
        /// </summary>
        /// <returns></returns>
        private bool P_RParen()
        {
            return CheckToken(TokenType.Rparen);
        }

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
        /// *
        /// </summary>
        /// <returns></returns>
        private bool P_Asterisk()
        {
            return CheckToken(TokenType.Asterisk);
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
        private bool P_Statement(bool isProc = false)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return ((P_WriteStmt(isProc) || P_ReadStmt(isProc) || P_ProcDeclStmt() || P_AssignStmt(isProc) ||
                     P_IfStmt(isProc) ||
                     P_CaseStmt(isProc) || P_NumDeclStmt(isProc) || P_ArrayStmt(isProc) ||
                     P_ForStatement(isProc) || P_StrDeclStmt(isProc) || P_FloatDeclStmt(isProc)) &&
                    P_Statement(isProc)) || true;
        }

        /// <summary>
        /// Handles both:
        /// [NUMBER ASSIGNMENT] & [ARRAY ASSIGNMENT]
        /// </summary>
        /// <returns></returns>
        private bool P_AssignStmt(bool isProc)
        {
            if (P_VarName(out var assignee))
            {
                if (!DoesVarExist(assignee?.Lex)) return false;

                if (GetTypeOfVar(assignee?.Lex) == AsmDataType.String)
                {
                    return P_StrAssignStmt(isProc, assignee);
                }

                if (GetTypeOfVar(assignee?.Lex) == AsmDataType.Float)
                {
                    return P_FloatAssignStmt(isProc, assignee);
                }

                return P_NumAssignStmt(isProc, assignee) || P_ArrayAssignStmt(isProc, assignee) ||
                       P_ProcCallStmt(assignee);
            }

            return false;
        }

        /// <summary>
        /// Handles variable names
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private bool P_VarName(out Token.Token? token)
        {
            if (_curr.Type != TokenType.VarName)
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