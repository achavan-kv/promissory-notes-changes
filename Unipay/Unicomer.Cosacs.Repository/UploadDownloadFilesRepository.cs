/* Version Number: 2.0
Date Changed: 12/10/2019 */

using Unicomer.Cosacs.Model;

namespace Unicomer.Cosacs.Repository
{
    public class UploadDownloadFilesRepository
    {
        public string CustCreditDocuemntsSave(CustCreditDocument custCreditDocument)
        {
            var CCDST = new CustCreditDocuemntsSaveTransaction();
            return CCDST.CustCreditDocuemntsSave(custCreditDocument);
        }
    }
}
