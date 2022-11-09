using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotMarketingCore5.Domain
{
	internal enum AnswerTypes { AnyNonEmptyString, PositiveInteger, List }
	internal class PollQuestion
	{
		public string id;
		public string questionText;
		public AnswerTypes answerType;
		public List<string> answerList = new();
		public List<string> nextIds = new();
		public List<string> FormattedAnswers { get => answerList.Select(x => x.ToLower().Trim()).ToList(); }
	}
}
