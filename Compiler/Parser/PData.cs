namespace Compiler.Parser
{
    public class PData
    {
        public string AsmName;
        public string ActualName;
        public string DataType;
        public string Value;

        public PData(string asmName, string actualName, string dataType, string value)
        {
            AsmName = asmName;
            ActualName = actualName;
            DataType = dataType;
            Value = value;
        }
    }
}