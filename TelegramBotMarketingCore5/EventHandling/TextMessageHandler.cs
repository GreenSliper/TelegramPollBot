using RuntimeConfig;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotMarketing.EventHandling.Abstract;
using TelegramBotMarketing.Service.Abstract;
using TelegramBotMarketingCore5.Service.Abstract;

namespace TelegramBotMarketing.EventHandling
{
	internal class TextMessageHandler : IBotUpdateHandlerChain
	{
		public IBotUpdateHandlerChain Next { get; private set; }
		readonly ICommandResponceGenerator commandResponceGenerator;
		readonly IMessageResponceGenerator messageResponceGenerator;
		readonly IConfig config;
		string systemTextAnswers = "SystemTextAnswers";

		public TextMessageHandler(IBotUpdateHandlerChain next, 
			ICommandResponceGenerator commandResponceGenerator,
			IMessageResponceGenerator messageResponceGenerator,
			IConfig config)
		{
			Next = next;
			this.commandResponceGenerator = commandResponceGenerator;
			this.messageResponceGenerator = messageResponceGenerator;
			this.config = config;
		}
		public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
		{
			if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message && !string.IsNullOrEmpty(update.Message.Text))
			{
				var message = update.Message;
				ResponceResult responce = null;
				if (message.Text.StartsWith('/'))
					responce = await commandResponceGenerator.GenerateResponce(message);
				else
					responce = await messageResponceGenerator.GenerateResponce(message);

				IReplyMarkup rkm = new ReplyKeyboardRemove(); 
				if (responce.type == ResponceResultType.Valid)
				{
					if (responce.buttons != null)
						rkm = new ReplyKeyboardMarkup(responce.buttons);
					await botClient.SendTextMessageAsync(update.Message.Chat, responce.message, replyMarkup: rkm);
				}
				else
					await botClient.SendTextMessageAsync(update.Message.Chat, config[systemTextAnswers, "dontUnderstand"], replyMarkup: rkm);
			}
			else if(!cancellationToken.IsCancellationRequested && Next != null) //interrupt chain if cancellation is requested
				await Next.HandleUpdateAsync(botClient, update, cancellationToken);
		}
	}
}
