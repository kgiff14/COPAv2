using Conservice.Payment.DataAccess.Enums;
using COPA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPA.Template.TemplateBase
{
    public class TestTemplateBase : TemplateBaseAbstract
    {
        public PaymentStep StartWork(TransactionProcess transactionProcess)
        {
            throw new NotImplementedException();
        }

        TemplateBaseAbstract TemplateBaseAbstract.CreateTemplateInstance(TransactionProcess transactionProcess)
        {
            throw new NotImplementedException();
        }

        void TemplateBaseAbstract.DriverCleanup()
        {
            throw new NotImplementedException();
        }

        void TemplateBaseAbstract.SetTemplateConditions()
        {
            throw new NotImplementedException();
        }

        void TemplateBaseAbstract.StartDriver()
        {
            throw new NotImplementedException();
        }

        PaymentStep TemplateBaseAbstract.StartWork(TransactionProcess transactionProcess)
        {
            throw new NotImplementedException();
        }
    }
}
