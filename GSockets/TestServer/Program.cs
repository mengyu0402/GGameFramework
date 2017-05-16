using System;
using System.IO;
using GSockets;
using GSockets.Listener;
using GSockets.Listener.Session;
using ProtoBuf;

namespace TestServer
{
	[ProtoContract]
	public class Message
	{
		[ProtoMember(1)]
		public int test1 = 123456;

		[ProtoMember(2)]
		public string test2 = "abcdef";
	}


	class MainClass
	{
		public static void Main(string[] args)
		{
			GTcpListener<GSession, GBuffStream> listener = new GTcpListener<GSession, GBuffStream>(8192);

			listener.decode += (msgId, body) => {

				using (MemoryStream stream = new MemoryStream(body))
				{
					return Serializer.Deserialize(typeof(Message), stream);
				}
			};

			listener.encode += (message) => {
				using (MemoryStream stream = new MemoryStream())
				{
					Serializer.Serialize(stream, message);
					return stream.ToArray();
				}
			};

			listener.onMessage += (own, msgId, message) => {

				Message msg = message as Message;

				Console.WriteLine(string.Format("OnMessage : sid:{0}, msgId:{1} arg1 : {2}-{3}", 
				                                ((GSession)own).sid,
				                                msgId,
				                                msg.test1,
				                                msg.test2
				                               ));

				Message to = new Message();
				to.test2 = "88888";
				to.test1 = 99999;

				listener.SendMessage(own as GSession, 100, to);
			};

			listener.Start();

			Console.WriteLine("Listener Start");

			Console.ReadKey();
			Console.ReadKey();
			Console.ReadKey();

			Console.WriteLine("Hello World!");
		}
	}
}
