using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Extensions;
using WebDriverManager.DriverConfigs.Impl;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.All)]
public class Tests
{
    [OneTimeSetUp]
    public static void Setup()
    {
        new WebDriverManager.DriverManager().SetUpDriver(new ChromeConfig());
    }

    [Test, Repeat(5)]
    public void Normal_WebDriver_Doesnt_Wait_And_Update_Title()
    {
        using var webdriver = GetChromeDriver();
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.FindElement(By.Id("title")).Text, Is.EqualTo("Example"));
        });
    }
    
    [Test, Repeat(5)]
    public void Wrapped_WebDriver_Does_Wait_And_Update_Title()
    {
        using var webdriver = GetChromeDriver().WithWaitingForBrowserRequests();
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.FindElement(By.Id("title")).Text, Is.EqualTo("Updated!"));
        });
    }
    
    private static ChromeDriver GetChromeDriver()
    {
        var chromeOptions = new ChromeOptions();
        
        chromeOptions.AddArgument("--headless=new");
        
        return new ChromeDriver(chromeOptions);
    }
}