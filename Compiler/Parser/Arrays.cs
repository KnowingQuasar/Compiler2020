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
            public List<ArrayData> ArrayChunks;
            public string Name;

            public class ArrayData
            {
                public int Lbound;
                public int Rbound;

                public ArrayData(int lbound, int rbound)
                {
                    Lbound = lbound;
                    Rbound = rbound;
                }
            }

            public PArray(string name, ArrayData ad)
            {
                ArrayChunks = new List<ArrayData> {ad};
                Name = name;
            }

            public int ComputeAllocSpace()
            {
                if (ArrayChunks == null || ArrayChunks.Count == 0) return -1;

                var space = 1;
                foreach (var arrayChunk in ArrayChunks)
                {
                    space *= (arrayChunk.Rbound - arrayChunk.Lbound + 1);
                }

                return space * 4;
            }

            public int GetRef(List<int> indices)
            {
                if (indices.Count != ArrayChunks.Count) return -1;
                var mappingValues = new List<int>();
                var relocationFactor = 0;
                for (var i = 0; i < ArrayChunks.Count; i++)
                {
                    var mVal = 1;
                    for (var j = i + 1; j < ArrayChunks.Count; j++)
                    {
                        mVal *= (ArrayChunks[j].Rbound - ArrayChunks[j].Lbound + 1);
                    }

                    mappingValues.Add(mVal);
                    relocationFactor += ArrayChunks[i].Lbound * mVal;
                }

                return indices.Select((t, j) => mappingValues[j] * t).Sum() - relocationFactor;
            }

            public void AddArrData(ArrayData ad)
            {
                ArrayChunks.Add(ad);
            }
        }

        private bool P_Array()
        {
            if (_curr.Type != TokenType.Array) return false;
            _curr = _scanner.GetNextToken();
            return true;
        }

        private bool P_LeftBracket()
        {
            if (_curr.Type != TokenType.Lbrack) return false;
            _curr = _scanner.GetNextToken();
            return true;
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
                if (int.TryParse(P_PosOrNeg().Lex, out var lbound))
                {
                    if (P_Dot() && P_Dot())
                    {
                        if (int.TryParse(P_PosOrNeg().Lex, out var rbound))
                        {
                            arr.AddArrData(new PArray.ArrayData(lbound, rbound));
                            return P_ArrayList(arr);
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
                var arrName = P_VarName();
                if (arrName != null)
                {
                    if (P_LeftBracket())
                    {
                        if (int.TryParse(P_PosOrNeg().Lex, out var lbound))
                        {
                            if (P_Dot() && P_Dot())
                            {
                                if (int.TryParse(P_PosOrNeg().Lex, out var rbound))
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

                LogError("Unexpected token in array declaration.");
            }

            return false;
        }

        private bool P_ArrayAssignStmt(Token.Token assignee)
        {
            if (P_LeftBracket())
            {
                if (int.TryParse(P_PosOrNeg().Lex, out var idx))
                {
                    var indices = new List<int> {idx};
                    return P_ArrayAssignList(assignee, indices);
                }
            }

            return false;
        }

        private bool P_ArrayAssignList(Token.Token assignee, List<int> indices)
        {
            if (P_Comma())
            {
                if (int.TryParse(P_PosOrNeg().Lex, out var idx))
                {
                    indices.Add(idx);
                    return P_ArrayAssignList(assignee, indices);
                }
            }

            if (P_RightBracket())
            {
                if (P_Eq())
                {
                    if (int.TryParse(P_PosOrNeg().Lex, out var val))
                    {
                        var arr = _arrs.FirstOrDefault(x => x.Name == assignee.Lex);
                        if (arr == null) return false;
                        _text += $"mov edi, {arr.GetRef(indices).ToString()}\n";
                        _text += $"add edi, {FindAsmName(assignee.Lex)}\n";
                        _text += $"mov DWORD[edi], {val.ToString()}\n";
                        _text += "xor edi, edi\n";
                        return P_Semicolon();
                    }
                }
            }

            return false;
        }
    }
}