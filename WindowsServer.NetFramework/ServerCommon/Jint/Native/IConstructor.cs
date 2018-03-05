using WindowsServer.Jint.Native.Object;

namespace WindowsServer.Jint.Native
{
    public interface IConstructor
    {
        JsValue Call(JsValue thisObject, JsValue[] arguments);
        ObjectInstance Construct(JsValue[] arguments);
    }
}
