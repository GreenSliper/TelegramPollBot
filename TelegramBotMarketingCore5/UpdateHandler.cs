using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using TelegramBotMarketing.EventHandling.Abstract;

namespace TelegramBotMarketing
{
	internal class UpdateHandler : IUpdateHandler
	{
		readonly IBotUpdateHandlerChain botUpdateHandler;
		public UpdateHandler(IBotUpdateHandlerChain botUpdateHandler)
		{
			this.botUpdateHandler = botUpdateHandler;
		}

		public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
		{
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
		}

		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
			await botUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
		}
	}
}
