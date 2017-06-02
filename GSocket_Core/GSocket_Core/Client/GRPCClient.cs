using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace GSockets.Client
{
    /// <summary>
    /// rpc client
    /// </summary>
    /// <typeparam name="TBuff"></typeparam>
    public class GRPCClient<TBuff, TRPC> : GTcpClient<TBuff> 
        where TBuff : IBuffStream
        where TRPC  : class, IRPCMessage
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

        /// <summary>
        /// rpc message type
        /// </summary>
        Type rpcMessageType = null;

        /// <summary>
        /// construct GRPCClient
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="size"></param>
        public GRPCClient(string host, int port, int size = 8192)
            : base(host, port)
        {
            //scan rpc interfaces
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
                GRPCMessageAttribute[] attrs =  type.GetTypeInfo().GetCustomAttributes<GRPCMessageAttribute>().ToArray();

                if (attrs.Length > 1) throw new Exception("only one! GRPSMessage");
                if (rpcMessageType != null && attrs.Length !=0 ) throw new Exception("only one! GRPSMessage");

                rpcMessageType = type;
                
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    foreach(GRPCAttribute rpcAttribute in method.GetCustomAttributes<GRPCAttribute>())
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
        /// call handler
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        void InvokeMethod(uint id, byte[] body)
        {
            GRPCNode node = null;

            idHandler.TryGetValue(id, out node);

            if (node == null) return;

            node.Invoke(this, DecodeEvent(0, node.type, body));
        }

        /// <summary>
        /// call handler
        /// </summary>
        /// <param name="key"></param>
        /// <param name="message"></param>
        void InvokeMethod(string key, byte[] body)
        {
            GRPCNode node = null;

            rpcHandler.TryGetValue(key, out node);

            if (node == null) return;

            node.Invoke(this, DecodeEvent(0, node.type, body));
        }

        /// <summary>
        /// register process handle
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="method"></param>
        void RegisterHandler(object o, GRPCAttribute a, MethodInfo m)
        {
            if(rpcHandler.ContainsKey(a.rpcKey))
                throw new Exception("register handler is error! key="+ a.rpcKey);

            ParameterInfo[] param = m.GetParameters();

            if (param.Length < 2) return;

            GRPCNode node = new GRPCNode();
            node.obj = o;
            node.id = a.rpcId;
            node.key = a.rpcKey;
            node.method = m;
            node.type = param[1].ParameterType;

            rpcHandler.Add(a.rpcKey, node);

            //no id
            if (a.rpcId == uint.MinValue) return;

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
            if (state != NetState.Connected) return;

            SendBegin(RPC_MSG_ID, SocketDefine.PACKET_RPC, MakeRpcMessage(key, message));

            PrintLog(LOG_ON_SEND, addr, RPC_MSG_ID, message != null ? message.GetType().ToString() : NONE);
        }

        /// <summary>
        /// make rpc message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object MakeRpcMessage(string key, object message)
        {
            IRPCMessage rpc = Activator.CreateInstance(typeof(TRPC)) as IRPCMessage;

            GRPCNode node = null;

            rpcHandler.TryGetValue(key, out node);

            rpc.idKey = node.id;
            rpc.rpcKey = node.id == uint.MinValue ? key : null;
            rpc.message = EncodeEvent(message);

            return rpc;
        }

        internal override void OnMessageEvent(object own, GNetPacket netPacket)
        {
            if (netPacket.type == SocketDefine.PACKET_RPC)
            {
                TRPC packet = DecodeEvent(netPacket.msgId, typeof(TRPC), netPacket.body) as TRPC;

                if (string.IsNullOrEmpty(packet.rpcKey))
                {
                    InvokeMethod(packet.idKey, packet.message);
                }
                else
                {
                    InvokeMethod(packet.rpcKey, packet.message);
                }
            }

            base.OnMessageEvent(own, netPacket);
        }
    }
}
