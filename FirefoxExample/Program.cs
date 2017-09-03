using System;
using System.Drawing.Imaging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebWhatsappAPI;
using WebWhatsappAPI.Firefox;

namespace FirefoxExample
{
    internal class Program
    {
        private static FirefoxWApp _driver;

        private static void Main(string[] args)
        {
            _driver = new FirefoxWApp();
            _driver.StartDriver();

            //Wait till we are on the login page
            while (!_driver.OnLoginPage())
            {
                Console.WriteLine("Not on login page");
                Thread.Sleep(1000);
            }

            Thread.Sleep(500);
            _driver.GetQrImage().Save("QR.jpg", ImageFormat.Jpeg);

            while (_driver.OnLoginPage())
            {
                Console.WriteLine("Please login");
                Thread.Sleep(5000);
            }
            Console.WriteLine("You have logged in");

            //IMPORTANT: Setup for the auto-replier(this.OnMsgRec)
            _driver.OnMsgRecieved += OnMsgRec;
            Task.Run(() => _driver.MessageScanner(new[] { "Ryan"}, true)); //No messages from Ryan (blacklist)

            ////
            //// if we only want to recieve messages from Ryan
            ////Task.Run(() => _driver.MessageScanner(new[] { "Ryan" }, false));
            ////
            //IMPORTANT

            Console.WriteLine("Use CTRL+C to exit");
            while (true)
            {
                //Check if phone is connected, because why not
                if (!_driver.IsPhoneConnected())
                {
                    Console.WriteLine("Phone is not connected");
                }
                Thread.Sleep(10000); //wait 10 sec. so the console doesn't fill up
            }
        }

        //Function which will recieve all messages
        private static void OnMsgRec(IWebWhatsappDriver.MsgArgs arg)
        {
            //show message with timestamp in console
            Console.WriteLine(arg.Sender + " Wrote: " + arg.Msg + " at " + arg.TimeStamp);
            var ser = new JavaScriptSerializer();
            using (var wc = new WebClient())
            {
                //Get random qoute from someone
                var json = wc.DownloadString("https://random-quote-generator.herokuapp.com/api/quotes/random");
                dynamic usr = ser.DeserializeObject(json);

                //Send message to the origional Sender
                _driver.SendMessage(usr["quote"] + "\n -" + usr["author"], arg.Sender);
            }
        }
    }
}