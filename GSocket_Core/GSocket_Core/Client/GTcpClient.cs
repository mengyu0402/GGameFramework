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
	public class GTcpClient<TBuff> : GSocketBase where TBuff : IBuffStream
	{
        #region Define
        protected const string LOG_ON_CONNECT 		= "OnConnect : address:{0}";
        protected const string LOG_ON_DISCONNECT 	= "OnDisconnect : address:{0}";
		protected const string LOG_ON_SEND 			= "OnSend : address:{0} msgId:{1} type:{2}";
        protected const string LOG_ON_PING 			= "OnPing : address:{0} type:{1}";
        protected const string LOG_ON_RPC 			= "OnRPC : address:{0} msgId:{1} type:{2}, action:{3}";
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
		protected NetState state;

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
				state = NetState.Connected;

				//begin reveive message
				ReceiveBegin();

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
        /// Disconnect to host
        /// </summary>
        public override void Disconnect(object arg=null)
		{
			try
			{
				if (socket == null) return;

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
		/// <param name="message">Message.</param>
		public void SendMessage(object message)
		{
			if (state != NetState.Connected) return;

            uint msgId = GetMsgId(message);

            if (msgId == uint.MinValue) return;

			SendBegin(msgId, SocketDefine.PACKET_STREAM, message);

			PrintLog(LOG_ON_SEND, addr, msgId, message != null ? message.GetType().ToString() : NONE);
		}

		/// <summary>
		/// send ping
		/// </summary>
		public void SendPing()
		{ 
			if (state != NetState.Connected) return;

			SendBegin(0, SocketDefine.PACKET_PING, null);

            PrintLog(LOG_ON_PING, addr, NONE);
		}

		/// <summary>
		/// aysnc send buf
		/// </summary>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="routeId">Route identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="message">Message.</param>
		protected void SendBegin(uint msgId, byte type, object message)
		{
			try
			{
				if (socket == null) return;

				byte[] buf = ToBytes(msgId, type, message);

                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(buf, 0, buf.Length);
                socket.SendAsync(sendArgs);
            }
			catch (Exception ex)
			{
				PrintLog("SendBegin Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

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

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(bufStream.buff, bufStream.position, bufStream.buff.Length - (bufStream.position + bufStream.length));
                args.Completed += RecevieAsyncCompleted;
                socket.ReceiveAsync(args);
			}
			catch (Exception ex)
			{ 
                PrintLog("ReceiveBegin Error! {0} {1}", ex.Message, ex.StackTrace != null ? ex.StackTrace : string.Empty);

				Dispose();
			}
		}

        private void RecevieAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred == 0)
                {
                    Dispose();
                }
                else
                {
                    bufStream.length += e.BytesTransferred;
                    //make packet
                    PacketProcess();
                }

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
		/// make packet
		/// </summary>
		void PacketProcess()
		{
			int offset = bufStream.position;
			int length = bufStream.position + bufStream.length;

			while (offset < length)
			{
				GNetPacket netPacket = ToNetPacket(bufStream.buff, offset, length);

				if (netPacket == null) break;

				offset += netPacket.body.Length + 9;

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

