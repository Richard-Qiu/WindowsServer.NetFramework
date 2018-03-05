using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class ArrayExpression : Expression
    {
        public IEnumerable<Expression> Elements;
    }
}