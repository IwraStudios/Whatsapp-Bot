using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebWhatsappAPI
{
    public class BaseClass
    {

        public class ChatSettings
        {
            public bool AllowGET = true; //TODO: implement
            public bool AutoSaveSettings = true;//Save Chatsettings and AutoSaveSettings generally on
            public bool SaveMessages = false; //TODO: implement
            public AutoSaveSettings SaveSettings = new AutoSaveSettings();
        }

        public class AutoSaveSettings
        {
            public uint Interval = 3600;//every hour
            public ulong BackupInterval = 3600 * 24 * 7;//every week
            public bool Backups = false;//Save backups which can be manually restored //TODO: implement
            public bool SaveProfiles = true;//Save Profiles with Save
            public bool SaveCookies = true;//Save Cookies with Save


            public List<ChatProfile> SavedProfiles = new List<ChatProfile>(); //For later usage
            public IReadOnlyCollection<OpenQA.Selenium.Cookie> SavedCookies = null; //For later usage
        }

        //TODO: change
        public class ChatProfile
        {
            public ChatProfile(string name)
            {
                string personName = name;
            }
            public string personName;
        }

        public class MsgArgs : EventArgs
        {
            string _Msg;
            string _Sender;
            DateTime _TimeStamp;
            public MsgArgs(string Message, string sender)
            {
                _TimeStamp = DateTime.Now;
                this._Msg = Message;
                this._Sender = sender;
            }
            public string Msg { get { return _Msg; } }
            public string Sender { get { return _Sender; } }
            public DateTime TimeStamp { get { return _TimeStamp; } }
        }

        public delegate void MsgRecievedEventHandler(MsgArgs e);
        public event MsgRecievedEventHandler OnMsgRecieved;

        protected void Raise_RecievedMessage(string Msg, string Sender)
        {
            OnMsgRecieved?.Invoke(new MsgArgs(Msg, Sender));
        }

        public ChatSettings settings = new ChatSettings();
        public List<ChatProfile> PDB = new List<ChatProfile>();

        protected IWebDriver driver = null;
        //static ChatSettings settings;


        public async Task MessageScanner()
        {
            while (true)//Make Cancelable(tokens)
            {

                IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.ClassName("unread-count"));
                if (unread.Count < 1)
                    continue;
                try
                {
                    unread.ElementAt(0).Click(); //Goto (first) Unread chat
                }
                catch (Exception e) { } //DEAL with Stale elements
                await Task.Delay(200);//Let it load

                string Pname = "";
                string message_text = GetLastestText(out Pname);
                Raise_RecievedMessage(message_text, Pname);

            }
        }

        public virtual void StartDriver(IWebDriver driver, string savefile)
        {
            StartDriver(driver);
            if (File.Exists(@savefile))
            {
                Console.WriteLine("Trying to restore settings");
                settings = Extensions.ReadFromBinaryFile<ChatSettings>("Save.bin");
                if (settings.SaveSettings.SaveCookies)
                {
                    settings.SaveSettings.SavedCookies.LoadCookies(driver);
                }
            }
            else
            {
                settings = new ChatSettings();
            }
        }

        public virtual void StartDriver()
        {
        }

        public virtual void StartDriver(IWebDriver driver)
        {
            this.driver = driver;
            this.driver.Navigate().GoToUrl("https://web.whatsapp.com");
            
            //this.driver.FindElement(By.ClassName("first")).Click();//Go to the first chat
        }

        protected virtual void AutoSave()
        {
            if (!settings.AutoSaveSettings)
                return;
            if (settings.SaveSettings.SaveCookies)
            {
                settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            if (settings.SaveSettings.SaveProfiles)
            {
                settings.SaveSettings.SavedProfiles = PDB;
            }
            settings.WriteToBinaryFile("Save.bin");
            if (settings.SaveSettings.Backups)
            {
                //TODO: implement
            }
        }

        public ChatProfile LookupPerson(string person)
        {
            foreach (ChatProfile p in PDB)
            {
                if (p.personName == person)
                {
                    return p;
                }
            }
            return null;
        }

        public string GetLastestText(out string Pname)
        {
            IWebElement chat = driver.FindElement(By.ClassName("active"));
            IWebElement nametag = chat.FindElement(By.ClassName("ellipsify"));
            Pname = nametag.GetAttribute("title");
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElement(By.ClassName("message-list")).FindElements(By.XPath("*"));
            }
            catch (Exception e) { } //DEAL with Stale elements
            IWebElement newmessage = messages.OrderBy((x) => x.Location.Y).Reverse().First(); //Get latest message
            IWebElement message_text_raw = newmessage.FindElement(By.ClassName("selectable-text"));
            return System.Text.RegularExpressions.Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
        }

        public void SendMessage(string message, string person = null)
        {
            if(person != null)
            {
                IReadOnlyCollection<IWebElement> AllChats = driver.FindElements(By.ClassName("chat-title"));
                foreach(IWebElement we in AllChats)
                {
                    IWebElement Title = we.FindElement(By.ClassName("emojitext"));
                    if(Title.GetAttribute("title") == person)
                    {
                        Title.Click();
                        System.Threading.Thread.Sleep(300);
                        break;
                    }
                    Console.WriteLine("Can't find person, not sending");
                    return;
                }
            }
            string outp = message.ToWhatsappText();
            IWebElement chatbox = driver.FindElement(By.ClassName("block-compose"));
            chatbox.Click();
            chatbox.SendKeys(outp);
            chatbox.SendKeys(Keys.Enter);
        }

        public virtual string HelpText
        {
            get
            {
                return ("/help: show this text \n" +
                        "* show anything else");
            }
        }

    }
}
