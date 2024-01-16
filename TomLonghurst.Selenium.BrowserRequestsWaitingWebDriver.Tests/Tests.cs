using System.Diagnostics;
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

        var stopWatch = Stopwatch.StartNew();
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.Title, Is.EqualTo("Example"));
            Assert.That(stopWatch.Elapsed.TotalSeconds, Is.LessThan(5));
        });
    }
    
    [Test, Repeat(5)]
    public void Wrapped_WebDriver_Does_Wait_And_Update_Title()
    {
        using var webdriver = GetChromeDriver().WithWaitingForBrowserRequests();

        var stopWatch = Stopwatch.StartNew();
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.Title, Is.EqualTo("Updated!"));
            Assert.That(stopWatch.Elapsed.TotalSeconds, Is.GreaterThanOrEqualTo(5));
        });
    }
    
    private static ChromeDriver GetChromeDriver()
    {
        var chromeOptions = new ChromeOptions();
        
        chromeOptions.AddArgument("--headless");
        
        return new ChromeDriver(chromeOptions);
    }
}