using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Blue.Cosacs.Shared;
using Blue.Cosacs.Shared.Services;
using Blue.Cosacs.Shared.Services.StoreCard;
using STL.Common;
using STL.Common.Constants.AccountTypes;
using STL.Common.Constants.ColumnNames;
using STL.PL.StoreCard.Common;
using STL.PL.StoreCard;
using STL.PL.Utils;
using Blue.Cosacs.Shared.Extensions;
using STL.Common.Constants.FTransaction;

namespace STL.PL
{

    public partial class Payment : CommonForm
    {
        StoreCardValidated storeCardValidated;

        StoreCardPaymentCalc paymentCalc;
        //StoreCardCardReader CardReader = new StoreCardCardReader();

        const string StoreCard = "StoreCard";
        const string DefaultPaymentFilter = CN.Additional + " in ('B', 'P', '') and code <> 0";
        const string AdditionalPaymentFilterStoreCard = " and " + CN.CodeDescript + " not in ('StoreCard')";

        //View_StoreCardAll cardInfo = null;


        //private void StoreCardButtonEnable(decimal? storeCardAvailable)
        //{
        //    if (storeCardAvailable > 0)
        //    {
        //        string path = "";
        //         Load the Storedcard icon from the server
        //        if (LoadServerIcon("StoreCard_icon.gif", out path))
        //        {
        //             replace with the image
        //            storecardimage.Image = Image.FromFile(path);
        //            storecardimage.BringToFront();
        //            storecardimage.SizeMode = PictureBoxSizeMode.Zoom;
        //            storecardimage.Visible = true;
        //            lAddTo.Visible = false;
        //            textStorecardBalance.Text = storeCardAvailable.Value.ToString(DecimalPlaces)?? Convert.ToString(0);

        //            textStorecardBalance.Visible = true;
        //            textStorecardBalance.BringToFront();

        //             ((DataTable)drpPayMethod.DataSource).DefaultView.RowFilter = DefaultPaymentFilter; //IP - 29/11/10 - Replaced with below due to error
        //            ((DataView)drpPayMethod.DataSource).RowFilter = DefaultPaymentFilter;

        //        }
        //    }
        //    else
        //    {
        //        storecardimage.Visible = false;
        //        storecardimage.SendToBack();
        //        lAddTo.Visible = true;
        //        textStorecardBalance.Visible = false;
        //        ((DataTable)drpPayMethod.DataSource).DefaultView.RowFilter = DefaultPaymentFilter + AdditionalPaymentFilterStoreCard;
        //    }
        //}

        private void StoreCardCheckAccountType(string AccountType)
        {
            ((DataView)drpPayMethod.DataSource).RowFilter = ""; //IP - 29/11/10 - Need to re-set the filter before applying new filter

            if ((bool)Country[CountryParameterNames.StoreCardEnabled])
            {
                if (AccountType != AT.Cash)
                {
                    //((DataTable)drpPayMethod.DataSource).DefaultView.RowFilter = DefaultPaymentFilter + AdditionalPaymentFilterStoreCard; //IP - 29/11/10 - Replaced with below due to error

                    ((DataView)drpPayMethod.DataSource).RowFilter = DefaultPaymentFilter + AdditionalPaymentFilterStoreCard;

                }
                else //IP - 24/12/10 - Bug #2669 - Apply the normal filter for all other accounts
                {
                    ((DataView)drpPayMethod.DataSource).RowFilter = DefaultPaymentFilter;
                }
            }
        }

        //IP - 11/01/11 - Commented out for now as incorporated into existing method to save payment.
        //private void SaveStoreCardTransfer(string acctno, short branchcode, decimal amountPaid)
        //{
        //    string err;
        //    AccountManager.TransferTransaction(StoreCardAcctno, acctno, TransType.StoreCardPayment, amountPaid, branchcode, Config.CountryCode, ServerTime.Request(), "", 0, 1, Convert.ToInt64(mtb_storecard.Text.Replace("-","")), out err); //IP - 29/11/10 - Store Card - added agrmtno of 1
        //    StoreCardManager.AccountUpdateOutstandingBalance(acctno);
        //    StoreCardAvailable -= amountPaid;
        //}

