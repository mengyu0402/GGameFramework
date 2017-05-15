using System;
using System.Net;
using System.Net.Sockets;
using GSockets.Listener.Session;

namespace GSockets.Listener
{
	/// <summary>
	/// tcp listenner
	/// </summary>
	public class GTcpListener<TClass, TBuff> : GSocketBase 
		where TClass: GSession 
		where TBuff : IBuffStream
	{
		/// <summary>
		/// listen list
		/// </summary>
		public int listenMax;

		/// <summary>
		/// accept event
		/// </summary>
		public event OnAccept onAccept;

		/// <summary>
		/// The session manager.
		/// </summary>
		GSessionManager<TClass, TBuff> sessionManager = new GSessionManager<TClass, TBuff>();

		/// <summary>
		/// Initializes
		/// </summary>
		/// <param name="port">Port.</param>
		public GTcpListener(int port) 
			: base(IPAddress.Any, port)
		{
			listenMax = 200;
		}

		/// <summary>
		/// run listener
		/// </summary>
		public void Start()
		{
			//create socket
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//bind
			socket.Bind(address);
			//listen
			socket.Listen(listenMax);
			//accept
			BeginAccept();
		}

		/// <summary>
		/// begin accept
		/// </summary>
		void BeginAccept()
		{
			try
			{
				if (socket == null) return;

				socket.BeginAccept(new AsyncCallback(EndAccept), socket);
			}
			catch (Exception ex)
			{
				PrintLog("Accept Error! {0} - {1}", ex.Message, ex.StackTrace);
			}
		}

		/// <summary>
		/// End accept.
		/// </summary>
		/// <param name="ar">Ar.</param>
		void EndAccept(IAsyncResult ar)
		{
			try
			{
				Socket s = socket.EndAccept(ar);

				GSession session = sessionManager.GetSession(this, s);

				session.ReceiveBegin();

				if(onAccept != null) onAccept(session);
			}
			catch (Exception ex)
			{
                PrintLog("End Accept Error! {0} - {1}", ex.Message, ex.StackTrace);
			}
			finally
			{
				BeginAccept();
			}
		}

		/// <summary>
		/// stop listener
		/// </summary>
		public void Stop()
		{
			if (socket == null) return;

			Dispose();
		}

		/// <summary>
		/// Disconnect the specified session.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="session">Session.</param>
		public void Disconnect(GSession session)
		{
			if (session == null) return;

			Disconnect(session.sid);
		}

		/// <summary>
		/// Disconnect the specified sid.
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="sid">Sid.</param>
		public void Disconnect(uint sid)
		{ 
		}

		/// <summary>
		/// 发送消息
		/// </summary>
		/// <param name="own">Own.</param>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="message">Message.</param>
		public void SendMessage(GSession own, uint msgId, object message)
		{
			if (own == null) return;

			own.SendMessage(msgId, message);
		}
	}
}

