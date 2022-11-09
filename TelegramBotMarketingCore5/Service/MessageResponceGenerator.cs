using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotMarketing.Domain;
using TelegramBotMarketing.Service.Abstract;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;
using TelegramBotMarketingCore5.Service.Abstract;
using TelegramBotMarketingCore5.Utility;
using static System.Net.Mime.MediaTypeNames;

namespace TelegramBotMarketing.Service
{
	internal class MessageResponceGenerator : IMessageResponceGenerator
	{
		readonly IUserRepository userRepository;
		readonly IPollAnswerManager pollAnswerManager;
		readonly IConfig config;

		string templateTextAnswersFile = "TemplateTextAnswers", systemAnswersFile = "SystemTextAnswers";
		public MessageResponceGenerator(IUserRepository userRepository, IPollAnswerManager pollAnswerManager, IConfig config)
		{
			this.userRepository = userRepository;
			this.pollAnswerManager = pollAnswerManager;
			this.config = config;
		}

		ResponceResult GetNonPollMessageResponce(string messageText)
		{
			if (config.Contains(templateTextAnswersFile, messageText))
				return new ResponceResult() { type = ResponceResultType.Valid, message = config[templateTextAnswersFile, messageText] };
			else
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = config[systemAnswersFile, "dontUnderstand"]
				};
		}

		async Task<ResponceResult> GetPollAnswerMessageResponce(UserData user, Message message)
		{
			var result = await pollAnswerManager.TryAddAnswer(message);
			var next = result.nextQuestion;

			KeyboardButton[][] buttons = null;
			if (next != null && next.answerType == AnswerTypes.List)
				buttons = ButtonExtensions.GenerateButtonSet(next.answerList);

			if (!result.valid)
			{
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = result.errorText,
					buttons = buttons
				};
			}

			if (next != null)
			{
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = config[systemAnswersFile, "nextQuestion"] + result.nextQuestion.questionText,
					buttons = buttons
				};
			}
			else
			{
				user.pollState = PollState.Completed;
				await userRepository.Update(user);
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = config[systemAnswersFile, "pollComplete"]
				};
			}
		}

		public async Task<ResponceResult> GenerateResponce(Message message)
		{
			string text = message.Text.Trim().ToLower();
			var user = await userRepository.Get(message.Chat.Id);
			if (user == null || user.pollState != Domain.PollState.Started)
				return GetNonPollMessageResponce(text);
			else if (user != null)
				return await GetPollAnswerMessageResponce(user, message);
			return new ResponceResult()
			{
				type = ResponceResultType.Invalid,
				message = config[systemAnswersFile, "dontUnderstand"]
			};
		}
	}
}
