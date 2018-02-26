using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using Polly;

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
    public abstract class IWebWhatsappDriver
    {
        /// <summary>
        /// Current settings
        /// </summary>
        public ChatSettings Settings = new ChatSettings();

        public bool HasStarted { get; protected set; }
        protected IWebDriver driver;

        private const string UNREAD_MESSAGES_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[3]/div[1]/div[1]/*/div/div/div[@class=\"_2EXPL CxUIE\"]";
        private const string TITLE_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[3]/div[1]/div[1]/*/div/div/div[@class=\"_2EXPL CxUIE\"]/div/div/div[@class=\"_25Ooe\"]";
        private const string UNREAD_MESSAGE_COUNT_XPATH = "div/div/div/span/div/span[@class=\"OUeyt\"]";
        private const string QR_CODE_XPATH = "//img[@alt='Scan me!']";
        private const string MAIN_APP_CLASS = "app";
        private const string ALERT_PHONE_NOT_CONNECTED_CLASS = "icon-alert-phone";
        private const string NAME_TAG_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[3]/div[1]/header[1]/div[2]/div[1]/div[1]/span[1]";
        private const string INCOME_MESSAGES_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[3]/div[1]/div[2]/div[1]/div[1]/div[3]/div/div[contains(@class, 'message-in')]";
        private const string SELECTABLE_MESSAGE_TEXT_CLASS = "selectable-text";
        private const string READ_MESSAGES_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[3]/div[1]/div[1]/*/div/div/div[@class=\"_2EXPL\"]";
        private const string CHAT_INPUT_TEXT_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[3]/div[1]/footer[1]/div[1]/div[2]/div[1]/div[2]";
        private const string ALL_CHATS_TITLE_XPATH = "/html[1]/body[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[3]/div[1]/div[1]/*/div/div/div/div/div/div[@class=\"_25Ooe\"]";

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
                throw new NullReferenceException("Can't use WebDriver before StartDriver()");
            }
        }

        private EventFiringWebDriver _eventDriver;

        /// <summary>
        /// An event WebDriver from selenium; Selenium.Support package required
        /// </summary>
        public EventFiringWebDriver EventDriver
        {
            get
            {
                if (_eventDriver != null)
                {
                    return _eventDriver;
                }
                throw new NullReferenceException("Can't use Event Driver before StartDriver()");
            }
        }

        /// <summary>
        /// The settings of the an driver
        /// </summary>
        public class ChatSettings
        {
            public bool AllowGET = true; //TODO: implement(what?)
            public bool AutoSaveSettings = true; //Save Chatsettings and AutoSaveSettings generally on
            public bool SaveMessages = false; //TODO: implement
            public AutoSaveSettings SaveSettings = new AutoSaveSettings();
        }

        /// <summary>
        /// The save settings of the an driver
        /// </summary>
        public class AutoSaveSettings
        {
            public uint Interval = 3600; //every hour
            public ulong BackupInterval = 3600 * 24 * 7; //every week
            public bool Backups = false; //Save backups which can be manually restored //TODO: implement
            public bool SaveCookies = true; //Save Cookies with Save

            public IReadOnlyCollection<Cookie> SavedCookies; //For later usage
        }

        /// <summary>
        /// Arguments used by Msg event
        /// </summary>
        public class MsgArgs : EventArgs
        {
            public MsgArgs(string message, string sender)
            {
                TimeStamp = DateTime.Now;
                Msg = message;
                Sender = sender;
            }

            public string Msg { get; }

            public string Sender { get; }

            public DateTime TimeStamp { get; }
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
                if (driver.FindElement(By.XPath(QR_CODE_XPATH)) != null)
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
                if (driver.FindElement(By.ClassName(ALERT_PHONE_NOT_CONNECTED_CLASS)) != null)
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
        private string GetQRImageRAW()
        {
            try
            {
                var qrcode = driver.FindElement(By.XPath("//img[@alt='Scan me!']"));
                var outp = qrcode.GetAttribute("src");
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
        public Image GetQrImage()
        {
            var pol = Policy<Image>
                .Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                });

            return pol.Execute(() =>
            {
                var base64Image = GetQRImageRAW();

                if (base64Image == null)
                    throw new Exception("Image not found");

                return Base64ToImage(base64Image);
            });
        }

        /// <summary>
        /// https://stackoverflow.com/a/18827264
        /// </summary>
        /// <param name="base64String">Base 64 string</param>
        /// <returns>an image</returns>
        private Image Base64ToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            var imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                var image = Image.FromStream(ms, true);
                return image;
            }
        }

        /// <summary>
        /// Scans for messages but only retreaves if person is in PeopleList
        /// </summary>
        /// <param name="PeopleList">List of People to filter on(case-sensitive)</param>
        /// <param name="isBlackList"> is it a black- or whitelist (default whitelist)</param>
        /// <returns>Nothing</returns>
        public async void MessageScanner(string[] PeopleList, bool isBlackList = false)
        {
            while (true)
            {
                IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.XPath(UNREAD_MESSAGES_XPATH));
                foreach (IWebElement x in unread.ToArray())//just in case
                {
                    var y = x.FindElement(By.XPath(TITLE_XPATH));
                    if (PeopleList.Contains(y.GetAttribute("title")) != isBlackList)
                    {
                        x.Click();
                        await Task.Delay(200); //Let it load
                        var Pname = "";
                        var message_text = GetLastestText(out Pname);
                        Raise_RecievedMessage(message_text, Pname);
                    }
                }
                await Task.Delay(50); //don't allow too much overhead
            }
        }

        /// <summary>
        /// Checks for messages which enables OnMsgRecieved event
        /// </summary>
        /// <returns>Nothing</returns>
        public async void MessageScanner()
        {
            while (true)
            {
                IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.ClassName("unread-count"));
                if (unread.Count < 1)
                {
                    Thread.Sleep(50); //we don't wan't too much overhead
                    continue;
                }
                try
                {
                    unread.ElementAt(0).Click(); //Goto (first) Unread chat
                }
                catch (Exception)
                {
                } //DEAL with Stale elements
                await Task.Delay(200); //Let it load
                var Pname = "";
                var message_text = GetLastestText(out Pname);
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
            if (File.Exists(savefile))
            {
                Console.WriteLine("Trying to restore settings");
                Settings = Extensions.ReadFromBinaryFile<ChatSettings>("Save.bin");
                if (Settings.SaveSettings.SaveCookies)
                {
                    Settings.SaveSettings.SavedCookies.LoadCookies(driver);
                }
            }
            else
            {
                Settings = new ChatSettings();
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
            this.driver = driver;
            driver.Navigate().GoToUrl("https://web.whatsapp.com");
            _eventDriver = new EventFiringWebDriver(WebDriver);
        }



        /// <summary>
        /// Saves to file
        /// </summary>
        protected virtual void AutoSave()
        {
            if (!Settings.AutoSaveSettings)
                return;
            if (Settings.SaveSettings.SaveCookies)
            {
                Settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            Settings.WriteToBinaryFile("Save.bin");
            if (!Settings.SaveSettings.Backups) return;
            Directory.CreateDirectory("./Backups");
            Settings.WriteToBinaryFile($"./Backups/Settings_{DateTime.UtcNow:yyyy-MM-dd-HH-mm-ss}.bin");
        }

        /// <summary>
        /// Saves settings and more to file
        /// </summary>
        /// <param name="FileName">Path/Filename to make the file (e.g. save1.bin)</param>
        public virtual void Save(string FileName)
        {
            if (!Settings.AutoSaveSettings)
                return;
            if (Settings.SaveSettings.SaveCookies)
            {
                Settings.SaveSettings.SavedCookies = driver.Manage().Cookies.AllCookies;
            }
            Settings.WriteToBinaryFile(FileName);
            if (Settings.SaveSettings.Backups)
            {
                Directory.CreateDirectory("./Backups");
                Settings.WriteToBinaryFile(String.Format("./Backups/Settings_{0}.bin",
                    DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")));
            }
        }

        /// <summary>
        /// Loads a file containing Settings and cookies
        /// </summary>
        /// <param name="FileName">path to Filename</param>
        public virtual void Load(string FileName)
        {
            Settings = Extensions.ReadFromBinaryFile<ChatSettings>(FileName);
            Settings.SaveSettings.SavedCookies.LoadCookies(driver);
        }

        /// <summary>
        /// Gets the latest test
        /// </summary>
        /// <param name="Pname">[Optional output] the person that send the message</param>
        /// <returns></returns>
        public string GetLastestText(out string Pname) //TODO: return IList<string> of all unread messages
        {
            var nametag = driver.FindElement(By.XPath(NAME_TAG_XPATH));
            Pname = nametag.GetAttribute("title");
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElements(By.XPath(INCOME_MESSAGES_XPATH));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            var newmessage = messages.OrderBy(x => x.Location.Y).Reverse().First(); //Get latest message
            var message_text_raw = newmessage.FindElement(By.ClassName(SELECTABLE_MESSAGE_TEXT_CLASS));
            return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
        }

        /// <summary>
        /// Gets Messages from Active/person's conversaton
        /// <param>Order not garanteed</param>
        /// </summary>
        /// <param name="Pname">[Optional input] the person to get messages from</param>
        /// <returns>Unordered List of messages</returns>
        public IEnumerable<string> GetMessages(string Pname = null)
        {
            if (Pname != null)
            {
                SetActivePerson(Pname);
            }
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElement(By.ClassName("message-list")).FindElements(By.XPath("*"));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            foreach (var x in messages)
            {
                var message_text_raw = x.FindElement(By.ClassName("selectable-text"));
                yield return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
            }
        }

        /// <summary>
        /// Gets messages ordered "newest first"
        /// </summary>
        /// <param name="Pname">[Optional input] person to get messages from</param>
        /// <returns>Ordered List of string's</returns>
        public List<string> GetMessagesOrdered(string Pname = null)
        {
            if (Pname != null)
            {
                SetActivePerson(Pname);
            }
            IReadOnlyCollection<IWebElement> messages = null;
            try
            {
                messages = driver.FindElement(By.ClassName("message-list")).FindElements(By.XPath("*"));
            }
            catch (Exception)
            {
            } //DEAL with Stale elements
            var outp = new List<string>();
            foreach (var x in messages.OrderBy(x => x.Location.Y).Reverse())
            {
                var message_text_raw = x.FindElement(By.ClassName("selectable-text"));
                outp.Add(Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", ""));
            }
            return outp;
        }

        /// <summary>
        /// Send message to person
        /// </summary>
        /// <param name="message">string to send</param>
        /// <param name="person">person to send to (if null send to active)</param>
        public void SendMessage(string message, string person = null)
        {
            if (person != null)
            {
                SetActivePerson(person);
            }
            var outp = message.ToWhatsappText();
            var chatbox = driver.FindElement(By.XPath(CHAT_INPUT_TEXT_XPATH));
            chatbox.Click();
            chatbox.SendKeys(outp);
            chatbox.SendKeys(Keys.Enter);
        }

        /// <summary>
        /// Set's Active person/chat by name
        /// <para>useful for default chat type of situations</para>
        /// </summary>
        /// <param name="person">the person to set active</param>
        public void SetActivePerson(string person)
        {
            IReadOnlyCollection<IWebElement> AllChats = driver.FindElements(By.XPath(ALL_CHATS_TITLE_XPATH));
            foreach (var title in AllChats)
            {
                if (title.GetAttribute("title") == person)
                {
                    title.Click();
                    Thread.Sleep(300);
                    return;
                }
            }
            Console.WriteLine("Can't find person, not sending");
        }

        /// <summary>
        /// Get's all chat names so you can make a selection menu
        /// </summary>
        /// <returns>Unorderd string 'Enumerable'</returns>
        public IEnumerable<string> GetAllChatNames()
        {
            HasStartedCheck();
            IReadOnlyCollection<IWebElement> AllChats = driver.FindElement(By.ClassName("chatlist")).FindElements(By.ClassName("chat-title"));
            foreach (var we in AllChats)
            {
                var Title = we.FindElement(By.ClassName("emojitext"));
                yield return Title.GetAttribute("title");
            }
        }

        /// <summary>
        /// only for internal use; throws exception if the driver has already started(can be inverted)
        /// </summary>
        protected void HasStartedCheck(bool Invert = false)
        {
            if (HasStarted ^ Invert)
            {
                throw new NotSupportedException(String.Format("Driver has {0} already started", Invert ? "not" : ""));
            }
        }
    }
}
