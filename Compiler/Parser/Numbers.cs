
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private Token.Token P_IntConst()
        {
            if (_curr.Type != TokenType.IntConst) return null;
            var intCont = _curr;
            _curr = _scanner.GetNextToken();
            return intCont;
        }
        
        /// <summary>
        /// [variable name] = [exp] ;
        /// </summary>
        /// <returns></returns>
        private bool P_NumAssignStmt()
        {
            var assignee = P_VarName();
            if(assignee != null) {
                if (P_Eq())
                {
                    if (P_Exp(assignee, false))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool P_NumDeclStmt()
        {
            if (_curr.Type != TokenType.Num) return false;
            _curr = _scanner.GetNextToken();
            var varName = P_VarName();
            return varName != null && P_Delta(varName);
        }

        private bool P_Delta(Token.Token declaredVar)
        {
            if (P_Semicolon())
            {
                _bssCtr++;
                _bss.Add(new BssData(GenerateBssName(declaredVar.Lex), declaredVar.Lex, "resd", "1"));
                return true;
            }

            return P_Eq() && P_Exp(declaredVar, true);

//            if (P_Eq())
//            {
//                _lHand = _curr.Lex;
//                _lHandType = _curr.Type;
//                if (P_IntConst() != null)
//                {
//                    if (P_Semicolon())
//                    {
//                        _dataCtr++;
//                        _data.Add(new PData(GenerateDataName(_lastVarName), _lastVarName, "dd", _lHand));
//                        return true;
//                    }
//                    
//                    return P_Ae() && P_Semicolon();
//                }
//
//                _lHand = _lastVarName;
//                _lHandType = TokenType.VarName;
//                if (P_VarName() != null)
//                {
//                    if (P_Semicolon())
//                    {
//                        _bssCtr++;
//                        _bss.Add(new BssData(GenerateBssName(_lHand), _lHand, "resd", "1"));
//                        _text += $"mov edi, DWORD[{FindAsmName(_lastVarName)}]\n";
//                        _text += $"mov DWORD[{FindAsmName(_lHand)}], edi\n";
//                        return true;
//                    }
//
//                    return P_Ae() && P_Semicolon();
//                }
//
//                return P_Exp() && P_Semicolon();
//            }
//
//            return false;
        }

        private bool P_Exp(Token.Token assignee, bool decl = false)
        {
            var lHandOp = P_Li();
            if (lHandOp == null) return false;
            var op = P_Operator();
            if (op == null)
            {
                if (!P_Semicolon()) return true;
                
                if (lHandOp.Type == TokenType.IntConst)
                {
                    if (decl)
                    {
                        _dataCtr++;
                        _data.Add(new PData(GenerateDataName(assignee.Lex), assignee.Lex, "dd", lHandOp.Lex));
                    }
                    else
                    {
                        if (FindAsmName(assignee.Lex) == null) return false;
                        _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], {lHandOp.Lex}\n";
                    }
                }
                else
                {
                    if(FindAsmName(lHandOp.Lex) != null)
                    {
                        if (decl)
                        {
                            _bssCtr++;
                            _bss.Add(new BssData(GenerateBssName(assignee.Lex), assignee.Lex, "resd", "1"));
                        }

                        _text += $"mov edi, DWORD[{FindAsmName(lHandOp.Lex)}]\n";
                        if (FindAsmName(assignee.Lex) == null) return false;
                        _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], edi\n";
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                var rHand = P_Li();
                if (rHand == null) return false;
                var asmOp = "";
                var asmRHand = rHand.Type == TokenType.IntConst ? rHand.Lex : $"DWORD[{FindAsmName(rHand.Lex)}]";
                var asmLHand = lHandOp.Type == TokenType.IntConst ? lHandOp.Lex : $"DWORD[{FindAsmName(lHandOp.Lex)}]";
                switch (op.Type)
                {
                    case TokenType.Plus:
                        asmOp = "add";
                        break;
                    case TokenType.Minus:
                        asmOp = "sub";
                        break;
                    case TokenType.Asterisk:
                        asmOp = "imul";
                        break;
                }

                if (op.Type == TokenType.Pow)
                {
                    _expCtr++;
                    _text += "xor edi, edi\n";
                    _text += "mov eax, 0x00000001\n";
                    _text += $"_exp_top_{_expCtr}:\n";
                    _text += $"cmp edi, {asmRHand}\n";
                    _text += $"jz _exp_out_{_expCtr}\n";
                    _text += $"imul eax, {asmLHand}\n";
                    _text += "inc edi\n";
                    _text += $"jmp _exp_top_{_expCtr}\n";
                    _text += $"_exp_out_{_expCtr}:\n";
                    _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], eax\n";
                }
                else
                {
                    _text += $"mov edi, {asmLHand}\n";
                    _text += $"{asmOp} edi, {asmRHand}\n";
                    if (FindAsmName(assignee.Lex) == null) return false;
                    _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], edi\n";
                }

                return P_Semicolon();
            }

            return true;
        }

        private Token.Token P_Li()
        {
            return P_Op();
        }

        private Token.Token P_Operator()
        {
            var op = P_Plus();
            op = op ?? P_Minus();
            op = op ?? P_Multiply();
            op = op ?? P_Pow();

            return op;
        }

//        private bool P_Ae()
//        {
//            var op = _curr.Type;
//            if (P_Plus() || P_Minus() || P_Multiply())
//            {
//                var rHand = _curr.Lex;
//                var rType = _curr.Type;
//                if (P_Li())
//                {
//                    string asmOp;
//                    switch (op)
//                    {
//                        case TokenType.Plus:
//                            asmOp = "add";
//                            break;
//                        case TokenType.Minus:
//                            asmOp = "sub";
//                            break;
//                        case TokenType.Asterisk:
//                            asmOp = "imul";
//                            break;
//                        default:
//                            return false;
//                    }
//                    
//                    string lRef;
//                    if (_lHandType == TokenType.IntConst)
//                    {
//                        lRef = _lHand;
//                    }
//                    else
//                    {
//                        lRef = $"DWORD[{FindAsmName(_lHand)}]\n";
//                        
//                    }
//                    
//                    _text += $"mov edi, {lRef}\n";
//
//                    string rRef;
//                    if (rType == TokenType.IntConst)
//                    {
//                        rRef = rHand;
//                    }
//                    else
//                    {
//                        rRef = $"DWORD[{FindAsmName(rHand)}]\n";
//                    }
//
//                    _text += $"{asmOp} edi, {rRef}\n";
//                    if (_assignee == "")
//                    {
//                        _text += $"mov {lRef}, edi\n";
//                    }
//                    else
//                    {
//                        _text += $"mov {_assignee}, edi\n";
//                    }
//                }
//
//                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
//                return P_Ae() || true;
//            }
//            
//            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
//            return (P_Divide() && P_Li() && P_Ae()) || (P_Pow() && P_Li() && P_Ae()) || true;
//        }

        private Token.Token P_Op()
        {
            var operand = P_PosOrNeg();
            return operand ?? P_VarName();
        }

        private Token.Token P_PosOrNeg()
        {
            return P_IntConst();
        }

        private Token.Token P_Plus()
        {
            if (_curr.Type != TokenType.Plus) return null;
            var op = _curr;
            _curr = _scanner.GetNextToken();
            return op;
        }

        private Token.Token P_Minus()
        {
            if (_curr.Type != TokenType.Minus) return null;
            var op = _curr;
            _curr = _scanner.GetNextToken();
            return op;

        }

        private Token.Token P_Divide()
        {
            if (_curr.Type != TokenType.Slash) return null;
            var op = _curr;
            _curr = _scanner.GetNextToken();
            return op;

        }

        private Token.Token P_Multiply()
        {
            if (_curr.Type != TokenType.Asterisk) return null;
            var op = _curr;
            _curr = _scanner.GetNextToken();
            return op;

        }

        private Token.Token P_Pow()
        {
            if (_curr.Type != TokenType.Pow) return null;
            var op = _curr;
            _curr = _scanner.GetNextToken();
            return op;

        }
    }
}