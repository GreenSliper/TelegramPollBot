using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace RuntimeConfig
{
	/// <summary>
	/// Each line should contain one key-value pair with equality sign (=) between them
	/// Use double forward slash for comments
	/// If the value contains equality sign or leading/trailing whitespaces, use double quotes
	/// Empty/invalid lines are ignored
	/// </summary>
	public class TextFileConfig : ConfigBase
	{
		string fileName;
		string folder;

		char splitChar = '=', quoteChar = '\"';
		string commentStringStart = "//";
		public TextFileConfig(string folder = "Configuration", string fileName = "config.conf", bool loadDataImmediately = false)
		{
			this.folder = folder;
			this.fileName = fileName;
			if (loadDataImmediately)
				LoadMainData();
		}

		bool SplitLine(string line, out string key, out string value)
		{
			key = value = null;
			line = line.Trim();
			int breakIndex = 0;
			if (line.StartsWith(commentStringStart) || (breakIndex = line.IndexOf(splitChar)) < 0)
				return false;
			key = line.Substring(0, breakIndex).TrimEnd();
			//value is null, valid
			if (breakIndex == line.Length - 1)
				return true;

			value = line.Substring(breakIndex + 1).TrimStart();
			bool isQuoted = value.StartsWith(quoteChar) && value.EndsWith(quoteChar);
			if(isQuoted)
				value = value.Trim(quoteChar);
			//double equality is not valid, except if the value is quoted 
			//and equality sign is a part of the value
			return !value.Contains(splitChar) || isQuoted;
		}

		protected override void Load(string file = null)
		{
			bool mainFile = file == null;
			var curDict = dict;
			string curFileName = "";
			if (mainFile)
				curFileName = fileName;
			else
			{
				curFileName = file + ".conf";
				if (additiveDicts.TryGetValue(file, out curDict))
					curDict.Clear();
				else
				{
					curDict = new();
					additiveDicts.Add(file, curDict);
				}
			}
			string path = string.IsNullOrEmpty(folder) ? curFileName : Path.Combine(folder, curFileName);
			using (var sr = new StreamReader(path))
			{
				string line = "";
				int cnt = -1;
				while ((line = sr.ReadLine()) != null)
				{
					cnt++;
					if (line.Contains("\\n"))
						line = line.Replace("\\n", "\n");
					if (SplitLine(line, out var key, out var val))
						curDict.Add(key, val);
				}
			}
		}
	}
}