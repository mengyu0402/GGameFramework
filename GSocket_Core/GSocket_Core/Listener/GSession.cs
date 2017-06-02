using System;
using System.Net.Sockets;
using GSockets;

namespace GSockets.Listener.Session
{
	/// <summary>
	/// socket session
	/// </summary>
	public class GSession
	{

		/// <summary>
		/// The listener.
		/// </summary>
		internal GSocketBase listener;

		/// <summary>
		/// session socket
		/// </summary>
		internal Socket socket;

		/// <summary>
		/// session ID
		/// </summary>
		public uint sid;

		/// <summary>
		/// The buffer stream.
		/// </summary>
		IBuffStream stream = null;

		/// <summary>
		/// Initializes
		/// </summary>
		/// <returns>The initializes.</returns>
		/// <param name="lenth">Lenth.</param>
		/// <typeparam name="TBuff">The 1st type parameter.</typeparam>
		internal void Initializes<TBuff>(int lenth) where TBuff : IBuffStream
		{
			stream = Activator.CreateInstance<TBuff>();
			stream.Resize(lenth);
		}

		/// <summary>
		/// Release this instance.
		/// </summary>
		public virtual void Release()
		{
			socket = null;
			stream.Zero();
		}

		/// <summary>
		/// disconect
		/// </summary>
		internal void Disconnect()
		{
			if (socket == null) return;

			if (socket.Connected) 
			{
				socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();

            }

			Release();
		}

		/// <summary>
		/// send mesage
		/// </summary>
		/// <param name="message">Message.</param>
		public void SendMessage(object message)
		{
            uint msgId = listener.GetMsgId(message);

            if (msgId == uint.MinValue) return;

            SendBegin(listener.ToBytes(msgId, SocketDefine.PACKET_STREAM, message));
		}

		/// <summary>
		/// begin recv message
		/// </summary>
		internal void ReceiveBegin()
		{
			try
			{
				if (socket == null) return;
				if (!socket.Connected) return;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.SetBuffer(stream.buff, stream.position, stream.buff.Length - (stream.position + stream.length));
                args.Completed += RecevieAsyncCompleted;
                socket.ReceiveAsync(args);
            }
			catch (Exception ex)
			{
				listener.PrintLog("Session - Receive Begin : sid:{0} message : {1} stack : {2}", sid, ex.Message, ex.StackTrace);
			}
		}

        private void RecevieAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred == 0)
                {
                    listener.Disconnect(sid);
                }
                else
                {
                    stream.length += e.BytesTransferred;
                    //make packet
                    PacketProcess();
                }

            }
            catch (Exception ex)
            {
                listener.Disconnect(sid);
                listener.PrintLog("Session - Receive End : sid:{0} message : {1} stack : {2}", sid, ex.Message, ex.StackTrace);
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
			int offset = stream.position;
			int length = stream.position + stream.length;

			while (offset < length)
			{
				GNetPacket netPacket = listener.ToNetPacket(stream.buff, offset, length);

				if (netPacket == null) break;

				offset += netPacket.body.Length + 9;

				listener.OnMessageEvent(this, netPacket);
			}

			//finish
			if (offset >= length) { stream.Zero(); return; }

			//封包内容过长
			if (offset == 0) return;

			//未处理完成，挪动数据
			Buffer.BlockCopy(stream.buff, offset, stream.buff, 0, length - offset);
			stream.position = 0;
			stream.length = length - offset;
		}

		/// <summary>
		/// send buff
		/// </summary>
		/// <param name="body">Body.</param>
		protected void SendBegin(byte[] body)
		{
			try
			{
				if (socket == null) return;

                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(body, 0, body.Length);
                socket.SendAsync(sendArgs);
            }
			catch (Exception ex)
			{ 
				listener.PrintLog("Session - Send Begin : sid:{0} message : {1} stack : {2}", sid, ex.Message, ex.StackTrace);
			}
		}
	}
}
