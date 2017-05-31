using System;
using System.Collections.Generic;
using System.Text;
using GSockets;

namespace TestClient
{
    public class RPCInterface
    {
        int rand = new Random().Next();

        [GRPC("123")]
        public void RPC1(object message)
        {
            Console.WriteLine("1111111111111:"+ rand.ToString());
        }

        [GRPC("321")]
        void RPC2(object message)
        {
            Console.WriteLine("222222222222:" + rand.ToString());
        }

        [GRPC("111111111111")]
        static void RPC3(object message)
        {
            Console.WriteLine("333333333333333");
        }
    }
}
