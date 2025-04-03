using System.Data;
using System.Data.SqlClient;

namespace STL.DAL
{
    public class DCashLoan : DALObject
    {
        public DataSet GetCashLoanDetails(string CustomerID, string AccountNo)
        {
            DataSet ds = null;
            try
            {
                ds = new DataSet();
                parmArray = new SqlParameter[2];
                parmArray[0] = new SqlParameter("@CustId", SqlDbType.VarChar, 20);
                parmArray[0].Value = CustomerID;
                parmArray[1] = new SqlParameter("@AcctNo", SqlDbType.VarChar, 12);
                parmArray[1].Value = AccountNo;
                this.RunSP("uspGetCashloanDetails", parmArray, ds);
            }
            catch (SqlException ex)
            {
                LogSqlException(ex);
                throw ex;
            }
            return ds;
        }
    }
}