        //private void GetStoreCardAccountandBalance(string custid)
        //{
        //    decimal? storeCardLimit = 0;
        //    decimal? storeCardAvailable = 0;

        //    StoreCardManager.StoreCardBalanceCustid(custid, out storeCardLimit, out storeCardLimit);

        //    cardInfo.StoreCardLimit = storeCardLimit;
        //    cardInfo.StoreCardAvailable = storeCardAvailable;
          
        //}

        //IP - 02/12/10 - Store Card
        //private void StoreCardBalanceFromCardNo()
        //{
        //    decimal? storeCardLimit = 0;
        //    decimal? storeCardAvailable = 0;

        //    StoreCardManager.StoreCardBalanceStoreCard(Convert.ToInt64(mtb_Cardno.Text.Replace("-", "")), out storeCardLimit, out storeCardAvailable);

        //    cardInfo.StoreCardLimit = storeCardLimit;
        //    cardInfo.StoreCardAvailable = storeCardAvailable;
        //}

        //IP - 11/01/10 - Store Card - Method to return a StoreCard object that contains all the required details.
        //private void GetStoreCardDetails()
        //{
            
        //}

        //private void ResetStoreCardValues()
        //{
        //    cardInfo = null;
        
        //    textStorecardBalance.Text = string.Empty; //IP - 02/12/10 - Store Card
        //    StoreCardValidated = false;
        //    errorProvider1.SetError(mtb_CardNo, "");
        //}

        //private void StoreCardBoxOn(bool on)
        //{
        //    txtCardNo.Visible = !on;
        //    lCardNo.Visible = !on;

        //    mtb_CardNo.Visible = on;
        //    lblStoreCardNo.Visible = on;
        //}

        ////IP - 29/11/10 - Store Card
        //private bool ValidateStorecard()
        //{
        //    //Static method that validates the store card number entered.
        //    var result = StoreCardValidation.ValidateCardNumber(mtb_storecard.Text);

        //    if (result.isValid)
        //    {
        //        //IP - 10/01/11 
        //        //StoreCardNo = Convert.ToInt64(mtb_storecard.Text.Replace("-",""));

        //        errorProvider1.SetError(mtb_storecard, "");
        //        //If the card number is valid then retrieve the details of the Store Card from the db
        //        cardInfo = StoreCardCommon.ConvertStoreCard(StoreCardManager.GetStoreCardAll(Convert.ToInt64(mtb_storecard.Text.Replace("-",""))));

        //        //Process other validation on the store card details return
        //        var valResult = StoreCardValidation.VaildateCard(cardInfo);

        //        result.isValid = valResult.isValid;

        //        //IP - 06/12/10 
        //        if(!result.isValid)
        //        {
        //            errorProvider1.SetError(mtb_storecard, valResult.Message);
        //            ResetStoreCardValues();
        //        }
        //    }
        //    else
        //    {
        //        errorProvider1.SetError(mtb_storecard, result.Message);
        //        ResetStoreCardValues();

        //    }

        //    return result.isValid;
        //}

        //private View_StoreCardWithPayments ConvertStoreCard(STL.PL.WSStoreCard.View_StoreCardAll scp)
        //{
        //    return new View_StoreCardWithPayments()
        //    {
        //        custid = scp.custid,
        //        CardNumber = scp.CardNumber,
        //        ExpirationMonth = scp.ExpirationMonth,
        //        ExpirationYear = scp.ExpirationYear
        //    };
        //}

