namespace DotNetConsoleAppTest
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;

    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    /// <summary>
    /// The program.
    /// ----------------------------
    ///           Metrics           |
    /// ----------------------------
    /// Maintainability Index: 70
    /// Cyclomatic Complexity: 16
    /// Depth of Inheritance: 1
    /// Class Coupling: 19
    /// Lines of Source code: 218
    /// Lines of Executable code: 65
    /// </summary>
    internal class Program
    {
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// The bring console to front.
        /// </summary>
        private static void BringConsoleToFront() => SetForegroundWindow(GetConsoleWindow());

        /// <summary>
        /// The driver executable path.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string DriverExecutablePath()
        {
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var driverExecutablePath = $"{Path.GetDirectoryName(executablePath)}";
            Print($"Web Driver Path : {driverExecutablePath}");
            return driverExecutablePath;
        }

        /// <summary>
        /// The main loop.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            Print(
                $"Error handling has not been implemented for this program.\nThis has been done as a quick and dirty refresher for both C# and Selenium.\n");

            var driver = WebDriver(out var alpha2Codes);

            var validGeoCodes = ValidGeoCodes(alpha2Codes);

            var userInputGeoCode = UserInputGeoCode(validGeoCodes);

            var userInputCustomSearch = UserInputCustomSearch(driver, userInputGeoCode);
            InputSelector(driver, userInputCustomSearch);

            Pause();
        }

        /// <summary>
        /// The pause utility.
        /// </summary>
        private static void Pause()
        {
            Print("This concludes the Selenium test application.");

            // Stop console from closing automatically
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        /// <summary>
        /// The input selector.
        /// </summary>
        /// <param name="driver">
        /// The driver.
        /// </param>
        /// <param name="userInputCustomSearch">
        /// The user input custom search.
        /// </param>
        private static void InputSelector(IWebDriver driver, string userInputCustomSearch)
        {
            var inputSelector = By.XPath("//input[@placeholder=\"Enter a search term or a topic\"]");
            var searchInput = driver.FindElement(inputSelector);
            searchInput.Click();
            searchInput.SendKeys($"{userInputCustomSearch}{Keys.Return}");
        }

        /// <summary>
        /// The user input custom search.
        /// </summary>
        /// <param name="driver">
        /// The driver.
        /// </param>
        /// <param name="userInputGeoCode">
        /// The user input geo code.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string UserInputCustomSearch(IWebDriver driver, string userInputGeoCode)
        {
            driver.Url = $"https://trends.google.com/trends/?geo={userInputGeoCode}";

            var topicSelector = By.ClassName("fe-explore-example-legend-text");
            IReadOnlyCollection<IWebElement> popularTopics = driver.FindElements(topicSelector);

            Print($"[{userInputGeoCode}] #1 Trending is {popularTopics.ElementAt(0).Text}");
            Print($"[{userInputGeoCode}] #2 Trending is {popularTopics.ElementAt(1).Text}");

            Print("Please enter a custom search parameter. eg.) Music");
            var userInputCustomSearch = Console.ReadLine();
            return userInputCustomSearch;
        }

        /// <summary>
        /// The user input geo code.
        /// </summary>
        /// <param name="validGeoCodes">
        /// The valid geo codes.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private static string UserInputGeoCode(List<string> validGeoCodes)
        {
            Print(
                "Please enter your 2 digit geo code. eg.) US\n\tYou may also just hit enter and we will default your geo code to US");
            var userInputGeoCode = Console.ReadLine();
            Print($"You selected {userInputGeoCode} as your geo code.");

            var bMatchFound = validGeoCodes.Any(geoCode => userInputGeoCode?.ToLower() == geoCode.ToLower());

            /* Default to United States on invalid geo code.*/
            if (!bMatchFound)
            {
                Print(
                    userInputGeoCode == string.Empty
                        ? "You did not select a geo-code. Defaulted to United States (US)"
                        : "Your geo code was invalid.\n\tYour search has been defaulted to the United States");

                userInputGeoCode = "US";
            }

            return userInputGeoCode;
        }

        /// <summary>
        /// The valid geo codes.
        /// </summary>
        /// <param name="alpha2Codes">
        /// The alpha 2 codes.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        private static List<string> ValidGeoCodes(IReadOnlyCollection<IWebElement> alpha2Codes)
        {
            Print($"Parsing targeted elements please wait...");
            var validGeoCodes = (from eGeoCode in alpha2Codes where eGeoCode.Text.Length == 2 select eGeoCode.Text).ToList();

            if (validGeoCodes.Count != 249)
            {
                Print(
                    $"Warning, Something went wrong when parsing geo codes. We should have 249 total and we have {validGeoCodes.Count}");
            }

            Print($"({validGeoCodes.Count} / 249) alpha-2 geo codes have been parsed.");
            return validGeoCodes;
        }

        /// <summary>
        /// The web driver.
        /// </summary>
        /// <param name="alpha2Codes">
        /// The alpha 2 codes.
        /// </param>
        /// <returns>
        /// The <see cref="IWebDriver"/>.
        /// </returns>
        private static IWebDriver WebDriver(out IReadOnlyCollection<IWebElement> alpha2Codes)
        {
            var driverExecutablePath = DriverExecutablePath();

            var options = new ChromeOptions();
            options.AddArgument("--kiosk"); // Start in fullscreen since we don't care about mobile support for now.
            options.AddArgument("--log-level=3"); // Silence driver logging.
            IWebDriver driver = new ChromeDriver(driverExecutablePath, options);
            driver.Url = "https://en.wikipedia.org/wiki/List_of_ISO_3166_country_codes";
            Print($"Driver Object : {driver}");

            var xPathSelector = By.XPath("//a[@title='ISO 3166-1 alpha-2']");
            Print($"Parsing DOM for target elements please wait...");
            alpha2Codes = driver.FindElements(xPathSelector);
            return driver;
        }

        /// <summary>
        /// The print utility.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private static void Print(object message)
        {
            BringConsoleToFront();
            Console.WriteLine(string.Concat(Enumerable.Repeat("-", 96)));
            Console.WriteLine(message.ToString());
            Console.WriteLine(string.Concat(Enumerable.Repeat("-", 96)));
        }
    }
}