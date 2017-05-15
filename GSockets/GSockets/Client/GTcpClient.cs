using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

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
	public class GTcpClient<TBuff> : GSocketBase where TBuff : IBuffStream
	{
		#region Define
		const string LOG_ON_CONNECT 		= "OnConnect : address:{0}";
		const string LOG_ON_DISCONNECT 		= "OnDisconnect : address:{0}";
		const string LOG_ON_SEND 			= "OnSend : address:{0} msgId:{1} type:{2}";
		const string LOG_ON_PING 			= "OnPing : address:{0} type:{1}";
		const string LOG_ON_RPC 			= "OnRPC : address:{0} msgId:{1} type:{2}, action:{3}";
		#endregion

		#region RPC

		///// <summary>
		///// 远程调用结构声明
		///// </summary>
		//public class RPCNode
		//{
		//	public uint routeId;
		//	public Action<object> action;
		//	public Type type;
		//}

		///// <summary>
		///// The rpc map.
		///// </summary>
		//Dictionary<uint, RPCNode> rpcMap = null;

		///// <summary>
		///// 远程调用ID
		///// </summary>
		//uint ROUTE_ID = 0;

		///// <summary>
		///// 最大远程调用ID
		///// </summary>
		//uint ROUTE_MAX = 100;

		///// <summary>
		///// 是否启用RPC机制
		///// </summary>
		///// <value><c>true</c> if is rpc; otherwise, <c>false</c>.</value>
		//public bool isRpc
		//{
		//	get { return rpcMap != null; }
		//	set
		//	{
		//		if (value == true && rpcMap == null)
		//		{
		//			rpcMap = new Dictionary<uint, RPCNode>();
		//		}
		//	}
		//}

  		#endregion

		/// <summary>
		/// net state
		/// </summary>
		NetState state;

		/// <summary>
		/// data buffer
		/// </summary>
		/// <value>The buffer stream.</value>
		public IBuffStream bufStream { get; set; }

		/// <summary>
		/// Initializes
		/// </summary>
		/// <param name="host">Host.</param>
		/// <param name="port">Port.</param>
		public GTcpClient(string host, int port, int size=8192) 
			: base(IPAddress.Parse(host), port)
		{
			state = NetState.Close;
			bufStream = Activator.CreateInstance<TBuff>();
			bufStream.Resize(size);
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

				if(action != null) action.Invoke();

				PrintLog(LOG_ON_CONNECT, addr);
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

				if(action != null) action.Invoke();

                PrintLog(LOG_ON_CONNECT, addr);
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
				if (socket == null) return;

				socket.Disconnect(true);

				state = NetState.Close;

				PrintLog(LOG_ON_DISCONNECT, addr);

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

			SendBegin(msgId, 0, SocketDefine.PACKET_STREAM, message);

			PrintLog(LOG_ON_SEND, addr, msgId, message != null ? message.GetType().ToString() : NONE);
		}

		/// <summary>
		/// send ping
		/// </summary>
		public void SendPing()
		{ 
			if (state != NetState.Connected) return;

			SendBegin(0, 0, SocketDefine.PACKET_PING, null);

            PrintLog(LOG_ON_PING, addr, NONE);
		}

		///// <summary>
		///// 远程调用
		///// </summary>
		///// <returns>The rpc.</returns>
		///// <param name="msgId">Message identifier.</param>
		///// <param name="message">Message.</param>
		///// <param name="action">Action.</param>
		///// <typeparam name="T">The 1st type parameter.</typeparam>
		//public void RPC<T>(uint msgId, object message, Action<object> action)
		//{
		//	//确定远程调用ID的范围
		//	if (ROUTE_ID >= ROUTE_MAX) ROUTE_ID = 0;

		//	//累加远程调用ID
		//	++ ROUTE_ID;

		//	//删除重复项
		//	if (rpcMap.ContainsKey(ROUTE_ID)) rpcMap.Remove(ROUTE_ID);

		//	//填充数据到结构中
		//	rpcMap.Add(ROUTE_ID, new RPCNode { routeId = ROUTE_ID, type = typeof(T), action = action});

		//	//发送数据
		//	SendBegin(msgId, ROUTE_ID, SocketDefine.PACKET_RPC, message);

		//	PrintLog(LOG_ON_RPC, addr, msgId, message.GetType().ToString(), action.ToString());
		//}

		/// <summary>
		/// aysnc send buf
		/// </summary>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="routeId">Route identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="message">Message.</param>
		void SendBegin(uint msgId, uint routeId, byte type, object message)
		{
			try
			{
				if (socket != null) return;

				byte[] buf = ToBytes(msgId, routeId, type, message);

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
				if (socket != null) return;

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
				if (socket != null) return;
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

				//重置长度
				bufStream.length += size;
				//make packet
				PacketProcess();
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

		/// <summary>
		/// 包分解
		/// </summary>
		void PacketProcess()
		{
			int offset = bufStream.position;
			int length = bufStream.position + bufStream.length;

			while (offset < length)
			{
				GNetPacket netPacket = packet.ToNetPacket(bufStream.buff, offset, length);

				if (netPacket == null) break;

				OnMessageEvent(this, netPacket);
			}

			//封包处理完成
			if (offset >= length) { bufStream.Zero(); return; }

			//封包内容过长
			if (offset == 0) return;

			//未处理完成，挪动数据
			Buffer.BlockCopy(bufStream.buff, offset, bufStream.buff, 0, length - offset);
			bufStream.position = 0;
			bufStream.length = length - offset;
		}
	}
}