        //IP - 03/12/10 - Store Card - Check if any items values have exceeded the max item value that can be purchased on a Store Card
        private void CheckMaxItemVal()
        {
              StringBuilder sb = new StringBuilder();

            var selectedAcct = (((DataView)dgAccountList.DataSource)[dgAccountList.CurrentRowIndex].Row[CN.AccountNumber]).ToString();
            var maxItemVal = Convert.ToInt32(Country[CountryParameterNames.MaxItemValStoreCard]);

            //Retrieve the lineitems on the selected account
            DataSet itemsDs = AccountManager.GetItemsForAccount(selectedAcct, out error);
         
            itemsStoreCardView = new DataView(itemsDs.Tables[0], "ItemType = 'S'", "", DataViewRowState.OriginalRows);
            sb.Append("Item(s): ");
            string sep = string.Empty;
            foreach (DataRowView row in itemsStoreCardView)
            {
                if (Convert.ToDecimal(row[CN.Price]) > maxItemVal)
                {
                    sb.Append(String.Format("{0}{1} ", sep, row[CN.ItemNo]));
                    sep = ",";
                }
            }
            sb.Append("have exceeded the max value of " + maxItemVal.ToString() + " Please remove the item(s) before completing the sale.");

            if (sep != string.Empty)
            {
                errorProviderStoreCard.SetError(drpPayMethod, sb.ToString());
            }
            else
            {
                errorProviderStoreCard.SetError(drpPayMethod, "");
            }
        }

        private void ValidateStoreCard()
        {
            if (!StoreCardValidation.IsStoreCardValid(mtb_CardNo.Text))
                errorProviderStoreCard.SetError(btnStoreCardManualEntry, "This is not a valid storecard number. Please enter number again.");
            else
            {
                Client.Call(new GetValidateCardRequest()
                {
                    CardNo = Convert.ToInt64(mtb_CardNo.Text)
                },
                response =>
                {
                    storeCardValidated = response.StoreCardValidated;
                    ValidateResponse(response.StoreCardValidated.Name, response.StoreCardValidated.Valid, response.StoreCardValidated.RejectReason);
                },
                this);
            }
           
        }

        private void SetStoreCardControls(bool set)
        {
            if (set)
            {
                //   StoreCardBoxOn(true);
                CheckMaxItemVal();  //IP - 03/12/10 - Store Card
                lBankAcctNo.Text = "Store Card Name";
                mtb_CardNo.Enabled = true;        
                mtb_CardNo.Visible = true;
                mtb_CardNo.Mask = "0000-0000-0000-0000";
                btnPay.Enabled = false;
                txtBankAcctNo.Enabled = true;
                txtBankAcctNo.ReadOnly = true;
                lBankAcctNo.Enabled = true;
                lCardNo.Enabled = true;
                txtBankAcctNo.ReadOnly = true;
                txtBankAcctNo.BackColor = Control.DefaultBackColor;
                btnStoreCardManualEntry.Visible = mtb_CardNo.ReadOnly = ((bool)Country[CountryParameterNames.StoreCardPasswordforManualEntry]);
                btnStoreCardManualEntry.Enabled = true;
            }
            else
            {
                btnStoreCardManualEntry.Enabled= false;
                mtb_CardNo.Mask = "XXXX-XXXX-XXXX-0000";
                //IP - 03/12/10 - Store Card
                errorProviderStoreCard.SetError(drpPayMethod, "");
                    itemsStoreCardView = null;
                lBankAcctNo.Text = "Bank Account No";
                txtBankAcctNo.ReadOnly = false;
                txtBankAcctNo.BackColor = Color.White;
            }
        }

        private void ValidateResponse(string name, bool valid, string reason)
        {
            errorProviderStoreCard.SetError(btnStoreCardManualEntry, reason);
            if (valid)
            {
                txtBankAcctNo.Text = name;
                txtBankAcctNo.ReadOnly = true;
                txtBankAcctNo.BackColor = Control.DefaultBackColor;
                txtFee.Text = storeCardValidated.StoreCardAvailable.Value.ToString(DecimalPlaces);
                //btn_calc.Enabled = true;          // #9859 only enable Calculator if selecting Storecard account  jec 11/04/12
            }
            StoreCardCheckPayButton();
        }


