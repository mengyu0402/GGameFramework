using System;
using System.Net.Sockets;
using System.Collections.Generic;
using GSockets.Listener;

namespace GSockets.Listener.Session
{
	/// <summary>
	/// session manager
	/// </summary>
	public class GSessionManager<TClass, TBuff> where TClass : GSession
	{
		/// <summary>
		/// The session queue.
		/// </summary>
		Queue<GSession> sessionQueue = new Queue<GSession>();

		/// <summary>
		/// using session map
		/// </summary>
		Dictionary<uint, GSession> sessionMap = new Dictionary<uint, GSession>();

		/// <summary>
		/// The session id begin.
		/// </summary>
		uint SESSION_BEGIN = 100000;

		/// <summary>
		/// Release this instance.
		/// </summary>
		internal void Release()
		{ 
			lock(this) {
				sessionQueue.Clear();
				sessionMap.Clear();
			}
		}

		/// <summary>
		/// get session
		/// </summary>
		/// <returns>The session.</returns>
		internal GSession GetSession(GSocketBase listener, Socket socket)
		{ 
			lock(this) {
				GSession session = null;

				if (sessionQueue.Count != 0)
				{
					session = sessionQueue.Dequeue();
				}
				else
				{
					session = Activator.CreateInstance<TClass>();
					session.sid = ++SESSION_BEGIN;
				}

				session.listener = listener;
				session.socket = socket;

				sessionMap.Add(session.sid, session);

				return session;
			}
		}

		/// <summary>
		/// 获取一个session
		/// </summary>
		/// <returns>The session.</returns>
		/// <param name="sid">Sid.</param>
		internal GSession GetSession(uint sid)
		{ 
			lock(this)
			{ 
				GSession session = null;

				sessionMap.TryGetValue(sid, out session);

				return session;
			}
		}

		/// <summary>
		/// Remove session
		/// </summary>
		/// <returns>The remove.</returns>
		/// <param name="sid">Sid.</param>
		internal void Remove(uint sid)
		{ 
			lock(this)
			{
				GSession session = null;

				sessionMap.TryGetValue(sid, out session);

				if (session == null)
					return;

				session.Release();

				sessionMap.Remove(sid);
				sessionQueue.Enqueue(session);
			}
		}

		/// <summary>
		/// Remove session.
		/// </summary>
		/// <returns>The remove.</returns>
		/// <param name="session">Session.</param>
		internal void Remove(GSession session)
		{
			Remove(session.sid);
		}


		/// <summary>
		/// change map default size
		/// </summary>
		/// <returns>The resize.</returns>
		/// <param name="max">Max.</param>
		public void Resize(int max)
		{
			int length = sessionQueue.Count + sessionMap.Count;

			for (int i = length; i < max; ++i)
			{ 
				GSession session = Activator.CreateInstance<TClass>();
				session.sid = ++SESSION_BEGIN;
				sessionQueue.Enqueue(session);
			}
		}
	}
}
