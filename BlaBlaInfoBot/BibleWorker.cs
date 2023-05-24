using DataBaseWorker;
using System.Data;
using System.Xml;


namespace BlaBlaInfoBot
{
    public class BibleWorker
    {
        public string pathBible;
        public Books books;

        public BibleWorker()
        {
            pathBible = Directory.GetCurrentDirectory() + "\\Bible.xml";
           
            books = new Books();

            XmlDocument doc = new XmlDocument();
            doc.Load(pathBible);
            
            XmlNodeList booksXml = doc.GetElementsByTagName("BIBLEBOOK");
            for (int i = 0; i < booksXml.Count; i++)
            {
                XmlNodeList chapters = booksXml[i].ChildNodes;
                ChapterList chList = new ChapterList();
                for (int j = 0; j < chapters.Count; j++)
                {

                    VerseList stList = new VerseList();
                    XmlNodeList verses = chapters[j].ChildNodes;
                    for (int n = 0; n < verses.Count; n++)
                    {
                        AddressVerse address = new AddressVerse(books[i ], j, n);
                        stList.Add(new Verse(address, verses[n].InnerText));
                    }
                    chList.Add(new Chapter(i, stList));
                }
                books[i].SetChapters(chList);
            }
        }
        public Verse GetRandomVerse()
        {
           return books.GetRandomVerse();
        }
        public Verse? GetVerse(string address)
        {
            string nameBook = address.Split(' ')[0];
            int numC = Convert.ToInt32(address.Split(' ')[1].Split(':')[0]);
            int numS = Convert.ToInt32(address.Split(' ')[1].Split(':')[1]);

            Book book = books.GetBookByShortName(nameBook);
            Chapter chapter = book.chapters[numC-1];
            if (chapter is null) return null;
            Verse verse = chapter.verses[numS-1];
            if (verse is null) return null;
            return verse;
            
        }
        public Verse GetNextVerse(Verse s)
        {
            Verse retVerse = GetVerse("Быт 1:1");
            if (s.AddressText == "Откр 22:21") return retVerse;
            else
            {
                AddressVerse a = s.Address;
                int bId = a.BookId - 1;
                int cId = a.ChapterId - 1;
                int sId = a.VerseId - 1;

                if (sId == a.MaxVerse)
                {
                    sId = 0;
                    cId++;
                    if (cId > a.MaxChapter)
                    {
                        cId = 0;
                        bId++;
                    }
                }
                else
                {
                    sId++;
                   
                }
                a = new AddressVerse(books[bId], cId, sId);
                return a.Vers;
            }
        }
        public Verse GetPreVerse(Verse s)
        {
            Verse retVerse = GetVerse("Откр 22:21");
            if (s.AddressText == "Быт 1:1") return retVerse;
            else
            {
                AddressVerse a = s.Address;
                int bId = a.BookId - 1;
                int cId = a.ChapterId - 1;
                int sId = a.VerseId - 1;

                if (sId == 0)
                {
                    cId--;
                    if (cId == -1)
                    {
                        bId--;
                        a = new AddressVerse(books[bId], cId, 1);
                        cId = a.MaxChapter;
                    }
                    a = new AddressVerse(books[bId], cId, sId);
                    a = new AddressVerse(books[bId], cId, a.MaxVerse);
                }
                else
                {
                    sId--;
                    a = new AddressVerse(books[bId], cId, sId);
                }
                
                return a.Vers;
            }
        }

        public Verse? SearchVerse(string text)
        {
            string[] split = text.Split(' ');
            if (split.Length==2) return books[0][0][0];
            Book selectedBook = null;
            int bookNum = -1;
            int chI=0;
            int stI=0;

            for (int i=0;i<split.Length;i++)
            {
                string s = split[i];
                string[] wDot = s.Split(':');
                if (wDot.Length == 2)
                {
                    chI = int.Parse(wDot[0])-1;
                    stI = int.Parse(wDot[1])-1;
                }else
                if (s == "1") { bookNum = 1; }
                else
                if (s == "2") { bookNum = 2; }
                else
                if (s == "3") { bookNum = 3; }
                else
                if (s == "4") { 
                    bookNum = 4; }
                else
                {
                    if (selectedBook is null)
                    {
                        foreach (Book b in books)
                        {
                            if (b.Name.ToLower().IndexOf(s.ToLower()) != -1 || b.ShortName.ToLower().IndexOf(s.ToLower()) != -1)
                            {
                                selectedBook = b;
                                break;
                            }
                        }
                        if (selectedBook is not null)
                        {
                            if (bookNum != -1)
                            {
                                bookNum--;
                                selectedBook = books[(selectedBook.Id - 1) + bookNum];
                                if (selectedBook.ShortName[0].ToString() != (bookNum + 1) + "") return null;
                            }
                        }
                        
                    }
                }
            }
            if (selectedBook is null) return null;
            Chapter chapter = selectedBook[chI];
            Verse verse = null;
            if (chapter is not null) verse = chapter[stI];
            return verse;
        }
    }
    public class Book 
    {
        int id;
        string name;
        string shortName;
        public ChapterList chapters;
        public Chapter? this[int chapterId]
        {
            get { return chapterId>=0 && chapterId < chapters.Count ? chapters[chapterId] : null; }
        }
        public int CountChapters { get { return chapters.Count; } }
        public string Name { get { return name; } }
        public string ShortName { get { return shortName; } }
        public int Id { get { return id; } }

