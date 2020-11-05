namespace Compiler.Parser
{
    public class BssData
    {
        public readonly string AsmName;
        public readonly string? ActualName;
        public readonly string DataType;
        public readonly string Size;
        public readonly Parser.AsmDataType AsmDataType;

        public BssData(string asmName, string? actualName, string dataType, string size, Parser.AsmDataType asmDataType)
        {
            AsmName = asmName.ToLower();
            ActualName = actualName?.ToLower();
            DataType = dataType;
            Size = size;
            AsmDataType = asmDataType;
        }
    }
}