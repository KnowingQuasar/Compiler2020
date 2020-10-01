namespace Compiler.Parser
{
    public class PData
    {
        public readonly string AsmName;
        public readonly string ActualName;
        public readonly string DataType;
        public readonly string Value;

        public PData(string asmName, string actualName, string dataType, string value)
        {
            AsmName = asmName;
            ActualName = actualName;
            DataType = dataType;
            Value = value;
        }
    }
}