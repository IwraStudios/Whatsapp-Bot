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
        /// <summary>
        /// The settings of the an driver
        /// </summary>
        public class ChatSettings
        {
            public bool AllowGET = true; //TODO: implement(what?)
            public bool AutoSaveSettings = true;//Save Chatsettings and AutoSaveSettings generally on
            public bool SaveMessages = false; //TODO: implement
            public AutoSaveSettings SaveSettings = new AutoSaveSettings();
        }

        /// <summary>
        /// The save settings of the an driver
        /// </summary>
        public class AutoSaveSettings
        {
            public uint Interval = 3600;//every hour
            public ulong BackupInterval = 3600 * 24 * 7;//every week
            public bool Backups = false;//Save backups which can be manually restored //TODO: implement
            public bool SaveCookies = true;//Save Cookies with Save

            public IReadOnlyCollection<OpenQA.Selenium.Cookie> SavedCookies = null; //For later usage
        }

        /// <summary>
        /// Arguments used by Msg event
        /// </summary>
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

        /// <summary>
        /// Current settings
        /// </summary>
        public ChatSettings settings = new ChatSettings();

        protected IWebDriver driver = null;
        //static ChatSettings settings;

        /// <summary>
        /// Checks for messages which enables OnMsgRecieved event
        /// </summary>
        /// <returns>Nothing</returns>
        public async Task MessageScanner()
        {
            while (true)//TODO: Make Cancelable(tokens)
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

        /// <summary>
        /// Starts selenium driver, while loading a save file
        /// </summary>
        /// <param name="driver">The driver</param>
        /// <param name="savefile">Path to savefile</param>
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
        /// <summary>
        /// Starts selenium driver
        /// </summary>
        public virtual void StartDriver()
        {
        }

        /// <summary>
        /// Starts selenium driver
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        public virtual void StartDriver(IWebDriver driver)
        {
            this.driver = driver;
            this.driver.Navigate().GoToUrl("https://web.whatsapp.com");
            
            //this.driver.FindElement(By.ClassName("first")).Click();//Go to the first chat
        }

        /// <summary>
        /// Saves to file
        /// </summary>
        protected virtual void AutoSave()
        {
            if (!settings.AutoSaveSettings)
                return;
            if (settings.SaveSettings.SaveCookies)
            {
                settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            settings.WriteToBinaryFile("Save.bin");
            if (settings.SaveSettings.Backups)
            {
                //TODO: implement
            }
        }

        /// <summary>
        /// Saves settings and more to file
        /// </summary>
        /// <param name="FileName">Path/Filename to make the file (e.g. save1.bin)</param>
        public virtual void Save(string FileName)
        {
            if (!settings.AutoSaveSettings)
                return;
            if (settings.SaveSettings.SaveCookies)
            {
                settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            settings.WriteToBinaryFile(FileName);
            if (settings.SaveSettings.Backups)
            {
                //TODO: implement
            }

        }

        public virtual void Load(string FileName)
        {
            settings = Extensions.ReadFromBinaryFile<ChatSettings>(@FileName);
            settings.SaveSettings.SavedCookies.LoadCookies(driver);
        }

        /// <summary>
        /// Gets the latest test
        /// </summary>
        /// <param name="Pname">[Optional output] the person that send the message</param>
        /// <returns></returns>
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

        /// <summary>
        /// Send message to person
        /// </summary>
        /// <param name="message">string to send</param>
        /// <param name="person">person to send to (if null send to active)</param>
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

    }
}
