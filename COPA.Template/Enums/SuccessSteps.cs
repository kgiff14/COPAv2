using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COPA.Template.Enums
{
    public enum SuccessStep
    {
        TransactionStartedTemplate = 6,
        DriverStarted = 7,
        AttemptingLogin = 11,
        LoginSuccessful = 12,
        NavigatingToAccount = 20,
        NavigatedToAccount = 21,
        EnteringPaymentInformation = 32,
        PaymentInformationEntered = 33,
        EnteringContactInformation = 38,
        ContactInformationEntered = 39,
        EnteringPaymentAmount = 40,
        EnteredPaymentAmount = 44,
        ReviewTransaction = 46,
        SubmittingTransaction = 49,
        WaitingForSuccessfulPayment = 50,
        GettingConfirmationNumber = 51,
        GettingAmountThatWasPaid = 52,
        PaymentSuccessful = 53
    }
}
