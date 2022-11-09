using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotMarketingCore5.Domain.Exceptions
{
	internal class NextQuestionUndecidableException : Exception
	{
		public NextQuestionUndecidableException()
		{
		}
		public NextQuestionUndecidableException(string message)
		: base(message)
		{
		}

		public NextQuestionUndecidableException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
