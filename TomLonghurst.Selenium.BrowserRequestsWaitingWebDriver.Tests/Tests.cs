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
    
    [Test, CancelAfter(60_000)]
    public void Normal_WebDriver_Doesnt_Wait_And_Update_Title(CancellationToken cancellationToken)
    {
        using var webdriver = GetChromeDriver();

        cancellationToken.Register(() => throw new TimeoutException());
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.FindElement(By.Id("title")).Text, Is.EqualTo("Example"));
        });
    }
    
    [Test, CancelAfter(60_000)]
    public void Wrapped_WebDriver_Does_Wait_And_Update_Title(CancellationToken cancellationToken)
    {
        using var webdriver = GetChromeDriver().WithWaitingForBrowserRequests();
        
        cancellationToken.Register(() => throw new TimeoutException());
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.FindElement(By.Id("title")).Text, Is.EqualTo("Updated!"));
        });
    }

    private static readonly object GetChromeDriverLock = new();
    private static ChromeDriver GetChromeDriver()
    {
        var options = new ChromeOptions();
        
        options.AddArgument("--headless=new");

        lock (GetChromeDriverLock)
        {
            return new ChromeDriver(options);
        }
    }
}
