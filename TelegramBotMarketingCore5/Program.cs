using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using TelegramBotMarketing.Utility;
using TelegramBotMarketing.EventHandling.Abstract;
using TelegramBotMarketing.EventHandling;
using TelegramBotMarketingCore5.Service.Abstract;
using TelegramBotMarketingCore5.Service;
using TelegramBotMarketing.Service.Abstract;
using TelegramBotMarketing.Service;
using TelegramBotMarketingCore5.Repository.Abstract;
using TelegramBotMarketingCore5.Repository;
using RuntimeConfig;

namespace TelegramBotMarketing
{
	internal class Program
	{

		public static Task Main(string[] args)
		{
			using var host = CreateHostBuilder(args).Build();
			RunBot(host.Services);
			return host.RunAsync();
		}

		static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			services.AddTransient<IConfig, TextFileConfig>();
			services.Chain<IBotUpdateHandlerChain>()
				.Add<TextMessageHandler>()
				.Add<NonTextMessageHandler>()
				.Configure();
			services.AddTransient<ICommandResponceGenerator, CommandResponceGenerator>();
			services.AddTransient<IMessageResponceGenerator, MessageResponceGenerator>();
			services.AddSingleton<IUpdateHandler, UpdateHandler>();
			services.AddSingleton<IUserRepository, UserFileRepository>();
			services.AddSingleton<IPollQuestionRepository, FilePollQuestionRepository>();
			services.AddSingleton<IPollAnswerManager, PollAnswerManager>();
			services.AddSingleton<ITelegramBot, TelegramBot>();
		}

		private static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args).
				ConfigureServices((context, services) => ConfigureServices(context, services));
		}

		public static void RunBot(IServiceProvider services)
		{
			using var serviceScope = services.CreateScope();
			var provider = serviceScope.ServiceProvider;

			var bot = provider.GetRequiredService<ITelegramBot>();
			var config = provider.GetRequiredService<IConfig>();
			bot.Start(config["botKey"]);
			Console.ReadLine();
		}
	}
}
