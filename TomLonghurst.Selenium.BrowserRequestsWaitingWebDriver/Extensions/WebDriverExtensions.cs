using System;
using OpenQA.Selenium;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Extensions
{
    public static class WebDriverExtensions
    {
        public static IWebDriver WithWaitingForBrowserRequests(this IWebDriver webDriver, TimeSpan? timeout = null)
        {
            return new BrowserRequestsWaitingWebDriver(webDriver, timeout);
        }
    }
}