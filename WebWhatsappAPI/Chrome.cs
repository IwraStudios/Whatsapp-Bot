using OpenQA.Selenium.Chrome;

namespace WebWhatsappAPI.Chrome
{
    public class ChromeWApp : BaseClass
    {
        static ChromeOptions ChromeOP;
        /// <summary>
        /// Make a new ChromeWhatsapp Instance
        /// </summary>
        public ChromeWApp()
        {
            ChromeOP = new ChromeOptions();
        }

        /// <summary>
        /// Starts the chrome driver with settings
        /// </summary>
        public override void StartDriver()
        {
            HasStartedCheck();
            var drive = new ChromeDriver(ChromeOP);
            base.StartDriver(drive);
        }
        /// <summary>
        /// Adds an extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="path"></param>
        public void AddExtension(string path)
        {
            HasStartedCheck();
            ChromeOP.AddExtension(path);
        }
        /// <summary>
        /// Adds an base64 encoded extension
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="base64">the extension</param>
        public void AddExtensionBase64(string base64)
        {
            HasStartedCheck();
            ChromeOP.AddEncodedExtension(base64);
        }
        /// <summary>
        /// Adds an argument when chrome is started
        /// Note: has to be before start of driver
        /// </summary>
        /// <param name="arg">the argument</param>
        public void AddStartArgument(string arg)
        {
            HasStartedCheck();
            ChromeOP.AddArgument(arg);
        }
    }
}
