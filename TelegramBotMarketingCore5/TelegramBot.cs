using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using TelegramBotMarketingCore5.Domain;
using TelegramBotMarketingCore5.Repository.Abstract;

namespace TelegramBotMarketing
{
	interface ITelegramBot
	{
		void Start(string key);
	}

	internal class TelegramBot : ITelegramBot
	{
		ITelegramBotClient bot;
		readonly IUpdateHandler updateHandler;
		readonly IPollQuestionRepository pollQuestionRepository;

		public TelegramBot(IUpdateHandler updateHandler, IPollQuestionRepository pollQuestionRepository)
		{
			this.updateHandler = updateHandler;
			this.pollQuestionRepository = pollQuestionRepository;
		}

		List<PollQuestion> pollQuestions = new()
		{
			new PollQuestion()
			{
				id = "first",
				questionText = "Каков ваш пол? (М/Ж)",
				answerType = AnswerTypes.List,
				answerList = new () { "м", "ж" },
				nextIds = new () { "1", "2" }
			},
			new PollQuestion()
			{
				id = "1",
				questionText = "Бывают ли у тебя депрессивные мысли? Как часто",
				answerType = AnswerTypes.List,
				answerList = new () {"часто", "редко"},
				nextIds = new () { }
			},
			new PollQuestion()
			{
				id = "2",
				questionText = "Сколько Вам лет?",
				answerType = AnswerTypes.PositiveInteger,
				answerList = new () { },
				nextIds = new () { }
			}
		};

		public void Start(string key)
		{
			pollQuestionRepository.SaveToFile(pollQuestions);
			bot = new TelegramBotClient(key);
			Console.WriteLine("Started bot " + bot.GetMeAsync().Result.FirstName);
			var cts = new CancellationTokenSource();
			var cancellationToken = cts.Token;
			var receiverOptions = new ReceiverOptions
			{
				AllowedUpdates = { }, // receive all update types
			};
			bot.StartReceiving(
				updateHandler,
				receiverOptions,
				cancellationToken
			);
		}
	}
}
