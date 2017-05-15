using System;
using System.Net.Sockets;
using GSockets;

namespace GSockets.Listener.Session
{
	/// <summary>
	/// socket session
	/// </summary>
	public class GSession
	{
		/// <summary>
		/// The listener.
		/// </summary>
		internal GSocketBase listener;

		/// <summary>
		/// session socket
		/// </summary>
		internal Socket socket;

		/// <summary>
		/// session ID
		/// </summary>
		public uint sid;

		/// <summary>
		/// The buffer stream.
		/// </summary>
		GBuffStream stream = null;

		/// <summary>
		/// Initializes
		/// </summary>
		/// <returns>The initializes.</returns>
		/// <param name="lenth">Lenth.</param>
		/// <typeparam name="TBuff">The 1st type parameter.</typeparam>
		internal void Initializes<TBuff>(int lenth)
		{ 
		}

		/// <summary>
		/// Release this instance.
		/// </summary>
		public virtual void Release()
		{
			socket = null;
		}

		/// <summary>
		/// send mesage
		/// </summary>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="message">Message.</param>
		public void SendMessage(uint msgId, object message)
		{
			SendBegin(listener.ToBytes(msgId, 0, SocketDefine.PACKET_STREAM, message));
		}

		/// <summary>
		/// begin recv message
		/// </summary>
		internal void ReceiveBegin()
		{ 

		}

		/// <summary>
		/// 发送缓冲区
		/// </summary>
		/// <param name="body">Body.</param>
		void SendBegin(byte[] body)
		{ 
		}
	}
}
