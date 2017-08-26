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

            //Wait till we are on the login page
            while(!driver.OnLoginPage())
            {
                Thread.Sleep(1000);
                Console.WriteLine("Not on login page");
            }
            
            //We are on the login page so wait for login(also save QR to file)
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
            
            //IMPORTANT: Setup for the auto-replier(this.OnMsgRec)
            driver.OnMsgRecieved += OnMsgRec;
            Task.Run(driver.MessageScanner);
            //IMPORTANT
            
            Console.WriteLine("Use CTRL+C to exit");
            while (true)
            {
                //Check if phone is connected, because why not
                if (!driver.IsPhoneConnected())
                {
                    Console.WriteLine("Phone is not connected");
                }
                Thread.Sleep(10000);//wait 10 sec. so the console doesn't fill up
            }
        }

        //Function which will recieve all messages
        static void OnMsgRec(WebWhatsappAPI.BaseClass.MsgArgs arg)
        {            
            //show message with timestamp in console
            Console.WriteLine(arg.Sender + " Wrote: " + arg.Msg + " at " + arg.TimeStamp.ToString());

            JavaScriptSerializer ser = new JavaScriptSerializer();
            using (WebClient wc = new WebClient())
            {
                //Get random qoute from someone
                var json = wc.DownloadString("https://random-quote-generator.herokuapp.com/api/quotes/random");
                dynamic usr = ser.DeserializeObject(json);
                
                //Send message to the origional Sender
                driver.SendMessage(usr["quote"] + "\n -" + usr["author"], arg.Sender);
                return;
            }

        }
    }
}
