using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace TelegramBotMarketingCore5.Service.Abstract
{
	internal interface ICommandResponceGenerator
	{
		Task<ResponceResult> GenerateResponce(Message message);
	}
}