        public static Book Empty { get { return new Book(-1,""); } }

        public static bool operator ==(Book b1, Book b2)
        {
            return b1.name == b2.Name;
        }
        public static bool operator !=(Book b1, Book b2)
        {
            return b1.name != b2.Name;
        }
        public string ToString()
        {
            return Name;
        }

        public Book(int id, string name, string shortName = "", ChapterList chapters = null)
        {
            chapters = chapters ?? new ChapterList();
            this.shortName = shortName;
            this.id = id;
            this.name = name;
            this.chapters = chapters;
            
        }
        public void SetChapters(ChapterList list)
        {
            chapters = list;
        }
    }
    public class Chapter
    {
        int idInBook;
        public VerseList verses;
        public Verse? this[int verseId]
        {
            get { return verseId >= 0 && verseId < verses.Count ? verses[verseId] : null; }
        }
        public Chapter(int idInBook, VerseList verses)
        {
            this.idInBook = idInBook;
            this.verses = verses;
        }
    }
    public class Verse
    {
        AddressVerse address;

        string text;
        string description;

       

        public string Text { get { return text; } }
        public AddressVerse Address { get { return address; } }
        public string AddressText { get { return address.Text; } }
        public Verse(AddressVerse address, string text, string description = "")
        {
            this.address = address;
            this.text = text;
            this.description = description;
        }
        public static Verse Empty { get { return new Verse(AddressVerse.Empty, ""); } }
        public bool IsEmpty { get { return this.AddressText==" 0:0" ; } }
         public string ToString()
        {
            return AddressText + " \"" + Text + "\"";
        }
    }
    public class AddressVerse
    {
        Book book;
        int chapterId;
        int verseId;

        public int BookId { get { return book.Id; } }
        public int ChapterId { get { return chapterId+1; } }
        public int VerseId { get { return verseId+1; } }

        public int MaxChapter { get { return book.chapters.Count-1; } }
        public int MaxVerse { get { return Chapter.verses.Count-1; } }

        public Book Book { get { return book; } }
        public Chapter Chapter { get { return book.chapters[chapterId]; } }
        public Verse Vers { get { return book.chapters[chapterId].verses[verseId]; } }

        public string Text { get { return book.ShortName+ " " + ChapterId + ":"+ VerseId; } }
        public bool IsEmpty { get { return this.BookId == -1; } }
        public static AddressVerse Empty { get { return new AddressVerse(Book.Empty,-1,-1); } }

        public AddressVerse(Book b, int c, int s)
        {
            book = b;
            chapterId = c;
            verseId = s;
        }
    }

