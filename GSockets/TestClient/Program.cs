using System;
using ProtoBuf;
using GSockets;
using GSockets.Client;
using System.IO;

namespace TestClient
{
	[ProtoContract]
	public class Message
	{
		[ProtoMember(1)]
		public int test1 = 123456;

		[ProtoMember(2)]
		public string test2 = "abcdef";	}


	class MainClass
	{
		public static void Main(string[] args)
		{
			GTcpClient<GBuffStream> client = new GTcpClient<GBuffStream>("127.0.0.1", 8192);

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

			client.onMessage += (own, msgId, message) => { 

				Message msg = message as Message;

				Console.WriteLine(string.Format("OnMessage : sid:{0}, msgId:{1} arg1 : {2}-{3}",
				                                own.ToString(),
				                                msgId,
				                                msg.test1,
				                                msg.test2
				                               ));
				
			};

			client.Connect(() => {
				Message msg = new Message();
				msg.test1 = 8192;
				msg.test2 = "client connect to server";

				client.SendMessage(101, msg);

				Console.WriteLine("111111111111111");
			});

			Console.ReadKey();
			Console.ReadKey();
			Console.ReadKey();
		}
	}
}
