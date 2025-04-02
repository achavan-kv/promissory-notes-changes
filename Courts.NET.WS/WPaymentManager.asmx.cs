using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using STL.DAL;
using STL.Common;
using STL.BLL;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using STL.Common.Static;
using STL.Common.Constants.Enums;
using STL.Common.Constants.ColumnNames;
using System.Web.Services.Protocols;
using STL.Common.Constants.FTransaction;
using Blue.Cosacs.Repositories;
using Blue.Cosacs;
using Blue.Cosacs.Shared;
using System.Collections.Generic;
using Blue.Admin;


namespace STL.WS
{
    /// <summary>
    /// Summary description for WPaymentManager.
    /// </summary>
    /// 
    [WebService(Namespace = "http://strategicthought.com/webservices/")]
    public class WPaymentManager : CommonService
    {
        public WPaymentManager()
        {
            //CODEGEN: This call is required by the ASP.NET Web Services Designer
            InitializeComponent();
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetPaymentAccounts(string customerID, string countryCode, bool lockAccounts, out decimal addToValue, out string err)
        {
            Function = "WPaymentManager::GetPaymentAccounts()";
            err = "";
            SqlConnection conn = null;

            DataSet accounts = null;
            BPayment payment = null;
            addToValue = 0;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            accounts = payment.GetPaymentAccounts(conn, trans, customerID, countryCode, lockAccounts, out addToValue);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return accounts;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int GetAccountSettlement(
            string countryCode, string accountNo, out decimal settlement, out decimal rebate, out decimal collectionFee, out string err)
        {
            Function = "WPaymentManager::GetAccountSettlement()";
            err = "";
            settlement = 0;
            rebate = 0;
            collectionFee = 0;
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BPayment payment = null;
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            payment.GetAccountSettlement(conn, trans, countryCode, accountNo, out settlement, out rebate, out collectionFee);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }


        [WebMethod(MessageName = "CalculateCreditFeeSet")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal CalculateCreditFee(
            ref DataSet accountSet,
            string countryCode,
            string paymentType,
            ref decimal paymentAmount,
            out string err)
        {
            Function = "WPaymentManager::CalculateCreditFee()";
            err = "";
            SqlConnection conn = null;

            BPayment payment = null;
            decimal creditFee = 0;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            creditFee = payment.CalculateCreditFee(conn, trans, countryCode, ref accountSet,
                                ref paymentAmount, paymentType);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return creditFee;
        }


        [WebMethod(MessageName = "CalculateCreditFee")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal CalculateCreditFee(
            string accountNo,
            string countryCode,
            string paymentType,
            ref int allocatedCourtsPerson,
            decimal arrears,
            bool reverseFeeCalc,            // #13746
            ref decimal paymentAmount,
            out decimal collectionFee,
            out decimal bailiffFee,
            out int debitAccount,
            out int segmentId,
            out string err)
        {
            Function = "WPaymentManager::CalculateCreditFee()";
            err = "";
            collectionFee = 0;
            bailiffFee = 0;
            debitAccount = 0;
            SqlConnection conn = null;

            BPayment payment = null;
            segmentId = 0;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            payment.CalculateCreditFee(conn, trans, countryCode, accountNo,
                                ref paymentAmount, paymentType,
                                ref allocatedCourtsPerson, arrears, out collectionFee, out bailiffFee,
                                out debitAccount, out segmentId, reverseFeeCalc);       // #13746

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return collectionFee;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet SavePayment(string accountNo, bool sundryCredit,
            short paymentMethod, string chequeNo,
            string bankCode, string bankAcctNo,
            short branchNo, DataSet payments,
            decimal localTender, decimal localChange,
            int authorisedBy,
            DateTime chequeClearance, int receiptNo,
            out int commissionRef, out int paymentRef,
            out int rebateRef, out decimal rebateSum,
            string countryCode,
            string voucherReference,
            bool courtsVoucher, int voucherAuthorisedBy,
            string accountNoCompany, int returnedChequeAuthorisedBy, //CR 543 Added Peter Chong [26-09-2006]
            int agrmtno,
            string storeCardAcctno,          //IP - 10/01/11 - Store Card
            long? storeCardNo,               //IP - 10/01/11 - Store Card
            out string err)
        {
            Function = "WPaymentManager::SavePayment()";
            err = "";
            commissionRef = 0;
            paymentRef = 0;
            rebateRef = 0;
            rebateSum = 0;

            SqlConnection conn = null;

            BPayment payment = null;
            DataSet transactionSet = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (var trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            transactionSet = payment.SavePayment(conn, trans, accountNo, sundryCredit,
                                paymentMethod, chequeNo, bankCode,
                                bankAcctNo, branchNo, payments,
                                localTender, localChange,
                                out commissionRef, out paymentRef,
                                out rebateRef, out rebateSum,
                                STL.Common.Static.Credential.UserId,
                                authorisedBy,
                                chequeClearance, receiptNo, countryCode,
                                voucherReference,
                                courtsVoucher, voucherAuthorisedBy,
                                accountNoCompany, returnedChequeAuthorisedBy, agrmtno,
                                storeCardAcctno, storeCardNo);

                            if (accountNo.Substring(3, 1) != "4") // if  credit or store card account then check if payment so can unblock...
                                new StoreCardRepository().BlockStoreCardAccountFeeCheck(conn, trans, DateTime.Now, accountNo, payment: true);

                            trans.Commit();

                            //66669 Need to check that data has been committed to the database. If any hasn't then no printing should be performed
                            transactionSet = payment.CheckForCommittedData(transactionSet);
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return transactionSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetTempReceipt(int receiptNo, out string err)
        {
            Function = "WPaymentManager::GetTempReceipt()";
            err = "";
            DataSet receipt = null;
            BPayment payment = null;
            try
            {
                payment = new BPayment();
                receipt = payment.GetTempReceipt(receiptNo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return receipt;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet CheckDeliveryDate(string accountNo, string countryCode, out DateTime chequeClearance, out string err)
        {
            Function = "WPaymentManager::CheckDeliveryDate()";
            err = "";
            chequeClearance = DateTime.MinValue;
            DataSet deliverySet = null;
            BPayment payment = null;
            try
            {
                payment = new BPayment();
                deliverySet = payment.CheckDeliveryDate(accountNo, countryCode, out chequeClearance);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return deliverySet;
        }



        #region Component Designer generated code

        //Required by the Web Services Designer 
        private IContainer components = null;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierTotals(short branchno, int employeeno,
            DateTime datefrom, DateTime dateto,
            bool listCheques, out decimal total,
            out string err)
        {
            Function = "WPaymentManager::GetCashierTotals()";
            DataSet ds = new DataSet();
            err = "";
            total = 0;

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetCashierTotals(branchno, employeeno, datefrom, dateto, listCheques, out total);

            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DateTime GetDateLastAudit(int empeeno, out string err)
        {
            Function = "WPaymentManager::GetDateLastAudit()";

            err = "";
            DateTime dateLast = DateTime.MinValue;


            try
            {
                BPayment bo = new BPayment();
                dateLast = bo.GetDateLastAudit(empeeno);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return dateLast;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetChequeDetails(string chequeno, string bankcode, string bankacctno, DateTime datefrom, DateTime dateto, out string err)
        {
            Function = "WPaymentManager::GetChequeDetails()";
            DataSet ds = new DataSet();
            err = "";


            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetChequeDetails(chequeno, bankcode, bankacctno, datefrom, dateto);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int ReverseCheque(string acctno, string chequeno, string bankcode,
            string bankacctno, decimal transvalue, short payMethod,
            short branchno, string lTransType, DateTime lDateTrans,
            int lTransRefNo, string countryCode, out string err)
        {
            Function = "WPaymentManager::ReverseCheque()";
            err = "";
            SqlConnection conn = null;

            BPayment payment = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            payment.ReverseCheque(conn, trans, acctno, chequeno, bankcode,
                                bankacctno, transvalue, payMethod,
                                branchno, lTransType, lDateTrans,
                                lTransRefNo, countryCode);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
        [TraceExtension]
#endif
        public int SaveCashierTotal(DateTime datefrom, DateTime dateto, int runno, int empeenoauth, bool canReverse, decimal usertotal, decimal systemtotal, decimal difference, decimal deposits, short branchno, int cashier, string countryCode, DataSet breakdown, out string err)
        {
            Function = "WPaymentManager::SaveCashierTotal()";
            int status = 0;
            err = "";
            SqlConnection conn = null;

            BPayment payment = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            payment = new BPayment();
                            payment.User = STL.Common.Static.Credential.UserId;
                            payment.SaveCashierTotal(conn, trans, datefrom, dateto, cashier, runno, empeenoauth, canReverse, usertotal, systemtotal, difference, deposits, branchno, countryCode, breakdown);

                            //IP - 12/12/11 - #8813 - CR1234
                            Dictionary<string, object> cashierAudit = new Dictionary<string, object>();

                            cashierAudit.Add("Cashier", cashier);
                            cashierAudit.Add("Date From", datefrom);
                            cashierAudit.Add("Date To", dateto);
                            cashierAudit.Add("User Total", usertotal);
                            cashierAudit.Add("System Total", systemtotal);
                            cashierAudit.Add("Difference", difference);
                            cashierAudit.Add("Deposits", deposits);

                            EventStore.Instance.Log(cashierAudit, "CashierTotalsSave", EventCategory.CashierTotals);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return status;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierTotalsHistory(int empeeno, out string err)
        {
            Function = "WPaymentManager::GetCashierTotalsHistory()";
            DataSet ds = new DataSet();
            err = "";


            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetCashierTotalsHistory(empeeno);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierTotalsReversal(
            int empeeno, out bool canReverse,
            out DateTime dateFrom, out DateTime dateTo,
            out decimal diffTotal, out decimal systemTotal,
            out decimal depositTotal, out decimal userTotal, out string err)
        {
            Function = "WPaymentManager::GetCashierTotalsReversal()";
            DataSet ds = new DataSet();
            err = "";
            canReverse = false;
            dateFrom = DateTime.Today;
            dateTo = DateTime.Today;
            diffTotal = 0;
            systemTotal = 0;
            depositTotal = 0;
            userTotal = 0;

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetCashierTotalsReversal(empeeno, out canReverse,
                    out dateFrom, out dateTo,
                    out diffTotal, out systemTotal,
                    out depositTotal, out userTotal);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCashierTotalsReversal(string countryCode, short branchNo, int empeeNo, out string err)
        {
            Function = "WPaymentManager::SaveCashierTotalsReversal()";
            SqlConnection conn = null;

            err = "";

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BPayment bp = new BPayment();
                            bp.SaveCashierTotalsReversal(conn, trans, countryCode, branchNo, empeeNo);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }

            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierDeposits(int empeeno, short postedtofact, DateTime datefrom, DateTime dateto, short branchNo, string depositType, bool branchFloats, int paymentMethod, out string err) //IP - 15/12/11 - #8810 - CR1234
        {
            Function = "WPaymentManager::GetCashierDeposits()";
            DataSet ds = new DataSet();
            err = "";


            try
            {
                BPayment bo = new BPayment();
                ds.Tables.Add(bo.GetCashierDeposits(empeeno, postedtofact, datefrom, dateto, branchNo, depositType, branchFloats, paymentMethod));  //IP - 15/12/11 - #8810 - CR1234


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCashierDeposits(short branchNo, int cashier, DataSet deposits, out string err)
        {
            Function = "WPaymentManager::SaveCashierDeposits()";
            int status = 0;
            err = "";

            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BPayment bo = new BPayment();
                            status = bo.SaveCashierDeposits(conn, trans, branchNo, cashier, deposits);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return status;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCashierDisbursements(DataSet deposits, out string err)
        {
            Function = "WPaymentManager::SaveCashierDisbursements()";
            int status = 0;
            err = "";

            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            EventStore.Instance.Log(deposits.Tables[0], "CashierDisbursements", EventCategory.CashierDisbursements);    //IP - 19/12/11 - #8812

                            BPayment bo = new BPayment();
                            status = bo.SaveCashierDisbursements(conn, trans, deposits);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return status;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int VoidCashierDeposit(int depositid, DateTime datevoided, bool reverse, out string err)
        {
            Function = "WPaymentManager::VoidCashierDeposit()";
            int status = 0;
            err = "";

            try
            {
                BPayment bo = new BPayment();
                status = bo.VoidCashierDeposit(depositid, datevoided, STL.Common.Static.Credential.UserId, reverse);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return status;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetAccountPayments(string acctno, out string err)
        {
            Function = "WPaymentManager::GetAccountPayments()";
            DataSet ds = new DataSet();
            err = "";


            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetAccountPayments(acctno);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal GetAmountPaid(string acctno, out string err)
        {
            Function = "WPaymentManager::GetAmountPaid()";
            err = "";
            decimal amount = 0;
            try
            {
                BPayment bo = new BPayment();
                amount = bo.GetAmountPaid(acctno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return amount;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int WriteFintransRecord(string accountNo, short branchNo,
            decimal amount, string transType, string bankCode,
            string bankAcctNo, string chequeNo, short payMethod,
            string countryCode, DateTime dateTrans,
            string footNote, out string err)
        {
            Function = "WPaymentManager::WriteFintransRecord()";
            err = "";
            SqlConnection conn = null;

            BBranch branch = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            branch = new BBranch();

                            BTransaction t = new BTransaction(conn, trans, accountNo, branchNo,
                                branch.GetTransRefNo(conn, trans, branchNo), amount,
                                STL.Common.Static.Credential.UserId, transType,
                                bankCode, bankAcctNo, chequeNo, payMethod,
                                countryCode, DateTime.Now, footNote, 0);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCorrection(string accountNo, short branchNo,
            decimal amount, string transType, string bankCode,
            string bankAcctNo, string chequeNo, short payMethod,
            string countryCode, DateTime dateTrans,
            string footNote, int paymentRef,
            int authorisedBy, out string err)
        {
            Function = "WPaymentManager::SaveCorrection()";
            err = "";
            SqlConnection conn = null;

            BPayment pay = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            pay = new BPayment();
                            pay.User = STL.Common.Static.Credential.UserId;
                            pay.SaveCorrection(conn, trans, accountNo,
                                branchNo, amount, transType, bankCode,
                                bankAcctNo, chequeNo, payMethod,
                                countryCode, DateTime.Now, footNote,
                                paymentRef, authorisedBy);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveBDWCorrection(
            string bdwAccountNo,
            short branchNo,
            decimal amount,
            string countryCode,
            DateTime lDateTrans,
            short lBranchNo,
            int paymentRef,
            int authorisedBy,
            out string err)
        {
            Function = "WPaymentManager::SaveBDWCorrection()";
            err = "";
            SqlConnection conn = null;

            BPayment pay = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            pay = new BPayment();
                            pay.User = STL.Common.Static.Credential.UserId;
                            pay.SaveBDWCorrection(
                                conn,
                                trans,
                                bdwAccountNo,
                                branchNo,
                                amount,
                                countryCode,
                                lDateTrans,
                                lBranchNo,
                                paymentRef,
                                authorisedBy);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveRefund(string accountNo, short branchNo,
            decimal amount, string transType, string bankCode,
            string bankAcctNo, string chequeNo, short payMethod,
            string countryCode, DateTime dateTrans,
            DateTime linkedDateTrans, short linkedBranchNo, int linkedRefNo,
            string footNote, int authorisedBy, out string err)
        {
            Function = "WPaymentManager::SaveRefund()";
            err = "";
            SqlConnection conn = null;

            BPayment pay = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {


                            pay = new BPayment();
                            pay.User = STL.Common.Static.Credential.UserId;
                            pay.SaveRefund(conn, trans, accountNo,
                                branchNo, amount, transType, bankCode,
                                bankAcctNo, chequeNo, payMethod,
                                countryCode, DateTime.Now,
                                linkedDateTrans, linkedBranchNo, linkedRefNo,
                                footNote, authorisedBy);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod(MessageName = "GetUnexportedCashierTotals")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetUnexportedCashierTotals(int branchno, out string err)
        {
            Function = "WPaymentManager::GetUnexportedCashierTotals()";
            err = "";
            DataSet ds = null;

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetUnexportedCashierTotals(branchno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetAllTransactionsByAccount(string accountNo, out string err)
        {
            Function = "WPaymentManager::GetAllTransactionsByAccount()";
            DataSet ds = null;
            err = "";

            try
            {
                BTransaction b = new BTransaction();
                ds = b.GetByAcctNo(null, null, accountNo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return ds;
        }

        [WebMethod(MessageName = "GetDDMandateByAccountNo")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDDMandate(string piCountryCode, string accountNo, out int rowCount, out string err)
        {
            Function = "WPaymentManager::GetDDMandate(overloaded by accountNo)";
            DataSet mandateSet = null;
            rowCount = 0;
            err = "";

            try
            {
                BDDMandate mandate = new BDDMandate(piCountryCode, Date.blankDate);
                mandateSet = mandate.Get(accountNo, out rowCount);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return mandateSet;
        }

        [WebMethod(MessageName = "GetDDMandateByMandateId")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDDMandate(string piCountryCode, int mandateId, out int rowCount, out string err)
        {
            Function = "WPaymentManager::GetDDMandate(overloaded by mandateId)";
            DataSet mandateSet = null;
            rowCount = 0;
            err = "";

            try
            {
                BDDMandate mandate = new BDDMandate(piCountryCode, Date.blankDate);
                mandateSet = mandate.Get(mandateId, out rowCount);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return mandateSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int AnotherMandate(string piCountryCode, int mandateId, string accountNo, out int anMandateId,
            out DateTime startDate, out DateTime endDate, out DateTime effDate, out string err)
        {
            Function = "WPaymentManager::AnotherMandate()";
            err = "";
            int rowCount = 0;
            anMandateId = 0;
            startDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            endDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            effDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);

            try
            {
                BDDMandate mandate = new BDDMandate(piCountryCode, Date.blankDate);
                rowCount = mandate.AnotherMandate(mandateId, accountNo, out anMandateId, out startDate, out endDate, out effDate);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return rowCount;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet InitDates(string piCountryCode, DateTime piRunDate, out string err)
        {
            Function = "WPaymentManager::InitDates()";
            err = "";
            DataSet giroDateSet = null;

            try
            {
                BDDMandate mandate = new BDDMandate(piCountryCode, Date.blankDate);
                giroDateSet = mandate.InitDates(piRunDate);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return giroDateSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet SaveMandate(string piCountryCode, bool addNewRecord, DataSet mandateSet, out string err)
        {
            Function = "WPaymentManager::SaveMandate()";
            err = "";
            SqlConnection conn = null;

            DataSet returnMandateSet = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BDDMandate mandate = new BDDMandate(piCountryCode, Date.blankDate);
                            mandate.changedBy = STL.Common.Static.Credential.UserId;
                            returnMandateSet = mandate.SaveMandate(conn, trans, addNewRecord, mandateSet);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return returnMandateSet;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDDPaymentExtraList(string countryCode, out string err)
        {
            Function = "WPaymentManager::GetDDPaymentExtraList";
            DataSet extraPaymentSet = null;
            err = "";

            try
            {
                BDDPaymentExtra extraPayment = new BDDPaymentExtra();
                extraPaymentSet = extraPayment.GetDDPaymentExtraList(countryCode);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return extraPaymentSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet SaveDDPaymentExtraList(DataSet extraPaymentSet, out string acctNo, out string customerName, out string err)
        {
            Function = "WPaymentManager::SaveDDPaymentExtraList()";
            err = "";
            SqlConnection conn = null;

            DataSet returnExtraPaymentSet = null;
            acctNo = "";
            customerName = "";

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BDDPaymentExtra extraPayment = new BDDPaymentExtra();
                            returnExtraPaymentSet = extraPayment.SaveDDPaymentExtraList(conn, trans, extraPaymentSet, out acctNo, out customerName);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return returnExtraPaymentSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDDRejectionList(string countryCode, out string err)
        {
            Function = "WPaymentManager::GetDDRejectionList";
            DataSet rejectionSet = null;
            err = "";

            try
            {
                BDDRejection dRejection = new BDDRejection();
                rejectionSet = dRejection.GetDDRejectionList(countryCode);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return rejectionSet;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet SaveDDRejectionList(DataSet rejectionSet, out string acctNo, out string customerName, out string err)
        {
            Function = "WPaymentManager::SaveDDRejectionList()";
            err = "";
            SqlConnection conn = null;

            DataSet returnRejectionSet = null;
            acctNo = "";
            customerName = "";

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BDDRejection dRejection = new BDDRejection();
                            returnRejectionSet = dRejection.SaveDDRejectionList(conn, trans, rejectionSet, out acctNo, out customerName);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return returnRejectionSet;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int WriteGeneralTransaction(string accountNo, short branchNo,
            decimal amount, string transType, string bankCode,
            string bankAcctNo, string chequeNo, short payMethod,
            string countryCode, string footNote,
            int creditDebit, out string err)
        {
            Function = "WPaymentManager::WriteGeneralTransaction()";
            err = "";
            SqlConnection conn = null;

            BBranch branch = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            branch = new BBranch();

                            BTransaction t = new BTransaction();
                            t.User = STL.Common.Static.Credential.UserId;
                            t.WriteGeneralTransaction(conn, trans, accountNo, branchNo,
                                amount, transType, bankCode,
                                bankAcctNo, chequeNo, payMethod,
                                countryCode, footNote, creditDebit);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int WriteFreeInstalment(string accountNo, short branchNo,
            decimal amount, string countryCode, out string err)
        {
            Function = "WPaymentManager::WriteFreeInstalment()";
            err = "";
            SqlConnection conn = null;

            //BBranch branch = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BPayment freePayment = new BPayment();
                            freePayment.User = STL.Common.Static.Credential.UserId;
                            freePayment.WriteFreeInstalment(conn, trans,
                                accountNo, branchNo, amount, countryCode);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public bool HasCashierTotalled(int empeeno, out string err)
        {
            Function = "WPaymentManager::HasCashierTotalled()";
            bool hasTotalled = false;

            err = "";
            try
            {
                BPayment bo = new BPayment();
                hasTotalled = bo.HasCashierTotalled(empeeno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return hasTotalled;
        }

        [WebMethod(MessageName = "GetUnexportedCashierSummery")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetUnexportedCashierTotals(short branchno, out decimal total, out string err)
        {
            Function = "WPaymentManager::GetUnexportedCashierTotals()";
            DataSet ds = new DataSet();
            err = "";
            total = 0;

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetUnexportedCashierTotals(branchno, out total);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierTotalsBreakdown(int id, out string err)
        {
            Function = "WPaymentManager::GetCashierTotalsBreakdown()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetCashierTotalsBreakdown(id, false);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetExchangeRates(out string err)
        {
            Function = "WPaymentManager::GetExchangeRates()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BPayment payment = new BPayment();
                ds = payment.GetExchangeRates(null, null);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetExchangeRateHistory(string currency, DateTime dateFrom, DateTime dateTo, out string err)
        {
            Function = "WPaymentManager::GetExchangeRateHistory()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BPayment payment = new BPayment();
                ds = payment.GetExchangeRateHistory(currency, dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal CalcForeignTender(int currency, decimal localTender, out string err)
        {
            Function = "WPaymentManager::CalcForeignTender()";
            decimal foreignTender = 0.0M;
            err = "";

            try
            {
                BPayment payment = new BPayment();
                foreignTender = payment.CalcForeignTender(null, null, currency, localTender);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return foreignTender;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal CalcExchangeAmount(int fromCurrency, int toCurrency, decimal amount, out string err)
        {
            Function = "WPaymentManager::CalcExchangeAmount()";
            decimal exchangeAmount = 0.0M;
            err = "";

            try
            {
                BPayment payment = new BPayment();
                exchangeAmount = payment.CalcExchangeAmount(null, null, fromCurrency, toCurrency, amount);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return exchangeAmount;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public void SaveExchangeRates(DataSet exchangeRateSet, out string err)
        {
            Function = "WPaymentManager::SaveExchangeRates()";
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BPayment payment = new BPayment();
                            payment.SaveExchangeRates(conn, trans, exchangeRateSet);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int ValidateGiftVoucher(string reference, bool courts, bool includeRedeemed,
            out decimal voucherValue,
            out DateTime expiry,
            out bool redeemed,
            out string err)
        {
            Function = "WPaymentManager::ValidateGiftVoucher()";
            voucherValue = 0;
            expiry = DateTime.MinValue.AddYears(1899);
            err = "";
            redeemed = false;

            try
            {
                BGiftVoucher bo = new BGiftVoucher();
                bo.Validate(reference, courts, out voucherValue, out expiry, includeRedeemed, out redeemed);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet JournalEnquiryGet(DateTime datefirst, DateTime datelast,
            int firstrefno, int lastrefno, int empeeno,
            int branch, int combination, out string err)
        {
            Function = "WPaymentManager::JournalEnquiryGet()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BTransaction tran = new BTransaction();
                ds = tran.JournalEnquiryGet(datefirst, datelast, firstrefno,
                    lastrefno, empeeno, branch, combination);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SellGiftVoucher(string reference,
            decimal voucherValue,
            DateTime dateExpiry,
            string countryCode,
            string bankCode,
            string bankAcctNo,
            string chequeNo,
            short payMethod,
            short branchNo,
            bool free,
            bool privilegeClub, DataSet accountSet,
            out string err)
        {
            Function = "WPaymentManager::SellGiftVoucher()";
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BGiftVoucher gv = new BGiftVoucher();
                            gv.User = STL.Common.Static.Credential.UserId;
                            gv.Sell(conn, trans, reference, voucherValue,
                                dateExpiry, countryCode, bankCode,
                                bankAcctNo, chequeNo, payMethod, branchNo,
                                free, privilegeClub, accountSet);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashiersWithOutstandingPayments(short branchno, out string err)
        {
            Function = "WPaymentManager::GetCashiersWithOutstandingPayments()";
            DataSet ds = new DataSet();
            err = "";


            try
            {
                BTransaction bo = new BTransaction();
                ds = bo.GetCashiersWithOutstandingPayments(branchno);


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int RedeemOtherGiftVoucher(string reference,
            string acctNoCompany,
            decimal voucherValue,
            out string err)
        {
            Function = "WPaymentManager::RedeemOtherGiftVoucher()";
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BGiftVoucher gv = new BGiftVoucher();
                            gv.User = STL.Common.Static.Credential.UserId;
                            gv.RedeemOther(conn, trans, reference, voucherValue, acctNoCompany);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCashDrawerOpen(int user, string reason, string tillid,
            out string err)
        {
            Function = "WPaymentManager::SaveCashDrawerOpen()";
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BUser u = new BUser();
                            u.SaveCashDrawerOpen(conn, trans, user, reason, tillid);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet LoadCashDrawerOpen(int user, out string err)
        {
            Function = "WPaymentManager::LoadCashDrawerOpen()";
            DataSet ds = null;
            err = "";

            try
            {
                BUser u = new BUser();
                ds = u.LoadCashDrawerOpen(user);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public bool IsDepositReferenceUnique(string reference, out string err)
        {
            Function = "WPaymentManager::IsDepositReferenceUnique()";
            bool unique = false;
            err = "";

            try
            {
                BPayment pay = new BPayment();
                unique = pay.IsDepositReferenceUnique(reference);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return unique;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierOutstandingIncome(short branchNo, out string err)
        {
            Function = "WPaymentManager::GetCashierOutstandingIncome()";
            err = "";
            DataSet ds = null;

            try
            {
                BPayment p = new BPayment();
                ds = p.GetCashierOutstandingIncome(branchNo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierOutstandingIncomeByPayMethod(int empeeno, short branchno, out string err)
        {
            Function = "WPaymentManager::GetCashierOutstandingIncomeByPayMethod()";
            err = "";
            DataSet ds = null;

            try
            {
                BPayment p = new BPayment();
                ds = p.GetCashierOutstandingIncomeByPayMethod(empeeno, branchno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCashierTotalsSummary(short branchno,
            DateTime datefrom,
            DateTime dateto,
            out string err)
        {
            Function = "WPaymentManager::GetCashierTotalsSummary()";
            err = "";
            DataSet ds = null;

            try
            {
                BPayment p = new BPayment();
                ds = p.GetCashierTotalsSummary(branchno, datefrom, dateto);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int LockDepositScreen(short branchno,
            out string err)
        {
            Function = "WPaymentManager::LockDepositScreen()";
            err = "";

            try
            {
                BPayment p = new BPayment();
                p.LockDepositScreen(branchno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int UnLockDepositScreen(short branchno,
            out string err)
        {
            Function = "WPaymentManager::UnLockDepositScreen()";
            err = "";

            try
            {
                BPayment p = new BPayment();
                p.UnLockDepositScreen(branchno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int CashierMustDeposit(int empeeno,
            out bool mustDeposit,
            out string err)
        {
            Function = "WPaymentManager::CashierMustDeposit()";
            err = "";
            mustDeposit = false;

            try
            {
                BPayment p = new BPayment();
                mustDeposit = p.CashierMustDeposit(empeeno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int GetOutstandingSafeDeposits(int empeeno,
            short branchno,
            out decimal safeDeposits,
            out string err)
        {
            Function = "WPaymentManager::GetOutstandingSafeDeposits()";
            err = "";
            safeDeposits = 0;

            try
            {
                BPayment p = new BPayment();
                safeDeposits = p.GetOutstandingSafeDeposits(empeeno, branchno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int ReverseSafeDeposits(int empeeno,
            out string err)
        {
            Function = "WPaymentManager::ReverseSafeDeposits()";
            err = "";

            try
            {
                BPayment p = new BPayment();
                p.User = STL.Common.Static.Credential.UserId;
                p.ReverseSafeDeposits(empeeno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetPaymentHolidays(string customerID, out string err)
        {
            err = "";
            DataSet ds = null;

            try
            {
                BPayment p = new BPayment();
                ds = p.GetPaymentHolidays(null, null, customerID);
            }
            catch (Exception ex)
            {
                Catch(ex, "GetPaymentHolidays", ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        //CR 543 Added to return a list of returned cheques
        public DataSet GetPaymentReturnedCheques(string customerId, out bool authorisationRequired, out string err)
        {
            err = "";
            DataSet ds = null;
            authorisationRequired = false;
            try
            {
                BPayment p = new BPayment();
                ds = p.GetReturnedCheques(null, null, customerId, out authorisationRequired);
            }
            catch (Exception ex)
            {
                Catch(ex, "GetPaymentReturnedCheques", ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SavePaymentHoliday(string accountNo, int agreementNo,
            DateTime newDueDate, DateTime dateTaken,
            out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BPayment p = new BPayment();
                            p.WritePaymentHoliday(conn, trans, accountNo, agreementNo, dateTaken, STL.Common.Static.Credential.UserId, newDueDate);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "SavePaymentHoliday", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int IncludeDeposits(int empNo, short includeDeposits, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BPayment p = new BPayment();
                            p.IncludeDeposits(conn, trans, empNo, includeDeposits);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "IncludeDeposits", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetBranchCashierList(short branchNo, DateTime dateFrom, DateTime dateTo, out string err)
        {
            Function = "WPaymentManager::GetBranchCashierList()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BPayment bo = new BPayment();
                ds = bo.GetBranchCashierList(branchNo, dateFrom, dateTo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet TemporaryReceiptEnquiry(int empoyeeNo, int firstReceipt, int lastreceipt, out string err)
        {
            Function = "WPaymentManager::TemporaryReceiptEnquiry()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BReceipt rt = new BReceipt();
                ds = rt.TemporaryReceiptEnquiry(empoyeeNo, firstReceipt, lastreceipt);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet BailiffTemporaryReceiptEnquiry(int empoyeeNo, out string err)
        {
            Function = "WPaymentManager::TemporaryReceiptEnquiry()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BReceipt rt = new BReceipt();
                ds = rt.BailiffTemporaryReceiptEnquiry(empoyeeNo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int CancelTempReceipt(int receiptNo, out string err)
        {
            Function = "WPaymentManager::CancelTempReceipt()";
            SqlConnection conn = null;

            err = "";
            BReceipt receipt = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            receipt = new BReceipt();
                            receipt.User = STL.Common.Static.Credential.UserId;
                            receipt.CancelTempReceipt(conn, trans, receiptNo);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int VoidTempReceipt(int receiptNo, out string err)
        {
            Function = "WPaymentManager::CancelTempReceipt()";
            SqlConnection conn = null;

            err = "";
            BReceipt receipt = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            receipt = new BReceipt();
                            receipt.VoidTempReceipt(conn, trans, receiptNo);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int AllocateTempReceipt(int empeeNo, int branchNo, int firstReceiptNo, int lastReceiptNo, DateTime issueDate, out string err)
        {
            Function = "WPaymentManager::AllocateTempReceipt()";
            SqlConnection conn = null;

            err = "";
            BReceipt receipt = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            receipt = new BReceipt();
                            receipt.User = STL.Common.Static.Credential.UserId;
                            receipt.AllocateTempReceipt(conn, trans, empeeNo, branchNo, firstReceiptNo, lastReceiptNo, issueDate);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int ReallocateTempReceipt(int empeeNo, int branchNo, int firstReceiptNo, int lastReceiptNo, out string err)
        {
            Function = "WPaymentManager::ReallocateTempReceipt()";
            SqlConnection conn = null;

            //int branchNo = Convert.ToInt32(Config.BranchCode);
            err = "";
            BReceipt receipt = null;
            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            receipt = new BReceipt();
                            receipt.User = STL.Common.Static.Credential.UserId;
                            receipt.ReallocateTempReceipt(conn, trans, empeeNo, branchNo, firstReceiptNo, lastReceiptNo);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int CheckReceiptNotIssued(int firstReceiptNo, int lastReceiptNo, int checkOption, ref int issuedCount, out string err)
        {
            Function = "WPaymentManager::CheckReceiptNotIssued()";
            err = "";
            BReceipt receipt = null;
            try
            {
                receipt = new BReceipt();
                receipt.User = STL.Common.Static.Credential.UserId;
                receipt.CheckReceiptNotIssued(firstReceiptNo, lastReceiptNo, checkOption, ref issuedCount);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int GetNextTemporaryReceptNo(ref int nextRecpNo, out string err)
        {
            Function = "WPaymentManager::GetNextTemporaryReceptNo()";

            err = "";
            try
            {
                BReceipt receipt = null;
                receipt = new BReceipt();
                receipt.GetNextTemporaryReceptNo(ref nextRecpNo);

            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }

            return 0;
        }
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDefaultCommissionBasis(string empeeType, out string err)
        {
            Function = "WPaymentManager::GetDefaultCommissionBasis()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BCommissionBasis commissionBasis = new BCommissionBasis();
                ds = commissionBasis.GetDefaultCommissionBasis(empeeType);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetBailiffCommissionBasis(int empoyeeNo, out string err)
        {
            Function = "WPaymentManager::GetBailiffCommissionBasis()";
            DataSet ds = new DataSet();
            err = "";

            try
            {
                BCommissionBasis commissionBasis = new BCommissionBasis();
                ds = commissionBasis.GetBailiffCommissionBasis(empoyeeNo);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCommissionBasis(
            decimal allocpercent,
            string collecttype,
            decimal collectionpercent,
            decimal commnpercent,
            string countrycode,
            short debitaccount,
            string empeetype,
            decimal maxvalue,
            decimal minvalue,
            decimal reposspercent,
            decimal reppercent,
            string statuscode, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            commissionBasis.SaveCommissionBasis(
                                conn,
                                trans,
                                allocpercent,
                                collecttype,
                                collectionpercent,
                                commnpercent,
                                countrycode,
                                debitaccount,
                                empeetype,
                                maxvalue,
                                minvalue,
                                reposspercent,
                                reppercent,
                                statuscode);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "SaveCommissionBasis", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveBailiffCommissionBasis(
            int empeeno,
            decimal allocpercent,
            string collecttype,
            decimal collectionpercent,
            decimal commnpercent,
            short debitaccount,
            string empeetype,
            decimal maxvalue,
            decimal minvalue,
            decimal reposspercent,
            decimal reppercent,
            string statuscode, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            //CR101 RD 02/12/05 Added user
                            commissionBasis.User = STL.Common.Static.Credential.UserId;

                            commissionBasis.SaveBailiffCommissionBasis(
                                conn,
                                trans,
                                empeeno,
                                allocpercent,
                                collecttype,
                                collectionpercent,
                                commnpercent,
                                debitaccount,
                                empeetype,
                                maxvalue,
                                minvalue,
                                reposspercent,
                                reppercent,
                                statuscode);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "SaveBailiffCommissionBasis", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeleteCommissionBasis(
            string countryCode,
            string statusCode,
            string collectType,
            string empeeType, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            commissionBasis.DeleteCommissionBasis(
                                conn,
                                trans,
                                countryCode,
                                statusCode,
                                collectType,
                                empeeType);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeleteCommissionBasis", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeleteBailiffCommissionBasis(
            int empeeNo,
            string statusCode,
            string collectType, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            commissionBasis.DeleteBailiffCommissionBasis(
                                conn,
                                trans,
                                empeeNo,
                                statusCode,
                                collectType);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeleteBailiffCommissionBasis", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCommissionTransactions(int empeeNo//, string type
         , out string err)
        {
            Function = "WPaymentManager::GetCommissionTransactions()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BCommissionBasis cb = new BCommissionBasis();
                ds = cb.GetCommissionTransactions(empeeNo);//, type);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeleteCommissionTransaction(int empeeNo, DateTime dateTrans, int transRefNo, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            commissionBasis.DeleteCommissionTransaction(
                                conn,
                                trans,
                                empeeNo,
                                dateTrans,
                                transRefNo);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeleteCommissionTransaction", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int RestoreCommissionTransaction(int empeeNo, DateTime dateTrans, int transRefNo, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis commissionBasis = new BCommissionBasis();
                            commissionBasis.RestoreCommissionTransaction(
                                conn,
                                trans,
                                empeeNo,
                                dateTrans,
                                transRefNo);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "RestoreCommissionTransaction", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int UpdateCommissionTransactionStatus(DataSet ds, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis cb = new BCommissionBasis();
                            cb.UpdateCommissionTransactionStatus(conn, trans, ds);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "UpdateCommissionTransactionStatus", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int PayBailiffCommission(int empeeNo, decimal commValue, out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BCommissionBasis cb = new BCommissionBasis();
                            cb.PayBailiffCommission(conn, trans, empeeNo, commValue);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "PayBailiffCommission", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetSUCBFinancialDetails(int runno, bool liveDatabase, out string err)
        {
            Function = "WPaymentManager::GetSUCBFinancialDetails()";
            SqlConnection conn = null;

            if (liveDatabase)
                conn = new SqlConnection(Connections.Default);
            else
                conn = new SqlConnection(Connections.Report);

            DataSet ds = new DataSet();
            err = "";

            using (conn)
            {
                try
                {
                    conn.Open();
                    using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                    {
                        BDelivery del = new BDelivery();

                        BTransaction btrans = new BTransaction();
                        ds = btrans.GetSUCBFinancialDetails(runno, conn, trans);
                        trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Catch(ex, Function, ref err);
                }
            }
            return ds;
        }

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal GetWarrantyAdjustment(string acctno, out string err)
        {
            Function = "WPaymentManager::GetWarrantyAdjustment()";
            err = "";
            decimal amount = 0;
            try
            {
                BPayment bo = new BPayment();
                amount = bo.GetWarrantyAdjustment(acctno);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return amount;
        }

        // get Payment File Definition
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetDefinition(out string err)
        {
            Function = "WPaymentManager::GetDefinition()";
            err = "";
            DataSet ds = new DataSet();

            try
            {
                BPaymentFileDefn bo = new BPaymentFileDefn();
                ds = bo.GetDefinition();
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        // Save Payment File Definition
        [WebMethod]
        [SoapHeader("authentication")]
#if (TRACE)
		[TraceExtension]
#endif
        public int SavePaymentDefinition(
            string bankName,
            string fileExt,                     //IP - 20/08/10 - CR1092 - COASTER to CoSACS Enhancements - Changed from fileName to fileExt
            int acctnoBegin,
            int acctnoLength,
            int moneyBegin,
            int moneyLength,
            int moneyPoint,
            int headLine,
            int dateBegin,
            int dateLength,
            string dateFormat,
            int trailerBegin,
            int trailerLength,
            int paymentMethod,
            int hasTrailer,
            int headerIdBegin,                  //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int headerIdLength,                 //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string headerId,                    //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int trailerIdBegin,                 //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int trailerIdLength,                //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string trailerId,                   //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
            bool isBatch,                       //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchHeaderIdBegin,             //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchHeaderIdLength,            //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string batchHeaderId,               //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            bool batchHeaderHasTotal,           //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchHeaderMoneyBegin,          //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchHeaderMoneyLength,         //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchTrailerIdBegin,            //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
            int batchTrailerIdLength,           //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
            string batchTrailerId,              //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
            bool isDelimited,                   //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string delimiter,                   //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements
            int delimitedNoOfCols,              //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string delimitedAcctNoColNo,        //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string delimitedDateColNo,          //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements
            string delimitedMoneyColNo,         //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
            bool isInterest,                    //IP - 03/09/10 - CR1112 - Tallyman Interest Charges
            out string err)
        {
            err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                        BPaymentFileDefn defn = new BPaymentFileDefn();
                        defn.SavePaymentDefinition(
                            conn,
                            trans,
                            bankName,
                            fileExt,                        //IP - 20/08/10 - CR1092 - COASTER to CoSACS Enhancements - Changed from fileName to fileExt
                            acctnoBegin,
                            acctnoLength,
                            moneyBegin,
                            moneyLength,
                            moneyPoint,
                            headLine,
                            dateBegin,
                            dateLength,
                            dateFormat,
                            trailerBegin,
                            trailerLength,
                            paymentMethod,
                            hasTrailer,
                            headerIdBegin,                  //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            headerIdLength,                 //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            headerId,                       //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            trailerIdBegin,                 //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            trailerIdLength,                //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            trailerId,                      //IP - 12/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            isBatch,                        //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderIdBegin,             //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderIdLength,            //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderId,                  //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderHasTotal,            //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderMoneyBegin,          //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchHeaderMoneyLength,         //IP - 19/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            batchTrailerIdBegin,            //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchTrailerIdLength,           //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
                            batchTrailerId,                 //IP - 03/09/10 - CR1092 - COASTER to CoSACS Enhancements
                            isDelimited,                    //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            delimiter,                      //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            delimitedNoOfCols,              //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            delimitedAcctNoColNo,           //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            delimitedDateColNo,             //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements 
                            delimitedMoneyColNo,            //IP - 25/08/10 - CR1092 - COASTER to CoSACS Enhancements      
                            isInterest);                    //IP - 03/09/10 - CR1112 - Tallyman Interest Charges    

                        trans.Commit();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "SavePaymentDefinition", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        // Delete Payment File Definition
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeletePaymentDefinition(
            string bankName,
            out string err)
        {
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {

                            BPaymentFileDefn defn = new BPaymentFileDefn();
                            defn.DeletePaymentDefinition(
                                conn,
                                trans,
                                bankName);

                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeletePaymentDefinition", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        // get Payment Methods
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetPayMethod(string category, string statusFlag, string tableName, out string err)
        {
            Function = "WPaymentManager::GetPayMethod()";
            err = "";
            DataSet ds = new DataSet();

            try
            {
                BPaymentFileDefn pm = new BPaymentFileDefn();
                ds = pm.GetPayMethod(category, statusFlag, tableName);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }
        // Get Sales Commission Rates
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif

        // Get Sales Commission Rates - CR36
        public DataSet GetSalesCommissionRates(string commItemStr, DateTime selectDate, out string err)
        {
            Function = "WPaymentManager::GetSalesCommissionRates()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BSalesCommission commissionRates = new BSalesCommission();
                ds = commissionRates.GetSalesCommissionRates(commItemStr, selectDate);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        // Save Sales Commission Rates
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif

        public void SaveCommissionRates(string commItemStr, DataSet commissionRateSet, out string err)
        {
            Function = "WPaymentManager::SaveCommissionRates()";
            err = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BSalesCommission commissionRates = new BSalesCommission();
                            commissionRates.User = STL.Common.Static.Credential.UserId;
                            commissionRates.SaveCommissionRates(conn, trans, commItemStr, commissionRateSet);
                            trans.Commit();
                        }
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
        }


        // Validate Commission Item
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public string ValidateCommItem(string commItemStr, string commItemText, DateTime CommDateFrom, DateTime CommDateTo, string CommSpiffBranch, out string err)   //CR1035
        {
            Function = "WPaymentManager::ValidateCommItem()";
            err = "";
            string exists = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted))
                        {
                            BSalesCommission Validate = new BSalesCommission();
                            exists = Validate.ValidateCommItem(conn, trans, commItemStr, commItemText, CommDateFrom, CommDateTo, CommSpiffBranch);      //CR1035
                            trans.Commit();
                        }
                        //return exists;
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }

                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return exists;
        }
        // Validate Product Categories
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public string ValidateCategory(out string err)
        {
            Function = "WPaymentManager::ValidateCategory()";
            err = "";
            string category = "";
            SqlConnection conn = null;


            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        //using (SqlTransaction trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted)) {
                        BSalesCommission Validate = new BSalesCommission();
                        category = Validate.ValidateCategory(conn, null);
                        // trans.Commit();
                        //return exists;
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }

                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return category;
        }

        // Get Sales Commission Rates
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif

        // Get Basic Sales Commission Details - CR36
        public DataSet GetBasicSalesCommission(string branchNo, string employee, DateTime fromDate, DateTime toDate, string accountNo, int agreementNo, string sumDet, string category, out string err)  //#15412
        {
            Function = "WPaymentManager::GetBasicSalesCommission()";
            DataSet ds = new DataSet();
            err = "";

            var empeeno = 0; //#15412
            try
            {
                BSalesCommission commission = new BSalesCommission();

                //#15412
                if (employee != "-99")
                {
                    if (employee != "0")
                    {
                        var user = UserRepository.GetUserByLogin(employee);
                        empeeno = user.Id;
                    }
                    else
                    {
                        empeeno = 0;
                    }
                }
                else
                {
                    empeeno = -99;
                }

                ds = commission.GetBasicSalesCommission(branchNo, empeeno, fromDate, toDate, accountNo, agreementNo, sumDet, category);         //#15412
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }




        // Get Sales Commission Report Header
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif

        // Get Sales Commission report header - CR36
        public DataSet GetSalesCommissionReportHeader(int branchNo, int employeeNo, DateTime fromDate, DateTime toDate, bool showStandardCommission, bool showSPIFFCommission, out string err)
        {
            Function = "WPaymentManager::GetBasicSalesCommission()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BSalesCommission commission = new BSalesCommission();
                ds = commission.GetSalesCommissionReportHeader(branchNo, employeeNo, fromDate, toDate, showStandardCommission, showSPIFFCommission);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }


        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif

        // Get Sales Commission report detail - CR36
        public DataSet GetSalesCommissionReportDetail(int employee, DateTime fromDate, DateTime toDate, bool showStandardCommission, bool showSPIFFCommission, out string err)
        {
            Function = "WPaymentManager::GetBasicSalesCommission()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BSalesCommission commission = new BSalesCommission();
                ds = commission.GetSalesCommissionReportDetail(employee, fromDate, toDate, showStandardCommission, showSPIFFCommission);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        // uat376 rdb BDW Reversal
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public bool ReverseBDW(string acctno, string countryCode)
        {
            Function = "WPaymentManager::ReverseBDW()";
            bool success = false;
            string err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);

                conn.Open();
                trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);
                BPayment payment = new BPayment();
                success = payment.ReverseBDW(conn, trans, acctno, countryCode, STL.Common.Static.Credential.UserId);
                trans.Commit();

            }
            catch (Exception ex)
            {
                trans.Rollback();
                Catch(ex, Function, ref err);
            }
            finally
            {
                // dont sav just yet

                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }



            return success;

        }

        //IP - 09/06/10 - CR1083 Collection Commission

        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int SaveCollectionCommissionRule(
            int id,
            string ruleName,
            string empeeType,
            char commissionType,
            string[] actionArr,
            float pCentArrearsColl,
            float pCentOfCalls,
            float pCentOfWorklist,
            int noOfCalls,
            int noOfDaysSinceAction,
            int noTimeFrameDays,
            decimal minBal,
            decimal maxBal,
            decimal minValColl,
            decimal maxValColl,
            int minMnthsArrears,
            int maxMnthsArrears,
            float pCentCommOnArrears,
            float pCentCommOnAmtPaid,
            float pCentCommOnFee,
            decimal commSetVal,
            out string err)
        {
            err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                        BCommissionBasis commissionBasis = new BCommissionBasis();

                        commissionBasis.User = STL.Common.Static.Credential.UserId;
                        commissionBasis.SaveCollectionCommissionRule(
                            conn,
                            trans,
                            id,
                            ruleName,
                            empeeType,
                            commissionType,
                            actionArr,
                            pCentArrearsColl,
                            pCentOfCalls,
                            pCentOfWorklist,
                            noOfCalls,
                            noOfDaysSinceAction,
                            noTimeFrameDays,
                            minBal,
                            maxBal,
                            minValColl,
                            maxValColl,
                            minMnthsArrears,
                            maxMnthsArrears,
                            pCentCommOnArrears,
                            pCentCommOnAmtPaid,
                            pCentCommOnFee,
                            commSetVal
                            );


                        trans.Commit();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "SaveCollectionCommissionRule", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        //IP - 15/06/10 - CR1083 - Collection Commissions
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetCollectionCommissionRules(string empeeType, out string err)
        {
            Function = "WPaymentManager::GetCollectionCommissionRules()";
            DataSet ds = new DataSet();
            err = "";
            try
            {
                BCommissionBasis commissionBasis = new BCommissionBasis();
                ds = commissionBasis.GetCollectionCommissionRules(empeeType);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        //IP - 15/06/10 - CR1083 - Collection Commissions
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeleteCollectionCommissionRule(
            int id,
            out string err)
        {
            err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                        BCommissionBasis commissionBasis = new BCommissionBasis();
                        commissionBasis.User = STL.Common.Static.Credential.UserId;
                        commissionBasis.DeleteCollectionCommissionRule(
                            conn,
                            trans,
                            id);

                        trans.Commit();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeleteCollectionCommission", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }


        //---
        //IP - 15/06/10 - CR1083 - Collection Commissions
        [WebMethod]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public int DeleteCollectionCommissionRuleActions(
            int id,
            out string err)
        {
            err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);
                do
                {
                    try
                    {
                        conn.Open();
                        trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                        BCommissionBasis commissionBasis = new BCommissionBasis();

                        commissionBasis.DeleteCollectionCommissionRuleActions(conn, trans, id);

                        trans.Commit();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        CatchDeadlock(ex, conn);
                    }
                } while (retries <= maxRetries);
            }
            catch (Exception ex)
            {
                Catch(ex, "DeleteCollectionCommissionRuleActions", ref err);
            }
            finally
            {
                if (conn.State != ConnectionState.Closed)
                    conn.Close();
            }
            return 0;
        }

        [WebMethod(Description = "This method returns a count for the item passed into the method from the database")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public DataSet GetUpliftCommissionRates()
        {
            Function = "WPaymentManager::GetUpliftCommissionRates()";
            var ds = new DataSet();

            var err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);

                conn.Open();
                trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                BranchRepository br = new BranchRepository();
                //dt = br.GetUpliftCommissionRates(conn, trans);
                ds.Tables.Add(br.GetUpliftCommissionRates(conn, trans));
                ds.Tables.Add(br.GetUserRoles(conn, trans));            // #11203


            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return ds;
        }

        [WebMethod(Description = "This method returns a count for the item passed into the method from the database")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public void SaveUpliftCommissionRates(DataTable upliftRates)
        {
            Function = "WPaymentManager::SaveUpliftCommissionRates()";

            var err = "";
            SqlConnection conn = null;
            SqlTransaction trans = null;

            try
            {
                conn = new SqlConnection(Connections.Default);

                conn.Open();
                trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

                BranchRepository br = new BranchRepository();
                br.SaveUpliftCommissionRates(conn, trans, upliftRates);

                trans.Commit();
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
        }

        //Early settlement figure for amortized cash loan accounts
        [WebMethod(Description = "This method returns early settlement figure for amortized cash loan account")]
        [SoapHeader("authentication")]
#if(TRACE)
		[TraceExtension]
#endif
        public decimal GetEarlySettlementFig(string accountNumber)
        {
            Function = "WPaymentManager::GetEarlySettlementFig()";

            var err = "";
            decimal settlementFig = 0;
            try
            {
                BPayment payment = new BPayment();
                payment.GetEarlySettlementFig(accountNumber, out settlementFig);
            }
            catch (Exception ex)
            {
                Catch(ex, Function, ref err);
            }
            return settlementFig;
        }

    


    #region CLA Outstanding Balance
    [WebMethod(Description = "This method is used for General financial transaction validation ")]
    [SoapHeader("authentication")]
#if (TRACE)
		[TraceExtension]
#endif
    public int CLGeneralFinanceTransactionValidation(string acctno, string transtype, out decimal amount, int creditDebit)
    {
        Function = "WPaymentManager::CLGeneralFinanceTransactionValidation";

        var err = "";
        amount = 0;
        SqlConnection conn = null;
        SqlTransaction trans = null;

        try
        {
            conn = new SqlConnection(Connections.Default);

            conn.Open();
            trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

            BPayment bp = new BPayment();
            int result = bp.CLGeneralFinanceTransactionValidation(conn, trans, acctno, out amount, creditDebit, transtype);

            trans.Commit();
            return result;
        }
        catch (Exception ex)
        {
            Catch(ex, Function, ref err);
            return -1;
        }
    }

    [WebMethod(Description = "This method will check if the account is clamortized or not")]
    [SoapHeader("authentication")]
#if (TRACE)
		[TraceExtension]
#endif
    public bool IsCashLoanAmortizedAccount(string acctno)
    {
        Function = "WPaymentManager::IsCashLoanAmortizedAccount";

        var err = "";
        SqlConnection conn = null;
        SqlTransaction trans = null;

        try
        {
            conn = new SqlConnection(Connections.Default);
            conn.Open();
            trans = conn.BeginTransaction(IsolationLevel.ReadUncommitted);

            BPayment bp = new BPayment();
            bool result = bp.IsCashLoanAmortizedAccount(conn, trans, acctno);
            trans.Commit();
            return result;

        }
        catch (Exception ex)
        {
            Catch(ex, Function, ref err);
            return false;
        }
    }
    #endregion
}
}


