using System;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.IE;
using OpenQA.Selenium;

namespace Bonobo.Git.Server.Test.IntegrationTests
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        protected static MvcWebApp app;
        protected static IntegrationTestHelpers ITH;
        protected static LoadedConfig lc;

        [ClassCleanup]
        public static void Cleanup()
        {
            app.Browser.Close();
        }

        [TestInitialize]
        public void InitTest()
        {
            // We can't use ClassInitialize in a base class
            if (app == null)
            {
                RemoteWebDriver driver = MvcWebApp.Driver.GetDriver();
                ICapabilities capabilities = driver.Capabilities;
                InternetExplorerOptions ieCapabilities = capabilities as InternetExplorerOptions;
                if (ieCapabilities != null)
                {
                    ieCapabilities.IgnoreZoomLevel = true;
                    ieCapabilities.EnableNativeEvents = false;
                    ieCapabilities.EnablePersistentHover = true;
                }

                app = new MvcWebApp();
                lc = AssemblyStartup.LoadedConfig;
                ITH = new IntegrationTestHelpers(app, lc);
            }
         
            
            Console.WriteLine("TestInit");
            ITH.LoginAndResetDatabase();
        }

    }
}