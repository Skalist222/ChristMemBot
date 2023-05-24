using BotWorkerSpace;
using Telegram.Bot;

var botClient = new TelegramBotClient("6178215431:AAGCQ2hBtmEcatAVZJ2EGgCZr_yoXtSp3T0");
var me = await botClient.GetMeAsync();

BotWorker bW = new BotWorker(botClient);
bW.ListenForMessagesAsync().GetAwaiter().GetResult();