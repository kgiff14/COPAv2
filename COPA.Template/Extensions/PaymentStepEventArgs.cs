using Conservice.Payment.DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPA.Template.Extensions
{
    public class PaymentStepEventArgs : EventArgs
    {
        public PaymentStep Step;

        public PaymentStepEventArgs(PaymentStep Step)
        {
            this.Step = Step;
        }
    }
}