        private void StoreCardCheckPayButton()
        {
            if (storeCardValidated != null && storeCardValidated.Valid)
            {
                if (MoneyStrToDecimal(txtPayAmount.Text) > MoneyStrToDecimal(txtAgreementTotal.Text))
                    txtCardNo.Text = txtPayAmount.Text = txtAgreementTotal.Text;
                if (MoneyStrToDecimal(txtPayAmount.Text) > storeCardValidated.StoreCardAvailable)
                    errorProviderStoreCard.SetError(txtPayAmount, String.Format("Available Store Card credit ({0:0.00}) is less than amount to pay.", storeCardValidated.StoreCardAvailable));
                else
                    errorProviderStoreCard.SetError(txtPayAmount, string.Empty);
            }
            else
            {
                if (errorProviderStoreCard.GetError(btnStoreCardManualEntry) == string.Empty)
                    errorProviderStoreCard.SetError(btnStoreCardManualEntry, "Please enter a valid storecard.");
            }

            btnPay.Enabled = errorProviderStoreCard.GetError(drpPayMethod) == string.Empty &&
                             errorProviderStoreCard.GetError(btnStoreCardManualEntry) == string.Empty &&
                               errorProviderStoreCard.GetError(txtPayAmount) == string.Empty &&
                               errorProviderStoreCard.GetError(drpPayMethod) == string.Empty;       // #10126
               
        }

        private void SetFeeforStoreCard(bool set, bool? DisplayRF = null)
        { //
            mtb_CardNo.ReadOnly = btnStoreCardManualEntry.Enabled = set && ((bool)Country[CountryParameterNames.StoreCardPasswordforManualEntry]);

            if (btnStoreCardManualEntry.Visible == false)
                mtb_CardNo.ReadOnly = false;
            if (set)
            {
                txtFee.Enabled = false;
                txtFee.Visible = true;
                lFee.Visible = true;
                lFee.Text = "StoreCard Balance";
            }
            else
            {
                lFee.Text = "Credit Fee";
                if (DisplayRF.HasValue)
                    this.txtFee.Enabled = !DisplayRF.Value;

                this.txtFee.Visible = this._debitFee;
                this.lFee.Visible = this._debitFee;
            }
        }

        private void btn_calc_Click(object sender, EventArgs e)
        {
            if (storeCardValidated != null)
                paymentCalc = new StoreCardPaymentCalc(Math.Round(storeCardValidated.StoreCardBalance.Value + Convert.ToDecimal(txtPayAmount.Text.StripNonNumeric().ToDecimal()), 2).ToString(),
                    storeCardValidated.StoreCardInterest.ToString(), txtAccountNo.Text.Replace("-", ""), Country[CountryParameterNames.DecimalPlaces].ToString());
            paymentCalc.ShowDialog();
        }

        private void CheckPaymentCalc(int index)
        {
            if (((DataView)dgAccountList.DataSource)[index][CN.AccountType].ToString().Trim() == "T")
            {
                Client.Call(new GetInterestRequest
                {
                    acctno = ((DataView)dgAccountList.DataSource)[index][CN.AcctNo].ToString()
                },
               response => 
               {
                   paymentCalc = new StoreCardPaymentCalc(((DataView)dgAccountList.DataSource)[index][CN.OutstandingBalance].ToString(),
                       //response.Interest.ToString(), txtAccountNo.Text.Replace("-", ""), Country[CountryParameterNames.DecimalPlaces].ToString());
                       response.Interest.ToString(), ((DataView)dgAccountList.DataSource)[index][CN.AcctNo].ToString(), Country[CountryParameterNames.DecimalPlaces].ToString(),
                       response.MinimumPayment);   // #9859 jec 12/04/12
                       btn_calc.Enabled = true;
               }
            , this);
                
                
            }
            else
                btn_calc.Enabled = false;

        }

        public bool IsStoreCardPaymentSelected()
        {
            if (Convert.ToInt16(drpPayMethod.SelectedValue) == PayMethod.StoreCard)
                return true;
            else
                return false;
        }

        
    }
}
