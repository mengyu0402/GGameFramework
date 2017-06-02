using System;
using System.Collections.Generic;
using System.Text;

namespace GSockets.Listener.Session
{
    public class GRPCSession : GSession
    {
        internal void Reponse<TRPC>(IRPCMessage rpc, object message)
        {
            SendBegin(listener.ToBytes(GSocketBase.RPC_MSG_ID, SocketDefine.PACKET_RPC, MakeRpcMessage<TRPC>(rpc, message)));
        }

        /// <summary>
        /// make rpc message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        object MakeRpcMessage<TRPC>(IRPCMessage rpc, object message)
        {
            rpc.message = listener.EncodeEvent(message);

            return rpc;
        }
    }
}
