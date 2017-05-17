using System;
using System.Net;

namespace GSockets
{
	/// <summary>
	/// the Socket define.
	/// </summary>
	public static class SocketDefine
	{
		#region packet type

		public const byte PACKET_PING 	= 0;
		public const byte PACKET_STREAM = 1;
		public const byte PACKET_RPC 	= 2;

  		#endregion
	}

	/// <summary>
	/// Net Packet
	/// </summary>
	public class GNetPacket
	{
		/// <summary>
		/// msg id
		/// </summary>
		public uint msgId { get; set; }

		/// <summary>
		/// RPC id
		/// </summary>
		/// <value>The route identifier.</value>
		public uint routeId { get; set; }

		/// <summary>
		/// packet type
		/// </summary>
		/// <value>The type.</value>
		public byte type { get; set; }

		/// <summary>
		/// packet body
		/// </summary>
		public byte[] body { get; set; }
	}

	/// <summary>
	/// Packet.
	/// </summary>
	internal class GPacket : IPacket
	{
		const int SRC_OFFSE = 0;
		const int BUFF_OFFSET = 9;
		const int BUFF_OFFSET_LEN = 0;
		const int BUFF_OFFSET_TYPE = 4;
		const int BUFF_OFFSET_ID = 5;
		const int BUFF_OFFSET_MSG = 9;
		const int INT_SIZE = 4;

		/// <summary>
		/// make buf
		/// </summary>
		/// <returns>The byte.</returns>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="type">Message type.</param>
		/// <param name="body">Body.</param>
		public byte[] ToByte(uint msgId, byte type, byte[] body)
		{
			byte[] buff = new byte[BUFF_OFFSET + body.Length];

			//max len
			Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(buff.Length - INT_SIZE)), SRC_OFFSE, buff, BUFF_OFFSET_LEN, INT_SIZE);
			//type
			buff[BUFF_OFFSET_TYPE] = type;
			//msgid
			Buffer.BlockCopy(BitConverter.GetBytes(msgId), SRC_OFFSE, buff, BUFF_OFFSET_ID, INT_SIZE);
			//message
			Buffer.BlockCopy(body, SRC_OFFSE, buff, BUFF_OFFSET_MSG, body.Length);

			return buff;
		}

		/// <summary>
		/// make netpacket
		/// </summary>
		/// <returns>The net packet.</returns>
		/// <param name="buf">Buffer.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="length">Length.</param>
		public GNetPacket ToNetPacket(byte[] buf, int offset, int length)
		{
			int current = offset + INT_SIZE;

			//不满足包头长度的话
			if (current > length) return null;

			//获取总长度
			int buffLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, offset));

			//包体总长度超过缓冲区长度
			if ((buffLen + current) > length) return null;

			byte t = buf[current++];

			GNetPacket packet = new GNetPacket
			{
				type = t,
				msgId = BitConverter.ToUInt32(buf, current),
				body = new byte[buffLen - BUFF_OFFSET_ID]
			};

			Buffer.BlockCopy(buf, offset + BUFF_OFFSET, packet.body, SRC_OFFSE, packet.body.Length);

			return packet;
		}
	}
}

