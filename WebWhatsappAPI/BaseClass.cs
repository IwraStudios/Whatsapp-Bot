using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/*! \mainpage Whatsapp api
 *
 * \section intro_sec Introduction
 *
 * This is an api made for automating Whatsapp behaviour
 * 
 * possible uses for this API:
 * * Service of sorts
 * * Personal usage
 * * Others...
 *
 * \section terms_sec Terms of Use
 * 
 *  * You will NOT use this API for marketing purposes (spam, massive sending...).
 *  * We do NOT give support to anyone that wants this API to send massive messages or similar.
 *  * We reserve the right to block any user of this repository that does not meet these conditions.
 *  * We are not associated with Whatsapp(tm) or Facebook(tm)
 * 
 * \section install_sec Installation
 *
 * \subsection step1 Step 1: Cloning the repository
 *  This can be done by simply going to %https://github.com/IwraStudios/Whatsapp-Bot and 
 *  clicking clone or download and downloading the zip file
 *  
 *  On linux machines it can be easier to do (although you won't have VS so how does that work?) \n
 *  git clone git@github.com:IwraStudios/Whatsapp-Bot.git \n
 *  or \n
 *  git clone %github.com/IwraStudios/Whatsapp-Bot.git \n
 *  
 *  \subsection step2 Step 2: Intergrating it into your own project
 *  ### Method 1: 
 *  Assuming you already have an VS C# project just right click on: Solution "your_project", in Solution Explorer  \n
 *  going to "Add >" and choosing existing project, then navigate to Whatsapp-Bot-master\\WebWhatsappAPI\\WebWhatsappAPI.csproj \n
 *  and clicking "Open" \n
 *  
 *  Now you can add it as a refrence by right-clicking Refrences in your project, going to Projects and checking the checkmark \n
 *  Aaaand your done. \n
 *  
 *  ### Method 2:
 *  As an alternative method you can open the project WhatsappBot.sln or WebWhatsappAPI.csproj and building it \n
 *  Then you can copy the .dll's and .exe's from %WebWhatsappAPI\\*\\bin\\*.dll and WebWhatsappAPI\*\\bin\\*.exe \n
 *  
 *  after you have done all that you can add the dll's as a refrence by going to refrences (right-click), clicking browse and going to the dll's \n
 *  when building your project you will have to either: \n
 *  copy the exe's to the %*\\bin\\ directory of your project \n
 *  or \n
 *  Adding a build step that does this for you \n
 *  
 *  \subsection step3 Step 3: Using it
 *  I could write an whole article about how to use the api but that is why these docs exist \n
 *  If you don't know how to do something look at the examples or the api docs \n
 *  
 *  If you, after you did that, still don't know how to do something go to %https://github.com/IwraStudios/Whatsapp-Bot/issues and create a new issue \n
 */



namespace WebWhatsappAPI
{
    public partial class BaseClass
    {

        /// <summary>
        /// Current settings
        /// </summary>
        public ChatSettings settings = new ChatSettings();
        public bool HasStarted { get; protected set; }
        protected IWebDriver driver = null;
        /// <summary>
        /// A refrence to the Selenium WebDriver used; Selenium.WebDriver required
        /// </summary>
        public IWebDriver WebDriver
        {
            get
            {
                if (driver != null)
                {
                    return driver;
                }
                else {
                    throw new NullReferenceException("Can't use WebDriver before StartDriver()");
                }
            }
        }

        protected EventFiringWebDriver eventDriver = null;

        /// <summary>
        /// An event WebDriver from selenium; Selenium.Support package required
        /// </summary>
        public EventFiringWebDriver EventDriver { get { if (eventDriver != null)
                {
                    return eventDriver;
                }
                else { throw new NullReferenceException("Can't use Event Driver before StartDriver()"); }
             } }

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
        /// Returns if the Login page and QR has loaded
        /// </summary>
        /// <returns></returns>
        public bool OnLoginPage()
        {
            try
            {
                if (driver.FindElement(By.XPath("//div[@id='window']/div/div/div/img")) != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Check's if we get the notification "PhoneNotConnected"
        /// </summary>
        /// <returns>bool; true if connected</returns>
        public bool IsPhoneConnected()
        {
            try
            {
                if (driver.FindElement(By.ClassName("icon-alert")) != null)
                {
                    return false;
                }
            }
            catch
            {
                return true;
            }
            return true;
        }


        /// <summary>
        /// Gets raw QR string 
        /// </summary>
        /// <returns>sting(base64) of the image; returns null if not available</returns>
        public string GetQRImageRAW()
        {
            try
            {
                IWebElement qrcode = driver.FindElement(By.XPath("//div[@id='window']/div/div/div/img"));
                string outp = qrcode.GetAttribute("src");
                outp = outp.Substring(22); //DELETE HEADER
                return outp;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an C# image of the QR on the homepage
        /// </summary>
        /// <returns>QR image; returns null if not available</returns>
        public Image GetQRImage()
        {
            try
            {
                string base64image = GetQRImageRAW();
                return Base64ToImage(base64image);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// https://stackoverflow.com/a/18827264
        /// </summary>
        /// <param name="base64String">Base 64 string</param>
        /// <returns>an image</returns>
        internal protected Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

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
                catch (Exception) { } //DEAL with Stale elements
                await Task.Delay(200);//Let it load
                string Pname = "";
                string message_text = GetLastestText(out Pname);
                Raise_RecievedMessage(message_text, Pname);

            }
        }

        /// <summary>
        /// Starts selenium driver, while loading a save file
        /// Note: these functions don't make drivers
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
        /// Starts selenium driver(only really used internally or virtually)
        /// Note: these functions don't make drivers
        /// </summary>
        public virtual void StartDriver()
        {
            //can't start a driver twice
            HasStartedCheck();
            HasStarted = true;
        }

        /// <summary>
        /// Starts selenium driver
        /// Note: these functions don't make drivers
        /// </summary>
        /// <param name="driver">The selenium driver</param>
        public virtual void StartDriver(IWebDriver driver)
        {
            StartDriver();
            this.driver = driver;
            this.driver.Navigate().GoToUrl("https://web.whatsapp.com");
            eventDriver = new EventFiringWebDriver(driver);
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

        /// <summary>
        /// Loads a file containing Settings and cookies
        /// </summary>
        /// <param name="FileName">path to Filename</param>
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
            catch (Exception) { } //DEAL with Stale elements
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

        /// <summary>
        /// only for internal use; throws exception if the driver has already started(can be inverted)
        /// </summary>
        protected void HasStartedCheck(bool Invert = false)
        {
            if (HasStarted ^ Invert)
            {
                throw new NotSupportedException(String.Format("Driver has {0} already started", Invert ? "not":""));
            }
        }

    }
}
