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
		/// set buff lenth
		/// </summary>
		public int recvBuffLen = 8192;

		/// <summary>
		/// accept event
		/// </summary>
		public event OnAccept onAccept;

		/// <summary>
		/// The session manager.
		/// </summary>
		GSessionManager<TClass, TBuff> sessionManager = new GSessionManager<TClass, TBuff>();

        /// <summary>
        /// accept args
        /// </summary>
        SocketAsyncEventArgs acceptArgs = new SocketAsyncEventArgs();

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
            acceptArgs.Completed += AcceptAsyncCompleted;

            CheckEvent();

			//create socket
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			//bind
			socket.Bind(address);
			//listen
			socket.Listen(listenMax);
			//accept
			AcceptAsyncBegin(acceptArgs);
		}

        private void AcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                GSession session = sessionManager.GetSession(this, e.AcceptSocket);

                session.Initializes<TBuff>(recvBuffLen);
                session.ReceiveBegin();

                if (onAccept != null) onAccept(session);
            }
            catch (Exception ex)
            {
                PrintLog("End Accept Error! {0} - {1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                AcceptAsyncBegin(e);
            }
        }

        /// <summary>
        /// begin accept
        /// </summary>
        void AcceptAsyncBegin(SocketAsyncEventArgs args)
		{
			try
			{
				if (socket == null) return;
                args.AcceptSocket = null;
                socket.AcceptAsync(args);
			}
			catch (Exception ex)
			{
				PrintLog("Accept Error! {0} - {1}", ex.Message, ex.StackTrace);
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
		public override void Disconnect(object arg=null)
		{
			if (arg == null) return;

			if (arg is uint)
			{
				Disconnect((uint)arg);
			}
			else 
			{
				Disconnect(arg as GSession);
			}
		}

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
			GSession session = sessionManager.GetSession(sid);

			if (session == null) return;

			session.Disconnect();

			OnDisconnetEvent(session);

			sessionManager.Remove(sid);
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

