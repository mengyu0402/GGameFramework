using System;
using ProtoBuf;
using GSockets;
using GSockets.Client;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace TestClient
{
    [ProtoContract]
    [ProtoInclude(1, typeof(Message))]
    public class RPCMessage : IRPCMessage
    {
        [ProtoMember(1)]
        public uint routeId { get; set; }

        [ProtoMember(2)]
        public byte[] message { get; set; }

        public string rpcKey { get; set; }
        public uint idKey { get; set; }

    }

    [ProtoContract]
    public class Message
    {
        [ProtoMember(1)]
        public int test1 = 123456;

        [ProtoMember(2)]
        public string test2 = "abcdef";

        [ProtoMember(3)]
        public byte[] test3 = new byte[1024*10];

        [ProtoMember(4)]
        public ulong id;
    }

    


    class MainClass
    {
        public static void Main(string[] args)
        {
            using (GRPCClient<GBuffStream, RPCMessage> client = new GRPCClient<GBuffStream, RPCMessage>("10.235.156.201", 8192, 1024*20*10))
            {
                client.decode += (msgId, body) =>
                {
                    if (msgId == 9999)
                    {
                        using (MemoryStream stream = new MemoryStream(body))
                        {
                            return Serializer.Deserialize(typeof(RPCMessage), stream);
                        }
                    }
                    using (MemoryStream stream = new MemoryStream(body))
                    {
                        return Serializer.Deserialize(typeof(Message), stream);
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

                client.Rpc("123", new Message());
                client.Rpc("321", new Message());
                client.Rpc("111111111111", new Message());

                //client.writeLog = true;
                //client.log += Client_log;
              

                //Dictionary<ulong, Stopwatch> watch = new Dictionary<ulong, Stopwatch>();



                //client.Connect(() =>
                //{
                //    while (true)
                //    {
                //        System.Threading.Thread.Sleep(100);

                //        Message msg = new Message();
                //        msg.test1 = 11111111;
                //        msg.test2 = "time call";
                //        msg.id = count++;

                //        Stopwatch w = new Stopwatch();
                //        watch.Add(msg.id, w);
                //        w.Start();
                //        client.SendMessage(102, msg);
                //    }
                //});



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