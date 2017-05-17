using System;

namespace GSockets
{
	/// <summary>
	/// log
	/// </summary>
	public interface ILog
	{
		/// <summary>
		/// is write log
		/// </summary>
		bool writeLog { get; set; }

		/// <summary>
		/// write log function
		/// </summary>
		event WriteLog log;	
	}

	/// <summary>
	/// code interface
	/// </summary>
	public interface IMessageCode
	{
		/// <summary>
		/// packet maker
		/// </summary>
		/// <value>The packet.</value>
		IPacket packet { get; set; }

		/// <summary>
		/// encode
		/// </summary>
		event Encode encode;

		/// <summary>
		/// decode
		/// </summary>
		event Decode decode;	
	}

	/// <summary>
	/// socket event
	/// </summary>
	public interface ISocketEvent
	{
		/// <summary>
		/// disconnect event
		/// </summary>
		event OnDisconnect onDisconnect;

		/// <summary>
		/// message event
		/// </summary>
		event OnMessage onMessage;	
	}

	/// <summary>
	/// Buffer stream.
	/// </summary>
	public interface IBuffStream
	{
		/// <summary>
		/// data buffer
		/// </summary>
		byte[] buff { get; set; }

		/// <summary>
		/// The length.
		/// </summary>
		int length { get; set; }

		/// <summary>
		/// The begin position.
		/// </summary>
		int position { get; set; }

		/// <summary>
		/// clear buf
		/// </summary>
		void Zero();

		/// <summary>
		/// put buffer
		/// </summary>
		/// <param name="buf">Buffer.data buf</param>
		/// <param name="len">Length.copy len</param>
		void PutBytes(byte[] buf, int len);

		/// <summary>
		/// Resize the specified size.
		/// </summary>
		/// <returns>The resize.</returns>
		/// <param name="size">Size.</param>
		void Resize(int size);
	}

	/// <summary>
	/// packet function interface
	/// </summary>
	public interface IPacket
	{
		/// <summary>
		/// make buf
		/// </summary>
		byte[] ToByte(uint msgId, byte type, byte[] body);

		/// <summary>
		/// make NetPakcet
		/// </summary>
		GNetPacket ToNetPacket(byte[] buf, int offset, int length);
	}
}

