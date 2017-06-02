using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;

namespace GSockets
{
	/// <summary>
	/// socket base
	/// </summary>
	public abstract class GSocketBase : IMessageCode, ISocketEvent, ILog, IDisposable
	{
        /// <summary>
        /// rpc msg id
        /// </summary>
        public readonly static uint RPC_MSG_ID = uint.MaxValue;

        #region Define
        protected const string NONE 			= "None";
		protected const string LOG_ON_MESSAGE 	= "OnMessageEvent : msgId:{0} type:{1}";
		protected const string LOG_DISPOSE 		= "Dispose! addr:{0}";
		protected const string LOG_SET_OPTION 	= "SetSocketOption : Level:{0} Name:{1} Value:{2}";
  		#endregion

		#region Interface
		/// <summary>
		/// is write log
		/// </summary>
		/// <value><c>true</c> write log; <c>false</c>.</value>
		public bool writeLog { get; set; }

		/// <summary>
		/// log
		/// </summary>
		public event WriteLog log;

		/// <summary>
		/// decode event
		/// </summary>
		public event Decode decode;

		/// <summary>
		/// encode event
		/// </summary>
		public event Encode encode;

		/// <summary>
		/// disconnect event
		/// </summary>
		public event OnDisconnect onDisconnect;

		/// <summary>
		/// message event
		/// </summary>
		public event OnMessage onMessage;

		/// <summary>
		/// ping event
		/// </summary>
		public event OnPing onPing;

		/// <summary>
		/// packet maker
		/// </summary>
		public IPacket packet { get; set; }

		#endregion

		/// <summary>
		/// get address
		/// </summary>
		/// <value>The address.</value>
		public string addr { get { return address == null ? "*.*.*.*:*" : address.ToString(); } }

		/// <summary>
		/// The socket.
		/// </summary>
		protected Socket socket;

		/// <summary>
		/// The address.
		/// </summary>
		protected IPEndPoint address;

        /// <summary>
        /// message type map
        /// </summary>
        protected Dictionary<uint, Type> messageMap = new Dictionary<uint, Type>();

		/// <summary>
		/// Initializes
		/// </summary>
		public GSocketBase(IPAddress addr, int port)
		{
			address = new IPEndPoint(addr, port);
			packet = new GPacket();

            ScanMessage();
		}

		/// <summary>
		/// Release this instance.
		/// </summary>
		public virtual void Release()
		{
			address = null;
			socket = null;
		}

		/// <summary>
		/// Print the log.
		/// </summary>
		/// <param name="format">Format.</param>
		/// <param name="args">Arguments.</param>
		internal void PrintLog(string format, params object[] args)
		{
			if (!writeLog) return;

			if(log != null) log(format, args);
		}

		/// <summary>
		/// Sets the socket option.
		/// </summary>
		/// <param name="optionLevel">Option level.</param>
		/// <param name="optionName">Option name.</param>
		/// <param name="optionValue">Option value.</param>
		public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
		{
			if (socket == null) return;

			socket.SetSocketOption(optionLevel, optionName, optionValue);

            PrintLog(LOG_SET_OPTION, optionLevel, optionName, optionValue);
		}

		/// <summary>
		/// Releases all resource
		/// </summary>
		public void Dispose()
		{
			if (socket != null) 
			{
				if(socket.Connected) socket.Shutdown(SocketShutdown.Both);

                socket.Dispose();
            }

			PrintLog(LOG_DISPOSE, addr);

			OnDisconnetEvent(this);

			Release();
		}

		/// <summary>
		/// disconnect event
		/// </summary>
		/// <returns>The disconnect.</returns>
		/// <param name="arg">Argument.</param>
		public abstract void Disconnect(object arg = null);

		/// <summary>
		/// make send buff
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="type">Type.</param>
		/// <param name="message">Message.</param>
		internal byte[] ToBytes(uint msgId, byte type, object message)
		{
			return packet.ToByte(msgId, type, encode(message));
		}

		/// <summary>
		/// make packet
		/// </summary>
		/// <returns>The net packet.</returns>
		/// <param name="buff">Buff.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="length">Length.</param>
		internal GNetPacket ToNetPacket(byte[] buff, int offset, int length)
		{
			return packet.ToNetPacket(buff, offset, length);
		}

		/// <summary>
		/// check event
		/// </summary>
		protected void CheckEvent()
		{ 
			if (encode == null || decode == null) throw new Exception("endcode or decode is null!");
		}

		/// <summary>
		/// 接受数据，处理数据
		/// </summary>
		/// <param name="own">Own.</param>
		/// <param name="netPacket">Net packet.</param>
		internal virtual void OnMessageEvent(object own, GNetPacket netPacket)
		{
			switch (netPacket.type)
			{ 
				case SocketDefine.PACKET_PING:
					if (onPing != null) onPing(own);
					break;
				case SocketDefine.PACKET_STREAM:
                    if (onMessage != null)
                    {
                        Type type = messageMap[netPacket.msgId];
                        onMessage(own, netPacket.msgId, decode(netPacket.msgId, type, netPacket.body));
                    }
					break;
				default:
					break;
			}

           	PrintLog(LOG_ON_MESSAGE, netPacket.msgId, netPacket.type);
		}

		/// <summary>
		/// disconnect eveent
		/// </summary>
		/// <param name="arg">Argument.</param>
		protected void OnDisconnetEvent(object arg)
		{
			if (onDisconnect != null) onDisconnect(arg);
		}

        /// <summary>
        /// scan message
        /// register message
        /// </summary>
        protected virtual void ScanMessage()
        {
            Assembly assembly = Assembly.GetEntryAssembly();

            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (GMessageAttribute attribute in type.GetTypeInfo().GetCustomAttributes<GMessageAttribute>())
                {
                    if (messageMap.ContainsKey(attribute.msgId))
                    {
                        throw new Exception("some msgId");
                    }
                    messageMap.Add(attribute.msgId, type);
                }
            }
        }

        protected object DecodeEvent(uint msgId, Type type, byte[] body)
        {
            if (type == null) type = messageMap[msgId];
            return decode(msgId, type, body);
        }

        internal byte[] EncodeEvent(object message)
        {
            return encode(message);
        }

        /// <summary>
        /// get msgId
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal uint GetMsgId(object message)
        {
            Type type = message.GetType();

            GMessageAttribute attribute = type.GetTypeInfo().GetCustomAttribute<GMessageAttribute>();

            return attribute == null ? uint.MinValue : attribute.msgId;
        }
	}
}

