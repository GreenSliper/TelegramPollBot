using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotMarketing.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;
using File = System.IO.File;

namespace TelegramBotMarketingCore5.Repository
{
	internal class UserFileRepository : IUserRepository
	{
		Dictionary<long, UserData> loaded = new();
		string Folder { get => Path.Combine(Directory.GetCurrentDirectory(), "UserData"); }
		string GetChatPath(long chatId) => Path.Combine(Folder, chatId + ".json");
		public async Task<UserData> Create(long chatId)
		{
			string path = GetChatPath(chatId);
			if (loaded.ContainsKey(chatId) || File.Exists(path))
				throw new ArgumentException("Data already exists!");
			UserData userData = new UserData() { chatId = chatId };
			loaded.Add(chatId, userData);
			await WriteFile(chatId, userData);
			return userData;
		}

		public async Task Delete(long chatId)
		{
			if (loaded.ContainsKey(chatId))
				loaded.Remove(chatId);
			string path = GetChatPath(chatId);
			if (File.Exists(path))
			{
				//faster delete in separate thread
				await Task.Factory.StartNew(() =>
				{
					using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None,
						4096, FileOptions.DeleteOnClose)) { }
				}
				);
			}
		}

		public async Task<UserData> Get(long chatId)
		{
			if (loaded.TryGetValue(chatId, out var data))
				return data;
			var loadTry = await TryLoad(chatId);
			if (loadTry.success)
			{
				data = loadTry.data;
				loaded.Add(chatId, data);
				return data;
			}
			return null;
		}

		public async Task Update(UserData data)
		{
			if (!loaded.ContainsKey(data.chatId))
				throw new ArgumentException("Cannot update file that was not loaded!");
			await WriteFile(data.chatId, data);
		}

		async Task<(bool success, UserData data)> TryLoad(long chatId)
		{
			string path = GetChatPath(chatId);
			if (!File.Exists(path))
				return (false, null);
			string json;
			using (var strr = new StreamReader(path))
				json = await strr.ReadToEndAsync();
			return (true, JsonConvert.DeserializeObject<UserData>(json));
		}

		async Task WriteFile(long chatId, UserData data)
		{
			string path = GetChatPath(chatId);
			if (!Directory.Exists(Folder))
				Directory.CreateDirectory(Folder);
			using (var strw = new StreamWriter(path, false))
				await strw.WriteAsync(JsonConvert.SerializeObject(data));
		}
	}
}
