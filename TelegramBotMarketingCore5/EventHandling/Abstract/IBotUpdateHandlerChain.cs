using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramBotMarketing.EventHandling.Abstract
{
	internal interface IBotUpdateHandlerChain
	{
		public abstract Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);
	}
}
