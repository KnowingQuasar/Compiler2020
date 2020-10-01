using System.Collections.Generic;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        private bool PerformExpression(Token.Token assignee)
        {
            var postfixStack = new Stack<string>();
            var exp = new Queue<string>();
            if (!P_Exp(postfixStack, exp)) return false;
            for (;;)
            {
                if (!postfixStack.TryPop(out var val)) break;
                exp.Enqueue(val);
            }

            if (exp.Count == 1)
            {
                var val = exp.Dequeue();
                if (int.TryParse(val, out var result))
                {
                    _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], {result.ToString()}\n";
                    return true;
                }

                _text += $"mov esi, DWORD[{FindAsmName(val)}]\n";
                _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], esi\n";
                return true;
            }
            var reg = ExecutePostfix(exp);
            if (reg == null) return false;
            _text += $"mov esi, DWORD[{FindAsmName(reg)}]\n";
            _text += $"mov DWORD[{FindAsmName(assignee.Lex)}], esi\n";
            return true;
        }
        
        private Token.Token P_IntConst()
        {
            if (_curr.Type != TokenType.IntConst)
            {
                if (_curr.Type != TokenType.Minus) return null;
                _curr = _scanner.GetNextToken();
                if (_curr.Type != TokenType.IntConst) return null;
                var num = _curr;
                _curr = _scanner.GetNextToken();
                return new Token.Token(TokenType.IntConst, $"-{num.Lex}", num.Line, num.Col);

            }

            var intCont = _curr;
            _curr = _scanner.GetNextToken();
            return intCont;
        }

        /// <summary>
        /// [variable name] = [exp] ;
        /// </summary>
        /// <returns></returns>
        private bool P_NumAssignStmt(Token.Token assignee)
        {
            if (!P_Eq()) return false;
            return PerformExpression(assignee) && P_Semicolon();
        }

        private bool P_NumDeclStmt()
        {
            if (_curr.Type != TokenType.Num) return false;
            _curr = _scanner.GetNextToken();
            var varName = P_VarName();
            if (varName != null)
            {
                _bssCtr++;
                _bss.Add(new BssData(GenerateBssName(varName.Lex), varName.Lex, "resd", "1"));
                return P_Delta(varName);
            }

            return false;
        }

        private bool P_Delta(Token.Token declaredVar)
        {
            if (P_Semicolon())
            {
                _bssCtr++;
                _bss.Add(new BssData(GenerateBssName(declaredVar.Lex), declaredVar.Lex, "resd", "1"));
                return true;
            }

            if (!P_Eq()) return false;

            var postfixStack = new Stack<string>();
            var exp = new Queue<string>();
            if (!P_Exp(postfixStack, exp)) return false;
            for (;;)
            {
                if (!postfixStack.TryPop(out var val)) break;
                exp.Enqueue(val);
            }

            if (exp.Count == 1)
            {
                var val = exp.Dequeue();
                if (int.TryParse(val, out var result))
                {
                    _text += $"mov DWORD[{FindAsmName(declaredVar.Lex)}], {result.ToString()}\n";
                    return P_Semicolon();
                }

                _text += $"mov esi, DWORD[{FindAsmName(val)}]\n";
                _text += $"mov DWORD[{FindAsmName(declaredVar.Lex)}], esi\n";
                return P_Semicolon();
            }

            var reg = ExecutePostfix(exp);
            if (P_Semicolon())
            {
                _text += $"mov esi, DWORD[{FindAsmName(reg)}]\n";
                _text += $"mov DWORD[{FindAsmName(declaredVar.Lex)}], esi\n";
                return true;
            }

            return false;
        }

        private string DetermineAsmOperand(string operand)
        {
            if (operand == "edi") return "edi";
            return int.TryParse(operand, out var val) ? val.ToString() : "DWORD[" + FindAsmName(operand) + "]";
        }

        private string ExecuteOperator(string op1, string op2, string oper)
        {
            if (!IsOperator(oper)) return null;

            _tmpCtr++;
            _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));
            
            switch (oper)
            {
                case "+":
                    _text += $"mov esi, {DetermineAsmOperand(op2)}\n";
                    _text += $"add esi, {DetermineAsmOperand(op1)}\n";
                    _text += $"mov DWORD[_{_tmpCtr}_tmp], esi\n";
                    break;
                case "-":
                    _text += $"mov esi, {DetermineAsmOperand(op2)}\n";
                    _text += $"sub esi, {DetermineAsmOperand(op1)}\n";
                    _text += $"mov DWORD[_{_tmpCtr}_tmp], esi\n";
                    break;
                case "*":
                    _text += $"mov esi, {DetermineAsmOperand(op2)}\n";
                    _text += $"imul esi, {DetermineAsmOperand(op1)}\n";
                    _text += $"mov DWORD[_{_tmpCtr}_tmp], esi\n";
                    break;
                case "^":
                    _expCtr++;
                    _text += "xor esi, esi\n";
                    _text += "mov eax, 0x00000001\n";
                    _text += $"_exp_top_{_expCtr}:\n";
                    _text += $"cmp esi, {DetermineAsmOperand(op1)}\n";
                    _text += $"jz _exp_out_{_expCtr}\n";
                    _text += $"imul eax, {DetermineAsmOperand(op2)}\n";
                    _text += "inc esi\n";
                    _text += $"jmp _exp_top_{_expCtr}\n";
                    _text += $"_exp_out_{_expCtr}:\n";
                    _text += $"mov DWORD[_{_tmpCtr}_tmp], eax\n";
                    break;
                default:
                    return null;
            }

            return $"_{_tmpCtr}_tmp";
        }
        
        private string ExecutePostfix(Queue<string> exp)
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

                    var tmpVal = ExecuteOperator(op1, op2, val);
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

        private bool P_Exp(Stack<string> postfixStack, Queue<string> exp)
        {
            return P_Li(postfixStack, exp) & P_Ae(postfixStack, exp);
        }

        private bool P_Li(Stack<string> postfixStack, Queue<string> exp)
        {
            var op = P_Op();
             if (op == null) return P_Paren(postfixStack, exp);
            exp.Enqueue(op.Lex);
            return true;

        }

        private bool P_Paren(Stack<string> postfixStack, Queue<string> exp)
        {
            if (_curr.Type != TokenType.Lparen) return false;
            postfixStack.Push("(");
            _curr = _scanner.GetNextToken();
            P_Exp(postfixStack, exp);
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

        private bool P_Ae(Stack<string> postfixStack, Queue<string> exp)
        {
            if (P_Multiply())
            {
                return HandleOperator(postfixStack, exp, "*") && P_Li(postfixStack, exp) && P_Ae(postfixStack, exp);
            }

            if (P_Divide())
            {
                return HandleOperator(postfixStack, exp, "/") && P_Li(postfixStack, exp) && P_Ae(postfixStack, exp);
            }

            if (P_Pow())
            {
                return HandleOperator(postfixStack, exp, "^") && P_Li(postfixStack, exp) && P_Ae(postfixStack, exp);
            }
            if (P_Plus())
            {
                return HandleOperator(postfixStack, exp, "+") && P_Li(postfixStack, exp) && P_Ae(postfixStack, exp);
            }

            if (P_Minus())
            {
                return HandleOperator(postfixStack, exp, "-") && P_Li(postfixStack, exp) && P_Ae(postfixStack, exp);
            }

            return true;
        }

        private Token.Token P_Op()
        {
            var operand = P_PosOrNeg();
            return operand ?? P_VarName();
        }

        private Token.Token P_PosOrNeg()
        {
            return P_IntConst();
        }

        private bool P_Plus()
        {
            if (_curr.Type != TokenType.Plus) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Minus()
        {
            if (_curr.Type != TokenType.Minus) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Divide()
        {
            if (_curr.Type != TokenType.Slash) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Multiply()
        {
            if (_curr.Type != TokenType.Asterisk) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Pow()
        {
            if (_curr.Type != TokenType.Pow) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }
    }
}