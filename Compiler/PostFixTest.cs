using System;
using System.Collections.Generic;
using System.Globalization;

namespace Compiler
{
    public static class PostFixTest
    {
        private static bool IsOperator(char op)
        {
            return op == '^' || op == '*' || op == '/' || op == '+' || op == '-';
        }

        private static int DeterminePrecedence(char op)
        {
            switch (op)
            {
                case '^':
                    return 3;
                case '*':
                case '/':
                    return 2;
                case '+':
                case '-':
                    return 1;
                default:
                    return 0;
            }
        }

        private static bool IsHigherPriority(char op1, char op2)
        {
            return DeterminePrecedence(op1) > DeterminePrecedence(op2);
        }

        public static int EvalPostfix(string postfix)
        {
            postfix += ")";
            var exp = new Stack<string>();
            var result = 0;
            foreach (var c in postfix)
            {
                if (char.IsDigit(c))
                {
                    exp.Push(c.ToString());
                }
                else if (IsOperator(c))
                {
                    if ((!exp.TryPop(out var op1) ||
                         !exp.TryPop(out var op2)) ||
                        !(int.TryParse(op1, out var num1) && int.TryParse(op2, out var num2))) break;

                    switch (c)
                    {
                        case '*':
                            exp.Push((num2 * num1).ToString());
                            break;
                        case '/':
                            exp.Push((num2 / num1).ToString());
                            break;
                        case '^':
                            exp.Push((Math.Pow(num2, num1).ToString(CultureInfo.InvariantCulture)));
                            break;
                        case '+':
                            exp.Push((num2 + num1).ToString());
                            break;
                        case '-':
                            exp.Push((num2 - num1).ToString());
                            break;
                    }

                    result = int.Parse(exp.Peek());
                }
                else if (c == ')')
                {
                    break;
                }
                else
                {
                    throw new Exception("Invalid postfix.");
                }
            }

            return result;
        }

        public static string InfixToPostfix(string infix)
        {
            infix += ')';
            var postfixStack = new Stack<char>();
            postfixStack.Push('(');
            var postfix = "";

            foreach (var c in infix)
            {
                if (char.IsLetterOrDigit(c))
                {
                    postfix += c;
                }
                else if (c == '(')
                {
                    postfixStack.Push('(');
                }
                else if (IsOperator(c))
                {
                    for (;;)
                    {
                        if (!postfixStack.TryPeek(out var potentialOp)) throw new Exception("Invalid infix.");

                        if (IsOperator(potentialOp) && IsHigherPriority(potentialOp, c))
                        {
                            postfixStack.Pop();
                            postfix += potentialOp;
                        }
                        else
                        {
                            break;
                        }
                    }

                    postfixStack.Push(c);
                }
                else if (c == ')')
                {
                    for (;;)
                    {
                        if (!postfixStack.TryPop(out var stackItem)) throw new Exception("Invalid infix.");

                        if (stackItem == '(') break;

                        postfix += stackItem;
                    }
                }
            }

            return postfix;
        }
    }
}