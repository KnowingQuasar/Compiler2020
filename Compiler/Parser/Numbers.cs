using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool PerformExpression(bool isProc, Token.Token? assignee)
        {
            var postfixStack = new Stack<string>();
            var exp = new Queue<string>();
            if (!P_Exp(isProc, postfixStack, exp)) return false;
            for (;;)
            {
                if (!postfixStack.TryPop(out var val)) break;
                exp.Enqueue(val);
            }

            var tmp = new ArrayList();
            if (exp.Count == 1)
            {
                var val = exp.Dequeue();
                if (int.TryParse(val, out var result))
                {
                    tmp.Add($"mov DWORD[{FindAsmName(assignee?.Lex)}], {result.ToString()}");
                    AddToCorrectSection(isProc, tmp);
                    return true;
                }

                tmp.Add($"mov esi, DWORD[{FindAsmName(val)}]");
                tmp.Add($"mov DWORD[{FindAsmName(assignee?.Lex)}], esi");
                AddToCorrectSection(isProc, tmp);
                return true;
            }

            var reg = ExecutePostfix(isProc, exp);
            tmp.Add($"mov esi, DWORD[{FindAsmName(reg)}]");
            tmp.Add($"mov DWORD[{FindAsmName(assignee?.Lex)}], esi");
            AddToCorrectSection(isProc, tmp);
            return true;
        }

        /// <summary>
        /// NUM
        /// </summary>
        /// <returns></returns>
        private bool P_Num()
        {
            return CheckToken(TokenType.Num);
        }

        private bool P_IntConst(out Token.Token? num)
        {
            if (_curr.Type != TokenType.IntConst)
            {
                if (_curr.Type == TokenType.Minus)
                {
                    _curr = _scanner.GetNextToken();
                    if (_curr.Type == TokenType.IntConst)
                    {
                        num = new Token.Token(TokenType.IntConst, $"-{_curr.Lex}", _curr.Line, _curr.Col);
                        _curr = _scanner.GetNextToken();
                        return true;
                    }
                }

                num = null;
                return false;
            }

            var part = _curr;
            num = _curr;
            _curr = _scanner.GetNextToken();

            if (P_Dot())
            {
                if (_curr.Type == TokenType.IntConst)
                {
                    num = new Token.Token(TokenType.FloatConst, $"{part?.Lex}.{_curr.Lex}", _curr.Line, _curr.Col);
                    _curr = _scanner.GetNextToken();
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// [variable name] = [exp] ;
        /// </summary>
        /// <returns></returns>
        private bool P_NumAssignStmt(bool isProc, Token.Token? assignee)
        {
            if (!P_Eq()) return false;
            return PerformExpression(isProc, assignee) && P_Semicolon();
        }

        private bool P_NumDeclStmt(bool isProc)
        {
            if (P_Num())
            {
                if (P_VarName(out var varName))
                {
                    _bssCtr++;
                    _bss.Add(new BssData(GenerateBssName(varName?.Lex), varName?.Lex, "resd", "1", AsmDataType.Num));
                    return P_Delta(isProc, varName);
                }
            }

            return false;
        }

        private bool P_Delta(bool isProc, Token.Token? declaredVar)
        {
            if (P_Semicolon())
            {
                _bssCtr++;
                _bss.Add(new BssData(GenerateBssName(declaredVar?.Lex), declaredVar?.Lex, "resd", "1",
                    AsmDataType.Num));
                return true;
            }

            if (!P_Eq()) return false;

            var postfixStack = new Stack<string>();
            var exp = new Queue<string>();
            if (!P_Exp(isProc, postfixStack, exp)) return false;
            for (;;)
            {
                if (!postfixStack.TryPop(out var val)) break;
                exp.Enqueue(val);
            }

            var tmp = new ArrayList();
            if (exp.Count == 1)
            {
                var val = exp.Dequeue();
                if (int.TryParse(val, out var result))
                {
                    tmp.Add($"mov DWORD[{FindAsmName(declaredVar.Lex)}], {result.ToString()}");
                    AddToCorrectSection(isProc, tmp);
                    return P_Semicolon();
                }

                if (float.TryParse(val, out var fresult))
                {
                    tmp.Add($"mov DWORD[{FindAsmName(declaredVar.Lex)}], __float32__({fresult.ToString()})");
                    AddToCorrectSection(isProc, tmp);
                    return P_Semicolon();
                }

                tmp.Add($"mov esi, DWORD[{FindAsmName(val)}]");
                tmp.Add($"mov DWORD[{FindAsmName(declaredVar.Lex)}], esi");
                AddToCorrectSection(isProc, tmp);
                return P_Semicolon();
            }

            var reg = ExecutePostfix(isProc, exp);
            if (P_Semicolon())
            {
                tmp.Add($"mov esi, DWORD[{FindAsmName(reg)}]");
                tmp.Add($"mov DWORD[{FindAsmName(declaredVar.Lex)}], esi");
                AddToCorrectSection(isProc, tmp);
                return true;
            }

            return false;
        }

        private string? ExecuteOperator(bool isProc, string op1, string op2, string oper)
        {
            if (!IsOperator(oper)) return null;

            _tmpCtr++;
            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));

            var tmp = new ArrayList();
            if (!DoesVarExist("flt_left_stack"))
            {
                _bss.Add(new BssData("flt_left_stack", "flt_left_stack", "resb", "4", AsmDataType.Float));
            }

            if (!DoesVarExist("flt_right_stack"))
            {
                _bss.Add(new BssData("flt_right_stack", "flt_right_stack", "resb", "4", AsmDataType.Float));
            }

            if (!DoesVarExist("flt_result"))
            {
                _bss.Add(new BssData("flt_result", "flt_result", "resb", "4", AsmDataType.Float));
            }
            
            var lhandType = GetTypeOfVar(op2);
            var rhandType = GetTypeOfVar(op1);

            switch (oper)
            {
                case "+":
                    if (lhandType == AsmDataType.Float || rhandType == AsmDataType.Float ||
                        lhandType == AsmDataType.FloatLiteral || rhandType == AsmDataType.FloatLiteral)
                    {
                        if (lhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op2)}]");
                        }
                        else if (lhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], __float32__({op2})");
                            tmp.Add("fld DWORD[flt_left_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], {op2}");
                            tmp.Add("fild DWORD[flt_left_stack]");
                        }

                        if (rhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op1)}]");
                        }
                        else if (rhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], __float32__({op1})");
                            tmp.Add("fld DWORD[flt_right_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], {op1}");
                            tmp.Add("fild DWORD[flt_right_stack]");
                        }

                        tmp.Add("fadd");
                        tmp.Add("fstp QWORD[flt_result]");
                        tmp.Add("fld QWORD[flt_result]");
                        tmp.Add($"fstp DWORD[_{_tmpCtr}_tmp]");
                    }
                    else
                    {
                        tmp.Add($"mov esi, {DetermineAsmOperand(op2)}");
                        tmp.Add($"add esi, {DetermineAsmOperand(op1)}");
                        tmp.Add($"mov DWORD[_{_tmpCtr}_tmp], esi");
                    }

                    break;
                case "-":
                    if (lhandType == AsmDataType.Float || rhandType == AsmDataType.Float ||
                        lhandType == AsmDataType.FloatLiteral || rhandType == AsmDataType.FloatLiteral)
                    {
                        if (lhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op2)}]");
                        }
                        else if (lhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], __float32__({op2})");
                            tmp.Add("fld DWORD[flt_left_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], {op2}");
                            tmp.Add("fild DWORD[flt_left_stack]");
                        }

                        if (rhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op1)}]");
                        }
                        else if (rhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], __float32__({op1})");
                            tmp.Add("fld DWORD[flt_right_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], {op1}");
                            tmp.Add("fild DWORD[flt_right_stack]");
                        }

                        tmp.Add("fsub");
                        tmp.Add("fstp QWORD[flt_result]");
                        tmp.Add("fld QWORD[flt_result]");
                        tmp.Add($"fstp DWORD[_{_tmpCtr}_tmp]");
                    }
                    else
                    {
                        tmp.Add($"mov esi, {DetermineAsmOperand(op2)}");
                        tmp.Add($"sub esi, {DetermineAsmOperand(op1)}");
                        tmp.Add($"mov DWORD[_{_tmpCtr}_tmp], esi");
                    }

                    break;
                case "*":
                    if (lhandType == AsmDataType.Float || rhandType == AsmDataType.Float ||
                        lhandType == AsmDataType.FloatLiteral || rhandType == AsmDataType.FloatLiteral)
                    {
                        if (lhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op2)}]");
                        }
                        else if (lhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], __float32__({op2})");
                            tmp.Add("fld DWORD[flt_left_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_left_stack], {op2}");
                            tmp.Add("fild DWORD[flt_left_stack]");
                        }

                        if (rhandType == AsmDataType.Float)
                        {
                            tmp.Add($"fld DWORD[{FindAsmName(op1)}]");
                        }
                        else if (rhandType == AsmDataType.FloatLiteral)
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], __float32__({op1})");
                            tmp.Add("fld DWORD[flt_right_stack]");
                        }
                        else
                        {
                            tmp.Add($"mov DWORD[flt_right_stack], {op1}");
                            tmp.Add("fild DWORD[flt_right_stack]");
                        }

                        tmp.Add("fmul");
                        tmp.Add("fstp QWORD[flt_result]");
                        tmp.Add("fld QWORD[flt_result]");
                        tmp.Add($"fstp DWORD[_{_tmpCtr}_tmp]");
                    }
                    else
                    {
                        tmp.Add($"mov esi, {DetermineAsmOperand(op2)}");
                        tmp.Add($"imul esi, {DetermineAsmOperand(op1)}");
                        tmp.Add($"mov DWORD[_{_tmpCtr}_tmp], esi");
                    }

                    break;
                case "^":
                    if (GetTypeOfVar(op1) == AsmDataType.Float || GetTypeOfVar(op2) == AsmDataType.Float)
                    {
                        return null;
                    }

                    _expCtr++;
                    tmp.Add("xor esi, esi");
                    tmp.Add("mov eax, 0x00000001");
                    tmp.Add($"_exp_top_{_expCtr}:");
                    tmp.Add($"cmp esi, {DetermineAsmOperand(op1)}");
                    tmp.Add($"jz _exp_out_{_expCtr}");
                    tmp.Add($"imul eax, {DetermineAsmOperand(op2)}");
                    tmp.Add("inc esi");
                    tmp.Add($"jmp _exp_top_{_expCtr}");
                    tmp.Add($"_exp_out_{_expCtr}:");
                    tmp.Add($"mov DWORD[_{_tmpCtr}_tmp], eax");
                    break;
                default:
                    return null;
            }

            AddToCorrectSection(isProc, tmp);

            return $"_{_tmpCtr}_tmp";
        }

        private string ExecutePostfix(bool isProc, Queue<string> exp)
        {
            var stack = new Stack<string>();
            var finalReg = "";
            while (exp.Count > 0)
            {
                var val = exp.Dequeue();
                if (IsOperator(val))
                {
                    if (stack.Count < 2) return null;

                    var op1 = stack.Pop();
                    var op2 = stack.Pop();

                    if (op1 == null || op2 == null) return null;

                    var tmpVal = ExecuteOperator(isProc, op1, op2, val);
                    finalReg = tmpVal;
                    stack.Push(tmpVal);
                }
                else
                {
                    stack.Push(val);
                }
            }

            return finalReg;
        }

        private bool P_Exp(bool isProc, Stack<string> postfixStack, Queue<string> exp)
        {
            return P_Li(isProc, postfixStack, exp) & P_Ae(isProc, postfixStack, exp);
        }

        private bool P_Li(bool isProc, Stack<string> postfixStack, Queue<string> exp)
        {
            if (!P_Op(isProc, out var op)) return P_Paren(isProc, postfixStack, exp);
            exp.Enqueue(op.Lex);
            return true;
        }

        private bool P_Paren(bool isProc, Stack<string> postfixStack, Queue<string> exp)
        {
            if (_curr.Type != TokenType.Lparen) return false;
            postfixStack.Push("(");
            _curr = _scanner.GetNextToken();
            P_Exp(isProc, postfixStack, exp);
            if (_curr.Type != TokenType.Rparen) return false;
            _curr = _scanner.GetNextToken();
            for (;;)
            {
                if (!postfixStack.TryPop(out var value)) return false;
                if (value == "(") break;
                exp.Enqueue(value);
            }

            return true;
        }

        private static bool IsOperator(string op)
        {
            return op == "^" || op == "*" || op == "/" || op == "+" || op == "-";
        }

        private static int DeterminePrecedence(string op)
        {
            switch (op)
            {
                case "^":
                    return 3;
                case "*":
                case "/":
                    return 2;
                case "+":
                case "-":
                    return 1;
                default:
                    return 0;
            }
        }

        private static bool IsHigherPriority(string op1, string op2)
        {
            return DeterminePrecedence(op1) >= DeterminePrecedence(op2);
        }

        private static bool HandleOperator(Stack<string> postfixStack, Queue<string> exp, string op)
        {
            for (;;)
            {
                if (!postfixStack.TryPeek(out var potentialOp)) break;

                if (IsOperator(potentialOp) && IsHigherPriority(potentialOp, op))
                {
                    postfixStack.Pop();
                    exp.Enqueue(potentialOp);
                }
                else
                {
                    break;
                }
            }

            postfixStack.Push(op);
            return true;
        }

        private bool P_Ae(bool isProc, Stack<string> postfixStack, Queue<string> exp)
        {
            if (P_Multiply())
            {
                return HandleOperator(postfixStack, exp, "*") && P_Li(isProc, postfixStack, exp) &&
                       P_Ae(isProc, postfixStack, exp);
            }

            if (P_Divide())
            {
                return HandleOperator(postfixStack, exp, "/") && P_Li(isProc, postfixStack, exp) &&
                       P_Ae(isProc, postfixStack, exp);
            }

            if (P_Pow())
            {
                return HandleOperator(postfixStack, exp, "^") && P_Li(isProc, postfixStack, exp) &&
                       P_Ae(isProc, postfixStack, exp);
            }

            if (P_Plus())
            {
                return HandleOperator(postfixStack, exp, "+") && P_Li(isProc, postfixStack, exp) &&
                       P_Ae(isProc, postfixStack, exp);
            }

            if (P_Minus())
            {
                return HandleOperator(postfixStack, exp, "-") && P_Li(isProc, postfixStack, exp) &&
                       P_Ae(isProc, postfixStack, exp);
            }

            return true;
        }

        private bool P_Op(bool isProc, out Token.Token? operand)
        {
            if (P_PosOrNeg(out operand)) return true;
            if (P_VarName(out operand))
            {
                if (P_Ref(isProc, operand, out var result))
                {
                    if (result != null) operand = result;
                    return true;
                }
            }

            return false;
        }

        private bool P_Ref(bool isProc, Token.Token? operand, out Token.Token? result)
        {
            if (P_Ap(isProc, operand, out result))
            {
                return true;
            }

            return true;
        }

        private bool P_Ap(bool isProc, Token.Token? operand, out Token.Token? result)
        {
            if (P_LeftBracket())
            {
                var indices = new ArrayList();
                for (;;)
                {
                    _tmpCtr++;
                    var idx = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                    _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));

                    if (PerformExpression(isProc, idx))
                    {
                        indices.Add(idx);
                        if (!P_Comma()) break;
                    }
                    else
                    {
                        break;
                    }
                }

                if (P_RightBracket())
                {
                    var arr = _arrs.FirstOrDefault(x => x.Name == operand.Lex);
                    if (arr == null)
                    {
                        result = null;
                        return false;
                    }

                    var tmp = new ArrayList();
                    tmp.AddRange(arr.GetRef(indices));

                    _tmpCtr++;
                    result = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                    _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1", AsmDataType.Num));
                    tmp.Add($"add eax, {FindAsmName(operand?.Lex)}");
                    tmp.Add($"mov eax, DWORD[eax]");
                    tmp.Add($"mov {DetermineAsmOperand(result.Lex)}, eax");
                    AddToCorrectSection(isProc, tmp);
                    return true;
                }
            }

            result = null;
            return false;
        }

        private bool P_PosOrNeg(out Token.Token? num)
        {
            return P_IntConst(out num);
        }

        private bool P_Plus()
        {
            return CheckToken(TokenType.Plus);
        }

        private bool P_Minus()
        {
            return CheckToken(TokenType.Minus);
        }

        private bool P_Divide()
        {
            return CheckToken(TokenType.Slash);
        }

        private bool P_Multiply()
        {
            return CheckToken(TokenType.Asterisk);
        }

        private bool P_Pow()
        {
            return CheckToken(TokenType.Pow);
        }
    }
}