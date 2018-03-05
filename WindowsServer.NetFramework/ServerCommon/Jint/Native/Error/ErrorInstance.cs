using WindowsServer.Jint.Native.Object;
using WindowsServer.Jint.Runtime;

namespace WindowsServer.Jint.Native.Error
{
    public class ErrorInstance : ObjectInstance
    {
        public ErrorInstance(Engine engine, string name)
            : base(engine)
        {
            FastAddProperty("name", name, true, false, true);
        }

        public override string Class
        {
            get
            {
                return "Error";
            }
        }

        public override string ToString()
        {
            return Engine.Error.PrototypeObject.ToString(this, Arguments.Empty).ToObject().ToString();
        }
    }
}
