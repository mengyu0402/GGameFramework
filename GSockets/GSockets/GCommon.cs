using System;

namespace GSockets
{
	/// <summary>
	/// log
	/// </summary>
	public delegate void WriteLog(string format, params object[] args);

	/// <summary>
	/// Encode message
	/// </summary>
	public delegate byte[] Encode(object message);

	/// <summary>
	/// Decode message 
	/// </summary>
	public delegate object Decode(byte[] body);

	/// <summary>
	/// disconnect event
	/// </summary>
	public delegate void OnDisconnect(object arg);

	/// <summary>
	/// message event
	/// </summary>
	public delegate void OnMessage(object own, uint msgId, object message);
}

