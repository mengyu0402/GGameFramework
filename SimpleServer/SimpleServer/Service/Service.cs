using System.Threading;
using log4net;
using System.Reflection;
namespace SimpleServer.Services
{
	/// <summary>
	/// 服务入口
	/// </summary>
	public class Service
	{
		/// <summary>
		/// log interface
		/// </summary>
		static ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// 服务是否存活
		/// </summary>
		public static volatile bool Alive;

		/// <summary>
		/// 对外入口函数
		/// </summary>
		public static void Run()
		{
			Begin();
			Loop();
			End();
		}

		/// <summary>
		/// 准备服务
		/// </summary>
		static void Begin()
		{ 
			log.Info("Simple Server Start...");
		}

		/// <summary>
		/// 服务运行中
		/// </summary>
		static void Loop()
		{
			while (Alive)
			{
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// 服务结束
		/// </summary>
		static void End()
		{ 
			log.Info("Simple Server Shutdown");
		}
	}
}
