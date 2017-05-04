using System;
using System.Net;

namespace sgPlugins.Sockets.Listerner
{
	/// <summary>
	/// tcp listerner
	/// </summary>
	public class sgTcpListerner : sgSocketBase
	{
		public sgTcpListerner(int port) 
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
