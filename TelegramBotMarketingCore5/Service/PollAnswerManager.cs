using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotMarketing.Domain;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Domain.Exceptions;
using TelegramBotMarketingCore5.Repository.Abstract;
using TelegramBotMarketingCore5.Service.Abstract;

namespace TelegramBotMarketingCore5.Service
{
	internal class PollAnswerManager : IPollAnswerManager
	{
		readonly IUserRepository userRepository;
		readonly IPollQuestionRepository pollQuestionRepository;
		readonly IConfig config;

		string systemAnswersFile = "SystemTextAnswers";
		public PollAnswerManager(IUserRepository userRepository, IPollQuestionRepository pollQuestionRepository,
			IConfig config)
		{
			this.userRepository = userRepository;
			this.pollQuestionRepository = pollQuestionRepository;
			this.config = config;
		}

		public async Task<PollQuestion> GetNextQuestion(long chatId)
		{
			var user = await userRepository.Get(chatId);
			if (user == null)
				throw new ArgumentException("Chat not found! Cannot process request!");
			if(!user.questionAnswers.Any())
				return await pollQuestionRepository.Get("first");
			var lastAns = user.questionAnswers.Last();
			return await GetNextQuestion(await pollQuestionRepository.Get(lastAns.questionId), lastAns.answer);
		}

		async Task<PollQuestion> GetNextQuestion(PollQuestion current, string currentAnswer)
		{
			switch (current.answerType)
			{
				case AnswerTypes.List:
					if(current.nextIds.Count == 1)
						return await pollQuestionRepository.Get(current.nextIds[0]);
					int answerId = current.FormattedAnswers.IndexOf(currentAnswer);
					if (current.answerList.Count != current.nextIds.Count || answerId == -1)
						throw new NextQuestionUndecidableException();
					return await pollQuestionRepository.Get(current.nextIds[answerId]);
				default: 
					if(current.nextIds.Any())
						return await pollQuestionRepository.Get(current.nextIds[0]);
					return null;
			}
		}

		public async Task<PollAnswerFeedback> TryAddAnswer(Message message)
		{
			var pollQuestion = await GetNextQuestion(message.Chat.Id);
			var user = await userRepository.Get(message.Chat.Id);

			string errorMessage = "";
			string ans = message.Text.Trim().ToLower();
			switch (pollQuestion.answerType)
			{
				case AnswerTypes.AnyNonEmptyString:
					if (string.IsNullOrEmpty(ans))
						errorMessage = config[systemAnswersFile, "errorAnswerEmpty"];
					break;
				case AnswerTypes.PositiveInteger:
					if (!int.TryParse(ans, out var num) || num < 1)
						errorMessage = config[systemAnswersFile, "errorAnswerNonPositiveInt"];
					break;
				case AnswerTypes.List:
					if (!pollQuestion.FormattedAnswers.Contains(ans))
						errorMessage = config[systemAnswersFile, "errorAnswerNotFromList"];
					break;
			}
			if (!string.IsNullOrEmpty(errorMessage))
				return new PollAnswerFeedback(){
					valid = false,
					errorText = errorMessage,
					nextQuestion = pollQuestion
				};
			user.questionAnswers.Add(new UserData.Answer() { answer = ans, questionId = pollQuestion.id });
			await userRepository.Update(user);
			return new PollAnswerFeedback()
			{
				valid = true,
				nextQuestion = pollQuestion.nextIds.Any()?await GetNextQuestion(pollQuestion, ans):null
			};
		}
	}
}
