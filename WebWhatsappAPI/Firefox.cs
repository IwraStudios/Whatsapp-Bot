using System;
using OpenQA.Selenium.Firefox;

namespace WebWhatsappAPI.Firefox
{
    public class FirefoxWApp : BaseClass
    {
        /// <summary>
        /// Initialize Firefox Driver
        /// </summary>
        public FirefoxWApp()
        {
        }

        /// <summary>
        /// Start the selenium engine and finalize the initialisation
        /// </summary>
        public override void StartDriver()
        {
            HasStartedCheck();
            var firefoxOptions = new FirefoxOptions {Profile = new FirefoxProfile(AppDomain.CurrentDomain.BaseDirectory + @"\whatsappProfile", false) };

            base.StartDriver(new FirefoxDriver(firefoxOptions));
        }
    }
}