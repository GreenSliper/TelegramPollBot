using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotMarketingCore5.Service.Abstract;

namespace TelegramBotMarketing.Service.Abstract
{
	internal interface IMessageResponceGenerator
	{
		Task<ResponceResult> GenerateResponce(Message message);
	}
}
