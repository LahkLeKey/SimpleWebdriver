using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DotNetConsoleAppTest
{
    class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static void BringConsoleToFront()
        {
            SetForegroundWindow(GetConsoleWindow());
        }

        private static void Print(object message)
        {
            BringConsoleToFront();
            Console.WriteLine(String.Concat(Enumerable.Repeat("-", 96)));
            Console.WriteLine(message.ToString());
            Console.WriteLine(String.Concat(Enumerable.Repeat("-", 96)));
        }

        static void Main(string[] args)
        {
            Print($"Error handling has not been implemented for this program.\nThis has been done as a quick and dirty refresher for both C# and Selenium.\n");

            string assemblyExecutablePath = Assembly.GetExecutingAssembly().Location;
            string webDriverExecutablePath = $"{Path.GetDirectoryName(assemblyExecutablePath)}";
            Print($"Web Driver Path : {webDriverExecutablePath}");

            IList<string> validGeoCodes = new List<string>();

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--kiosk"); // Start in fullscreen since we don't care about mobile support for now.
            options.AddArgument("--log-level=3"); // Silence driver logging.
            IWebDriver driver = new ChromeDriver(webDriverExecutablePath, options);
            driver.Url = "https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes";
            Print($"Driver Object : {driver}");


            By xPathSelector = By.XPath("//a[@title='ISO 3166-1 alpha-2']");
            Print($"Parsing DOM for target elements please wait...");
            IReadOnlyCollection<IWebElement> alpha2Codes = driver.FindElements(xPathSelector);

            Print($"Parsing targeted elements please wait...");
            foreach (IWebElement EGeoCode in alpha2Codes)
            {   
                // We are using Alpha-2 Codes. Everything else should be ignored.
                if (EGeoCode.Text.Length == 2)
                {
                    validGeoCodes.Add(EGeoCode.Text);
                }
            }


            int foundGeoCount = validGeoCodes.Count;
            if (foundGeoCount != 249)
                Print($"Warning, Something went wrong when parsing geo codes. We should have 249 total and we have {foundGeoCount}");

            Print($"({validGeoCodes.Count} / 249) alpha-2 geo codes have been parsed.");

            Print("Please enter your 2 digit geo code. eg.) US\n\tYou may also just hit enter and we will default your geo code to US");
            string userInputGeoCode = Console.ReadLine();
            Print($"You selected {userInputGeoCode} as your geo code.");

            bool bMatchFound = false;
            if (validGeoCodes.Any(geoCode => userInputGeoCode.ToLower() == geoCode.ToLower()))
                bMatchFound = true;

            /* Default to United States on invalid geo code.*/
            if (!bMatchFound)
            {
                if (userInputGeoCode == "")
                {
                   Print("You did not select a geo-code. Defaulted to United States (US)");
                } else {
                   Print("Your geo code was invalid.\n\tYour search has been defaulted to the United States");
                }
                userInputGeoCode = "US";
            }
            
            driver.Url = $"https://trends.google.com/trends/?geo={userInputGeoCode}";

            By topicSelector = By.ClassName("fe-explore-example-legend-text");
            IReadOnlyCollection<IWebElement> popularTopics = driver.FindElements(topicSelector);

            Print($"[{userInputGeoCode}] #1 Trending is {popularTopics.ElementAt(0).Text}");
            Print($"[{userInputGeoCode}] #2 Trending is {popularTopics.ElementAt(1).Text}");

            Print("Please enter a custom search parameter. eg.) Music");
            string userInputCustomSearch = Console.ReadLine();

            By inputSelector = By.XPath("//input[@placeholder=\"Enter a search term or a topic\"]");
            IWebElement searchInput = driver.FindElement(inputSelector);
            searchInput.Click();
            searchInput.SendKeys($"{userInputCustomSearch}{Keys.Return}");

            Print("This concludes the Selenium test application.");

            // Stop console from closing automatically
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
