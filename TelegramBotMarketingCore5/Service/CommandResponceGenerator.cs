using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;
using TelegramBotMarketingCore5.Service.Abstract;
using static System.Net.Mime.MediaTypeNames;
using TelegramBotMarketingCore5.Utility;
using RuntimeConfig;

namespace TelegramBotMarketingCore5.Service
{
	internal class CommandResponceGenerator : ICommandResponceGenerator
	{
		Dictionary<string, Func<Message, Task<ResponceResult>>> functionalCommands = new ();

		readonly IUserRepository userRepository;
		readonly IPollAnswerManager answerManager;
		readonly IConfig config;

		string commandsSystemMessagesFile = "SystemTextAnswers", textCommandAnswers = "TextCommandAnswers";
		public CommandResponceGenerator(IUserRepository userRepository, IPollAnswerManager answerManager, 
			IConfig config)
		{
			//add here your commands that need any logics except printing text in responce
			functionalCommands.Add("/startpoll", (x) => StartPoll(x));
			functionalCommands.Add("/abortpoll", (x) => AbortPoll(x));
			//functionalCommands.Add("/prev", (x) => PrevQuestion(x));
			//functionalCommands.Add("/next", (x) => NextQuestion(x));
			this.userRepository = userRepository;
			this.answerManager = answerManager;
			this.config = config;
		}

		public async Task<ResponceResult> GenerateResponce(Message message)
		{
			var incomingMessage = message.Text.Trim().ToLower();
			if (config.Contains(textCommandAnswers, incomingMessage))
				return new ResponceResult() { type = ResponceResultType.Valid, message = config[textCommandAnswers, incomingMessage] };
			if(functionalCommands.TryGetValue(incomingMessage, out var func))
				return await func(message);
			return new ResponceResult() { type = ResponceResultType.Invalid, message = "" };
		}

		async Task<ResponceResult> StartPoll(Message message)
		{
			var user = await userRepository.Get(message.Chat.Id);
			if (user == null)
				user = await userRepository.Create(message.Chat.Id);
			switch (user.pollState)
			{
				case TelegramBotMarketing.Domain.PollState.NotStarted:
					user.pollState = TelegramBotMarketing.Domain.PollState.Started;
					await userRepository.Update(user);
					var question = await answerManager.GetNextQuestion(message.Chat.Id);
					KeyboardButton[][] buttons = null;
					if (question.answerType == AnswerTypes.List)
						buttons = ButtonExtensions.GenerateButtonSet(question.answerList);
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[commandsSystemMessagesFile, "firstQuestionIntro"] +
						question.questionText,
						buttons = buttons
					};
				case TelegramBotMarketing.Domain.PollState.Started:
					question = await answerManager.GetNextQuestion(message.Chat.Id);
					buttons = null;
					if (question.answerType == AnswerTypes.List)
						buttons = ButtonExtensions.GenerateButtonSet(question.answerList);
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[commandsSystemMessagesFile, "pollRunningLastQuestionReminder"] +
						(await answerManager.GetNextQuestion(message.Chat.Id)).questionText,
						buttons = buttons
					};
				case TelegramBotMarketing.Domain.PollState.Completed:
					return new ResponceResult()
					{
						type = ResponceResultType.Valid,
						message = config[commandsSystemMessagesFile, "cannotAddSecondAttempt"]
					};
				default: 
					return new ResponceResult() 
					{ 
						type = ResponceResultType.Invalid,
						message = ""
					};
			}             
		}
		async Task<ResponceResult> AbortPoll(Message message)
		{
			var userData = await userRepository.Get(message.Chat.Id);
			if (userData == null || userData.pollState == TelegramBotMarketing.Domain.PollState.NotStarted)
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = config[commandsSystemMessagesFile, "cannotAbortPollNotStarted"]
				};
			else if (userData != null)
			{
				userData.pollState = TelegramBotMarketing.Domain.PollState.NotStarted;
				userData.questionAnswers = new();
				await userRepository.Update(userData);
				return new ResponceResult()
				{
					type = ResponceResultType.Valid,
					message = config[commandsSystemMessagesFile, "allAnswersRemoved"]
				};
			}
			return new ResponceResult()
			{
				type = ResponceResultType.Valid,
				message = config[commandsSystemMessagesFile, "impossibleAction"]
			};
		}

		async Task<ResponceResult> PrevQuestion(Message message)
		{
			throw new NotImplementedException();
		}

		async Task<ResponceResult> NextQuestion(Message message)
		{
			throw new NotImplementedException();
		}
	}
}
