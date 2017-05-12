using System;
using System.Net;
using System.Net.Sockets;

namespace GSockets.Listener
{
	public class GTcpListener : GSocketBase
	{
		public GTcpListener(int port) 
			: base(IPAddress.Any, port)
		{ 
		}

		public void Start()
		{ 
		}

		public void Stop()
		{ 
		}

		public void SendMessage(object own, uint msgId, object message)
		{ 
		}
	}
}

