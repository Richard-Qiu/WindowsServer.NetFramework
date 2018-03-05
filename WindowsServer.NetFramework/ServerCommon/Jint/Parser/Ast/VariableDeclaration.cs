using System.Collections.Generic;

namespace WindowsServer.Jint.Parser.Ast
{
    public class VariableDeclaration : Statement
    {
        public IEnumerable<VariableDeclarator> Declarations;
        public string Kind;
    }
}