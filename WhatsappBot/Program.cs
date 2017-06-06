using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System.Threading;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;

namespace WhatsappBot
{
    class Program
    {
        public enum AutoTypes
        {
            None = -1,//Obsolete(for now)
            Help,
            FamousQoute,

        }

        public class ChatSettings {
            public AutoTypes DefaultReply = AutoTypes.FamousQoute;
            public bool AllowGET = true; //TODO: implement
            public bool AutoSaveSettings = true;//Save Chatsettings and AutoSaveSettings generally on
            public bool SaveMessages = false; //TODO: implement
            public AutoSaveSettings SaveSettings = new AutoSaveSettings();
        }

        [Serializable]
        public class AutoSaveSettings{
            public uint Interval = 3600;//every hour
            public ulong BackupInterval = 3600 * 24 * 7;//every week
            public bool Backups = false;//Save backups which can be manually restored //TODO: implement
            public bool SaveProfiles = true;//Save Profiles with Save
            public bool SaveCookies = true;//Save Cookies with Save

            
            public List<ChatProfile> SavedProfiles = new List<ChatProfile>(); //For later usage
            public IReadOnlyCollection<OpenQA.Selenium.Cookie> SavedCookies = null; //For later usage
        }

        public class ChatProfile {
            public ChatProfile(string name, AutoTypes Defaultreply = AutoTypes.FamousQoute)
            {
                string personName = name;
                AutoTypes DefaultReply = Defaultreply;
            }
            public string personName;
            public AutoTypes DefaultReply = AutoTypes.FamousQoute;


        }

        static string HelpText { get
            {
                return ("/help: show this text \n" +
                        "* show anything else");
            }
                               }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        static List<ChatProfile> PDB = new List<ChatProfile>();
        static IWebDriver driver = null;
        static ChatSettings settings;
        static void Main(string[] args)
        {
            _handler += new EventHandler(ExitHandler);
            SetConsoleCtrlHandler(_handler, true);


            FirefoxProfile foxProfile = new FirefoxProfile();
            foxProfile.AcceptUntrustedCertificates = false;
            foxProfile.AlwaysLoadNoFocusLibrary = true;
            

            using (driver = new FirefoxDriver())
            {
                driver.Navigate().GoToUrl("https://web.whatsapp.com");
                Thread.Sleep(1000);
                Console.ReadKey();//TODO: auto-detect
                //TODO: cookies remember
                
               
                driver.FindElement(By.ClassName("first")).Click();//Go to the first chat
                if (File.Exists(@"Save.bin"))
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
                
                while (true)
                {
                    Thread.Sleep(500);
                    IReadOnlyCollection<IWebElement> unread = driver.FindElements(By.ClassName("unread-count"));
                    if (unread.Count < 1)
                        continue;
                    try
                    {
                        unread.ElementAt(0).Click(); //Goto (first) Unread chat
                    }catch(Exception e) { } //DEAL with Stale elements
                    Thread.Sleep(200); //Let it load

                    string Pname = "";
                    string message_text = GetLastestText(out Pname);

                    Console.WriteLine("recieved text from " + Pname + " Saying: " + message_text);
                    AutoTypes reply = ParseMessage(Pname, message_text);
                    AutoMessage(reply, Pname);
                    //TODO: make timestamp (algo?)

                }
            }

        }

        static void AutoSave()
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
        }

        static string GetLastestText(out string Pname)
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
            return Regex.Replace(message_text_raw.Text, "<!--(.*?)-->", "");
        }

        static ChatProfile LookupPerson(string person)
        {
            foreach(ChatProfile p in PDB)
            {
                if(p.personName == person)
                {
                    return p;
                }
            }
            return null;
        }

        static AutoTypes ParseMessage(string from, string message)
        {
            ChatProfile Person = null;
            if(LookupPerson(from) != null)
            {
                Person = LookupPerson(from);
            }
            else
            {
                PDB.Add(new ChatProfile(from));
            }

            if(message == "/help")
            {
                return AutoTypes.Help;
            }

            if(Person != null)
            {
                return Person.DefaultReply;
            }

            return settings.DefaultReply;//Default reply for unknown (very last resort)
        }

        static void AutoMessage(AutoTypes type, string to)
        {
            string text = "";
            
            if(type == AutoTypes.FamousQoute)
            {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadString("https://random-quote-generator.herokuapp.com/api/quotes/random");
                    dynamic usr = ser.DeserializeObject(json);
                    text = usr["quote"] + "\n -" + usr["author"];
                    SendMessage(text);
                    return;
                }
            }

            if(type == AutoTypes.Help)
            {
                text = HelpText;
                SendMessage(text);
            }
           

        }
        
        static void SendMessage(string message)
        {
            string outp = message.ToWhatsappText();
            IWebElement chatbox = driver.FindElement(By.ClassName("block-compose"));
            chatbox.Click();
            chatbox.SendKeys(outp);
            chatbox.SendKeys(Keys.Enter);
        }

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static bool ExitHandler(CtrlType sig)
        {//TODO: implement
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    AutoSave();
                    break;
                default:
                    return false;
            }
            return false;
        }
        
    }

    public static class Extensions
    {
        public static string ToWhatsappText(this string inp) //Makes sure newlines don't submit
        {
            return inp.Replace("\n", (Keys.Shift + Keys.Enter));
        }

        /// <summary>
        /// Writes the given object instance to a binary file.
        /// <para>Object type (and all child types) must be decorated with the [Serializable] attribute.</para>
        /// <para>To prevent a variable from being serialized, decorate it with the [NonSerialized] attribute; cannot be applied to properties.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the XML file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the XML file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToBinaryFile<T>(this T objectToWrite, string filePath, bool append = false)
        {
            using (System.IO.Stream stream = System.IO.File.Open(filePath, append ? System.IO.FileMode.Append : System.IO.FileMode.Create))
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Serialize(stream, objectToWrite);
        }

        /// <summary>
        /// Reads an object instance from a binary file.
        /// </summary>
        /// <typeparam name="T">The type of object to read from the XML.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the binary file.</returns>
        public static T ReadFromBinaryFile<T>(string filePath)
        {
            using (System.IO.Stream stream = System.IO.File.Open(filePath, System.IO.FileMode.Open))
                return (T)(new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter().Deserialize(stream));
        }

        public static void LoadCookies(this IReadOnlyCollection<OpenQA.Selenium.Cookie> Cookies, IWebDriver driver)
        {
            foreach (OpenQA.Selenium.Cookie cookie in Cookies)
            {
                driver.Manage().Cookies.AddCookie(cookie);
            }
        }

    }
}
