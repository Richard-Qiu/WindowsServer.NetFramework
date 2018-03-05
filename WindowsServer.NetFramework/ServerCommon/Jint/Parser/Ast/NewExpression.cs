using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class NewExpression : Expression
    {
        public Expression Callee;
        public IEnumerable<Expression> Arguments;
    }
}