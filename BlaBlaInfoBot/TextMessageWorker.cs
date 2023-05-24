using DataBaseWorker;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using static BlaBlaInfoBot.Commands;
using static System.Net.Mime.MediaTypeNames;

namespace BlaBlaInfoBot
{
    /// <summary>
    /// Для добавления команды нужно:
    /// 1) в WordsCommands добавить новую команду
    /// 2) в TextMessageWorker добавить поле только для ьчтения
    /// 3) в  конструкторе TextMessageWorker добавить присвоение значение этой переменной
    /// 4) в ComandWariants создать комбинатор
    /// 5) в ComandWariants в commands добавить текстовое представление команды такое же какое прописал в WordsCommands
    /// 6) в Commands добавить валидатор типа IsCommand
    /// </summary>


    public class FunctionLecal
    {
        Words lecalWords;
        Function func;
        public FunctionLecal(Words lecalWords, Function func)
        {
            this.lecalWords = lecalWords;
            this.func = func;
        }
    }
    public class Function
    {
        string command;
        string description;
        public string Command { get { return command; } }
        public string Description { get { return description; } }

        public Function(string command, string description)
        {
            this.command = command;
            this.description = description;
        }
        
    }
    public class Words : List<Word> 
    {
        public Words(string[] args)
        {
            foreach (string a in args)
            {
                Add(new Word(a)) ;
            }
        }
        public static Words Get(string text,char separator=' ')
        {
            string[] split = text.Split(separator);
            Words w = new Words(split);
            return w;
        }
       
    }
    public class Word
    {
        string text;
        public string Text { get { return text; } }
        public Word(string t)
        {
            this.text = t;
        }
        public static bool operator ==(Word w1, Word w2)
        {
            return w1.text.ToLower() == w2.text.ToLower();
        }
        public static bool operator !=(Word w1, Word w2)
        {
            return !(w1 == w2);
        }

        public string ToString()
        {
            return text;
        }
    }
   
