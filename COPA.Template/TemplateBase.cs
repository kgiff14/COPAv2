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

namespace COPA.Template
{
    public class TemplateBase
    {
        public Driver Driver { get; set; }
        public Portal Portal { get; set; }
        public Payment Payment { get; set; }
        internal PaymentStep Step { get; set; }
        internal bool IsCardNumberVisible { get; set; } = false;
        internal List<Func<PaymentStep>> PortalHooks { get; set; }

        public TemplateBase(Driver Driver)
        {
            this.Driver = Driver;
        }

        public void SetTemplateConditions()
        {
            //intentionally empty
        }

        public List<Func<PaymentStep>> SetPortalHooks()
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
    }
}
