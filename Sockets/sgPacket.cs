using System;
using System.Net;

namespace sgPlugins.Sockets
{
	/// <summary>
	/// Net Packet
	/// </summary>
	public class sgNetPacket
	{
		/// <summary>
		/// msg id
		/// </summary>
		public uint msgId { get; set; }

		/// <summary>
		/// packet body
		/// </summary>
		public byte[] body { get; set; }
	}

	/// <summary>
	/// Packet.
	/// </summary>
	public class sgPacket : IPacket
	{
		/// <summary>
		/// make buf
		/// </summary>
		/// <returns>The byte.</returns>
		/// <param name="msgId">Message identifier.</param>
		/// <param name="body">Body.</param>
		public byte[] ToByte(uint msgId, byte[] body)
		{
			byte[] buff = new byte[8 + body.Length];

			Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(buff.Length - 4)), 0, buff, 0, 4);
			Buffer.BlockCopy(BitConverter.GetBytes(msgId), 0, buff, 4, 4);
			Buffer.BlockCopy(body, 0, buff, 8, body.Length);

			return buff;
		}

		/// <summary>
		/// make netpacket
		/// </summary>
		/// <returns>The net packet.</returns>
		/// <param name="buf">Buffer.</param>
		/// <param name="offset">Offset.</param>
		/// <param name="length">Length.</param>
		public sgNetPacket ToNetPacket(byte[] buf, int offset, int length)
		{
			int current = offset + 4;

			//不满足包头长度的话
			if (current > length) return null;

			//获取总长度
			int buffLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, offset));

			//包体总长度超过缓冲区长度
			if ((buffLen + current) > length) return null;

			sgNetPacket packet = new sgNetPacket();

			packet.msgId = BitConverter.ToUInt32(buf, current);
			packet.body = new byte[buffLen - 4];

			Buffer.BlockCopy(buf, offset + 8, packet.body, 0, packet.body.Length);

			return packet;
		}
	}
}
