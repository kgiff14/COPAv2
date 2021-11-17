using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Selenium.WebDriver;
using Conservice.Selenium.WebFramework;
using COPA.Models;
using COPA.Template.Enums;
using COPA.Template.Extensions;
using COPAv2.BLL.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Conservice.Selenium.WebDriver.DriverStore;

namespace COPA.Template
{
    public class TemplateBase
    {
        public Driver Driver { get; set; }
        public Portal Portal { get; set; }
        internal TemplateBase Template { get; set; }
        internal Payment Payment { get; set; }
        internal DriverTimeout WebDriverTimeout { get; set; } = DriverTimeout.T3;
        internal DriverTimeout CommandTimeout { get; set; } = DriverTimeout.T3;
        internal DriverType DriverType { get; set; } = DriverType.DefaultChromeDriver;
        internal PaymentStep Step { get; set; }
        internal bool IsCardNumberVisible { get; set; } = false;
        internal List<Func<PaymentStep>> PortalHooks { get; set; }

        internal readonly ILogger logger;
        internal readonly ICOPALogger COPALogger;
        internal readonly ScreenshotHelper screenshotHelper;

        public TemplateBase(ILogger logger, ICOPALogger COPALogger, ScreenshotHelper screenshotHelper)
        {
            this.logger = logger;
            this.COPALogger = COPALogger;
            this.screenshotHelper = screenshotHelper;
        }

        public PaymentStep StartWork()
        {
            try
            {
                DriverCleanup();
                SetPortalHooks();
                RunTemplate();
                return Step;
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Trace, e);

                if(Payment.PaymentStatus == PaymentStatus.AwaitingConfirmation
                    || Payment.PaymentStatus == PaymentStatus.Paid)
                {
                    Payment.PaymentStatus = PaymentStatus.Unconfirmed;

                    if (!IsCardNumberVisible)
                    {
                        try
                        {
                            screenshotHelper.TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false, Payment);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {Payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, ex);
                        }
                    }
                }

                return Step;
            }
            finally
            {
                Driver.Dispose();
            }
        }

        internal TemplateBase CreateTemplateInstance(TransactionProcess transactionProcess)
        {
            throw new NotImplementedException();
        }

        internal void DriverCleanup()
        {
            var driverNames = new string[] { "chromedriver", "geckodriver" };
            foreach (var name in driverNames)
            {
                logger.Log(LogLevel.Trace, $"\n\tKilling all {name} processes");
                var driverProcesses = System.Diagnostics.Process.GetProcessesByName(name);

                foreach (var driverProcess in driverProcesses)
                {
                    driverProcess.Kill();
                    driverProcess.Close();
                }
            }
        }

        internal void SetTemplateConditions()
        {
            //intentionally empty
        }

        internal List<Func<PaymentStep>> SetPortalHooks()
        {
            PortalHooks = new List<Func<PaymentStep>>()
            {
                NavigateLogin,
                NavigateToBillPay,
                PayBill,
                SubmitTransaction,
                VerifyTransaction,
            };

            return PortalHooks;
        }

        internal PaymentStep NavigateLogin()
        {
            throw new Exception();
        }

        internal PaymentStep NavigateToBillPay()
        {
            throw new Exception();
        }

        internal PaymentStep PayBill()
        {
            throw new Exception();
        }

        internal PaymentStep SubmitTransaction()
        {
            throw new Exception();
        }

        internal PaymentStep VerifyTransaction()
        {
            throw new Exception();
        }


        internal void StartDriver()
        {
            throw new NotImplementedException();
        }

        internal void RunTemplate()
        {
            foreach(var hook in PortalHooks)
            {
                Step = hook.Invoke();

                if (true)
                {
                    break;
                }
            }
        }

    }
}
