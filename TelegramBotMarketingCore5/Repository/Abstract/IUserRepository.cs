using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBotMarketing.Domain;

namespace TelegramBotMarketingCore5.Repository.Abstract
{
	internal interface IUserRepository
	{
		Task<UserData> Create(long chatId);
		Task<UserData> Get(long chatId);
		Task Update(UserData data);
		Task Delete(long chatId);
	}
}
