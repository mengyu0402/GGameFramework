using System;
using System.Net;
using System.Net.Sockets;

namespace GSockets.Client
{
	/// <summary>
	/// socket net state
	/// </summary>
	public enum NetState
	{ 
		Close,
		Connecting,
		Connected,
	}

	/// <summary>
	///tcp client
	/// </summary>
	public class GTcpClient : GSocketBase
	{
		/// <summary>
		/// net state
		/// </summary>
		NetState state;

		/// <summary>
		/// Initializes
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="port">Port.</param>
		public GTcpClient(string host, int port) 
			: base(IPAddress.Parse(host), port)
		{
			state = NetState.Close;
		}

		/// <summary>
		/// Connect to host
		/// </summary>
		public void Connect(Action action=null)
		{
			try
			{
				CheckEvent();

				state = NetState.Connecting;

				//make socket
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				//connect to host
				socket.Connect(address);
				//begin recv message
				ReceiveBegin();

				state = NetState.Connected;

				if (action != null) action.Invoke();
			}
			catch (Exception ex)
			{ 
				PrintLog("connect to {1} error! {0} {2}", ex.Message, addr, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// Async Connect to host
		/// </summary>
		public void ConnectAsync(Action action = null)
		{ 
			try
			{
				//update state
				state = NetState.Connecting;
				//make socket
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				//connect to host
				socket.BeginConnect(address, new AsyncCallback(ConnectEnd), action);
			}
			catch (Exception ex)
			{
				PrintLog("ConnectAsync to {1} error! {0} {2}", ex.Message, addr, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// Connect end.
		/// </summary>
		/// <param name="ar">Ar.</param>
		void ConnectEnd(IAsyncResult ar)
		{
			try
			{
				Action action = ar.AsyncState as Action;

				//connected
				socket.EndConnect(ar);
				//begin recv
				ReceiveBegin();
				//update state
				state = NetState.Connected;

				action?.Invoke();
			}
			catch (Exception ex)
			{ 
				PrintLog("ConnectAsync End to {1} error! {0} {2}", ex.Message, addr, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// Disconnect to host
		/// </summary>
		public void Disconnect()
		{
			try
			{
				socket.Disconnect(true);

				state = NetState.Close;

				Dispose();
			}
			catch (Exception ex)
			{ 
				PrintLog("Disconnect to {1} error! {0} {2}", ex.Message, addr, ex.StackTrace != null ? ex.StackTrace : string.Empty);
			}
		}

		/// <summary>
		/// Send message.
		/// </summary>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="message">Message.</param>
		public void SendMessage(uint msgId, object message)
		{
			if (state != NetState.Connected) return;

			SendBegin(msgId, SocketDefine.PACKET_STREAM, message);
		}

		/// <summary>
		/// send ping
		/// </summary>
		public void SendPing(object message = null)
		{ 
			if (state != NetState.Connected) return;

			SendBegin(0, SocketDefine.PACKET_PING, message);
		}


		public void RPC()
		{ 
		}

		/// <summary>
		/// aysnc send buf
		/// </summary>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="message">Message.</param>
		void SendBegin(uint msgId, byte type, object message)
		{
			try
			{
				if (socket == null) return;

				byte[] buf = ToBytes(msgId, type, message);

				socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(SendEnd), null);
			}
			catch (Exception ex)
			{
				PrintLog("SendBegin Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// async send end
		/// </summary>
		/// <param name="ar">Ar.</param>
		void SendEnd(IAsyncResult ar)
		{
			try 
			{ 
				if (socket == null) return;

				socket.EndSend(ar);
			}
			catch(Exception ex) 
			{
				PrintLog("SendEnd Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// Async Receives the begin.
		/// </summary>
		void ReceiveBegin()
		{
			try 
			{ 
				if (socket == null) return;
				if (state != NetState.Connected) return;

				socket.BeginReceive(bufStream.buff, bufStream.position, bufStream.length, SocketFlags.None, new AsyncCallback(ReceiveEnd), null);
			}
			catch (Exception ex)
			{ 
                PrintLog("ReceiveBegin Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

		/// <summary>
		/// Async Receives the end.
		/// </summary>
		/// <param name="ar">Ar.</param>
		void ReceiveEnd(IAsyncResult ar)
		{
			try
			{
				int size = socket.EndReceive(ar);

				//make packet
			}
			catch (Exception ex)
			{
				PrintLog("ReceiveEnd Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
			finally
			{
				ReceiveBegin();
			}
		}
	}
}