    public class Commands : List<TextCommand> 
    {
        public static Commands Empty { get { return new Commands(); } }
        public bool Have(string com)
        {
            foreach (TextCommand c in this)
            {
                if (c.Text == com) return true;
            }
            return false;
        }
        public string GetA(string com)
        {
            if (Have(com)) return Base.GetRandomAnswere(com);
            else return "";
        }
        public string GetAllA()
        {
            string retStr = "";
            foreach (TextCommand c in this)
            {
                retStr+= Base.GetRandomAnswere(c.Text) ?? "";
            }
            return retStr;
        }
        public string GetAllA(string[] commands)
        {
            string retStr = "";
            foreach (string s in commands)
            {
                retStr += GetA(s)+" ";
            }
            return retStr;
        }
        public class CommandWariants
        {
            static string[] commands = new string[] { 
            /*0*/"/start",
            /*1*/"/get", 
            /*2*/"/screen",
            /*3*/"/stih", 
            /*4*/"/mem", 
            /*5*/"/info",
            /*6*/"/add",
            /*7*/"/gold",
            /*8*/"/vp",
            /*9*/"/left",
            /*10*/"/right",
            /*11*/"/search",
            /*12*/"/howAreYou",
            /*13*/"/whatDo",
            /*14*/"/hahaha",
            /*15*/"/helo",
            /*16*/"/rest"
        };
            public static string[] Commands { get { return commands; } }
            public static Commands MemAdd
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(4, commands[4]));
                    c.Add(new TextCommand(6, commands[6]));
                    return c;
                }
            }
            public static Commands GetScreen
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(1, commands[1]));
                    c.Add(new TextCommand(2, commands[2]));
                    return c;
                }
            }
            public static Commands GetStih
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(1, commands[1]));
                    c.Add(new TextCommand(3, commands[3]));
                    return c;
                }
            }
            public static Commands GetMem
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(1, commands[1]));
                    c.Add(new TextCommand(4, commands[4]));
                    return c;
                }
            }
            public static Commands GetInfo
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(1, commands[1]));
                    c.Add(new TextCommand(5, commands[5]));
                    return c;
                }
            }
            public static Commands AddGoldStih
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(3, commands[3]));
                    c.Add(new TextCommand(6, commands[6]));
                    c.Add(new TextCommand(7, commands[7]));
                    return c;
                }
            }
            public static Commands Screen
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(2, commands[2]));
                    return c;
                }
            }
            public static Commands GoldStih
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(3, commands[3]));
                    c.Add(new TextCommand(7, commands[7]));
                    return c;
                }
            }
            public static Commands Stih
            {
                get
                {
                    Commands c = new Commands();

                    c.Add(new TextCommand(3, commands[3]));
                    return c;
                }
            }
            public static Commands Mem
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(4, commands[4]));
                    return c;
                }
            }
            public static Commands Info
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(5, commands[5]));
                    return c;
                }
            }
            public static Commands Start
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(0, commands[0]));
                    return c;
                }
            }
            public static Commands VP
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(8, commands[8]));
                    return c;
                }
            }
            public static Commands MemVP
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(4, commands[4]));
                    c.Add(new TextCommand(8, commands[8]));
                    return c;
                }
            }
            public static Commands Left
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(9, commands[9]));
                    return c;
                }
            }
            public static Commands Right
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(10, commands[10]));
                    return c;
                }
            }
            public static Commands Search
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(11, commands[11]));
                    return c;
                }
            }
            public static Commands SearchStih
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(3, commands[3]));
                    c.Add(new TextCommand(11, commands[11]));
                    return c;
                }
            }
            public static Commands HowAreYou
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(12, commands[12]));
                    return c;
                }
            }
            public static Commands WhatDo
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(13, commands[13]));
                    return c;
                }
            }
            public static Commands Hahaha
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(14, commands[14]));
                    return c;
                }
            }
            public static Commands HelloMan
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(15, commands[15]));
                    return c;
                }
            }
            public static Commands Rest
            {
                get
                {
                    Commands c = new Commands();
                    c.Add(new TextCommand(16, commands[16]));
                    return c;
                }
            }

        }
        public static Commands SelectByText(string text,char split=' ')
        {
            try
            {
                Commands cmds = new Commands();
                string[] splitS = text.Split(' ');
                foreach (string s in splitS)
                {
                    DataTable t = Base.Select($"Select idCommand From responce_word Where word Like '{s}'");
                    if (t.Rows.Count > 0)
                    {
                        TextCommand tCom = Base.GetCommand(t.Rows[0][0].ToString());
                        if (tCom is not null) cmds.Add(tCom);
                        else {
                            int a = 0;
                        }
                    }
                }
                return cmds;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
        public bool IsEmpty { get { 
                return this == Empty;
            } }
        public bool IsInfo { get { return this.Count == 1 && this[0].Text == "/info"; } }
        public bool IsStih { get { return this.Count == 1 && this[0].Text == "/stih"; } }
        public bool IsMem { get { return this.Count == 1 && this[0].Text == "/mem"; } }
        public bool IsAddMem { get {
                if (this.Count == 2)
                {
                    bool add = false;
                    bool mem = false;
                    foreach (TextCommand c in this)
                    {
                        if (c.Text == "/add") add = true;
                        if (c.Text == "/mem") mem = true;
                    }
                    return add && mem;
                }
                else
                {
                    return false;
                }
            } }
        public bool IsGoldStih { get {
                if (this.Count == 2)
                {
                    bool gold = false;
                    bool stih = false;
                    foreach (TextCommand c in this)
                    {
                        if (c.Text == "/gold") gold = true;
                        if (c.Text == "/stih") stih = true;
                    }
                    return gold && stih;
                }
                else
                {
                    return false;
                }
            } }
        public bool IsAddGoldStih { get {

                if (this.Count == 3)
                {
                    bool add = false;
                    bool gold = false;
                    bool mem = false;
                    foreach (TextCommand c in this)
                    {
                        if (c.Text == "/add") add = true;
                        if (c.Text == "/mem") mem = true;
                        if (c.Text == "/gold") gold = true;
                    }
                    return add && gold && mem;
                }
              
                if (this.Count == 2)
                {
                    bool add = false;
                    bool gold = false;
                    foreach (TextCommand c in this)
                    {
                        if (c.Text == "/add") add = true;
                        if (c.Text == "/gold") gold = true;
                    }
                    return add && gold;
                }
                else
                {
                    return false;
                }
            } }
        public bool IsStart { get { return this.Count == 1 && this[0].Text == "/start"; } }
        public bool IsVP { get { return this.Count == 1 && this[0].Text == "/vp"; } }
        public bool IsLeft { get { return this.Count == 1 && this[0].Text == "/left";  } }
        public bool IsRight { get { return this.Count == 1 && this[0].Text == "/right"; } }
        public bool IsSearch { get { return this.Count == 1 && this[0].Text == "/search"; } }
        public bool IsSearchStih { get {
                if (this.Count == 1) return this[0].Text == "/search";
                else
                {
                    if (this.Count == 2)
                    {
                        bool search = false;
                        bool stih = false;
                        foreach (TextCommand c in this)
                        {
                            if (c.Text == "/search") search = true;
                            if (c.Text == "/stih") stih = true;
                        }
                        return search && stih;
                    }
                    else
                    {
                        return false;
                    }
                }
            } }
        public bool IsHowAreYou { get { return this.Count == 1 && this[0].Text == "/howAreYou"; } }
        public bool IsWhatDo { get { return this.Count == 1 && this[0].Text == "/whatDo"; } } 
        public bool IsHaha { get { return this.Count == 1 && this[0].Text == "/hahaha"; } }
        public bool IsHelo { get { return this.Count == 1 && this[0].Text == "/helo"; } }
        public bool IsRest { get { return this.Count == 1 && this[0].Text == "/rest"; } }
        public bool IsHelp { get {return this.Count == 1 && this[0].Text == "/help";} }
        public static bool operator ==(Commands c1, Commands c2)
        {
            if (c1.Count != c2.Count) return false;
            else
            {
                for (int i = 0; i < c1.Count; i++)
                {
                    if (c1[i] != c2[i]) return false;
                }
                return true;
            } 
        }
        public static bool operator !=(Commands c1, Commands c2)
        {
            return !(c1==c2);
        }
        public static bool operator * (Commands c1, string command)
        {
            return c1.Have(command);
        }
       
        public string[] ToStringArray()
        {
            List<string> arr = new List<string>();
            foreach (TextCommand a in this)
            {
                arr.Add(a.Text);
            }
            return arr.ToArray();
        }
        public string ToString()
        {
            string ret = "";
            foreach (TextCommand com in this)
            {
                ret += com.Text;
            }
            return ret;
        }
    }
    
    public class TextCommand
    {
        int id;
        string textCommand;

        public int Id { get { return id; } }
        public string Text { get { return textCommand; } }

        public static bool operator ==(TextCommand c1, TextCommand c2)
        {
            return c1.Id == c2.Id;
        }
        public static bool operator !=(TextCommand c1, TextCommand c2)
        {
            return !(c1 == c2);
        }
        public TextCommand(int id, string textCommand)
        {
            this.id = id;
            this.textCommand = textCommand;
        }
    }
    public abstract class WordsCommand
    {
        internal string text = "";
        internal Words wrds;
        internal Words answers;
        internal Function func;

        public FunctionLecal Lecals { get; set; }
        public int FirstSelectedWord(string text = "")
        {
            //Обработка входящего текста, удаления ненужных символов
            if (text == "") text = this.text;
            text = TextHandler.DeleteChars(text);

            Words wInT = Words.Get(text);

            for (int i = 0; i < wInT.Count(); i++)
            {
                
                Word w = wInT[i];
               
                foreach (Word wI in wrds)
                {
                    if (wI.Text.ToLower() == text.ToLower())
                    {
                        return i;
                    }
                    if (w == wI)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        public static string operator &(WordsCommand w1, WordsCommand w2)
        {
            string text = (w1.FirstSelectedWord() != -1 ? w1.func.Command : "") + (w2.FirstSelectedWord() != -1 ? w2.func.Command : "");
            return text;
        }
        public static string operator &(string w1, WordsCommand w2)
        {
          
            string text = w1 + (w2.FirstSelectedWord() != -1 ? w2.func.Command : "");
            return text;
        }
        public string GetRandomAnswer()
        {
            Random r = new Random();
            int i = r.Next(0, answers.Count);
            return answers[i].Text;
        }
    }
    public class TextHandler
    {
        public static string DeleteChars(string text)
        {
            var charsToRemove = new string[] { "@", ",", ".", ";", "'", "/", "\\","?" };
            foreach (var c in charsToRemove)
            {
                text = text.Replace(c, string.Empty);
            }
            return text;
        }

    }

}
