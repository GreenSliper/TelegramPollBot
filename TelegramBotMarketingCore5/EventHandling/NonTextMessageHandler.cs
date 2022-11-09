using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramBotMarketing.EventHandling.Abstract;

namespace TelegramBotMarketing.EventHandling
{
	internal class NonTextMessageHandler : IBotUpdateHandlerChain
	{
		public IBotUpdateHandlerChain Next {get; private set;}
		readonly IConfig config;
		string systemTextAnswers = "SystemTextAnswers";
		public NonTextMessageHandler(IBotUpdateHandlerChain next, IConfig config)
		{
			Next = next;
			this.config = config;
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && update.Message != null)
			{
				await botClient.SendTextMessageAsync(update.Message.Chat, config[systemTextAnswers, "textOnly"]);
			}
			else if (!cancellationToken.IsCancellationRequested && Next != null) //interrupt chain if cancellation is requested
				await Next.HandleUpdateAsync(botClient, update, cancellationToken);
		}
	}
}
