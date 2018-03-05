using WindowsServer.Jint.Runtime;

namespace WindowsServer.Jint.Native
{
    public interface IPrimitiveInstance
    {
        Types Type { get; } 
        JsValue PrimitiveValue { get; }
    }
}
