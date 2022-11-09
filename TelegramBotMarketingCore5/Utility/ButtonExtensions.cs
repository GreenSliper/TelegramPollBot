using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotMarketingCore5.Utility
{
	public static class ButtonExtensions
	{
		public static KeyboardButton[][] GenerateButtonSet(IEnumerable<string> variants, int cols = 0, int rows = 0)
		{
			if (variants == null || !variants.Any())
				throw new ArgumentNullException();
			var variantList = variants.ToList();
			if (cols == 0)
				cols = 1;
			if (rows == 0)
				rows = (int)Math.Ceiling((float)variants.Count() / cols);
			var btns = new KeyboardButton[rows][];
			int cnt = 0;
			for (int i = 0; i < rows; i++)
			{
				btns[i] = new KeyboardButton[cols];
				for (int j = 0; j < cols; j++)
					btns[i][j] = new KeyboardButton(variantList[cnt++]);
			}
			return btns;
		}
	}
}
