using System;
using System.IO;
using GSockets;
using GSockets.Listener;
using GSockets.Listener.Session;
using ProtoBuf;

namespace TestServer
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
            GRPCListener<GRPCSession, GBuffStream, RPCMessage> listener = new GRPCListener<GRPCSession, GBuffStream, RPCMessage>(8192);
            listener.writeLog = true;
            listener.log += Listener_log;
            listener.recvBuffLen = 1024 * 20*10;


            listener.onPing += (own) =>
            {
                GSession session = own as GSession;

            };

            listener.decode += (msgId, type, body) =>
            {

                using (MemoryStream stream = new MemoryStream(body))
                {
                    return Serializer.Deserialize(type, stream);
                }
            };

            listener.encode += (message) =>
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, message);
                    return stream.ToArray();
                }
            };

            listener.onMessage += (own, msgId, message) =>
            {

                Message msg = message as Message;

                Console.WriteLine(string.Format("OnMessage [{4}]: sid:{0}, msgId:{1} arg1 : {2}-{3}",
                                                ((GSession)own).sid,
                                                msgId,
                                                msg.test1,
                                                msg.test2,
                                                msg.id
                                               ));

                listener.SendMessage(own as GSession, msg);
            };

            listener.Start();

            Console.WriteLine("Listener Start");

            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();

            Console.WriteLine("Hello World!");
        }

        private static void Listener_log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
}
