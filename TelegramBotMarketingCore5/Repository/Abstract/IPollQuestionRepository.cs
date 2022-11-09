using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotMarketingCore5.Domain;

namespace TelegramBotMarketingCore5.Repository.Abstract
{
	internal interface IPollQuestionRepository
	{
		Task<PollQuestion> Get(string id);
		Task SaveToFile(List<PollQuestion> pollQuestions);
	}
}
