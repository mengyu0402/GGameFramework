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
	public class Message
	{
		[ProtoMember(1)]
		public int test1 = 123456;

        

        [ProtoMember(2)]
        public byte[] test2 = new byte[3000];

        [ProtoMember(3)]
        public ulong id;
    }

    public class Node
    {
        public Stopwatch w;
        public long tick;
    }

	class MainClass
	{
		public static void Main(string[] args)
		{
			GTcpClient<GBuffStream> client = new GTcpClient<GBuffStream>("127.0.0.1", 8192);
            client.writeLog = true;
            client.log += Client_log;

            client.decode += (msgId, body) => { 
				using (MemoryStream stream = new MemoryStream(body))
				{
					return Serializer.Deserialize(typeof(Message), stream);
				}
			};

			client.encode += (message) => { 
				using (MemoryStream stream = new MemoryStream())
				{
					Serializer.Serialize(stream, message);
					return stream.ToArray();
				}
			};

            Dictionary<ulong, Node> watch = new Dictionary<ulong, Node>();

            ulong index = 0;
            client.onMessage += (own, msgId, message) => { 

				Message msg = message as Message;



                Node node = watch[msg.id];

                node.w.Stop();
                Console.WriteLine(string.Format("OnMessage [{6}] : sid:{0}, msgId:{1} arg1 : {2}-{3} {4} {5}",
												own.ToString(),
												msgId,
												msg.test1,
												"",
                                                node.w.ElapsedMilliseconds,
                                                DateTime.Now.Ticks - node.tick,
                                                msg.id
                                               ));
				
			};

			client.Connect(() => {
                while (true)
                {
                    System.Threading.Thread.Sleep(100);

                    Message msg = new Message();
                    msg.test1 = 8192;
                    msg.id = index++;
                    Node node = new Node();
                    node.w = new Stopwatch();
                    node.tick = DateTime.Now.Ticks;

                    watch.Add(msg.id, node);

                    node.w.Start();
                    client.SendMessage(101, msg);
                }
            });

            
            

			Console.ReadKey();
			Console.ReadKey();
			Console.ReadKey();
		}

        private static void Client_log(string format, params object[] args)
        {
            Console.WriteLine(string.Format(format, args));
        }
    }
}
