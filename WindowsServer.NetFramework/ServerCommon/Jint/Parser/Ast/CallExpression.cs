using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class CallExpression : Expression
    {
        public Expression Callee;
        public IEnumerable<Expression> Arguments;
    }
}