﻿using WindowsServer.Jint.Native;

namespace WindowsServer.Jint.Runtime.Interop
{
    /// <summary>
    /// When implemented, converts a CLR value to a <see cref="JsValue"/> instance
    /// </summary>
    public interface IObjectConverter
    {
        bool TryConvert(object value, out JsValue result);
    }
}
