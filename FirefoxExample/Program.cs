using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
            driver.OnMsgRecieved += OnMsgRec;
            Console.WriteLine("login now, then press any key to continue");
            Console.ReadKey();
            Task.Run(driver.MessageScanner);
            Console.ReadLine();
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
