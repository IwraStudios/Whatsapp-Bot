using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebWhatsappAPI;

namespace ModularExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Program x = new Program();
            x.MainS(null);
        }
        IWebWhatsappDriver _driver;
        void MainS(string[] args)
        {
            Console.WriteLine("1. FirefoxDriver");
            Console.WriteLine("2. ChromeDriver");
            string x = Console.ReadLine();
            switch (x)
            {
                case "FirefoxDriver":
                case "1":
                    Start(new WebWhatsappAPI.Firefox.FirefoxWApp());
                    break;
                case "ChromeDriver":
                case "2":
                    Start(new WebWhatsappAPI.Chrome.ChromeWApp());
                    break;
                default:
                    Main(null);
                    break;

            }
            Console.WriteLine("Done");
            Console.ReadKey();
        }

        void Start(IWebWhatsappDriver driver)
        {
            _driver = driver;
            driver.StartDriver();
            //Wait till we are on the login page
            while (!driver.OnLoginPage())
            {
                Console.WriteLine("Not on login page");
                Thread.Sleep(1000);
            }

            Thread.Sleep(500);

            while (driver.OnLoginPage())
            {
                Console.WriteLine("Please login");
                Thread.Sleep(5000);
            }
            Console.WriteLine("You have logged in");

            //IMPORTANT: Setup for the auto-replier(this.OnMsgRec)
            driver.OnMsgRecieved += OnMsgRec;
            Task.Run(() => driver.MessageScanner(new[] { "Casper","Ryan"}, false)); //Whitelist

            ////
            //// if we only want to recieve messages from Ryan
            ////Task.Run(() => _driver.MessageScanner(new[] { "Ryan" }, false));
            ////
            //IMPORTANT

            Console.WriteLine("Use CTRL+C to exit");
            while (true)
            {
                //Check if phone is connected, because why not
                if (!driver.IsPhoneConnected())
                {
                    Console.WriteLine("Phone is not connected");
                }
                Thread.Sleep(10000); //wait 10 sec. so the console doesn't fill up
            }
        }

        private void OnMsgRec(IWebWhatsappDriver.MsgArgs arg)
        {
            Console.WriteLine(arg.Sender + " Wrote: " + arg.Msg + " at " + arg.TimeStamp);
            if(arg.Msg.StartsWith("/"))
            {
                try
                {
                    var ser = new JavaScriptSerializer();
                    using (var wc = new WebClient())
                    {

                        wc.Headers.Add(HttpRequestHeader.Accept, "application/json");
                        //Get The FOAAS
                        var json = wc.DownloadString("http://foaas.com" + arg.Msg);
                        dynamic usr = ser.DeserializeObject(json);

                        if (arg.Msg.Contains("operations"))
                        {
                            var x = "";
                            for (int i = 0; i < 20; i++)
                            {
                                x += usr[i]["name"] + ' ' + usr[i]["url"] + '\n';
                            }
                            _driver.SendMessage(x, arg.Sender);
                            return;
                        }


                        //Send message to the origional Sender
                        _driver.SendMessage(usr["message"] + "\n -" + usr["subtitle"], arg.Sender);
                    }
                    return;
                }
                catch (Exception)
                {
                    _driver.SendMessage("Welcome to FOAAS Whatsapp", arg.Sender);
                    return;
                }
            }
            _driver.SendMessage("Welcome to FOAAS Whatsapp", arg.Sender);
        }
    }
}
