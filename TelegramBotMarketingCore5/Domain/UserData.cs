using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBotMarketing.Domain
{
	internal enum PollState { Started, NotStarted, Completed }
	internal class UserData
	{
		public long chatId;
		//public int pollCompletionTimes = 0;
		public PollState pollState = PollState.NotStarted;
		public List<Answer> questionAnswers = new();

		public class Answer
		{
			public string questionId, answer;
		}
	}
}
