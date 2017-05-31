using System;
using System.Reflection;
using System.Collections.Generic;

namespace GSockets.Client
{
    public class GRPCNode
    {
        public string       key;
        public uint         id;
        public MethodInfo   method;
        public object       obj;

        public void Invoke(object message)
        {
            method.Invoke(obj, new object[] { message });
        }
    }

    /// <summary>
    /// rpc client
    /// </summary>
    /// <typeparam name="TBuff"></typeparam>
    public class GRPCClient<TBuff, TRPC> : GTcpClient<TBuff> 
        where TBuff : IBuffStream
        where TRPC  : class, IRPCMessage
    {
        /// <summary>
        /// route id
        /// </summary>
        uint ROUTE_BEGIN = 0;

        /// <summary>
        /// rpc map
        /// </summary>
        Dictionary<uint, GRPCNode> rpcMap = new Dictionary<uint, GRPCNode>();

        /// <summary>
        /// rpc handler
        /// </summary>
        Dictionary<string, GRPCNode> rpcHandler = new Dictionary<string, GRPCNode>();

        /// <summary>
        /// rpc handler
        /// the index for id
        /// </summary>
        Dictionary<uint, GRPCNode> idHandler = new Dictionary<uint, GRPCNode>();

        /// <summary>
        /// object map
        /// </summary>
        Dictionary<string, object> objectMap = new Dictionary<string, object>();

        /// <summary>
        /// construct GRPCClient
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="size"></param>
        public GRPCClient(string host, int port, int size = 8192)
            : base(host, port)
        {
            //select rpc interfaces
            SelectRPCInterface();
        }

        /// <summary>
        /// select rpc interface
        /// </summary>
        void SelectRPCInterface()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    foreach(GRPCAttribute rpcAttribute in method.GetCustomAttributes<GRPCAttribute>())
                    {
                        if (!objectMap.ContainsKey(type.FullName))
                        {
                            objectMap.Add(type.FullName, Activator.CreateInstance(type));
                        }

                        RegisterHandle(objectMap[type.FullName], rpcAttribute, method);
                    }
                }

            }
        }

        /// <summary>
        /// call handler
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        void InvokeMethod(uint id, object message)
        {
            GRPCNode node = null;

            idHandler.TryGetValue(id, out node);

            if (node == null) return;

            node.Invoke(message);
        }

        /// <summary>
        /// call handler
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        void InvokeMethod(string key, object message)
        {
            GRPCNode node = null;

            rpcHandler.TryGetValue(key, out node);

            if (node == null) return;

            node.Invoke(message);
        }

        /// <summary>
        /// register process handle
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="method"></param>
        void RegisterHandle(object o, GRPCAttribute a, MethodInfo m)
        {
            if(rpcHandler.ContainsKey(a.rpcKey))
                throw new Exception("register handler is error! key="+ a.rpcKey);

            GRPCNode node = new GRPCNode { obj = o, key = a.rpcKey, id = a.rpcId, method = m };

            rpcHandler.Add(a.rpcKey, node);

            //no id
            if (a.rpcId == int.MinValue) return;

            if(idHandler.ContainsKey(a.rpcId))
                throw new Exception("register handler is error! id=" + a.rpcId.ToString());

            idHandler.Add(a.rpcId, node);
        }

        /// <summary>
        /// rpc message
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        public void Rpc(string key, object message)
        {
            SendMessage(RPC_MSG_ID, MakeRpcMessage(key, message));

            //byte[] bs = ToBytes(RPC_MSG_ID, SocketDefine.PACKET_STREAM, rpc);

            //GNetPacket o = packet.ToNetPacket(bs, 0, bs.Length);

            //IRPCMessage p1 = DecodeEvent(o.body) as IRPCMessage;

            //GNetPacket o1 = packet.ToNetPacket(p1.message, 0, bs.Length);

            //OnMessageEvent(this, o1);

            //SendMessage(RPC_MSG_ID, MakeRpcMessage(message));
        }

        /// <summary>
        /// make rpc message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object MakeRpcMessage(string key, object message)
        {
            IRPCMessage rpc = Activator.CreateInstance(typeof(TRPC)) as IRPCMessage;

            if (ROUTE_BEGIN == uint.MaxValue) ROUTE_BEGIN = 0;

            GRPCNode node = null;

            rpcHandler.TryGetValue(key, out node);

            rpc.routeId = ROUTE_BEGIN++;
            rpc.idKey = node.id;
            rpc.rpcKey = node.id == uint.MinValue ? null : key;
            rpc.message = ToBytes(rpc.routeId, SocketDefine.PACKET_RPC, message);

            return rpc;
        }
    }
}
