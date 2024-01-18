using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver
{
    public class BrowserRequestsWaitingWebDriver : EventFiringWebDriver
    {
        private readonly TimeSpan? _timeout;
        private int _pendingRequests;
        
        public BrowserRequestsWaitingWebDriver(IWebDriver parentDriver, TimeSpan? timeout = null) 
            : base(parentDriver ?? throw new ArgumentNullException(nameof(parentDriver)))
        {
            _timeout = timeout;
            
            var network = parentDriver.Manage().Network;

            network.NetworkRequestSent += (sender, args) =>
            {
                Interlocked.Increment(ref _pendingRequests);
            };
            
            network.NetworkResponseReceived += (sender, args) =>
            {
                Interlocked.Decrement(ref _pendingRequests);
            };

            Navigating += (sender, args) => WaitForBrowserRequestsToComplete();
            Navigated += (sender, args) => WaitForBrowserRequestsToComplete();
            NavigatedBack += (sender, args) => WaitForBrowserRequestsToComplete();
            NavigatedForward += (sender, args) => WaitForBrowserRequestsToComplete();
            
            FindingElement += (sender, args) => WaitForBrowserRequestsToComplete();
            FindElementCompleted += (sender, args) => WaitForBrowserRequestsToComplete();
            
            ElementClicking += (sender, args) => WaitForBrowserRequestsToComplete();
            ElementClicked += (sender, args) => WaitForBrowserRequestsToComplete();

            ElementValueChanging += (sender, args) => WaitForBrowserRequestsToComplete();
            ElementValueChanged += (sender, args) => WaitForBrowserRequestsToComplete();
            
            ScriptExecuting += (sender, args) => WaitForBrowserRequestsToComplete();
            ScriptExecuted += (sender, args) => WaitForBrowserRequestsToComplete();
            
            network.StartMonitoring();
        }

        ~BrowserRequestsWaitingWebDriver()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        private void WaitForBrowserRequestsToComplete()
        {
            var timeout = _timeout ?? WrappedDriver.Manage().Timeouts().PageLoad;

            var wait = new WebDriverWait(WrappedDriver, timeout)
            {
                Message = $"Browser requests did not finish within {timeout.TotalSeconds} seconds",
                Timeout = timeout
            };

            wait.Until(_ => Thread.VolatileRead(ref _pendingRequests) == 0);
        }
    }
}