using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Assert = NUnit.Framework.Assert;
using System.Threading;
using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.IO;
using System.Linq;

namespace UnitTestProject
{
    [TestFixture]
    public class NotePadTest
    {
        private WindowsDriver<WindowsElement> _driver;
       
        public void TestInit(string appUrl)
        {
            var options = new AppiumOptions();
            options.AddAdditionalCapability("app", appUrl);
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            options.AddAdditionalCapability("platormName", "Windows");
            _driver = new WindowsDriver<WindowsElement>(new Uri(" http://127.0.0.1:4723"), options, TimeSpan.FromMinutes(200));

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(20);
            _driver.Manage().Window.Maximize();

        }
        
        [Test]
        public void Scenario1()
        {
            TestInit("C:\\Windows\\notepad.exe");
            _driver.FindElementByName("Text Editor").SendKeys("TEST data to be filled in the Notepad \n" +
                  "Test data is filled \n"+
                  "Calculate the count of Test in it \n"+
                  "TEST DATA is deleted");
            SelectMenuOption("File", "Save As...	Ctrl+Shift+S");

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementExists(By.Name("File name:")));

            foreach (var fileNameitam in _driver.FindElementsByName("File name:"))
            {
                if(fileNameitam.Displayed)
                {
                    fileNameitam.Clear();
                    fileNameitam.SendKeys("c:\\TestData.txt");
                }
            }
            _driver.FindElementByName("Save").Click();
           
            WaitForFileCreate("C:\\TestData.txt", 2);
            Assert.IsTrue(File.Exists("C:\\TestData.txt"), "File is not present");
        }

        [Test]
        public void Scenario2()
        {
             TestInit("C:\\TestData.txt");
             SelectMenuOption("Edit", "Replace...	Ctrl+H");
             Assert.AreEqual(_driver.FindElementByXPath(".//*[@LocalizedControlType='title bar']").Text,
             "Replace", "Replace Pop is not displayed");
             Assert.AreEqual(_driver.FindElementByXPath(".//*[@AutomationId='65535']").Text, "Find what:",
                 "'Find what' is not matched");
             Assert.AreEqual(_driver.FindElementByName("Replace with:").Text, "Replace with:",
                "'Find what' is not matched");

             _driver.FindElementByXPath(".//*[@AutomationId='1152']").Clear();
             _driver.FindElementByXPath(".//*[@AutomationId='1152']").SendKeys("TEST");
             _driver.FindElementByXPath(".//*[@AutomationId='1153']").Clear();
             _driver.FindElementByXPath(".//*[@AutomationId='1153']").SendKeys("test");
             if(!_driver.FindElementByName("Match case").Selected)
             {
                 _driver.FindElementByName("Match case").Click();
             }

             Assert.IsTrue(_driver.FindElementByName("Match case").Selected, "Match case is not checked");
             _driver.FindElementByName("Replace All").Click();
             _driver.FindElementByName("Cancel").Click();

            _driver.FindElementByName("Text Editor").SendKeys(Keys.Control + "s");

            string value = File.ReadAllText("C:\\TestData.txt");
            string [] testData= value.Split(' ');
            Assert.IsFalse(Array.Exists(testData, element => element.Equals("TEST")), 
                "Is not replated");
        }

        [Test]
        public void Scenario3()
        {
            TestInit("C:\\TestData.txt");
            SelectMenuOption("Format", "Font...");
            _driver.FindElementByName("16").Click();
            _driver.FindElementByName("OK").Click();

            _driver.FindElementByName("Text Editor").SendKeys(Keys.Control + "a");
            _driver.FindElementByName("Text Editor").SendKeys(Keys.Backspace);
            _driver.FindElementByName("Text Editor").SendKeys(Keys.Control + "s");
            string value = File.ReadAllText("C:\\TestData.txt");
            Assert.IsEmpty(value, "Value is present");
        }

        [Test]
        public void Scenario4()
        {
            TestInit("C:\\TestData.txt");
            SelectMenuOption("Help", "About Notepad");
            Assert.AreEqual(_driver.FindElementByName("Microsoft Windows").Text, "Microsoft Windows",
                "'Find what' is not matched");
        }

        public void WaitForFileCreate(string fileName, int second = 10)
        {
            int time = 1;
            while(!File.Exists(fileName) && time < second)
            {
                Thread.Sleep(1000);
            }
        }

        public void SelectMenuOption(string menuItemOption, string optionsItem)
        {
            var allMenus = _driver.FindElementsByTagName("MenuItem");
            Console.WriteLine($"All menu items Count: {allMenus.Count}");
            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            foreach (var mainMenuItem in allMenus)
            {
                if (mainMenuItem.GetAttribute("Name").Equals(menuItemOption))
                {
                    mainMenuItem.Click();
                    var newMenu = _driver.FindElementByName(optionsItem);
                    wait.Until(x => newMenu.Displayed);
                    newMenu.Click();
                 }
            }
        }


        [TearDown]
        public void TestCleanup()
        {
            if (_driver != null)
            {
                _driver.Quit();
                _driver = null;
            }
        }
    }
}
