using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using WebWhatsappAPI.Firefox;


namespace FirefoxExample
{
    class Program
    {
        static FirefoxWApp driver;
        static void Main(string[] args)
        {
            driver = new FirefoxWApp();
            driver.StartDriver();

            while(!driver.OnLoginPage())
            {
                Thread.Sleep(1000);
                Console.WriteLine("Not on login page");
            }
            driver.GetQRImage().Save("QR.jpg", System.Drawing.Imaging.ImageFormat.Jpeg); //Get QR code and save to file
            int counter = 0;
            while (driver.OnLoginPage())
            {
                Thread.Sleep(2000);
                counter++;
                if(counter > 15)
                {
                    driver.GetQRImage().Save("QR.jpg", System.Drawing.Imaging.ImageFormat.Jpeg); //QR probably updated so re-save
                    counter = 0;
                }
                Console.WriteLine("Please login");
            }

            Console.WriteLine("You have logged in");
            
            driver.OnMsgRecieved += OnMsgRec;
            Task.Run(driver.MessageScanner);
            Console.WriteLine("Use CTRL+C to exit");
            while (true)
            {
                if (!driver.IsPhoneConnected())
                {
                    Console.WriteLine("Phone is not connected");
                }
                Thread.Sleep(10000);//wait 10 sec. so the console doesn't fill up
            }
        }

        static void OnMsgRec(WebWhatsappAPI.BaseClass.MsgArgs arg)
        {
            Console.WriteLine(arg.Sender + " Wrote: " + arg.Msg + " at " + arg.TimeStamp.ToString());

            JavaScriptSerializer ser = new JavaScriptSerializer();
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("https://random-quote-generator.herokuapp.com/api/quotes/random");
                dynamic usr = ser.DeserializeObject(json);
                driver.SendMessage(usr["quote"] + "\n -" + usr["author"], arg.Sender);
                return;
            }

        }
    }
}
