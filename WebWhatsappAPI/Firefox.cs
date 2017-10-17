using System;
using OpenQA.Selenium.Firefox;

namespace WebWhatsappAPI.Firefox
{
    public class FirefoxWApp : IWebWhatsappDriver
    {
        FirefoxOptions FirefoxOP;
        /// <summary>
        /// Initialize Firefox Driver
        /// </summary>
        public FirefoxWApp()
        {
            FirefoxOP = new FirefoxOptions
            {
                Profile = new FirefoxProfile(AppDomain.CurrentDomain.BaseDirectory + @"\whatsappProfile", false)
                {
                    AcceptUntrustedCertificates = false,
                    AlwaysLoadNoFocusLibrary = true,
                    DeleteAfterUse = false
                }
            };
        }

        /// <summary>
        /// Start the selenium engine and finalize the initialisation
        /// </summary>
        public override void StartDriver()
        {
            HasStartedCheck();
            base.StartDriver(new FirefoxDriver(FirefoxOP));
        }

        /// <summary>
        /// Adds an argument when firefox is started
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="arg">the argument</param>
        public void AddStartArguments(params string[] args)
        {
            HasStartedCheck();
            FirefoxOP.AddArguments(args);
        }

        /// <summary>
        /// Adds an extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="path">path to extension</param>
        public void AddExtension(string path)
        {
            HasStartedCheck();
            FirefoxOP.Profile.AddExtension(path);
        }
    }
}