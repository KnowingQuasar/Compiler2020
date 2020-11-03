using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Compiler.Token;

namespace Compiler.Parser
{
    public partial class Parser
    {
        public class PArray
        {
            private readonly List<ArrayData> _arrayChunks;
            public readonly string? Name;

            public class ArrayData
            {
                public readonly int Lbound;
                public readonly int Rbound;

                public ArrayData(int lbound, int rbound)
                {
                    Lbound = lbound;
                    Rbound = rbound;
                }
            }

            public PArray(string? name, ArrayData ad)
            {
                _arrayChunks = new List<ArrayData> {ad};
                Name = name;
            }

            public int ComputeAllocSpace()
            {
                if (_arrayChunks.Count == 0) return -1;

                var space = _arrayChunks.Aggregate(1, (current, arrayChunk) => current * (arrayChunk.Rbound - arrayChunk.Lbound + 1));

                return space * 4;
            }

            public ArrayList GetRef(ArrayList indices)
            {
                if (indices.Count != _arrayChunks.Count) return null;
                var mappingValues = new List<int>();
                var relocationFactor = 0;
                for (var i = 0; i < _arrayChunks.Count; i++)
                {
                    var mVal = 1;
                    for (var j = i + 1; j < _arrayChunks.Count; j++)
                    {
                        mVal *= (_arrayChunks[j].Rbound - _arrayChunks[j].Lbound + 1);
                    }

                    mappingValues.Add(mVal);
                    relocationFactor += _arrayChunks[i].Lbound * mVal;
                }

                var z = 0;
                var tmp = new ArrayList {$"mov eax, 0"};
                foreach (var idx in indices)
                {
                    tmp.Add($"mov edi, {mappingValues[z]}");
                    tmp.Add($"imul edi, DWORD[{((Token.Token) idx!)?.Lex}]");
                    tmp.Add("add eax, edi");
                    z++;
                }

                tmp.Add($"sub eax, {relocationFactor}");
                return tmp;
            }

            public void AddArrData(ArrayData ad)
            {
                _arrayChunks.Add(ad);
            }
        }

        private bool P_Array()
        {
            return CheckToken(TokenType.Array);
        }

        private bool P_LeftBracket()
        {
            return CheckToken(TokenType.Lbrack);
        }

        private bool P_RightBracket()
        {
            if (_curr.Type != TokenType.Rbrack) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_Comma()
        {
            if (_curr.Type != TokenType.Comma) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_ArrayList(PArray arr)
        {
            if (P_Comma())
            {
                if (P_PosOrNeg(out var num))
                {
                    if (int.TryParse(num.Lex, out var lbound))
                    {
                        if (P_Dot() && P_Dot())
                        {
                            if (P_PosOrNeg(out num))
                            {
                                if (int.TryParse(num.Lex, out var rbound))
                                {
                                    arr.AddArrData(new PArray.ArrayData(lbound, rbound));
                                    return P_ArrayList(arr);
                                }
                            }
                        }
                    }
                }
            }

            return P_RightBracket();
        }

        private bool P_ArrayStmt(bool isProc)
        {
            if (P_Array())
            {
                if (P_VarName(out var arrName))
                {
                    if (P_LeftBracket())
                    {
                        if (P_PosOrNeg(out var num))
                        {
                            if (int.TryParse(num.Lex, out var lbound))
                            {
                                if (P_Dot() && P_Dot())
                                {
                                    if (P_PosOrNeg(out num))
                                    {
                                        if (int.TryParse(num.Lex, out var rbound))
                                        {
                                            var arrData = new PArray.ArrayData(lbound, rbound);
                                            var arr = new PArray(arrName?.Lex, arrData);
                                            _arrs.Add(arr);
                                            P_ArrayList(arr);
                                            _arrCtr++;
                                            _bss.Add(new BssData($"arr_{_arrCtr}_{arrName?.Lex}", arrName?.Lex, "resb",
                                                arr.ComputeAllocSpace().ToString()));
                                            return P_Semicolon();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                LogError("Unexpected token in array declaration.");
            }

            return false;
        }

        private bool P_ArrayAssignStmt(bool isProc, Token.Token? assignee)
        {
            if (P_LeftBracket())
            {
                _tmpCtr++;
                var idx = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));

                if (PerformExpression(isProc, idx))
                {
                    var indices = new ArrayList {idx};
                    return P_ArrayAssignList(isProc, assignee, indices);
                }
               
            }

            return false;
        }

        private bool P_ArrayAssignList(bool isProc, Token.Token? assignee, ArrayList indices)
        {
            if (P_Comma())
            {
                _tmpCtr++;
                var idx = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));

                if (PerformExpression(isProc, idx))
                {
                    indices.Add(idx);
                    return P_ArrayAssignList(isProc, assignee, indices);
                }
            }

            if (P_RightBracket())
            {
                if (P_Eq())
                {
                    _tmpCtr++;
                    var val = new Token.Token(TokenType.VarName, $"_{_tmpCtr}_tmp", -1, -1);
                    _bss.Add(new BssData($"_{_tmpCtr}_tmp", $"_{_tmpCtr}_tmp", "resd", "1"));

                    if (PerformExpression(isProc, val))
                    {
                        var arr = _arrs.FirstOrDefault(x => x.Name == assignee.Lex);
                        if (arr == null) return false;
                        var tmp = new ArrayList();
                        tmp.AddRange(arr.GetRef(indices));

                        tmp.Add($"add eax, {FindAsmName(assignee?.Lex)}");
                        tmp.Add($"mov edi, {DetermineAsmOperand(val.Lex)}");
                        tmp.Add($"mov DWORD[eax], edi");
                        tmp.Add("xor edi, edi");
                        
                        AddToCorrectSection(isProc, tmp);
                        return P_Semicolon();
                    }
                }
            }

            return false;
        }
    }
}