using System;
using System.Collections.Generic;
using System.Text;
using GSockets;
using GSockets.Listener.Session;

namespace TestServer
{
    public class RPCInterface
    {
        int rand = new Random().Next();

        [GRPC("123")]
        public Message RPC1(GRPCSession session, Message message)
        {
            Console.WriteLine("1111111111111:" + message.test2);
            return message;
        }

    }
}
