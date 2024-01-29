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
    
    [Test, Retry(5), CancelAfter(30_000)]
    public void Normal_WebDriver_Doesnt_Wait_And_Update_Title(CancellationToken cancellationToken)
    {
        using var webdriver = GetChromeDriver();

        cancellationToken.Register(() => webdriver.Quit());
        
        webdriver.Navigate().GoToUrl(Path.GetFullPath("Example.html"));
       
        Assert.Multiple(() =>
        {
            Assert.That(webdriver.FindElement(By.Id("title")).Text, Is.EqualTo("Example"));
        });
    }
    
    [Test, Repeat(5), CancelAfter(30_000)]
    public void Wrapped_WebDriver_Does_Wait_And_Update_Title(CancellationToken cancellationToken)
    {
        using var webdriver = GetChromeDriver().WithWaitingForBrowserRequests();
        
        cancellationToken.Register(() => webdriver.Quit());
        
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
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");

        lock (GetChromeDriverLock)
        {
            return new ChromeDriver(options);
        }
    }
}
