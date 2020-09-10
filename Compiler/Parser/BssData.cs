using System.ComponentModel.DataAnnotations;

namespace Compiler.Parser
{
    public class BssData
    {
        public string AsmName;
        public string ActualName;
        public string DataType;
        public string Size;

        public BssData(string asmName, string actualName, string dataType, string size)
        {
            AsmName = asmName.ToLower();
            ActualName = actualName.ToLower();
            DataType = dataType;
            Size = size;
        }
    }
}