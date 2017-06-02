using System;
using ProtoBuf;
using GSockets;
using GSockets.Client;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace TestClient
{
    [GRPCMessage]
    [ProtoContract]
    public class RPCMessage : IRPCMessage
    {
        [ProtoMember(1)]
        public byte[] message { get; set; }
        [ProtoMember(2)]
        public string rpcKey { get; set; }
        [ProtoMember(3)]
        public uint idKey { get; set; }
    }

    [GMessage]
    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public int test1 = 123456;

        [ProtoMember(2)]
        public string test2 = "abcdef";

        [ProtoMember(3)]
        public ulong id;
    }

    


    class MainClass
    {
        public static void Main(string[] args)
        {
            using (GRPCClient<GBuffStream, RPCMessage> client = new GRPCClient<GBuffStream, RPCMessage>("10.235.156.201", 8192, 1024*20*10))
            {
                client.decode += (msgId, type, body) =>
                {
                    using (MemoryStream stream = new MemoryStream(body))
                    {
                        return Serializer.Deserialize(type, stream);
                    }
                };

                client.encode += (message) =>
                {
                    using (MemoryStream stream = new MemoryStream())
                    {
                        Serializer.Serialize(stream, message);
                        return stream.ToArray();
                    }
                };



                ulong count = 0;
                client.onMessage += (own, msgId, message) =>
                {

                    //Message msg = message as Message;

                    //Stopwatch w = null;
                    //watch.TryGetValue(msg.id, out w);

                    //w.Stop();
                    //watch.Remove(msg.id);
                    //Console.WriteLine(string.Format("OnMessage [{5}]: sid:{0}, msgId:{1} arg1 : {2}-{3} {4}",
                    //                                own.ToString(),
                    //                                msgId,
                    //                                msg.test1,
                    //                                msg.test2,
                    //                                w.ElapsedMilliseconds,
                    //                                msg.id
                    //                               ));

                };

                



                //client.writeLog = true;
                //client.log += Client_log;


                //Dictionary<ulong, Stopwatch> watch = new Dictionary<ulong, Stopwatch>();



                client.Connect(() =>
                {
                    client.Rpc("123", new Message());
                });



                Console.ReadKey();
                Console.ReadKey();
                Console.ReadKey();
            }
        }

        private static void Client_log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
}