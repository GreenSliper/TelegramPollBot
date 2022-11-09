using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;

namespace TelegramBotMarketingCore5.Repository
{
	internal class FilePollQuestionRepository : IPollQuestionRepository
	{
		string FilePath { get => Path.Combine(Directory.GetCurrentDirectory(), "Poll.json"); }

		Dictionary<string, PollQuestion> questions = new();
		public async Task<PollQuestion> Get(string id)
		{
			if (!questions.Any())
				await Load();
			if (questions.TryGetValue(id, out PollQuestion question))
				return question;
			else
				throw new ArgumentException($"Question ID not found in the {FilePath} file!");
		}

		/// <summary>
		/// DANGEROUS: WILL OVERRIDE ALL OF YOUR QUESTIONS WITH THE GIVEN SET
		/// </summary>
		public async Task SaveToFile(List<PollQuestion> pollQuestions)
		{
			string json = JsonConvert.SerializeObject(pollQuestions, Formatting.Indented);
			using (var strw = new StreamWriter(FilePath, false))
				await strw.WriteAsync(json);
		}

		async Task Load()
		{
			using (var strr = new StreamReader(FilePath))
			{
				var json = await strr.ReadToEndAsync();
				List<PollQuestion> poll;
				try
				{
					poll = JsonConvert.DeserializeObject<List<PollQuestion>>(json);
				}
				catch (Exception e)
				{
					throw new Exception($"Cannot parse json from {FilePath}! Check if it's correct", e);
				}
				foreach (var question in poll)
					questions.Add(question.id, question);
			}
		}
	}
}
