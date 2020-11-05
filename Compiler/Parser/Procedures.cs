using System.Collections;
using System.Collections.Generic;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private void ResetVariables(IEnumerable passByRefs)
        {
            foreach (KeyValuePair<Token.Token?, int>? kvp in passByRefs)
            {
                _procs.Add($"mov eax, DWORD[{FindAsmName(kvp?.Key?.Lex)}]");
                _procs.Add($"mov edi, DWORD[esp+{kvp?.Value.ToString()}]");
                _procs.Add($"mov DWORD[edi], eax");
            }
        }
        
        /// <summary>
        /// PROCEDURE 
        /// </summary>
        /// <returns></returns>
        private bool P_Proc()
        {
            return CheckToken(TokenType.Proc);
        }

        /// <summary>
        /// ) { [STATEMENT] } | [VARIABLE LIST]
        /// </summary>
        /// <returns></returns>
        private bool P_VarPassing(ArrayList passByVals, ArrayList passByRefs)
        {
            if (P_RParen())
            {
                if (P_LBrace())
                {
                    if (P_Statement(true))
                    {
                        ResetVariables(passByRefs);
                        _procs.Add("ret");
                        return P_Rbrace();
                    }
                }
            }

            return P_VarList(passByVals, passByRefs);
        }

        /// <summary>
        /// [PASS BY VALUE] | [PASS BY REFERENCE]
        /// </summary>
        /// <returns></returns>
        private bool P_VarList(ArrayList passByVals, ArrayList passByRefs)
        {
            return P_Num() && (P_PassByVal(passByVals, passByRefs) || P_PassByRef(passByVals, passByRefs));
        }

        /// <summary>
        /// NUM [VARIABLE NAME][VARIABLE LISTING] | STRING [VARIABLE NAME][VARIABLE LISTING]
        /// </summary>
        /// <returns></returns>
        private bool P_PassByVal(ArrayList passByVals, ArrayList passByRefs)
        {
            if (P_VarName(out var param))
            {
                if (!DoesVarExist(param?.Lex))
                {
                    _bssCtr++;
                    _bss.Add(new BssData(GenerateBssName(param?.Lex), param?.Lex, "resd", "1", AsmDataType.Num));
                }

                _procs.Add($"mov eax, DWORD[esp+{(passByVals.Count + passByRefs.Count) * 4 + 4}]");
                _procs.Add($"mov eax, DWORD[eax]");
                _procs.Add($"mov {DetermineAsmOperand(param?.Lex)}, eax");
                passByVals.Add(new KeyValuePair<Token.Token?, int>(param, (passByVals.Count + passByRefs.Count) * 4 + 4));
                return P_VarListing(passByVals, passByRefs);
            }

            return false;
        }

        /// <summary>
        /// NUM *[VARIABLE NAME][VARIABLE LISTING] | STRING *[VARIABLE NAME][VARIABLE LISTING]
        /// </summary>
        /// <returns></returns>
        private bool P_PassByRef(ArrayList passByVals, ArrayList passByRefs)
        {
            if (P_Asterisk())
            {
                if (P_VarName(out var param))
                {
                    if (!DoesVarExist(param?.Lex))
                    {
                        _bssCtr++;
                        _bss.Add(new BssData(GenerateBssName(param?.Lex), param?.Lex, "resd", "1", AsmDataType.Num));
                    }

                    _procs.Add($"mov eax, DWORD[esp+{(passByVals.Count + passByRefs.Count) * 4 + 4}]");
                    _procs.Add($"mov eax, DWORD[eax]");
                    _procs.Add($"mov {DetermineAsmOperand(param?.Lex)}, eax");
                    passByRefs.Add(new KeyValuePair<Token.Token?, int>(param, (passByVals.Count + passByRefs.Count) * 4 + 4));
                    return P_VarListing(passByVals, passByRefs);
                }
            }
            
            return false;
        }

        /// <summary>
        /// ,[VARIABLE LIST] | ) { [STATEMENT] }
        /// </summary>
        /// <returns></returns>
        private bool P_VarListing(ArrayList passByVals, ArrayList passByRefs)
        {
            if (P_Comma())
            {
                if (P_VarList(passByVals, passByRefs))
                {
                    return true;
                }
            }

            if (P_RParen())
            {
                if (P_LBrace())
                {
                    if (P_Statement(true))
                    {
                        ResetVariables(passByRefs);
                        _procs.Add("ret");
                        return P_Rbrace();
                    }
                }
            }

            return false;
        }

        private bool P_VarsListed()
        {
            if (P_Comma())
            {
                return P_VarPassIn();
            }

            return P_RParen() && P_Semicolon();
        }

        private bool P_VarPassIn()
        {
            if (P_VarName(out var param))
            {
                _text.Add($"push {FindAsmName(param?.Lex)}");
                return P_VarsListed();
            }

            return false;
        }

        private bool P_VarPass()
        {
            if (P_RParen() && P_Semicolon()) return true;
            return P_VarPassIn();
        }

        /// <summary>
        /// PROCEDURE [VARIABLE NAME] ([VARIABLE PASSING]
        /// </summary>
        /// <returns></returns>
        private bool P_ProcDeclStmt()
        {
            if (P_Proc())
            {
                if (P_VarName(out var procName))
                {
                    ArrayList passByVals = new ArrayList();
                    ArrayList passByRefs = new ArrayList();

                    _procs.Add($"{procName?.Lex}:");
                    if (P_LParen())
                    {
                        return P_VarPassing(passByVals, passByRefs);
                    }
                }
            }

            return false;
        }

        private bool P_ProcCallStmt(Token.Token? procName)
        {
            if (P_LParen())
            {
                if (P_VarPass())
                {
                    _text.Add($"call {procName?.Lex}");
                    return true;
                }
            }

            return false;
        }
    }
}