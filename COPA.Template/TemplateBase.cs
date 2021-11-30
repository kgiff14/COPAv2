using Conservice.Logging;
using Conservice.Payment.DataAccess.Enums;
using Conservice.Selenium.WebDriver;
using Conservice.Selenium.WebFramework;
using COPA.Models;
using COPA.Template.Enums;
using COPA.Template.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Conservice.Selenium.WebDriver.DriverStore;
using static COPA.Template.Extensions.PaymentStepEvent;

namespace COPA.Template
{
    public class TemplateBase
    {
        public Driver Driver { get; set; }
        public Portal Portal { get; set; }
        public Payment Payment { get; set; }
        internal bool IsCardNumberVisible { get; set; } = false;
        public List<PaymentStepDelegate> PortalHooks { get; set; }
        private readonly PaymentStepEvent paymentStepEvent;

        public TemplateBase(Driver Driver, Payment Payment, PaymentStepEvent paymentStepEvent)
        {
            this.Driver = Driver;
            this.Payment = Payment;
            this.paymentStepEvent = paymentStepEvent;
        }

        public virtual void SetTemplateConditions()
        {
            //intentionally empty
        }

        public List<PaymentStepDelegate> SetPortalHooks()
        {
            PortalHooks = new List<PaymentStepDelegate>()
            {
                NavigateLogin,
                NavigateToBillPay,
                PayBill,
                SubmitTransaction,
                VerifyTransaction
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
    }
}
