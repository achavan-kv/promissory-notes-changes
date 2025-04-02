/* Version Number: 2.0
Date Changed: 12/10/2019 */

using Unicomer.Cosacs.Model;

namespace Unicomer.Cosacs.Business.Interfaces
{
    public interface ICredits
    {
        JResponse GetMaxWithdrawalAmount(string custId);
        JResponse GetPaymentOptionsByAmount(string CustId, string loanAmount);
        JResponse UpdateCreditInformation(CreditInformation CrInformation);
    }
}
