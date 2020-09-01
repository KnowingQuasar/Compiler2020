using System;
using System.Collections.Generic;

namespace Compiler.Token
{
    public class Token
    {
        public TokenType Type { get; set; }

        public string Lex { get; set; }

        public int Line { get; set; }

        protected bool Equals(Token other)
        {
            return Type == other.Type && string.Equals(Lex, other.Lex) && Line == other.Line && Col == other.Col;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Token) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Type;
                hashCode = (hashCode * 397) ^ (Lex != null ? Lex.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Line;
                hashCode = (hashCode * 397) ^ Col;
                return hashCode;
            }
        }

        public int Col { get; set; }

        public Token(TokenType type, string lex, int line, int col)
        {
            Type = type;
            Lex = lex;
            Line = line;
            Col = col;
        }

        public void PrintToken()
        {
            Console.WriteLine($"Token: {Type} | Lexeme: {Lex} | Line: {Lex} | Col: {Col}");
        }
    }
}