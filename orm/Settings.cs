using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;



namespace Orm.Settings
{
	public class SettingsError : ApplicationException
	{
		public SettingsError(string msg) : base(msg) {}
	}


	public class Settings
	{
		private static Dictionary<string, object> info;


		/// <summary>
		/// Access the settings.
		/// </summary>
		public static object Get(string key)
		{
			return info[key];
		}

		/// <summary>
		/// Initialize <seealso cref="Orm.Settings.Setting">Setting</seealso> 
		/// from file "settings.xml".
		/// </summary>
		public static void Init()
		{
			Init("settings.xml");
		}

		/// <summary>
		/// Initialize <seealso cref="Orm.Settings.Setting">Setting</seealso> 
		/// from <seealso cref="System.Collections.Generic.Dictionary">
		/// Dictionary</seealso>.
		/// </summary>
		/// <param name="dict">A<seealso cref="System.Collections.Generic.Dictionary">
		/// Dictionary<string, object></seealso></param>
		public static void Init(Dictionary<string, object> dict)
		{
			if (info != null)
				throw new SettingsError("Settings.info is not null.");
			foreach(KeyValuePair<string, object> kv in dict)
				info[kv.Key] = kv.Value;
		}

		/// <summary>
		/// Initialize <seealso cref="Orm.Settings.Setting">Setting</seealso> 
		/// from settings file.
		/// </summary>
		/// <param name="path"></param>
		public static void Init(string path)
		{
			CheckPath(path);
			Dictionary<string, object> info = ParseXmlFile(path);
			Init(info);
		}

		private static void CheckPath(string path)
		{
			if(!File.Exists(path))
				throw new SettingsError(string.Format("Path non-existent: {0}", path));
		}

		protected static Dictionary<string, object> ParseXmlFile(string path)
		{
			Dictionary<string, object> dict = new Dictionary<string,object>();

			XmlReader reader = XmlReader.Create(path);
			reader.MoveToFirstAttribute();
			while (reader.HasAttributes)
			{
				reader.MoveToNextAttribute();
				dict[reader.Name] = Convert.ChangeType(reader.Value, reader.ValueType);
			}
			return dict;
		}
	}

}
