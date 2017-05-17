using System;

namespace GSockets
{
	/// <summary>
	/// Buff stream.
	/// </summary>
	public class GBuffStream : IBuffStream
	{
		/// <summary>
		/// data buffer
		/// </summary>
		public byte[] buff { get; set; }

		/// <summary>
		/// The length.
		/// </summary>
		public int length { get; set; }

		/// <summary>
		/// The begin position.
		/// </summary>
		public int position { get; set; }


		public GBuffStream() { }

		/// <summary>
		/// Initializes
		/// </summary>
		/// <param name="len">Length.</param>
		public GBuffStream(int len)
		{
			buff = new byte[len];
			Zero();
		}

		/// <summary>
		/// Resize the specified size.
		/// </summary>
		/// <returns>The resize.</returns>
		/// <param name="size">Size.</param>
		public void Resize(int size)
		{
			if (buff == null)
			{
				buff = new byte[size];
			}
			else 
			{
				byte[] temp = buff;
				int len = length;
				buff = new byte[size];
				Zero();
				PutBytes(temp, len);
			}
		}

		/// <summary>
		/// clear buf
		/// </summary>
		public void Zero() { length = position = 0; }

		/// <summary>
		/// Puts the bytes.
		/// </summary>
		/// <param name="buf">Buffer.source buf</param>
		/// <param name="pos">Length.copy len</param>
		public void PutBytes(byte[] buf, int len)
		{
			byte[] array = buff;

			int real = len + (position + length);

			if(real > array.Length)
				Array.Resize<byte>(ref array, real);

			buff = array;

			Buffer.BlockCopy(buf, 0, buff, length, len);

			length += len;
		}
	}
}

