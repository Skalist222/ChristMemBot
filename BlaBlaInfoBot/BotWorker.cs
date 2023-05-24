using MemWorkerSpace;
using BlaBlaInfoBot;
using DataBaseWorker;
using Microsoft.Win32;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using UserWorker;
using YandexMusicApi;
using static Constantdata.ConstData;
using System;
using System.Runtime.CompilerServices;

namespace BotWorkerSpace
{
    public class USL 
    {
        public long idUser;
        public Verse lastStih;
        public USL (long id,Verse stih)
        {
            idUser = id;
            lastStih = stih;
        }
    }
    public class UserStihList: List<USL>
    {
        public Verse LastStih(long id)
        {
            for (int i = Count-1; i >= 0; i--)
            {
                if (this[i].idUser == id) return this[i].lastStih;
            }
            return Verse.Empty;
        }
        public string LastStihAddres(long id)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].idUser == id) return this[i].lastStih.AddressText;
            }
            return "Ин 3:16";
        }



        public void Add(USL usl)
        {
            for (int i = 0; i < Count; i++)
            {
                if (usl.idUser == this[i].idUser)
                {
                    this[i].lastStih = usl.lastStih;
                    return;
                }
            }
            base.Add(usl);
        }
    }

    public class BotWorker
    {
       
        //static UserStihList lastUserShih = new UserStihList();
        static bool getMessage = false;
        static bool cleanerWork = true;
       
        static string mediaGroupId = "";

        static CancellationToken token;
        static BibleWorker bW;
        static UserList users ;
        private readonly TelegramBotClient _botClient;
        static List<long> banList;
        static GoldenVerseList goldStihs;
        static Thread MultiSender;

        public BotWorker(TelegramBotClient botClient)
        {
            bW = new BibleWorker();
             users = new UserList(bW);
            _botClient = botClient;
            goldStihs = new GoldenVerseList(Constantdata.ConstData.PathGoldStih, bW);
            Thread cleaner = new Thread(CleanerWaiter);
            cleaner.Start();
            MultiSender = new Thread(SenderWorker);
            MultiSender.Start();
            banList = new List<long>();
        }
        public void CleanerWaiter()
        {
            Console.WriteLine("Запущен клинер ресендов");
            while (cleanerWork)
            {
                Thread.Sleep(10000);
                if (getMessage == false)
                {
                    string[] files = Directory.GetFiles(PathResends);
                    foreach (string f in files)
                    {
                        try 
                        { 
                            System.IO.File.Delete(f);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Не удалось полностью очиститиь папку ресенда:"+e.Message);
                        }
                       
                    }
                }
            }
        }
        public void SenderWorker()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                if (now.Hour == 00 && now.Minute == 00)
                {
                    banList = new List<long>();
                }
                if (now.Hour == 17 && now.Minute == 07)
                {
                    CreatorMessage.MultiSendRandomMem(_botClient,2);
                    Thread.Sleep(60000);
                }
                if (now.Hour == 7 && now.Minute == 15)
                {
                    CreatorMessage.MultiSendRandomMem(_botClient, 1);
                    Thread.Sleep(60000);
                }
                if (now.Hour == 21 && now.Minute == 20)
                {
                    CreatorMessage.MultiSendRandomGold(_botClient,3);
                    Thread.Sleep(60000);
                }
                if (now.Hour == 6 && now.Minute == 30)
                {
                    CreatorMessage.MultiSendRandomGold(_botClient, 1);
                    Thread.Sleep(60000);
                }
                Thread.Sleep(1000);
            }
        }

        public async Task ListenForMessagesAsync()
        {
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Сервер запущене @{me.Username}");
            Console.ReadLine();
        }
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            token = cancellationToken;
            // Only process Message updates
            if (update.Message is not { } message)
            {
                return;
            }
            getMessage = true;
            CreatorMessage.CreateAnsvere(botClient, update);
            getMessage = false;
        }
        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        private static async Task SendMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string info,long idChat = -1)
        {
            
            long id = idChat == -1 ? message.Chat.Id : idChat;
            try
            {
                Message sendArtwork = await botClient.SendTextMessageAsync(
                chatId: id,
                text: info,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine("Какой то дурак вызвал ексепшан ("+id+"): "+e.Message);
            }
        }
        private static async Task SendAdminMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken,MessageType type,string text ="")
        {
            try
            {
                long id = 1094316046;// кому отправлять
                if (message.Chat.Id != id)
                {
                    if (text == "")
                    {
                        switch (type)
                        {
                            case MessageType.Text:
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: id,
                                    text: message.Text,
                                    entities: message.Entities,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: token
                                    );
                                }
                                break;
                            case MessageType.Photo:
                                {
                                    var file = await botClient.GetFileAsync(message.Photo.LastOrDefault().FileId);
                                    var fileName = PathResends+"\\resendphoto" + file.FileId + "." + file.FilePath.Split('.').Last();
                                    using (FileStream imageSaver = new FileStream(fileName, FileMode.Create))
                                    {
                                        await botClient.DownloadFileAsync(file.FilePath, imageSaver);
                                    }
                                    SendImage(botClient, message, token, fileName);
                                }
                                break;
                            case MessageType.Sticker:
                                {
                                    await botClient.SendStickerAsync(
                                    chatId: id,
                                          sticker: message.Sticker.FileId,
                                          cancellationToken: token
                                        );
                                }
                                break;
                            case MessageType.Video:
                                {
                                    await botClient.SendTextMessageAsync(
                                    chatId: id,
                                    text: "Видео",
                                    entities: message.Entities,
                                    parseMode: ParseMode.Html,
                                    cancellationToken: token
                                    );
                                }
                                break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(
                                  chatId: id,
                                  text: text,
                                  entities: message.Entities,
                                  parseMode: ParseMode.Html,
                                  cancellationToken: token
                                  );
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Какой то дурак вызвал ексепшан: " + e.Message);
            }
        }
        private static async Task SendImage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string pathImage, long idChat = -1,string caption ="")
        {
            long id = idChat == -1 ? message.Chat.Id : idChat;
            var cap = caption == "" ? message.Caption : caption;
            var bm = Bitmap.FromFile(pathImage);
            var ms = new MemoryStream();
            bm.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            ms.Position = 0;

            await botClient.SendPhotoAsync(
            chatId: id,
                photo: new InputOnlineFile(ms, "ScreenShot.png"),
                caption: cap,
                parseMode: ParseMode.Html,
                cancellationToken: cancellationToken);
        }
        private static async Task SendImageById(ITelegramBotClient botClient, Message message,string idTelegramPhoto, long idChat = -1, string caption = "")
        {
            long id = idChat == -1 ? message.Chat.Id : idChat;
            await botClient.SendPhotoAsync(id, idTelegramPhoto);
        }
        private static async Task SendVideo(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, string pathVideo, long idChat = -1, string caption = "")
        {
            //long id = idChat == -1 ? message.Chat.Id : idChat;
            //var cap = caption == "" ? message.Caption : caption;
            //var bm = Bitmap.FromFile(pathImage);
            //var ms = new MemoryStream();
            //bm.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            //ms.Position = 0;

            //await botClient.SendPhotoAsync(
            //chatId: id,
            //    photo: new InputOnlineFile(ms, "ScreenShot.png"),
            //    caption: cap,
            //    parseMode: ParseMode.Html,
            //    cancellationToken: cancellationToken);
        }

        public class CreatorMessage
        {
            public async static void CreateAnsvere(ITelegramBotClient botClient, Update up)
            {
                Message mes = up.Message;
                if (mes is not null)
                {
                    MessageType type = up.Message.Type;
                    User user = up.Message.From;
                    string text = (up.Message.Text ?? up.Message.Caption) ?? "";
                    Commands commands = Commands.SelectByText(TextHandler.DeleteChars(text));//(tmW.GetCommand());
                    string responceText = "";
                    if (banList.IndexOf(user.Id) != -1) return;
                    if (mediaGroupId == mes.MediaGroupId && mes.MediaGroupId != "")
                    {
                        AddMem(botClient, up, token);
                    }
                    else
                    {
                        if (text == "///")
                        {
                            CommandAdmin(botClient, mes);
                            return;
                        }
                        mediaGroupId = "";
                        if (commands.IsEmpty)
                        {
                            responceText = Base.GetRandomAnswere("/empty");
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        if (commands.IsStart)
                        {
                            responceText = Base.GetRandomAnswere("/start");
                            Start(botClient, up, token, responceText);
                        }
                        else
                        if (commands.IsInfo)
                        {
                            responceText = GetInfo();
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        if (commands.IsMem)
                        {
                            responceText = Base.GetRandomAnswere("/mem");
                            SendMessageAsync(botClient, mes, token, responceText);
                            string pathMem = Mem();
                            SendImage(botClient, mes, token, pathMem);
                            SendImage(botClient, mes, token, pathMem, 1094316046L);
                        }
                        else
                        if (commands.IsAddMem)
                        {
                            if (type == MessageType.Photo)
                            {
                                //AgACAgIAAxkBAAIVKWRsVt6B12aC_n7HzoL-Gq3A5tLOAAItyzEbDe9gSxVbzv5UsBQ3AQADAgADcwADLwQ
                                //SendImageById(botClient, mes, "AgACAgIAAxkBAAIVKWRsVt6B12aC_n7HzoL-Gq3A5tLOAAItyzEbDe9gSxVbzv5UsBQ3AQADAgADcwADLwQ");
                                //SendImage(botClient,mes,token,mes.Photo.LastOrDefault().FileId);

                                responceText = Base.GetRandomAnswere("/add") + " за мем спасибо!)";
                                AddMem(botClient, up, token);
                                SendMessageAsync(botClient, mes, token, responceText);
                                SendAdminMessageAsync(botClient, mes, token, mes.Type, "Добавлен новый мем! От " + user.FirstName + "(" + user.Id + ")");
                            }
                            else
                            {
                                responceText = "Я конечно добавлю мем, но ты пришли картинку и в подписи напиши что это новый мем";
                                SendMessageAsync(botClient, mes, token, responceText);
                            }
                        }
                        else
                        if (commands.IsStih)
                        {
                            responceText = Verse(Base.GetRandomAnswere("/stih"), user.Id);
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                         if (commands.IsSearchStih || commands.IsSearch)
                        {
                            responceText = Base.GetRandomAnswere("/search") + ", " + Base.GetRandomAnswere("/stih").ToLower();
                            //SendMessageAsync(botClient, mes, token, responceText);
                            Verse selectedS = bW.SearchVerse(text);
                            if (selectedS is null) SendMessageAsync(botClient, mes, token, Base.GetRandomAnswere("/dontSelectedStih"));
                            else
                            {
                                users[user.Id].LastStih = selectedS;
                                responceText += Environment.NewLine + selectedS.ToString();
                                SendMessageAsync(botClient, mes, token, responceText);
                            }
                        }
                        else
                        if (commands.IsGoldStih)
                        {
                            GoldenVerseList gs = new GoldenVerseList(Constantdata.ConstData.PathGoldStih, bW);
                            responceText = GoldVerse(gs, Base.GetRandomAnswere("/gold"));
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        if (commands.IsAddGoldStih)
                        {
                            responceText = Base.GetRandomAnswere("/add") + ". Спасибо за " + Base.GetRandomAnswere("/gold");
                            SendMessageAsync(botClient, mes, token, CreateGoldenStih(responceText, user.Id));
                        }
                        else
                        if (commands.IsLeft)
                        {
                            responceText = PreVerse(user.Id);
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        if (commands.IsRight)
                        {
                            responceText = NextVerse(user.Id);
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        if (commands.IsHelp)
                        {
                            responceText = GetHelp();
                            SendMessageAsync(botClient, mes, token, responceText);
                        }
                        else
                        {
                          
                            responceText = commands.GetAllA(new string[] 
                            //Список команд, которые будут добавлять ответы
                            { "/hello","/rest","/haha","/howAreYou","/whatDo","/which","/expletive"});
                            if (commands.Have("/expletive")) 
                            {
                                if (users[user.Id].Expletive())
                                {
                                    responceText = "Прости, но я не буду разговаривать с грубияном (бан на день)";
                                }
                            }
                            SendMessageAsync(botClient, mes, token, responceText);
                        }

                        SendAdminMessageAsync(botClient, mes, token, type, "От " + user.FirstName + "(" + user.Id + ")");
                        SendAdminMessageAsync(botClient, mes, token, type, text);
                        SaveMessage(text, up, responceText);
                    }
                }
                else
                {
                    Console.WriteLine("Получено пустое сообщение");
                }
            }
            public static void SaveMessage(string message, Update up, string responce)
            {
                string textInFile = "----------------Start Message---------------" + Environment.NewLine;
                textInFile += "(" + up.Message.From.Username + " " + up.Message.From.FirstName + " " + up.Message.From.LastName + ") ";
                textInFile += up.Message.Date.ToLongDateString() + Environment.NewLine;
                textInFile += "mesText: " + message + Environment.NewLine;
                textInFile += "responce: " + responce + Environment.NewLine;
                textInFile += "____________________________________________" + Environment.NewLine;
                string pathFile = Path.Combine(DefaultPath, "Messages.txt");
                Console.WriteLine(textInFile);
                System.IO.File.AppendAllText(pathFile, textInFile);
            }




            // Commands
            public static string GetInfo()
            {
                string info = System.IO.File.ReadAllText(PathInfo);
                info += Environment.NewLine;
                info += "Мемов:" + new MemWorker(PathMems).Length + Environment.NewLine;
                info += "Золотых стихов:" + goldStihs.Count() + Environment.NewLine;
                return info;
            }
            public static string GetHelp()
            {
                string info = System.IO.File.ReadAllText(PathHelp);
                info += Environment.NewLine;
                info += "Мемов:" + new MemWorker(PathMems).Length + Environment.NewLine;
                info += "Золотых стихов:" + goldStihs.Count() + Environment.NewLine;
                return info;
            }
            public static string Verse(string preview, long idUser)
            {
                UserI selectuser = users[idUser];
                Verse newStih = bW.GetRandomVerse();
                if (selectuser is not null)
                {
                    users[idUser].LastStih = newStih;
                }

                string t = preview + Environment.NewLine + Environment.NewLine;
                //lastUserShih.Add(new USL(idUser, stih));
                return t + newStih.ToString();
            }
            public static string PreVerse(long idUser)
            {
                Verse l = users[idUser] is not null ? users[idUser].LastStih : null;
                if (l is not null)
                {
                    Verse preStih = bW.GetPreVerse(l);
                    users[idUser].LastStih = preStih;
                    return preStih.ToString();
                }
                else
                {
                    return Verse("Ты еще не запрашивал случайный стих, поэтому вот тебе он", idUser);
                }
            }
            public static string NextVerse(long idUser)
            {
                Verse l = users[idUser] is not null ? users[idUser].LastStih : null;
                if (l is not null)
                {
                    Verse nextSt = bW.GetNextVerse(l);
                    users[idUser].LastStih = nextSt;
                    return nextSt.ToString();
                }
                else
                {
                    return Verse("Ты еще не запрашивал случайный стих, поэтому вот тебе он", idUser);
                }

            }
            public static string GoldVerse(GoldenVerseList gs, string preview)
            {
                string t = preview + Environment.NewLine;
                Verse stih = gs.GetRandomGoldVerse();
                string stihS = stih.ToString();
                return t + stihS;
            }
            public static string CreateGoldenStih(string preview, long idUser)
            {
                Verse newGoldStih = users[idUser] is not null ? users[idUser].LastStih : null;
                if (newGoldStih is null)
                {
                    return Verse("Ты еще не получал случайный стих и не можешь добавить золотой стих =(" + Environment.NewLine + "Но не печалься, вот тебе случайный стих, можешь его добавить если понравится 🥰", idUser);
                }
                try
                {
                    users[idUser].LastAddGold = newGoldStih;
                    goldStihs.Add(newGoldStih);
                    return preview;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Добавление золотого стиха " + e.Message);
                    return "кажется какая то кракозябра... не работает добавление, скажи об этом владельцу бота";
                }
            }
            public static async void AddMem(ITelegramBotClient botClient, Update up, CancellationToken token)
            {
                try
                {
                    int countPic = 0;
                    mediaGroupId = up.Message.MediaGroupId ?? "";
                    string idFile = up.Message.Photo.LastOrDefault().FileId;
                    var file = await botClient.GetFileAsync(idFile);
                    var fileName = idFile + "." + file.FilePath.Split('.').Last();
                    using (FileStream imageSaver = new FileStream(fileName, FileMode.Create))
                    {
                        await botClient.DownloadFileAsync(file.FilePath, imageSaver);
                    }
                    Base.CreateNewMem(idFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка добавления мема!");
                    Console.WriteLine(ex);
                }
            }
            public static async void Start(ITelegramBotClient botClient, Update up, CancellationToken token, string answer)
            {

                answer += Environment.NewLine;
                User us = up.Message.From;
                UserI user = users[us.Id];
                if (user is null)
                {
                    users.Add(new UserI(us.Id, us.Username, us.FirstName, us.LastName, bW));
                    answer += $"Дорогой друг, {us.FirstName}, добро пожаловать!Вот информация о текущей версии бота:" + Environment.NewLine;
                    answer += GetInfo();
                }
                
                
                KeyboardButton mem = new KeyboardButton("Мем");
                KeyboardButton start = new KeyboardButton("Старт");
                KeyboardButton previous = new KeyboardButton("<-");
                KeyboardButton next = new KeyboardButton("->");

                KeyboardButton gold = new KeyboardButton("Золотой стих");
                KeyboardButton stih = new KeyboardButton("Стих");

                KeyboardButton addGoldStih = new KeyboardButton("Добавить золотой стих");
                KeyboardButton help = new KeyboardButton("Помощь");

                List<List<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>> board = new List<List<Telegram.Bot.Types.ReplyMarkups.KeyboardButton>>()
                { 
                    new List<KeyboardButton>(){ mem, start,previous,next }, 
                    new List<KeyboardButton>(){ gold, stih},
                    new List<KeyboardButton>(){ addGoldStih},
                    new List<KeyboardButton>(){ help }
                };
                ReplyKeyboardMarkup mrkp = new ReplyKeyboardMarkup(keyboard: board);
                mrkp.ResizeKeyboard = true;
                await botClient.SendTextMessageAsync(
                    chatId: up.Message.Chat.Id,
                    text: answer,
                    replyMarkup: mrkp
                );

            }
            public static string Mem()
            {
                try
                {
                    MemWorker mw = new MemWorker(PathMems);
                    string ImagePath = mw.GetRandomPathMem();
                    return ImagePath;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return "";
                }
            }

            public static void CommandAdmin(ITelegramBotClient botClient, Message message)
            {
                //1094316046 я
                //1048864593L Катя
                //743951077L Алинчик
                //5077255153 лёшка
                //950360052 Вика
                SendMessageAsync(botClient, message, token, $"Внимание Внимание!{Environment.NewLine}{GetHelp()}{Environment.NewLine} P.S. С любовью к братьям и сестрам Валентин.", 1094316046L);

                //MultiSendText(botClient,$"Внимание Внимание!{Environment.NewLine}{GetBotInfo()}{Environment.NewLine} P.S. С любовью к братьям и сестрам Валентин.");
            }
            public static void MultiSendText(ITelegramBotClient botClient, string text)
            {
                foreach (UserI u in users)
                {
                    SendMessageAsync(botClient, new Message(), token, text, u.Id);
                }
            }
            public static void MultiSendRandomMem(ITelegramBotClient botClient, byte time)
            {
                string caption = "";
                if (time == 1) caption = "Ежедневный утренний мем";
                if (time == 2) caption = "Ежедневный вечерний мем";
                foreach (UserI u in users)
                {
                    string pathMem = new MemWorker(PathMems).GetRandomPathMem();
                    SendImage(botClient, new Message(), token, pathMem, u.Id, caption + Environment.NewLine);
                }

            }
            public static void MultiSendRandomGold(ITelegramBotClient botClient, byte time)
            {
                string caption = "";
                if (time == 1) caption = "Золотой стих с утра";
                if (time == 3) caption = "Золотой стих на ночь";
                foreach (UserI u in users)
                {
                    Verse stih = goldStihs.GetRandomGoldVerse();
                    SendMessageAsync(botClient, new Message(), token, caption + Environment.NewLine + Environment.NewLine + stih.ToString(), u.Id);
                    //SendImage(botClient, new Message(), token, pathMem, u.Id, caption + Environment.NewLine);
                }

            }
        }

    }
}
