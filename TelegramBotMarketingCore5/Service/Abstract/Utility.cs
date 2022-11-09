using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotMarketingCore5.Service.Abstract
{
	enum ResponceResultType { Valid, Invalid }
	internal class ResponceResult
	{
		public ResponceResultType type;
		public string message;
		public KeyboardButton[][] buttons;
	}
}
