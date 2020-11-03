namespace Compiler.Parser
{
    public class BssData
    {
        public readonly string AsmName;
        public readonly string ActualName;
        public readonly string DataType;
        public readonly string Size;

        public BssData(string asmName, string? actualName, string dataType, string size)
        {
            AsmName = asmName.ToLower();
            ActualName = actualName.ToLower();
            DataType = dataType;
            Size = size;
        }
    }
}