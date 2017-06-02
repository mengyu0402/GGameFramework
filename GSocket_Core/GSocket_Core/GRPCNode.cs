using System.Reflection;
using System;

namespace GSockets
{
    public class GRPCNode
    {
        public string key;
        public uint id;
        public MethodInfo method;
        public object obj;
        public Type type;

        public object Invoke<TClass>(TClass tobj, object message)
        {
            return method.Invoke(this.obj, new object[] { tobj, message });
        }
    }
}
