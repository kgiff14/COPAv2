using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Selenium.WebDriver;
using COPA.Models;
using COPA.Template.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Conservice.Selenium.WebDriver.DriverStore;

namespace COPA.Template.TemplateBase
{
    public class LiveTemplateBase : TemplateBaseAbstract
    {
        internal ImageManager.AutomationCOPAImageManager ImageManager;
        public LiveTemplateBase(ILogger logger)
        {
            this.logger = logger;
        }

        public override PaymentStep StartWork(TransactionProcess transactionProcess)
        {
            try
            {
                DriverCleanup();
                SetPortalHooks();
                RunTemplate();
                return Step ?? PaymentStep.AutomationEncounteredError;
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
                            TrySaveScreenshot(Driver.Instance.PageSource, Driver.Instance.GetScreenshot(), false);
                        }
                        catch (Exception ex)
                        {
                            logger.Log(LogLevel.Trace, null, $"PaymentInvoiceNumber: {Payment.PaymentInvoiceNumber}; Error encountered when attempting to save ScreenShot", null, ex);
                        }
                    }
                    LogException(e);
                }

                return Step ?? PaymentStep.AutomationEncounteredError;
            }
            finally
            {
                Driver.Dispose();
            }
        }

        internal override TemplateBaseAbstract CreateTemplateInstance(TransactionProcess transactionProcess)
        {
            throw new NotImplementedException();
        }

        internal override void DriverCleanup()
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

        internal override void LogException(Exception e)
        {
            throw new NotImplementedException();
        }

        internal override void SetTemplateConditions()
        {
            //intentionally empty
        }

        internal override List<Func<PaymentStep>> SetPortalHooks()
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


        internal override void StartDriver()
        {
            throw new NotImplementedException();
        }

        private void TrySaveScreenshot(string pageSource, OpenQA.Selenium.Screenshot screenshot, bool isSuccessImage)
        {
            //Regex comes from: http://www.richardsramblings.com/regex/credit-card-numbers/
            string creditCardRegex = @"\b(?:3[47]\d{2}([\ \-]?)\d{6}\1\d|(?:(?:4\d|5[1-5]|65)\d{2}|6011)([\ \-]?)\d{4}\2\d{4}\2)\d{4}\b";

            if (!Regex.Match(pageSource, creditCardRegex).Success)
            {
                SaveScreenshot(screenshot, isSuccessImage);
            }
        }

        private void SaveScreenshot(OpenQA.Selenium.Screenshot screenshot, bool isSuccessImage)
        {
            var controlNumber = ImageManager.GetControlNumber(isSuccessImage);
            Payment.AssociatedTransaction.Confirmation.ControlNumber = controlNumber;
            string filePath = Path.GetTempPath() + $"{controlNumber}.png";

            screenshot.SaveAsFile(filePath, OpenQA.Selenium.ScreenshotImageFormat.Png);

            ImageManager.Save(filePath, Payment.Website.ProviderName);
            new FileInfo(filePath).Delete();
        }

        internal override void RunTemplate()
        {
            foreach(var hook in PortalHooks)
            {
                Step = hook.Invoke();

                if (IsSuccessStep())
                {
                    break;
                }
            }
        }

    }
}
