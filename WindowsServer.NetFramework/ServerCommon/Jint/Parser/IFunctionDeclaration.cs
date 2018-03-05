using System.Collections.Generic;
using WindowsServer.Jint.Parser.Ast;

namespace WindowsServer.Jint.Parser
{
    public interface IFunctionDeclaration : IFunctionScope
    {
        Identifier Id { get; }
        IEnumerable<Identifier> Parameters { get; }
        Statement Body { get; }
        bool Strict { get; }
    }
}