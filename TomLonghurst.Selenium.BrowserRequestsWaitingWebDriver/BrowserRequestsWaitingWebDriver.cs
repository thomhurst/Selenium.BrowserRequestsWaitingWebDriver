using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver
{
    public class BrowserRequestsWaitingWebDriver : EventFiringWebDriver
    {
        private readonly TimeSpan? _timeout;
        private int _pendingRequests;

        private readonly object _locker = new object();
        
        public BrowserRequestsWaitingWebDriver(IWebDriver parentDriver, TimeSpan? timeout = null) 
            : base(parentDriver ?? throw new ArgumentNullException(nameof(parentDriver)))
        {
            _timeout = timeout;
            
            var network = parentDriver.Manage().Network;

            network.NetworkRequestSent += (sender, args) =>
            {
                lock (_locker)
                {
                    _pendingRequests++;
                }
            };
            
            network.NetworkResponseReceived += (sender, args) =>
            {
                lock (_locker)
                {
                    _pendingRequests--;
                }
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

            for (var i = 0; i < 3; i++)
            {
                var wait = new WebDriverWait(WrappedDriver, timeout)
                {
                    Message = $"Browser requests did not finish within {timeout.TotalSeconds} seconds",
                    Timeout = timeout
                };

                wait.Until(_ =>
                {
                    lock (_locker)
                    {
                        return _pendingRequests == 0;
                    }
                });
            }
        }
    }
}