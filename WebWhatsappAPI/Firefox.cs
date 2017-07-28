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
            FirefoxProfile foxProfile = new FirefoxProfile()
            {
                AcceptUntrustedCertificates = false,
                AlwaysLoadNoFocusLibrary = true
            };
            var driver_tmp = new FirefoxDriver(foxProfile);
            base.StartDriver(driver_tmp);

        }


    }
}
