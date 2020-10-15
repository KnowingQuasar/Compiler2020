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
            public readonly string Name;

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

            public PArray(string name, ArrayData ad)
            {
                _arrayChunks = new List<ArrayData> {ad};
                Name = name;
            }

            public int ComputeAllocSpace()
            {
                if (_arrayChunks == null || _arrayChunks.Count == 0) return -1;

                var space = _arrayChunks.Aggregate(1, (current, arrayChunk) => current * (arrayChunk.Rbound - arrayChunk.Lbound + 1));

                return space * 4;
            }

            public int GetRef(List<int> indices)
            {
                if (indices.Count != _arrayChunks.Count) return -1;
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

                return indices.Select((t, j) => mappingValues[j] * t).Sum() - relocationFactor;
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

        private bool P_ArrayStmt()
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
                                            var arr = new PArray(arrName.Lex, arrData);
                                            _arrs.Add(arr);
                                            P_ArrayList(arr);
                                            _arrCtr++;
                                            _bss.Add(new BssData($"arr_{_arrCtr}_{arrName.Lex}", arrName.Lex, "resb",
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

        private bool P_ArrayAssignStmt(Token.Token assignee)
        {
            if (P_LeftBracket())
            {
                if (P_PosOrNeg(out var num))
                {
                    if (int.TryParse(num.Lex, out var idx))
                    {
                        var indices = new List<int> {idx};
                        return P_ArrayAssignList(assignee, indices);
                    }
                }
               
            }

            return false;
        }

        private bool P_ArrayAssignList(Token.Token assignee, List<int> indices)
        {
            if (P_Comma())
            {
                if (P_PosOrNeg(out var num))
                {
                    if (int.TryParse(num.Lex, out var idx))
                    {
                        indices.Add(idx);
                        return P_ArrayAssignList(assignee, indices);
                    }
                }
            }

            if (P_RightBracket())
            {
                if (P_Eq())
                {
                    if (P_PosOrNeg(out var num))
                    {
                        if (int.TryParse(num.Lex, out var val))
                        {
                            var arr = _arrs.FirstOrDefault(x => x.Name == assignee.Lex);
                            if (arr == null) return false;
                            _text.Add($"mov edi, {arr.GetRef(indices).ToString()}");
                            _text.Add($"add edi, {FindAsmName(assignee.Lex)}");
                            _text.Add($"mov DWORD[edi], {val.ToString()}");
                            _text.Add("xor edi, edi");
                            return P_Semicolon();
                        }
                    }
                }
            }

            return false;
        }
    }
}