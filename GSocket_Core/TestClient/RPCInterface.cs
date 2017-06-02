﻿using System;
using System.Collections.Generic;
using System.Text;
using GSockets;
using GSockets.Client;

namespace TestClient
{
    public class RPCInterface
    {
        int rand = new Random().Next();

        [GRPC("123")]
        public void RPC1(GRPCClient<GBuffStream, RPCMessage> client, Message message)
        {
            Console.WriteLine("1111111111111:"+ message.test2);
        }
    }
}
