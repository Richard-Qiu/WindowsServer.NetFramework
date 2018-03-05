using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class BlockStatement : Statement
    {
        public IEnumerable<Statement> Body;
    }
}