    public class Books : List<Book>
    {
        public Books()
        {
            // Add(new Book(1, "Name", "Socr"));
            Add(new Book(1, "Бытие", "Быт"));
            Add(new Book(2, "Исход", "Исх"));
            Add(new Book(3, "Левит", "Лев"));
            Add(new Book(4, "Числа", "Чис"));
            Add(new Book(5, "Второзаконие", "Втор"));

            Add(new Book(6,  "Иисус Навин", "Нав"));
            Add(new Book(7,  "Судьи", "Суд"));
            Add(new Book(8,  "Руфь", "Руфь"));
            Add(new Book(9,  "1 Царств", "1Цар"));
            Add(new Book(10, "2 Царств", "2Цар"));
            Add(new Book(11, "3 Царств", "3Цар"));
            Add(new Book(12, "4 Царств", "4Цар"));
            Add(new Book(13, "1 Паралипоменон", "1Пар"));
            Add(new Book(14, "2 Паралипоменон", "2Пар"));
            Add(new Book(15, "Ездра", "Езд"));
            Add(new Book(16, "Неемия", "Неем"));
            Add(new Book(17, "Есфирь", "Есф"));

            Add(new Book(18, "Иов", "Иов"));
            Add(new Book(19, "Псалтырь", "Пс"));
            Add(new Book(20, "Притчи Соломона", "Притч"));
            Add(new Book(21, "Екклесиаст", "Екл"));
            Add(new Book(22, "Песнь Песней Соломона", "Песн"));

            Add(new Book(23, "Исаия", "Ис"));
            Add(new Book(24, "Иеремия", "Иер"));
            Add(new Book(25, "Плач Иеремии", "Плч"));
            Add(new Book(26, "Иезекииль", "Иез"));
            Add(new Book(27, "Даниил", "Дан"));

            Add(new Book(28, "Осия", "Ос"));
            Add(new Book(29, "Иоиль", "Иоил"));
            Add(new Book(30, "Амос", "Ам"));
            Add(new Book(31, "Авдий", "Авд"));
            Add(new Book(32, "Иона", "Ион"));
            Add(new Book(33, "Михей", "Мих"));
            Add(new Book(34, "Наум", "Наум"));
            Add(new Book(35, "Аввакум", "Авв"));
            Add(new Book(36, "Софония", "Соф"));
            Add(new Book(37, "Аггей", "Агг"));
            Add(new Book(38, "Захария", "Зах"));
            Add(new Book(39, "Малахия", "Мал"));

            Add(new Book(40, "От Матфея", "Мф"));
            Add(new Book(41, "От Марка", "Мк"));
            Add(new Book(42, "От Луки", "Лк"));
            Add(new Book(43, "От Иоанна", "Ин"));

            Add(new Book(44, "Деяния Апостолов", "Деян"));


            Add(new Book(45, "Послание к Римлянам", "Рим"));
            Add(new Book(46, "Первое послание к Коринфянам", "1Кор"));
            Add(new Book(47, "Второе послание к Коринфянам", "2Кор"));
            Add(new Book(48, "Послание к Галатам", "Гал"));
            Add(new Book(49, "Послание к Ефесянам", "Еф"));
            Add(new Book(50, "Послание к Филлипийцам", "Флп"));
            Add(new Book(51, "Послание Колоссянам", "Кол"));
            Add(new Book(52, "Первое послание к Фессылоникийцам (Солунянам)", "1Фес"));
            Add(new Book(53, "Второе послание к Фессылоникийцам (Солунянам)", "2Фес"));
            Add(new Book(54, "Первое послание Тимофею", "1Тим"));
            Add(new Book(55, "Второе послание Тимофею", "2Тим"));
            Add(new Book(56, "Послание Титу", "Тит"));
            Add(new Book(57, "Послание Филимону", "Флм"));
            Add(new Book(58, "Послание Евреям", "Евр"));

            Add(new Book(59, "Иакова", "Иак"));
            Add(new Book(60, "1 Петра", "1Пет"));
            Add(new Book(61, "2 Петра", "2Пет"));
            Add(new Book(62, "1 Иоанна", "1Ин"));
            Add(new Book(63, "2 Иоанна", "2Ин"));
            Add(new Book(64, "3 Иоанна", "3Ин"));
            Add(new Book(65, "Иуды", "Иуд"));
            Add(new Book(66, "Откровение", "Откр"));
        }
        public Verse GetRandomVerse()
        {
            Random r = new Random();
            int i = r.Next(0,65);
            Book b = this[i];
            i = r.Next(0, b.CountChapters);
            Chapter c = b.chapters[i];

            i = r.Next(c.verses.Count);
            Verse s = c.verses[i];
            return s;
        }
        public Book GetBookByName(string name)
        {
            foreach (Book b in this)
            {
                if (b.Name.ToLower().IndexOf(name.ToLower()) != -1) return b;
            }
            return Book.Empty;
        }
        public Book GetBookByShortName(string name)
        {
            foreach (Book b in this)
            {
                if (b.ShortName.ToLower()== name.ToLower())
                    return b;
            }
            return Book.Empty;
        }
    }
    public class ChapterList : List<Chapter> { }
    public class VerseList:List<Verse>{}
    public class GoldenVerseList : VerseList
    {
        public GoldenVerseList(string pathFileArray, BibleWorker wortker)
        {
            DataTable t = Base.GetAllGoldverses();
            foreach (DataRow row in t.Rows)
            {
                Verse s = wortker.GetVerse(row["address"].ToString());
                if (s is not null)
                {
                    base.Add(s); 
                }
            }
        }
        public Verse? GetRandomGoldVerse()
        {
            try
            {
                Random r = new Random();
                int id = r.Next(0, base.Count);
                Verse verse = this[id];
                return verse;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        public void Add(Verse s)
        {
            if (this.IndexOf(s) == -1)
            {
                base.Add(s);
                Base.CreateGold(s);
            }
           
        }
        public int Count
        {
            get 
            {
                return Base.GetAllGoldverses().Rows.Count;
            }
        }
    }
}
