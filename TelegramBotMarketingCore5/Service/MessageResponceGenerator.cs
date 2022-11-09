using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotMarketing.Service.Abstract;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;
using TelegramBotMarketingCore5.Service.Abstract;
using TelegramBotMarketingCore5.Utility;

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

		public async Task<ResponceResult> GenerateResponce(Message message)
		{
			string text = message.Text.Trim().ToLower();
			var user = await userRepository.Get(message.Chat.Id);
			if (user == null || user.pollState != Domain.PollState.Started)
			{
				if (config.Contains(templateTextAnswersFile, text))
					return new ResponceResult() { type = ResponceResultType.Valid, message = config[templateTextAnswersFile, text] };
				else
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[systemAnswersFile, "dontUnderstand"]
					};
			}
			else if (user != null)
			{
				var result = await pollAnswerManager.TryAddAnswer(message);
				if(!result.valid)
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = result.errorText
					};
				var next = result.nextQuestion;

				if (next != null)
				{
					KeyboardButton[][] buttons = null;
					if (next.answerType == AnswerTypes.List)
						buttons = ButtonExtensions.GenerateButtonSet(next.answerList);
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[systemAnswersFile, "nextQuestion"] + result.nextQuestion.questionText,
						buttons = buttons
					};
				}
				else
				{
					user.pollState = Domain.PollState.Completed;
					await userRepository.Update(user);
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[systemAnswersFile, "pollComplete"]
					};
				}
			}
			return new ResponceResult()
			{
				type = ResponceResultType.Invalid,
				message = config[systemAnswersFile, "dontUnderstand"]
			};
		}
	}
}
