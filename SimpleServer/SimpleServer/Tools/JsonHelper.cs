using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

namespace SimpleServer
{
	public static class JsonHelper
	{
		/// <summary>
		/// 序列化
		/// </summary>
		/// <returns>The serializer.</returns>
		public static string Serializer(object obj)
		{
			if (obj == null) return string.Empty;

			DataContractJsonSerializer js = new DataContractJsonSerializer(obj.GetType());
			using (MemoryStream memmory = new MemoryStream())
			{
				js.WriteObject(memmory, obj);
				memmory.Position = 0;


				using (StreamReader sr = new StreamReader(memmory, Encoding.UTF8))
				{
					return sr.ReadToEnd();
				}
			}
		}

		/// <summary>
		/// 反序列化
		/// </summary>
		/// <returns>The deseralizer.</returns>
		/// <param name="json">Json.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T Deseralizer<T>(string json)
		{
			using (var memmory = new MemoryStream(Encoding.Unicode.GetBytes(json)))
			{
				var ds = new DataContractJsonSerializer(typeof(T));

				return (T)ds.ReadObject(memmory);

			}
		}
	}
}
