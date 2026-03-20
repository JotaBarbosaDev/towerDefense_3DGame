using System.IO;
using UnityEngine;

namespace Core.Data
{
	/// <summary>
	/// Json implementation of file saver
	/// </summary>
	public class JsonSaver<T> : FileSaver<T> where T : IDataStore
	{
		public JsonSaver(string filename)
			: base(filename)
		{
		}

		/// <summary>
		/// Save the specified data store
		/// </summary>
		public override void Save(T data)
		{
			if (data == null)
			{
				return;
			}

			string json = JsonUtility.ToJson(data);

			using (StreamWriter writer = GetWriteStream())
			{
				writer.Write(json);
			}
		}

		/// <summary>
		/// Load the specified data store
		/// </summary>
		public override bool Load(out T data)
		{
			if (!File.Exists(m_Filename))
			{
				data = default(T);
				return false;
			}

			string json;
			using (StreamReader reader = GetReadStream())
			{
				json = reader.ReadToEnd();
			}

			if (string.IsNullOrWhiteSpace(json))
			{
				data = default(T);
				return false;
			}

			data = JsonUtility.FromJson<T>(json);
			return data != null;
		}
	}
}
