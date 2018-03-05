using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class ObjectExpression : Expression
    {
        public IEnumerable<Property> Properties;
    }
}