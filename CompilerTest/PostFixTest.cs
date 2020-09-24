using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompilerTest
{
    [TestClass]
    public class PostFixTest
    {
        [TestMethod]
        public void T1()
        {
            var t = Compiler.PostFixTest.InfixToPostfix("21^2*4+3^(1+1)");
            var a = Compiler.PostFixTest.EvalPostfix(t);
            Console.Out.WriteLine(t);
        } 
    }
}