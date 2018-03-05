using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class SequenceExpression : Expression
    {
        public IList<Expression> Expressions;
    }
}