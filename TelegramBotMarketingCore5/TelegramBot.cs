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
				questionText = "Вы мужчина или женщина?",
				answerType = AnswerTypes.List,
				answerList = new () { "Мужчина", "Женщина" },
				nextIds = new () { "2" }
			},
			new PollQuestion()
			{
				id = "2",
				questionText = "Сколько Вам лет?",
				answerType = AnswerTypes.PositiveInteger,
				answerList = new () { },
				nextIds = new () { "3" }
			},
			new PollQuestion()
			{
				id = "3",
				questionText = "Вы работаете?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет" },
				nextIds = new () { "3.1", "4" }
			},
			new PollQuestion()
			{
				id = "3.1",
				questionText = "Вы работаете удаленно (имеете желание работать в таком формате)?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет", "Возможно" },
				nextIds = new () { "4" }
			},
			new PollQuestion()
			{
				id = "4",
				questionText = "Вы учитесь?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет" },
				nextIds = new () { "4.1", "5" }
			},
			new PollQuestion()
			{
				id = "4.1",
				questionText = "Нужен ли Вам ноутбук на занятиях?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет", "Редко (скорее нет)" },
				nextIds = new () { "5" }
			},
			new PollQuestion()
			{
				id = "5",
				questionText = "У Вас есть ПК (не считая ноутбук)?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет" },
				nextIds = new () { "6" }
			},
			new PollQuestion()
			{
				id = "6",
				questionText = "Вас интересуют современные компьютерные игры?",
				answerType = AnswerTypes.List,
				answerList = new () { "Да", "Нет" },
				nextIds = new () { "7" }
			},
			new PollQuestion()
			{
				id = "7",
				questionText = "Вы предпочитаете использовать мышь (иной внешний контроллер) или тачпад?",
				answerType = AnswerTypes.List,
				answerList = new () { "Контроллер", "Тачпад" },
				nextIds = new () { "8" }
			},
			new PollQuestion()
			{
				id = "8",
				questionText = "Часто ли Вы используете ноутбук без подключения зарядного устройства?",
				answerType = AnswerTypes.List,
				answerList = new () { "Часто", "Иногда", "Редко", "Никогда" },
				nextIds = new () { "9" }
			},
			new PollQuestion()
			{
				id = "9",
				questionText = "Вы часто смотрите фильмы, сериалы и другой видео-контент?",
				answerType = AnswerTypes.List,
				answerList = new () { "Часто", "Иногда", "Редко", "Никогда" },
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
