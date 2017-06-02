using System;
using System.Collections.Generic;
using System.Reflection;
using GSockets.Listener.Session;

namespace GSockets.Listener
{
    public class GRPCListener<TClass, TBuff, TRPC> : GTcpListener<TClass, TBuff>
        where TClass : GRPCSession
        where TBuff  : IBuffStream
        where TRPC : class, IRPCMessage
    {
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

        public GRPCListener(int port)
            : base(port)
        {
            ScanRPCInterface();
        }

        /// <summary>
        /// select rpc interface
        /// </summary>
        void ScanRPCInterface()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    foreach (GRPCAttribute rpcAttribute in method.GetCustomAttributes<GRPCAttribute>())
                    {
                        if (!objectMap.ContainsKey(type.FullName))
                        {
                            objectMap.Add(type.FullName, Activator.CreateInstance(type));
                        }

                        RegisterHandler(objectMap[type.FullName], rpcAttribute, method);
                    }
                }

            }
        }

        /// <summary>
        /// register handler
        /// </summary>
        void RegisterHandler(object obj, GRPCAttribute attr, MethodInfo method)
        {
            if (rpcHandler.ContainsKey(attr.rpcKey))
                throw new Exception("register handler is error! key=" + attr.rpcKey);

            ParameterInfo[] param = method.GetParameters();

            if (param.Length < 2) return;

            GRPCNode node = new GRPCNode();
            node.obj = obj;
            node.key = attr.rpcKey;
            node.id = attr.rpcId;
            node.method = method;
            node.type = param[1].ParameterType;
            
            rpcHandler.Add(attr.rpcKey, node);

            //no id
            if (attr.rpcId == uint.MinValue) return;

            if (idHandler.ContainsKey(attr.rpcId))
                throw new Exception("register handler is error! id=" + attr.rpcId.ToString());

            idHandler.Add(attr.rpcId, node);
        }

        /// <summary>
        /// call handler
        /// </summary>
        object InvokeMethod(TClass obj, uint id, byte[] body)
        {
            GRPCNode node = null;

            idHandler.TryGetValue(id, out node);

            if (node == null) return null;

            object message = DecodeEvent(0, node.type, body);

            return node.Invoke<TClass>(obj, message);
        }

        /// <summary>
        /// call handler
        /// </summary>
        object InvokeMethod(TClass obj, string key, byte[] body)
        {
            GRPCNode node = null;

            rpcHandler.TryGetValue(key, out node);

            if (node == null) return null;

            object message = DecodeEvent(0, node.type, body);

            return node.Invoke<TClass>(obj, message);
        }

        /// <summary>
        /// message event
        /// </summary>
        /// <param name="own"></param>
        /// <param name="netPacket"></param>
        internal override void OnMessageEvent(object own, GNetPacket netPacket)
        {
            //rpc
            if (netPacket.type == SocketDefine.PACKET_RPC)
            {
                
                TRPC packet = DecodeEvent(netPacket.msgId, typeof(TRPC), netPacket.body) as TRPC;

                object message = null;

                if (string.IsNullOrEmpty(packet.rpcKey))
                {
                    message = InvokeMethod(own as TClass, packet.idKey, packet.message);
                }
                else
                {
                    message = InvokeMethod(own as TClass, packet.rpcKey, packet.message);
                }

                GRPCSession session = own as GRPCSession;

                session.Reponse<TRPC>(packet, message);
            }

            base.OnMessageEvent(own, netPacket);
        }
    }
}
