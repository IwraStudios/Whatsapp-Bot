using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

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
            FirefoxProfile foxProfile = new FirefoxProfile();
            foxProfile.AcceptUntrustedCertificates = false;
            foxProfile.AlwaysLoadNoFocusLibrary = true;

            var driver_tmp = new FirefoxDriver(foxProfile);

            base.StartDriver(driver_tmp);

        }


    }
}
