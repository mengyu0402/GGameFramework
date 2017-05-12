using System;
using System.Net.Sockets;
using System.Net;

namespace sgPlugins.Sockets
{
	/// <summary>
	/// socket base
	/// </summary>
	internal abstract class sgSocketBase : IMessageCode, ISocketEvent, ILog, IDisposable
	{
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
		/// packet maker
		/// </summary>
		public IPacket packet { get; set; }

		/// <summary>
		/// decode event
		/// </summary>
		public event Decode decode;

		/// <summary>
		/// encode event
		/// </summary>
		public event Encode encode;

		/// <summary>
		/// data buffer
		/// </summary>
		/// <value>The buffer stream.</value>
		public IBuffStream bufStream { get; set; }

		/// <summary>
		/// disconnect event
		/// </summary>
		public event OnDisconnect onDisconnect;

		/// <summary>
		/// message event
		/// </summary>
		public event OnMessage onMessage;

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
		/// Initializes
		/// </summary>
		public sgSocketBase(IPAddress addr, int port)
		{
			address = new IPEndPoint(addr, port);
			packet = new sgPacket();
			bufStream = new sgBuffStream(64*1024);
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
		protected void PrintLog(string format, params object[] args)
		{
			if (writeLog && log != null) log(format, args);
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
		}

		/// <summary>
		/// Releases all resource
		/// </summary>
		public void Dispose()
		{
			if (socket != null)
			{
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();

				if(onDisconnect != null) onDisconnect(this);
			}

			Release();
		}

		/// <summary>
		/// make send buf
		/// </summary>
		/// <returns>The bytes.</returns>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="message">Message.</param>
		protected byte[] ToBytes(uint msgId, object message)
		{
			return packet.ToByte(msgId, encode(message));
		}

		/// <summary>
		/// check event
		/// </summary>
		protected void CheckEvent()
		{ 
			if (encode == null || decode == null) throw new Exception("endcode or decode is null!");
		}
	}
}
