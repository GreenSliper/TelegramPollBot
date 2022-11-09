using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TelegramBotMarketingCore5.Domain;

namespace TelegramBotMarketingCore5.Service.Abstract
{
	internal class PollAnswerFeedback
	{
		public bool valid;
		public string errorText;
		public PollQuestion nextQuestion;
	}

	internal interface IPollAnswerManager
	{
		/// <summary>
		/// Returns true if answer was accepted
		/// </summary>
		Task<PollAnswerFeedback> TryAddAnswer(Message message);
		/// <summary>
		/// Get next question basing on given answers
		/// </summary>
		Task<PollQuestion> GetNextQuestion(long chatId);
	}
}
