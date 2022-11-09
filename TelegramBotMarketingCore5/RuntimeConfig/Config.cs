using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RuntimeConfig
{
	public interface IConfig
	{
		string this[string key] { get; }
		string this[string file, string key] { get; }
		bool Contains(string key);
		bool Contains(string file, string key);
	}
	public abstract class ConfigBase : IConfig
	{
		/// <param name="loadDataImmediately">If set to <see langword="false"/>, load logics is invoked only after any value is requested</param>
		public ConfigBase(bool loadDataImmediately = false)
		{
			if (loadDataImmediately)
				LoadMainData();
		}

		protected bool isDirty, loaded;
		protected Dictionary<string, string> dict;
		protected Dictionary<string, Dictionary<string, string>> additiveDicts = new();
		public string this[string key] {
			get
			{
				if (!loaded)
					LoadMainData();
				return dict[key];
			}
		}

		public string this[string file, string key]
		{
			get
			{
				if (!additiveDicts.TryGetValue(file, out var dict))
					Load(file);
				return additiveDicts[file][key];
			}
		}

		protected void LoadMainData()
		{
			dict = new Dictionary<string, string>();
			Load();
			loaded = true;
		}

		/// <summary>
		/// Do NOT call directly, use LoadData
		/// </summary>
		protected abstract void Load(string file = null);

		public bool Contains(string key)
		{
			if (!loaded)
				LoadMainData();
			return dict.ContainsKey(key);
		}

		public bool Contains(string file, string key)
		{
			if (!additiveDicts.TryGetValue(file, out var dict))
				Load(file);
			return additiveDicts[file].ContainsKey(key);
		}
	}
}
