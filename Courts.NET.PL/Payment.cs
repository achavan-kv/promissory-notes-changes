using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Blue.Cosacs.Client;
using Blue.Cosacs.Shared;
using Crownwood.Magic.Menus;
using Microsoft.PointOfService;
using SHDocVw;
using STL.Common;
using STL.Common.Constants.AccountTypes;
using STL.Common.Constants.Categories;
using STL.Common.Constants.ColumnNames;
using STL.Common.Constants.EmployeeTypes;
using STL.Common.Constants.FTransaction;
using STL.Common.Constants.TableNames;
using STL.Common.Static;
using Blue.Cosacs.Shared.Services;
using Blue.Cosacs.Shared.Services.Warranty;



namespace STL.PL
{
    /// <summary>
    /// Customer payment screen to credit manual payments to customer accounts.
    /// This screen can load by an account number or by a customer id. All unsettled
    /// accounts for the customer are listed. All Ready Finance agreements are grouped 
    /// together so that the payment amount is spread proportionately across a set
    /// of Ready Finance agreements. The proportion is based upon the normal instalment
    /// amount on each account. Account details are displayed including arrears,
    /// bailiff allocation and past transctions. Fees are automatically calculated
    /// for an allocated account that is in arrears. Payments can be taken as a variety
    /// of payment methods and a foreign exchange calculator allows conversion from
    /// foreign currency. Printing options allow a receipt to be printed or a 
    /// customer payment card to be updated.
    /// </summary>
    public partial class Payment : CommonForm
    {
        private string error = "";

        // Change event control
        private bool _userChanged = false;
        private string _lastCustId = "";
        private string _lastAccountNo = "";
        private bool _lastCombinedRF = false;
        private string _lastReceiptNo = "";
        private string _lastPayMethod = "";
        private decimal _lastCardRowNo = 1;
        private decimal _lastTotalAmount = -1;
        private decimal _lastFee = 0;
        private decimal _lastPayAmount = 0;
        private decimal _initPayAmount = 0;
        private int _precision = 2;


        // current customer and account data
        private string _blankAcctNo = "000-0000-0000-0";
        private bool _curSundryCredit = false;
        private DataRow _curAccount = null;
        private DataSet _curRFTransactionSet = null;
        private DataSet _curAccountTransactionSet = null;
        private DataSet _printTransactionSet = null;
        private DataSet _paymentSet = null;
        private DataRow _curReceipt = null;
        private DataSet _paymentHolidays = null;
        private bool _debitFee = false;
        private decimal _calculatedFee = 0;
        private DateTime _curChequeClearance = DateTime.MinValue;
        private bool _combinedRF = false;
        private bool _slipRequired = true;
        private decimal _addToAmount = 0;
        private decimal _availableSpend = 0;
        private int _authorisedBy = 0;
        private int _returnedChequeAuthorisedBy = 0; //CR 543 Added Peter Chong [26-Sep-2006]
        private DateTime dateNow = System.DateTime.Now;     //CR1084
        private bool popAdditionalInfo = false;  //CR1084
        private string AdditionalNotes = "";

        private bool popCustLeftAddr = false;  //CR1084
        private bool popAcctsInArrears = false;  //CR1084
        private bool popPhoneOutOfService = false;  //CR1084

        private DataView itemsStoreCardView; //IP - 03/12/10 - Store Card
        //private bool itemValExceeded;        //IP - 03/12/10 - Store Card


        public int VoucherAuthorisedBy = 0;
        public string VoucherCompanyAcctNo = "";
        public bool CourtsVoucher = true;
        public string VoucherReference = "";
        private decimal _giftVoucherValue = 0;
        public decimal GiftVoucherValue
        {
            get
            {
                return _giftVoucherValue;
            }
            set
            {
                _giftVoucherValue = value;
            }
        }

        private int PaymentCardType = 0;

        private string m_fileName = String.Empty;

        //CR855
        public string fileName
        {
            get
            {
                return m_fileName;
            }
            set
            {
                m_fileName = value;
            }
        }


        //NM Dispose problme when closed within load method
        public bool AvoidControlDispose { get; private set; }
        public delegate void InvokeDelegate();


        // Form data
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnClear;
        private STL.PL.AddressTab addressTab1;
        private System.Windows.Forms.TextBox txtDateFInstalment;
        private System.Windows.Forms.TextBox txtCurrentStatus;
        private System.Windows.Forms.TextBox txtArrears;
        private System.Windows.Forms.TextBox txtOutstandingBalance;
        private System.Windows.Forms.TextBox txtAgreementTotal;
        private System.Windows.Forms.TextBox txtRebate;
        private System.Windows.Forms.Label lPrivilegeClub;
        private System.Windows.Forms.TextBox txtCustomerName;
        private System.Windows.Forms.Label lFullyDelivered;
        private Crownwood.Magic.Controls.TabControl tcAccount;
        private System.Windows.Forms.TextBox txtToFollow;
        private System.Windows.Forms.TextBox txtInstalment;
        private System.Windows.Forms.TextBox txtSettlement;
        private System.Windows.Forms.Label lArrears;
        private System.Windows.Forms.Label lAcctNo;
        private System.Windows.Forms.Label lCombinedRF;
        public System.Windows.Forms.TextBox txtSelectedAcctNo;
        private Crownwood.Magic.Menus.MenuControl menuControl1;
        private System.Windows.Forms.Label lSettlement;
        private System.Windows.Forms.Label lRebate;
        private System.Windows.Forms.TextBox txtReceiptNo;
        private System.Windows.Forms.TextBox txtTotalAmount;
        private System.Windows.Forms.TextBox txtFee;
        private Crownwood.Magic.Controls.TabPage tpDetails;
        private Crownwood.Magic.Controls.TabPage tpAllocation;
        private Crownwood.Magic.Controls.TabPage tpTransactions;
        private System.Windows.Forms.TextBox txtPayAmount;
        private System.Windows.Forms.TextBox txtChange;
        private System.Windows.Forms.TextBox txtTendered;
        private System.Windows.Forms.Label lTotalAmount;
        private System.Windows.Forms.Label lFee;
        private System.Windows.Forms.Label lChange;
        private System.Windows.Forms.ComboBox drpPayMethod;
        private System.Windows.Forms.ComboBox drpBank;
        private System.Windows.Forms.TextBox txtBankAcctNo;
        private System.Windows.Forms.ComboBox drpCardType;
        private System.Windows.Forms.Label lCardType;
        private System.Windows.Forms.Label lBankAcctNo;
        private System.Windows.Forms.Label lBank;
        private System.Windows.Forms.Label lCardNo;
        private System.Windows.Forms.TextBox txtCardNo;
        private System.Windows.Forms.Button btnPrintAcctNo;
        private System.Windows.Forms.Label lAddTo;
        private System.Windows.Forms.Label lCustomerId;
        private System.Windows.Forms.Button btnPay;
        private System.Windows.Forms.Label lTendered;
        private System.Windows.Forms.Label lPayMethod;
        private System.Windows.Forms.Label lPaymentAmount;
        private System.Windows.Forms.Label lReceiptNo;
        private System.Windows.Forms.Label lToFollow;
        private System.Windows.Forms.Label lAccountStatus;
        private System.Windows.Forms.Label lDueDate;
        private System.Windows.Forms.Label lOutstandingBalance;
        private System.Windows.Forms.Label lInstalmentAmount;
        private System.Windows.Forms.Label lAgreementTotal;
        private System.Windows.Forms.CheckBox cbNonPrinted;
        private System.Windows.Forms.GroupBox gbCustomer;
        private System.Windows.Forms.GroupBox gbAccountList;
        private System.Windows.Forms.GroupBox gbNewPayment;
        private System.Windows.Forms.GroupBox gbSelectedAccount;
        private System.Windows.Forms.DataGrid dgTransactionList;
        private System.Windows.Forms.Label lTransactionCount;
        private Crownwood.Magic.Menus.MenuCommand menuFile;
        private STL.PL.DataGridCellTips dgAccountList;
        private System.Windows.Forms.ImageList imageList1;
        public STL.PL.AccountTextBox txtAccountNo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCopyInstallment;
        private System.Windows.Forms.Button btnCopyAgreementTotal;
        private Crownwood.Magic.Menus.MenuCommand menuPrint;
        private Crownwood.Magic.Menus.MenuCommand menuTaxInvoice;
        private Crownwood.Magic.Menus.MenuCommand menuStatement;
        private System.Windows.Forms.ToolTip ttPayment;
        private System.Windows.Forms.Button btnPaymentList;
        private Crownwood.Magic.Menus.MenuCommand menuPaymentCard;
        private System.Windows.Forms.Label lAuthorise;
        private System.Windows.Forms.TextBox txtAddToAmount;
        private System.Windows.Forms.Label lFreeInstalment;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Button btnExchange;
        private System.Windows.Forms.TextBox txtBadDebtBalance;
        private System.Windows.Forms.TextBox txtBadDebtCharges;
        private System.Windows.Forms.Label lBadDebtBalance;

        private Crownwood.Magic.Menus.MenuCommand menuCashTill;
        private Crownwood.Magic.Menus.MenuCommand menuWarrantRenewals;
        private Crownwood.Magic.Menus.MenuCommand menuOpenCashTill;
        private Crownwood.Magic.Controls.TabPage tpPaymentHolidays;
        private System.Windows.Forms.Button btnPaymentHoliday;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNewDueDate;
        private System.Windows.Forms.NumericUpDown numPaymentHolidaysLeft;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGrid dgPaymentHolidays;
        private System.Windows.Forms.CheckBox chxPaymentHolidayCancelled;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numMinimumPayments;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtTotalArrears;
        private System.Windows.Forms.TextBox txtNextPaymentDue;
        private Crownwood.Magic.Menus.MenuCommand menuPaymentCardDueDate;
        private System.Windows.Forms.Label lNextPayDue;
        public System.Windows.Forms.TextBox txtCustId;
        private System.Windows.Forms.Label lAccountNo;
        private System.Windows.Forms.GroupBox gbCourtsAlloc;
        private System.Windows.Forms.Label lEmpType;
        private System.Windows.Forms.Label lEmpName;
        private System.Windows.Forms.TextBox txtEmployeeName;
        private System.Windows.Forms.TextBox txtEmployeeType;
        private System.Windows.Forms.TextBox txtEmployeeNo;
        private System.Windows.Forms.Label lEmployeeNo;
        private System.Windows.Forms.GroupBox gbTallymanAlloc;
        private System.Windows.Forms.TextBox txtSegmentId;
        private System.Windows.Forms.Label lSegmentId;
        private System.Windows.Forms.Label lTotalArrears;
        private Crownwood.Magic.Menus.MenuCommand menuCheckExpiringWarranties;
        private System.Windows.Forms.Button btnSearchAccount;
        private Crownwood.Magic.Menus.MenuCommand menuHelp;
        private Crownwood.Magic.Menus.MenuCommand menuLaunchHelp;
        private Label lServiceRequestNo;
        private MenuCommand menuSESPopup;
        private MenuCommand menuSES;
        private Button button1;
        private GroupBox gbSPA;
        private TextBox txtExpiryDate;
        private TextBox txtSPAInstalment;
        private Label label7;
        private Label label6;
        private Label Loyalty_lbl;
        private PictureBox LoyaltyLogo_pb;
        private ErrorProvider errorProvider1;
        private Button btnLaunchTally;
        private TextBox txtSegmentName;
        private Label label8;
        private TextBox txtAgrmtNo;
        private Label lblAgrmtNo;
        private System.Windows.Forms.Label lBadDebtCharges;
        private GroupBox gbPromiseToPay;
        private Label label10;
        private Label label9;
        private TextBox txtPTPDueDate;
        private TextBox txtPTPAmount;
        private GroupBox gbBadDebt;
        public string SrNo = string.Empty; //IP - UAT5.1.9.0 - UAT(9) - Initialised variable.



        private bool custLeftAddress;       //CR1084
        private bool additionalInfo;       //CR1084
        private bool telNotInService;
        private Button btnCustomerDetails;
        private PictureBox storecardimage;
        private TextBox textStorecardBalance;
        private ErrorProvider errorProviderStoreCard; //CR1084#
        private MaskedTextBox mtb_CardNo;
        private Button btn_calc;
        private Button btnStoreCardManualEntry;
        private bool otherArrears;
        private Label lblCashLoan;
        private CheckBox cbMiniStat;
        private Label lblMiniStat;       //CR1084
        public StoreCardMagStripeReader MagStripeReader;

        /// <summary>
        // Form Constructors
        /// </summary>
        public Payment(TranslationDummy d)
        {
            InitializeComponent();
            /***************************************************************************************************************************************************
            //this.addressTab1 = new STL.PL.AddressTab(FormRoot);             //CR1084 this gets deleted from InitializeComponent() when change made to designer
             ***************************************************************************************************************************************************/
            menuMain = new Crownwood.Magic.Menus.MenuControl();
            menuMain.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] { menuFile, menuPrint, menuCashTill, menuWarrantRenewals, menuSESPopup, menuHelp });

            if (IsNumeric(DecimalPlaces.Substring(1, 1)))
            {
                this._precision = System.Convert.ToInt32(DecimalPlaces.Substring(1, 1));
            }

            // Dont ask why
            this.txtAccountNo.Leave += new System.EventHandler(this.txtAccountNo_Leave);
            //this.txtCustId.Leave += new System.EventHandler(this.txtCustId_Leave);

            AvoidControlDispose = false;


        }

        public Payment(string piCustId, string piAcctNo, decimal piPayAmount, Form root, Form parent)
        {
            dynamicMenus = new Hashtable();
            FormRoot = root;
            FormParent = parent;
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();


            menuMain = new Crownwood.Magic.Menus.MenuControl();
            menuMain.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] { menuFile, menuPrint, menuCashTill, menuWarrantRenewals, menuSESPopup, menuHelp });
            txtCustId.Text = piCustId.Trim();
            if (piAcctNo.Trim().Length > 0) txtAccountNo.Text = piAcctNo.Trim();
            if (piPayAmount > 0) this._initPayAmount = piPayAmount;

            if (IsNumeric(DecimalPlaces.Substring(1, 1)))
            {
                this._precision = System.Convert.ToInt32(DecimalPlaces.Substring(1, 1));
            }

            // Dont ask why
            this.txtAccountNo.Leave += new System.EventHandler(this.txtAccountNo_Leave);
            //this.txtCustId.Leave += new System.EventHandler(this.txtCustId_Leave);
            gbCustomer.Controls.Add(this.addressTab1);

            HashMenus();
            ApplyRoleRestrictions();

            // set visibility for tallyman link - Malaysia Merge
            bool linkToTallyman = Convert.ToBoolean(Country[CountryParameterNames.LinkToTallyman]);
            //Active Action Popups
            popAdditionalInfo = Convert.ToBoolean(Country[CountryParameterNames.PopUpAdditInfo]);  //CR1084
            popCustLeftAddr = Convert.ToBoolean(Country[CountryParameterNames.PopUpCustomerLeftAddr]);  //CR1084
            popAcctsInArrears = Convert.ToBoolean(Country[CountryParameterNames.PopUpAcctsInArrears]);  //CR1084
            popPhoneOutOfService = Convert.ToBoolean(Country[CountryParameterNames.PopUpPhoneOutOfService]);  //CR1084

            txtSegmentName.Visible = linkToTallyman;
            lSegmentId.Visible = linkToTallyman;
            btnLaunchTally.Visible = linkToTallyman;

            //CR855
            // Determine if Show Photograph button should be enabled
            // This method is no longer required since the constructor is not passed any customer or account details
            //EnablePhotographButton();

            btnCustomerDetails.Enabled = Credential.HasPermission(Blue.Cosacs.Shared.CosacsPermissionEnum.PaymentsCustomerSearch);
            AvoidControlDispose = false;
        }

        private void HashMenus()
        {
            dynamicMenus[this.Name + ":menuWarrantRenewals"] = this.menuWarrantRenewals;
            dynamicMenus[this.Name + ":btnCustomerDetails"] = this.btnCustomerDetails;               //CR1084 UAT29
        }


        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Payment));
            this.gbCustomer = new System.Windows.Forms.GroupBox();
            this.storecardimage = new System.Windows.Forms.PictureBox();
            this.btnCustomerDetails = new System.Windows.Forms.Button();
            this.Loyalty_lbl = new System.Windows.Forms.Label();
            this.LoyaltyLogo_pb = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lServiceRequestNo = new System.Windows.Forms.Label();
            this.lPrivilegeClub = new System.Windows.Forms.Label();
            this.btnSearchAccount = new System.Windows.Forms.Button();
            this.lTotalArrears = new System.Windows.Forms.Label();
            this.txtCustId = new System.Windows.Forms.TextBox();
            this.txtAddToAmount = new System.Windows.Forms.TextBox();
            this.txtNextPaymentDue = new System.Windows.Forms.TextBox();
            this.txtCustomerName = new System.Windows.Forms.TextBox();
            this.lNextPayDue = new System.Windows.Forms.Label();
            this.txtTotalArrears = new System.Windows.Forms.TextBox();
            this.lAuthorise = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtAccountNo = new STL.PL.AccountTextBox();
            this.lAccountNo = new System.Windows.Forms.Label();
            this.lAddTo = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lCustomerId = new System.Windows.Forms.Label();
            this.textStorecardBalance = new System.Windows.Forms.TextBox();
            this.gbAccountList = new System.Windows.Forms.GroupBox();
            this.dgAccountList = new STL.PL.DataGridCellTips();
            this.gbNewPayment = new System.Windows.Forms.GroupBox();
            this.cbMiniStat = new System.Windows.Forms.CheckBox();
            this.lblMiniStat = new System.Windows.Forms.Label();
            this.lblCashLoan = new System.Windows.Forms.Label();
            this.btnStoreCardManualEntry = new System.Windows.Forms.Button();
            this.btn_calc = new System.Windows.Forms.Button();
            this.btnExchange = new System.Windows.Forms.Button();
            this.mtb_CardNo = new System.Windows.Forms.MaskedTextBox();
            this.btnPay = new System.Windows.Forms.Button();
            this.drpCardType = new System.Windows.Forms.ComboBox();
            this.lCardType = new System.Windows.Forms.Label();
            this.btnPrintAcctNo = new System.Windows.Forms.Button();
            this.drpBank = new System.Windows.Forms.ComboBox();
            this.drpPayMethod = new System.Windows.Forms.ComboBox();
            this.lBankAcctNo = new System.Windows.Forms.Label();
            this.txtBankAcctNo = new System.Windows.Forms.TextBox();
            this.lBank = new System.Windows.Forms.Label();
            this.lCardNo = new System.Windows.Forms.Label();
            this.txtCardNo = new System.Windows.Forms.TextBox();
            this.lChange = new System.Windows.Forms.Label();
            this.txtChange = new System.Windows.Forms.TextBox();
            this.lTendered = new System.Windows.Forms.Label();
            this.txtTendered = new System.Windows.Forms.TextBox();
            this.lPayMethod = new System.Windows.Forms.Label();
            this.lPaymentAmount = new System.Windows.Forms.Label();
            this.txtPayAmount = new System.Windows.Forms.TextBox();
            this.lTotalAmount = new System.Windows.Forms.Label();
            this.txtTotalAmount = new System.Windows.Forms.TextBox();
            this.lReceiptNo = new System.Windows.Forms.Label();
            this.txtReceiptNo = new System.Windows.Forms.TextBox();
            this.lFee = new System.Windows.Forms.Label();
            this.txtFee = new System.Windows.Forms.TextBox();
            this.gbSelectedAccount = new System.Windows.Forms.GroupBox();
            this.txtAgrmtNo = new System.Windows.Forms.TextBox();
            this.txtSelectedAcctNo = new System.Windows.Forms.TextBox();
            this.lAcctNo = new System.Windows.Forms.Label();
            this.tcAccount = new Crownwood.Magic.Controls.TabControl();
            this.tpDetails = new Crownwood.Magic.Controls.TabPage();
            this.gbPromiseToPay = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPTPDueDate = new System.Windows.Forms.TextBox();
            this.txtPTPAmount = new System.Windows.Forms.TextBox();
            this.gbBadDebt = new System.Windows.Forms.GroupBox();
            this.txtBadDebtBalance = new System.Windows.Forms.TextBox();
            this.txtBadDebtCharges = new System.Windows.Forms.TextBox();
            this.lBadDebtCharges = new System.Windows.Forms.Label();
            this.lBadDebtBalance = new System.Windows.Forms.Label();
            this.btnPaymentList = new System.Windows.Forms.Button();
            this.txtSegmentName = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.btnLaunchTally = new System.Windows.Forms.Button();
            this.gbSPA = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtExpiryDate = new System.Windows.Forms.TextBox();
            this.txtSPAInstalment = new System.Windows.Forms.TextBox();
            this.lFreeInstalment = new System.Windows.Forms.Label();
            this.btnCopyAgreementTotal = new System.Windows.Forms.Button();
            this.btnCopyInstallment = new System.Windows.Forms.Button();
            this.lToFollow = new System.Windows.Forms.Label();
            this.txtToFollow = new System.Windows.Forms.TextBox();
            this.lFullyDelivered = new System.Windows.Forms.Label();
            this.lAccountStatus = new System.Windows.Forms.Label();
            this.lDueDate = new System.Windows.Forms.Label();
            this.txtDateFInstalment = new System.Windows.Forms.TextBox();
            this.txtCurrentStatus = new System.Windows.Forms.TextBox();
            this.lSettlement = new System.Windows.Forms.Label();
            this.txtArrears = new System.Windows.Forms.TextBox();
            this.txtOutstandingBalance = new System.Windows.Forms.TextBox();
            this.lArrears = new System.Windows.Forms.Label();
            this.lOutstandingBalance = new System.Windows.Forms.Label();
            this.txtAgreementTotal = new System.Windows.Forms.TextBox();
            this.txtRebate = new System.Windows.Forms.TextBox();
            this.txtSettlement = new System.Windows.Forms.TextBox();
            this.lInstalmentAmount = new System.Windows.Forms.Label();
            this.lRebate = new System.Windows.Forms.Label();
            this.lAgreementTotal = new System.Windows.Forms.Label();
            this.txtInstalment = new System.Windows.Forms.TextBox();
            this.tpAllocation = new Crownwood.Magic.Controls.TabPage();
            this.gbTallymanAlloc = new System.Windows.Forms.GroupBox();
            this.txtSegmentId = new System.Windows.Forms.TextBox();
            this.lSegmentId = new System.Windows.Forms.Label();
            this.gbCourtsAlloc = new System.Windows.Forms.GroupBox();
            this.lEmpType = new System.Windows.Forms.Label();
            this.lEmpName = new System.Windows.Forms.Label();
            this.txtEmployeeName = new System.Windows.Forms.TextBox();
            this.txtEmployeeType = new System.Windows.Forms.TextBox();
            this.txtEmployeeNo = new System.Windows.Forms.TextBox();
            this.lEmployeeNo = new System.Windows.Forms.Label();
            this.tpTransactions = new Crownwood.Magic.Controls.TabPage();
            this.lTransactionCount = new System.Windows.Forms.Label();
            this.cbNonPrinted = new System.Windows.Forms.CheckBox();
            this.dgTransactionList = new System.Windows.Forms.DataGrid();
            this.tpPaymentHolidays = new Crownwood.Magic.Controls.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chxPaymentHolidayCancelled = new System.Windows.Forms.CheckBox();
            this.numMinimumPayments = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.numPaymentHolidaysLeft = new System.Windows.Forms.NumericUpDown();
            this.btnPaymentHoliday = new System.Windows.Forms.Button();
            this.txtNewDueDate = new System.Windows.Forms.TextBox();
            this.dgPaymentHolidays = new System.Windows.Forms.DataGrid();
            this.lblAgrmtNo = new System.Windows.Forms.Label();
            this.lCombinedRF = new System.Windows.Forms.Label();
            this.menuControl1 = new Crownwood.Magic.Menus.MenuControl();
            this.menuFile = new Crownwood.Magic.Menus.MenuCommand();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.menuPrint = new Crownwood.Magic.Menus.MenuCommand();
            this.menuTaxInvoice = new Crownwood.Magic.Menus.MenuCommand();
            this.menuPaymentCard = new Crownwood.Magic.Menus.MenuCommand();
            this.menuStatement = new Crownwood.Magic.Menus.MenuCommand();
            this.menuPaymentCardDueDate = new Crownwood.Magic.Menus.MenuCommand();
            this.ttPayment = new System.Windows.Forms.ToolTip(this.components);
            this.menuCashTill = new Crownwood.Magic.Menus.MenuCommand();
            this.menuOpenCashTill = new Crownwood.Magic.Menus.MenuCommand();
            this.menuWarrantRenewals = new Crownwood.Magic.Menus.MenuCommand();
            this.menuCheckExpiringWarranties = new Crownwood.Magic.Menus.MenuCommand();
            this.menuHelp = new Crownwood.Magic.Menus.MenuCommand();
            this.menuLaunchHelp = new Crownwood.Magic.Menus.MenuCommand();
            this.menuSESPopup = new Crownwood.Magic.Menus.MenuCommand();
            this.menuSES = new Crownwood.Magic.Menus.MenuCommand();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.errorProviderStoreCard = new System.Windows.Forms.ErrorProvider(this.components);
            this.gbCustomer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.storecardimage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoyaltyLogo_pb)).BeginInit();
            this.gbAccountList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgAccountList)).BeginInit();
            this.gbNewPayment.SuspendLayout();
            this.gbSelectedAccount.SuspendLayout();
            this.tcAccount.SuspendLayout();
            this.tpDetails.SuspendLayout();
            this.gbPromiseToPay.SuspendLayout();
            this.gbBadDebt.SuspendLayout();
            this.gbSPA.SuspendLayout();
            this.tpAllocation.SuspendLayout();
            this.gbTallymanAlloc.SuspendLayout();
            this.gbCourtsAlloc.SuspendLayout();
            this.tpTransactions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgTransactionList)).BeginInit();
            this.tpPaymentHolidays.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.addressTab1 = new STL.PL.AddressTab(FormRoot);
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumPayments)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPaymentHolidaysLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgPaymentHolidays)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderStoreCard)).BeginInit();
            this.SuspendLayout();
            // 
            // gbCustomer
            // 
            this.gbCustomer.BackColor = System.Drawing.SystemColors.Control;
            this.gbCustomer.Controls.Add(this.storecardimage);
            this.gbCustomer.Controls.Add(this.btnCustomerDetails);
            this.gbCustomer.Controls.Add(this.Loyalty_lbl);
            this.gbCustomer.Controls.Add(this.LoyaltyLogo_pb);
            this.gbCustomer.Controls.Add(this.button1);
            this.gbCustomer.Controls.Add(this.lServiceRequestNo);
            this.gbCustomer.Controls.Add(this.lPrivilegeClub);
            this.gbCustomer.Controls.Add(this.btnSearchAccount);
            this.gbCustomer.Controls.Add(this.lTotalArrears);
            this.gbCustomer.Controls.Add(this.txtCustId);
            this.gbCustomer.Controls.Add(this.txtAddToAmount);
            this.gbCustomer.Controls.Add(this.txtNextPaymentDue);
            this.gbCustomer.Controls.Add(this.txtCustomerName);
            this.gbCustomer.Controls.Add(this.lNextPayDue);
            this.gbCustomer.Controls.Add(this.txtTotalArrears);
            this.gbCustomer.Controls.Add(this.lAuthorise);
            this.gbCustomer.Controls.Add(this.label1);
            this.gbCustomer.Controls.Add(this.txtAccountNo);
            this.gbCustomer.Controls.Add(this.lAccountNo);
            this.gbCustomer.Controls.Add(this.lAddTo);
            this.gbCustomer.Controls.Add(this.btnExit);
            this.gbCustomer.Controls.Add(this.btnClear);
            this.gbCustomer.Controls.Add(this.lCustomerId);
            this.gbCustomer.Controls.Add(this.textStorecardBalance);
            this.gbCustomer.Location = new System.Drawing.Point(8, 0);
            this.gbCustomer.Name = "gbCustomer";
            this.gbCustomer.Size = new System.Drawing.Size(786, 110);
            this.gbCustomer.TabIndex = 39;
            this.gbCustomer.TabStop = false;
            this.gbCustomer.Text = "Customer";
            // 
            // storecardimage
            // 
            this.storecardimage.Location = new System.Drawing.Point(259, 76);
            this.storecardimage.Name = "storecardimage";
            this.storecardimage.Size = new System.Drawing.Size(57, 26);
            this.storecardimage.TabIndex = 60;
            this.storecardimage.TabStop = false;
            this.storecardimage.Visible = false;
            // 
            // btnCustomerDetails
            // 
            this.btnCustomerDetails.Font = new System.Drawing.Font("Segoe UI", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCustomerDetails.Location = new System.Drawing.Point(488, 12);
            this.btnCustomerDetails.Name = "btnCustomerDetails";
            this.btnCustomerDetails.Size = new System.Drawing.Size(48, 32);
            this.btnCustomerDetails.TabIndex = 59;
            this.btnCustomerDetails.Text = "Customer Details";
            this.btnCustomerDetails.UseVisualStyleBackColor = true;
            this.btnCustomerDetails.Click += new System.EventHandler(this.btnCustomerDetails_Click);
            // 
            // Loyalty_lbl
            // 
            this.Loyalty_lbl.AutoSize = true;
            this.Loyalty_lbl.BackColor = System.Drawing.Color.Transparent;
            this.Loyalty_lbl.ForeColor = System.Drawing.Color.Red;
            this.Loyalty_lbl.Location = new System.Drawing.Point(213, 97);
            this.Loyalty_lbl.Name = "Loyalty_lbl";
            this.Loyalty_lbl.Size = new System.Drawing.Size(163, 13);
            this.Loyalty_lbl.TabIndex = 58;
            this.Loyalty_lbl.Text = "Home Club payment outstanding!";
            this.Loyalty_lbl.Visible = false;
            // 
            // LoyaltyLogo_pb
            // 
            this.LoyaltyLogo_pb.Image = ((System.Drawing.Image)(resources.GetObject("LoyaltyLogo_pb.Image")));
            this.LoyaltyLogo_pb.Location = new System.Drawing.Point(415, 47);
            this.LoyaltyLogo_pb.Name = "LoyaltyLogo_pb";
            this.LoyaltyLogo_pb.Size = new System.Drawing.Size(121, 29);
            this.LoyaltyLogo_pb.TabIndex = 57;
            this.LoyaltyLogo_pb.TabStop = false;
            this.LoyaltyLogo_pb.Visible = false;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(415, 80);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 56;
            this.button1.Text = "Show Photograph";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lServiceRequestNo
            // 
            this.lServiceRequestNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lServiceRequestNo.Location = new System.Drawing.Point(215, 78);
            this.lServiceRequestNo.Name = "lServiceRequestNo";
            this.lServiceRequestNo.Size = new System.Drawing.Size(194, 18);
            this.lServiceRequestNo.TabIndex = 23;
            this.lServiceRequestNo.Visible = false;
            // 
            // lPrivilegeClub
            // 
            this.lPrivilegeClub.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lPrivilegeClub.Location = new System.Drawing.Point(686, 83);
            this.lPrivilegeClub.Name = "lPrivilegeClub";
            this.lPrivilegeClub.Size = new System.Drawing.Size(86, 16);
            this.lPrivilegeClub.TabIndex = 0;
            this.lPrivilegeClub.Text = "Privilege Club";
            this.lPrivilegeClub.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lPrivilegeClub.Visible = false;
            // 
            // btnSearchAccount
            // 
            this.btnSearchAccount.Image = ((System.Drawing.Image)(resources.GetObject("btnSearchAccount.Image")));
            this.btnSearchAccount.Location = new System.Drawing.Point(417, 12);
            this.btnSearchAccount.Name = "btnSearchAccount";
            this.btnSearchAccount.Size = new System.Drawing.Size(32, 32);
            this.btnSearchAccount.TabIndex = 21;
            this.btnSearchAccount.Click += new System.EventHandler(this.btnSearchAccount_Click);
            // 
            // lTotalArrears
            // 
            this.lTotalArrears.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTotalArrears.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lTotalArrears.Location = new System.Drawing.Point(7, 80);
            this.lTotalArrears.Name = "lTotalArrears";
            this.lTotalArrears.Size = new System.Drawing.Size(92, 16);
            this.lTotalArrears.TabIndex = 20;
            this.lTotalArrears.Text = "Total Arrears";
            this.lTotalArrears.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCustId
            // 
            this.txtCustId.BackColor = System.Drawing.SystemColors.Window;
            this.txtCustId.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCustId.Location = new System.Drawing.Point(302, 12);
            this.txtCustId.MaxLength = 20;
            this.txtCustId.Name = "txtCustId";
            this.txtCustId.Size = new System.Drawing.Size(109, 20);
            this.txtCustId.TabIndex = 19;
            this.txtCustId.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCustId_KeyDown);
            this.txtCustId.Leave += new System.EventHandler(this.txtCustId_Leave);
            // 
            // txtAddToAmount
            // 
            this.txtAddToAmount.BackColor = System.Drawing.SystemColors.Window;
            this.txtAddToAmount.Location = new System.Drawing.Point(322, 56);
            this.txtAddToAmount.MaxLength = 10;
            this.txtAddToAmount.Name = "txtAddToAmount";
            this.txtAddToAmount.ReadOnly = true;
            this.txtAddToAmount.Size = new System.Drawing.Size(87, 20);
            this.txtAddToAmount.TabIndex = 0;
            this.txtAddToAmount.TabStop = false;
            // 
            // txtNextPaymentDue
            // 
            this.txtNextPaymentDue.BackColor = System.Drawing.SystemColors.Window;
            this.txtNextPaymentDue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtNextPaymentDue.Location = new System.Drawing.Point(100, 56);
            this.txtNextPaymentDue.MaxLength = 20;
            this.txtNextPaymentDue.Name = "txtNextPaymentDue";
            this.txtNextPaymentDue.ReadOnly = true;
            this.txtNextPaymentDue.Size = new System.Drawing.Size(94, 20);
            this.txtNextPaymentDue.TabIndex = 17;
            this.txtNextPaymentDue.TabStop = false;
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.BackColor = System.Drawing.SystemColors.Window;
            this.txtCustomerName.Location = new System.Drawing.Point(100, 34);
            this.txtCustomerName.MaxLength = 80;
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.Size = new System.Drawing.Size(311, 20);
            this.txtCustomerName.TabIndex = 0;
            this.txtCustomerName.TabStop = false;
            // 
            // lNextPayDue
            // 
            this.lNextPayDue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lNextPayDue.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lNextPayDue.Location = new System.Drawing.Point(-7, 58);
            this.lNextPayDue.Name = "lNextPayDue";
            this.lNextPayDue.Size = new System.Drawing.Size(106, 16);
            this.lNextPayDue.TabIndex = 18;
            this.lNextPayDue.Text = "Next Payment Due";
            this.lNextPayDue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTotalArrears
            // 
            this.txtTotalArrears.BackColor = System.Drawing.SystemColors.Window;
            this.txtTotalArrears.Location = new System.Drawing.Point(100, 78);
            this.txtTotalArrears.MaxLength = 10;
            this.txtTotalArrears.Name = "txtTotalArrears";
            this.txtTotalArrears.ReadOnly = true;
            this.txtTotalArrears.Size = new System.Drawing.Size(94, 20);
            this.txtTotalArrears.TabIndex = 15;
            this.txtTotalArrears.TabStop = false;
            // 
            // lAuthorise
            // 
            this.lAuthorise.Enabled = false;
            this.lAuthorise.Location = new System.Drawing.Point(456, 56);
            this.lAuthorise.Name = "lAuthorise";
            this.lAuthorise.Size = new System.Drawing.Size(16, 16);
            this.lAuthorise.TabIndex = 14;
            // 
            // label1
            // 
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(-1, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Customer Name";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAccountNo
            // 
            this.txtAccountNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtAccountNo.Location = new System.Drawing.Point(100, 12);
            this.txtAccountNo.MaxLength = 20;
            this.txtAccountNo.Name = "txtAccountNo";
            this.txtAccountNo.PreventPaste = false;
            this.txtAccountNo.Size = new System.Drawing.Size(94, 20);
            this.txtAccountNo.TabIndex = 1;
            this.txtAccountNo.Text = "000-0000-0000-0";
            this.txtAccountNo.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtAccountNo_KeyDown);
            // 
            // lAccountNo
            // 
            this.lAccountNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAccountNo.Location = new System.Drawing.Point(17, 14);
            this.lAccountNo.Name = "lAccountNo";
            this.lAccountNo.Size = new System.Drawing.Size(82, 16);
            this.lAccountNo.TabIndex = 0;
            this.lAccountNo.Text = "Account No";
            this.lAccountNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lAddTo
            // 
            this.lAddTo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAddTo.Location = new System.Drawing.Point(200, 56);
            this.lAddTo.Name = "lAddTo";
            this.lAddTo.Size = new System.Drawing.Size(119, 19);
            this.lAddTo.TabIndex = 0;
            this.lAddTo.Text = "Add To Potential";
            this.lAddTo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(718, 47);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(48, 23);
            this.btnExit.TabIndex = 5;
            this.btnExit.Text = "Exit";
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(718, 16);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(48, 23);
            this.btnClear.TabIndex = 4;
            this.btnClear.Text = "Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lCustomerId
            // 
            this.lCustomerId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lCustomerId.Location = new System.Drawing.Point(218, 14);
            this.lCustomerId.Name = "lCustomerId";
            this.lCustomerId.Size = new System.Drawing.Size(78, 16);
            this.lCustomerId.TabIndex = 0;
            this.lCustomerId.Text = "Customer ID";
            this.lCustomerId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textStorecardBalance
            // 
            this.textStorecardBalance.BackColor = System.Drawing.SystemColors.Window;
            this.textStorecardBalance.Location = new System.Drawing.Point(322, 74);
            this.textStorecardBalance.MaxLength = 10;
            this.textStorecardBalance.Name = "textStorecardBalance";
            this.textStorecardBalance.ReadOnly = true;
            this.textStorecardBalance.Size = new System.Drawing.Size(87, 20);
            this.textStorecardBalance.TabIndex = 61;
            this.textStorecardBalance.TabStop = false;
            // 
            // gbAccountList
            // 
            this.gbAccountList.BackColor = System.Drawing.SystemColors.Control;
            this.gbAccountList.Controls.Add(this.dgAccountList);
            this.gbAccountList.Location = new System.Drawing.Point(8, 107);
            this.gbAccountList.Name = "gbAccountList";
            this.gbAccountList.Size = new System.Drawing.Size(472, 156);
            this.gbAccountList.TabIndex = 0;
            this.gbAccountList.TabStop = false;
            this.gbAccountList.Text = "Account List";
            // 
            // dgAccountList
            // 
            this.dgAccountList.AllowNavigation = false;
            this.dgAccountList.AllowSorting = false;
            this.dgAccountList.CaptionVisible = false;
            this.dgAccountList.DataMember = "";
            this.dgAccountList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgAccountList.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgAccountList.Location = new System.Drawing.Point(3, 16);
            this.dgAccountList.Name = "dgAccountList";
            this.dgAccountList.ReadOnly = true;
            this.dgAccountList.Size = new System.Drawing.Size(466, 137);
            this.dgAccountList.TabIndex = 6;
            this.dgAccountList.TabStop = false;
            this.dgAccountList.ToolTipColumn = 1;
            this.dgAccountList.ToolTipDelay = 200;
            this.dgAccountList.CurrentCellChanged += new System.EventHandler(this.dgAccountList_CurrentCellChanged);
            this.dgAccountList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgAccountList_MouseUp);
            // 
            // gbNewPayment
            // 
            this.gbNewPayment.BackColor = System.Drawing.SystemColors.Control;
            this.gbNewPayment.Controls.Add(this.cbMiniStat);
            this.gbNewPayment.Controls.Add(this.lblMiniStat);
            this.gbNewPayment.Controls.Add(this.lblCashLoan);
            this.gbNewPayment.Controls.Add(this.btnStoreCardManualEntry);
            this.gbNewPayment.Controls.Add(this.btn_calc);
            this.gbNewPayment.Controls.Add(this.btnExchange);
            this.gbNewPayment.Controls.Add(this.mtb_CardNo);
            this.gbNewPayment.Controls.Add(this.btnPay);
            this.gbNewPayment.Controls.Add(this.drpCardType);
            this.gbNewPayment.Controls.Add(this.lCardType);
            this.gbNewPayment.Controls.Add(this.btnPrintAcctNo);
            this.gbNewPayment.Controls.Add(this.drpBank);
            this.gbNewPayment.Controls.Add(this.drpPayMethod);
            this.gbNewPayment.Controls.Add(this.lBankAcctNo);
            this.gbNewPayment.Controls.Add(this.txtBankAcctNo);
            this.gbNewPayment.Controls.Add(this.lBank);
            this.gbNewPayment.Controls.Add(this.lCardNo);
            this.gbNewPayment.Controls.Add(this.txtCardNo);
            this.gbNewPayment.Controls.Add(this.lChange);
            this.gbNewPayment.Controls.Add(this.txtChange);
            this.gbNewPayment.Controls.Add(this.lTendered);
            this.gbNewPayment.Controls.Add(this.txtTendered);
            this.gbNewPayment.Controls.Add(this.lPayMethod);
            this.gbNewPayment.Controls.Add(this.lPaymentAmount);
            this.gbNewPayment.Controls.Add(this.txtPayAmount);
            this.gbNewPayment.Controls.Add(this.lTotalAmount);
            this.gbNewPayment.Controls.Add(this.txtTotalAmount);
            this.gbNewPayment.Controls.Add(this.lReceiptNo);
            this.gbNewPayment.Controls.Add(this.txtReceiptNo);
            this.gbNewPayment.Controls.Add(this.lFee);
            this.gbNewPayment.Controls.Add(this.txtFee);
            this.gbNewPayment.Location = new System.Drawing.Point(8, 264);
            this.gbNewPayment.Name = "gbNewPayment";
            this.gbNewPayment.Size = new System.Drawing.Size(472, 211);
            this.gbNewPayment.TabIndex = 0;
            this.gbNewPayment.TabStop = false;
            this.gbNewPayment.Text = "Payment";
            this.gbNewPayment.Enter += new System.EventHandler(this.gbNewPayment_Enter);
            // 
            // cbMiniStat
            // 
            this.cbMiniStat.AutoSize = true;
            this.cbMiniStat.Location = new System.Drawing.Point(112, 173);
            this.cbMiniStat.Name = "cbMiniStat";
            this.cbMiniStat.Size = new System.Drawing.Size(15, 14);
            this.cbMiniStat.TabIndex = 75;
            this.cbMiniStat.UseVisualStyleBackColor = true;
            // 
            // lblMiniStat
            // 
            this.lblMiniStat.AutoSize = true;
            this.lblMiniStat.Location = new System.Drawing.Point(13, 173);
            this.lblMiniStat.Name = "lblMiniStat";
            this.lblMiniStat.Size = new System.Drawing.Size(77, 13);
            this.lblMiniStat.TabIndex = 74;
            this.lblMiniStat.Text = "Mini Statement";
            // 
            // lblCashLoan
            // 
            this.lblCashLoan.AutoSize = true;
            this.lblCashLoan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCashLoan.ForeColor = System.Drawing.Color.Green;
            this.lblCashLoan.Location = new System.Drawing.Point(113, 10);
            this.lblCashLoan.Name = "lblCashLoan";
            this.lblCashLoan.Size = new System.Drawing.Size(226, 15);
            this.lblCashLoan.TabIndex = 73;
            this.lblCashLoan.Text = "** Customer is qualified for Cash Loan **";
            // 
            // btnStoreCardManualEntry
            // 
            this.btnStoreCardManualEntry.Image = ((System.Drawing.Image)(resources.GetObject("btnStoreCardManualEntry.Image")));
            this.btnStoreCardManualEntry.Location = new System.Drawing.Point(271, 96);
            this.btnStoreCardManualEntry.Name = "btnStoreCardManualEntry";
            this.btnStoreCardManualEntry.Size = new System.Drawing.Size(22, 22);
            this.btnStoreCardManualEntry.TabIndex = 72;
            this.ttPayment.SetToolTip(this.btnStoreCardManualEntry, "Enter Card Number from keyboard");
            this.btnStoreCardManualEntry.Click += new System.EventHandler(this.button2_Click);
            // 
            // btn_calc
            // 
            this.btn_calc.Image = global::STL.PL.Properties.Resources.Calc;
            this.btn_calc.Location = new System.Drawing.Point(326, 173);
            this.btn_calc.Name = "btn_calc";
            this.btn_calc.Size = new System.Drawing.Size(36, 31);
            this.btn_calc.TabIndex = 71;
            this.btn_calc.UseVisualStyleBackColor = true;
            this.btn_calc.Click += new System.EventHandler(this.btn_calc_Click);
            // 
            // btnExchange
            // 
            this.btnExchange.Image = ((System.Drawing.Image)(resources.GetObject("btnExchange.Image")));
            this.btnExchange.Location = new System.Drawing.Point(240, 40);
            this.btnExchange.Name = "btnExchange";
            this.btnExchange.Size = new System.Drawing.Size(20, 24);
            this.btnExchange.TabIndex = 67;
            this.btnExchange.Visible = false;
            this.btnExchange.Click += new System.EventHandler(this.btnExchange_Click);
            // 
            // mtb_CardNo
            // 
            this.mtb_CardNo.Location = new System.Drawing.Point(112, 97);
            this.mtb_CardNo.Mask = "XXXX-XXXX-XXXX-0000";
            this.mtb_CardNo.Name = "mtb_CardNo";
            this.mtb_CardNo.Size = new System.Drawing.Size(151, 20);
            this.mtb_CardNo.TabIndex = 70;
            this.mtb_CardNo.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            this.mtb_CardNo.Visible = false;
            this.mtb_CardNo.TextChanged += new System.EventHandler(this.mtb_Cardno_TextChanged);
            // 
            // btnPay
            // 
            this.btnPay.Location = new System.Drawing.Point(389, 179);
            this.btnPay.Name = "btnPay";
            this.btnPay.Size = new System.Drawing.Size(75, 23);
            this.btnPay.TabIndex = 66;
            this.btnPay.Text = "Pay";
            this.btnPay.Click += new System.EventHandler(this.btnPay_Click);
            // 
            // drpCardType
            // 
            this.drpCardType.BackColor = System.Drawing.SystemColors.Window;
            this.drpCardType.Items.AddRange(new object[] {
            ""});
            this.drpCardType.Location = new System.Drawing.Point(112, 74);
            this.drpCardType.Name = "drpCardType";
            this.drpCardType.Size = new System.Drawing.Size(104, 21);
            this.drpCardType.TabIndex = 52;
            // 
            // lCardType
            // 
            this.lCardType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lCardType.Location = new System.Drawing.Point(42, 74);
            this.lCardType.Name = "lCardType";
            this.lCardType.Size = new System.Drawing.Size(64, 16);
            this.lCardType.TabIndex = 0;
            this.lCardType.Text = "Card Type";
            this.lCardType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnPrintAcctNo
            // 
            this.btnPrintAcctNo.Location = new System.Drawing.Point(389, 148);
            this.btnPrintAcctNo.Name = "btnPrintAcctNo";
            this.btnPrintAcctNo.Size = new System.Drawing.Size(75, 23);
            this.btnPrintAcctNo.TabIndex = 65;
            this.btnPrintAcctNo.Text = "Print AcctNo";
            this.btnPrintAcctNo.Click += new System.EventHandler(this.btnPrintAcctNo_Click);
            // 
            // drpBank
            // 
            this.drpBank.BackColor = System.Drawing.SystemColors.Window;
            this.drpBank.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drpBank.Location = new System.Drawing.Point(112, 122);
            this.drpBank.Name = "drpBank";
            this.drpBank.Size = new System.Drawing.Size(184, 21);
            this.drpBank.TabIndex = 54;
            // 
            // drpPayMethod
            // 
            this.drpPayMethod.BackColor = System.Drawing.SystemColors.Window;
            this.drpPayMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.drpPayMethod.Location = new System.Drawing.Point(112, 50);
            this.drpPayMethod.Name = "drpPayMethod";
            this.drpPayMethod.Size = new System.Drawing.Size(128, 21);
            this.drpPayMethod.TabIndex = 51;
            this.drpPayMethod.SelectedIndexChanged += new System.EventHandler(this.drpPayMethod_SelectedIndexChanged);
            // 
            // lBankAcctNo
            // 
            this.lBankAcctNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lBankAcctNo.Location = new System.Drawing.Point(10, 146);
            this.lBankAcctNo.Name = "lBankAcctNo";
            this.lBankAcctNo.Size = new System.Drawing.Size(96, 16);
            this.lBankAcctNo.TabIndex = 0;
            this.lBankAcctNo.Text = "Bank Account No";
            this.lBankAcctNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtBankAcctNo
            // 
            this.txtBankAcctNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtBankAcctNo.Location = new System.Drawing.Point(112, 146);
            this.txtBankAcctNo.MaxLength = 30;
            this.txtBankAcctNo.Name = "txtBankAcctNo";
            this.txtBankAcctNo.Size = new System.Drawing.Size(184, 20);
            this.txtBankAcctNo.TabIndex = 55;
            // 
            // lBank
            // 
            this.lBank.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lBank.Location = new System.Drawing.Point(66, 122);
            this.lBank.Name = "lBank";
            this.lBank.Size = new System.Drawing.Size(40, 16);
            this.lBank.TabIndex = 0;
            this.lBank.Text = "Bank";
            this.lBank.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lCardNo
            // 
            this.lCardNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lCardNo.Location = new System.Drawing.Point(10, 98);
            this.lCardNo.Name = "lCardNo";
            this.lCardNo.Size = new System.Drawing.Size(96, 16);
            this.lCardNo.TabIndex = 0;
            this.lCardNo.Text = "Cheque / Card No";
            this.lCardNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCardNo
            // 
            this.txtCardNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtCardNo.Location = new System.Drawing.Point(113, 96);
            this.txtCardNo.MaxLength = 30;
            this.txtCardNo.Name = "txtCardNo";
            this.txtCardNo.Size = new System.Drawing.Size(152, 20);
            this.txtCardNo.TabIndex = 53;
            // 
            // lChange
            // 
            this.lChange.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lChange.Location = new System.Drawing.Point(306, 122);
            this.lChange.Name = "lChange";
            this.lChange.Size = new System.Drawing.Size(56, 16);
            this.lChange.TabIndex = 0;
            this.lChange.Text = "Change";
            this.lChange.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtChange
            // 
            this.txtChange.BackColor = System.Drawing.SystemColors.Window;
            this.txtChange.Location = new System.Drawing.Point(368, 122);
            this.txtChange.MaxLength = 10;
            this.txtChange.Name = "txtChange";
            this.txtChange.ReadOnly = true;
            this.txtChange.Size = new System.Drawing.Size(96, 20);
            this.txtChange.TabIndex = 64;
            // 
            // lTendered
            // 
            this.lTendered.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lTendered.Location = new System.Drawing.Point(306, 98);
            this.lTendered.Name = "lTendered";
            this.lTendered.Size = new System.Drawing.Size(56, 16);
            this.lTendered.TabIndex = 0;
            this.lTendered.Text = "Tendered";
            this.lTendered.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtTendered
            // 
            this.txtTendered.BackColor = System.Drawing.SystemColors.Window;
            this.txtTendered.Location = new System.Drawing.Point(368, 98);
            this.txtTendered.MaxLength = 10;
            this.txtTendered.Name = "txtTendered";
            this.txtTendered.Size = new System.Drawing.Size(96, 20);
            this.txtTendered.TabIndex = 63;
            this.txtTendered.Validating += new System.ComponentModel.CancelEventHandler(this.txtTendered_Validating);
            // 
            // lPayMethod
            // 
            this.lPayMethod.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lPayMethod.Location = new System.Drawing.Point(26, 50);
            this.lPayMethod.Name = "lPayMethod";
            this.lPayMethod.Size = new System.Drawing.Size(80, 16);
            this.lPayMethod.TabIndex = 0;
            this.lPayMethod.Text = "Pay Method";
            this.lPayMethod.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lPaymentAmount
            // 
            this.lPaymentAmount.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lPaymentAmount.Location = new System.Drawing.Point(250, 74);
            this.lPaymentAmount.Name = "lPaymentAmount";
            this.lPaymentAmount.Size = new System.Drawing.Size(112, 16);
            this.lPaymentAmount.TabIndex = 0;
            this.lPaymentAmount.Text = "Payment Amount";
            this.lPaymentAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtPayAmount
            // 
            this.txtPayAmount.BackColor = System.Drawing.SystemColors.Window;
            this.txtPayAmount.Location = new System.Drawing.Point(368, 74);
            this.txtPayAmount.MaxLength = 10;
            this.txtPayAmount.Name = "txtPayAmount";
            this.txtPayAmount.Size = new System.Drawing.Size(96, 20);
            this.txtPayAmount.TabIndex = 62;
            this.txtPayAmount.Leave += new System.EventHandler(this.txtPayAmount_Leave);
            // 
            // lTotalAmount
            // 
            this.lTotalAmount.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lTotalAmount.Location = new System.Drawing.Point(290, 26);
            this.lTotalAmount.Name = "lTotalAmount";
            this.lTotalAmount.Size = new System.Drawing.Size(72, 16);
            this.lTotalAmount.TabIndex = 0;
            this.lTotalAmount.Text = "Total Amount";
            this.lTotalAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lTotalAmount.Visible = false;
            // 
            // txtTotalAmount
            // 
            this.txtTotalAmount.BackColor = System.Drawing.SystemColors.Window;
            this.txtTotalAmount.Location = new System.Drawing.Point(368, 26);
            this.txtTotalAmount.MaxLength = 10;
            this.txtTotalAmount.Name = "txtTotalAmount";
            this.txtTotalAmount.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtTotalAmount.Size = new System.Drawing.Size(96, 20);
            this.txtTotalAmount.TabIndex = 60;
            this.txtTotalAmount.Visible = false;
            this.txtTotalAmount.Leave += new System.EventHandler(this.txtTotalAmount_Leave);
            // 
            // lReceiptNo
            // 
            this.lReceiptNo.Location = new System.Drawing.Point(34, 26);
            this.lReceiptNo.Name = "lReceiptNo";
            this.lReceiptNo.Size = new System.Drawing.Size(72, 16);
            this.lReceiptNo.TabIndex = 0;
            this.lReceiptNo.Text = "Receipt No";
            this.lReceiptNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtReceiptNo
            // 
            this.txtReceiptNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtReceiptNo.Location = new System.Drawing.Point(112, 27);
            this.txtReceiptNo.MaxLength = 10;
            this.txtReceiptNo.Name = "txtReceiptNo";
            this.txtReceiptNo.Size = new System.Drawing.Size(96, 20);
            this.txtReceiptNo.TabIndex = 50;
            this.txtReceiptNo.Leave += new System.EventHandler(this.ReceiptNo_Leave);
            // 
            // lFee
            // 
            this.lFee.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lFee.Location = new System.Drawing.Point(246, 50);
            this.lFee.Name = "lFee";
            this.lFee.Size = new System.Drawing.Size(116, 21);
            this.lFee.TabIndex = 0;
            this.lFee.Text = "Credit Fee";
            this.lFee.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lFee.Visible = false;
            // 
            // txtFee
            // 
            this.txtFee.BackColor = System.Drawing.SystemColors.Window;
            this.txtFee.Location = new System.Drawing.Point(368, 50);
            this.txtFee.MaxLength = 10;
            this.txtFee.Name = "txtFee";
            this.txtFee.Size = new System.Drawing.Size(96, 20);
            this.txtFee.TabIndex = 61;
            this.txtFee.Visible = false;
            this.txtFee.Leave += new System.EventHandler(this.txtFee_Leave);
            // 
            // gbSelectedAccount
            // 
            this.gbSelectedAccount.BackColor = System.Drawing.SystemColors.Control;
            this.gbSelectedAccount.Controls.Add(this.txtAgrmtNo);
            this.gbSelectedAccount.Controls.Add(this.txtSelectedAcctNo);
            this.gbSelectedAccount.Controls.Add(this.lAcctNo);
            this.gbSelectedAccount.Controls.Add(this.tcAccount);
            this.gbSelectedAccount.Controls.Add(this.lblAgrmtNo);
            this.gbSelectedAccount.Controls.Add(this.lCombinedRF);
            this.gbSelectedAccount.Location = new System.Drawing.Point(480, 102);
            this.gbSelectedAccount.Name = "gbSelectedAccount";
            this.gbSelectedAccount.Size = new System.Drawing.Size(314, 373);
            this.gbSelectedAccount.TabIndex = 0;
            this.gbSelectedAccount.TabStop = false;
            this.gbSelectedAccount.Text = "Selected Account";
            // 
            // txtAgrmtNo
            // 
            this.txtAgrmtNo.Location = new System.Drawing.Point(238, 16);
            this.txtAgrmtNo.Name = "txtAgrmtNo";
            this.txtAgrmtNo.Size = new System.Drawing.Size(73, 20);
            this.txtAgrmtNo.TabIndex = 2;
            // 
            // txtSelectedAcctNo
            // 
            this.txtSelectedAcctNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtSelectedAcctNo.Location = new System.Drawing.Point(64, 16);
            this.txtSelectedAcctNo.MaxLength = 12;
            this.txtSelectedAcctNo.Name = "txtSelectedAcctNo";
            this.txtSelectedAcctNo.ReadOnly = true;
            this.txtSelectedAcctNo.Size = new System.Drawing.Size(96, 20);
            this.txtSelectedAcctNo.TabIndex = 0;
            this.txtSelectedAcctNo.TabStop = false;
            // 
            // lAcctNo
            // 
            this.lAcctNo.AccessibleRole = System.Windows.Forms.AccessibleRole.SplitButton;
            this.lAcctNo.Location = new System.Drawing.Point(4, 17);
            this.lAcctNo.Name = "lAcctNo";
            this.lAcctNo.Size = new System.Drawing.Size(69, 16);
            this.lAcctNo.TabIndex = 0;
            this.lAcctNo.Text = "Account No";
            this.lAcctNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tcAccount
            // 
            this.tcAccount.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tcAccount.IDEPixelArea = true;
            this.tcAccount.Location = new System.Drawing.Point(3, 42);
            this.tcAccount.Name = "tcAccount";
            this.tcAccount.PositionTop = true;
            this.tcAccount.SelectedIndex = 0;
            this.tcAccount.SelectedTab = this.tpDetails;
            this.tcAccount.Size = new System.Drawing.Size(308, 328);
            this.tcAccount.TabIndex = 0;
            this.tcAccount.TabPages.AddRange(new Crownwood.Magic.Controls.TabPage[] {
            this.tpDetails,
            this.tpAllocation,
            this.tpTransactions,
            this.tpPaymentHolidays});
            // 
            // tpDetails
            // 
            this.tpDetails.BackColor = System.Drawing.SystemColors.Control;
            this.tpDetails.Controls.Add(this.gbPromiseToPay);
            this.tpDetails.Controls.Add(this.gbBadDebt);
            this.tpDetails.Controls.Add(this.btnPaymentList);
            this.tpDetails.Controls.Add(this.txtSegmentName);
            this.tpDetails.Controls.Add(this.label8);
            this.tpDetails.Controls.Add(this.btnLaunchTally);
            this.tpDetails.Controls.Add(this.gbSPA);
            this.tpDetails.Controls.Add(this.lFreeInstalment);
            this.tpDetails.Controls.Add(this.btnCopyAgreementTotal);
            this.tpDetails.Controls.Add(this.btnCopyInstallment);
            this.tpDetails.Controls.Add(this.lToFollow);
            this.tpDetails.Controls.Add(this.txtToFollow);
            this.tpDetails.Controls.Add(this.lFullyDelivered);
            this.tpDetails.Controls.Add(this.lAccountStatus);
            this.tpDetails.Controls.Add(this.lDueDate);
            this.tpDetails.Controls.Add(this.txtDateFInstalment);
            this.tpDetails.Controls.Add(this.txtCurrentStatus);
            this.tpDetails.Controls.Add(this.lSettlement);
            this.tpDetails.Controls.Add(this.txtArrears);
            this.tpDetails.Controls.Add(this.txtOutstandingBalance);
            this.tpDetails.Controls.Add(this.lArrears);
            this.tpDetails.Controls.Add(this.lOutstandingBalance);
            this.tpDetails.Controls.Add(this.txtAgreementTotal);
            this.tpDetails.Controls.Add(this.txtRebate);
            this.tpDetails.Controls.Add(this.txtSettlement);
            this.tpDetails.Controls.Add(this.lInstalmentAmount);
            this.tpDetails.Controls.Add(this.lRebate);
            this.tpDetails.Controls.Add(this.lAgreementTotal);
            this.tpDetails.Controls.Add(this.txtInstalment);
            this.tpDetails.Location = new System.Drawing.Point(0, 25);
            this.tpDetails.Name = "tpDetails";
            this.tpDetails.Size = new System.Drawing.Size(308, 303);
            this.tpDetails.TabIndex = 0;
            this.tpDetails.Title = "Details";
            // 
            // gbPromiseToPay
            // 
            this.gbPromiseToPay.Controls.Add(this.label10);
            this.gbPromiseToPay.Controls.Add(this.label9);
            this.gbPromiseToPay.Controls.Add(this.txtPTPDueDate);
            this.gbPromiseToPay.Controls.Add(this.txtPTPAmount);
            this.gbPromiseToPay.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.gbPromiseToPay.Location = new System.Drawing.Point(7, 230);
            this.gbPromiseToPay.Name = "gbPromiseToPay";
            this.gbPromiseToPay.Size = new System.Drawing.Size(296, 33);
            this.gbPromiseToPay.TabIndex = 105;
            this.gbPromiseToPay.TabStop = false;
            this.gbPromiseToPay.Text = "Promise to Pay";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(161, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 13);
            this.label10.TabIndex = 3;
            this.label10.Text = "Pay by";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(17, 15);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 13);
            this.label9.TabIndex = 2;
            this.label9.Text = "Amount";
            // 
            // txtPTPDueDate
            // 
            this.txtPTPDueDate.BackColor = System.Drawing.Color.Orange;
            this.txtPTPDueDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPTPDueDate.Location = new System.Drawing.Point(207, 11);
            this.txtPTPDueDate.Name = "txtPTPDueDate";
            this.txtPTPDueDate.ReadOnly = true;
            this.txtPTPDueDate.Size = new System.Drawing.Size(74, 20);
            this.txtPTPDueDate.TabIndex = 1;
            // 
            // txtPTPAmount
            // 
            this.txtPTPAmount.BackColor = System.Drawing.Color.Orange;
            this.txtPTPAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPTPAmount.Location = new System.Drawing.Point(68, 11);
            this.txtPTPAmount.Name = "txtPTPAmount";
            this.txtPTPAmount.ReadOnly = true;
            this.txtPTPAmount.Size = new System.Drawing.Size(70, 20);
            this.txtPTPAmount.TabIndex = 0;
            // 
            // gbBadDebt
            // 
            this.gbBadDebt.Controls.Add(this.txtBadDebtBalance);
            this.gbBadDebt.Controls.Add(this.txtBadDebtCharges);
            this.gbBadDebt.Controls.Add(this.lBadDebtCharges);
            this.gbBadDebt.Controls.Add(this.lBadDebtBalance);
            this.gbBadDebt.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.gbBadDebt.Location = new System.Drawing.Point(8, 200);
            this.gbBadDebt.Name = "gbBadDebt";
            this.gbBadDebt.Size = new System.Drawing.Size(296, 33);
            this.gbBadDebt.TabIndex = 104;
            this.gbBadDebt.TabStop = false;
            this.gbBadDebt.Text = "Bad Debt ";
            // 
            // txtBadDebtBalance
            // 
            this.txtBadDebtBalance.BackColor = System.Drawing.SystemColors.Window;
            this.txtBadDebtBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBadDebtBalance.Location = new System.Drawing.Point(68, 12);
            this.txtBadDebtBalance.MaxLength = 10;
            this.txtBadDebtBalance.Name = "txtBadDebtBalance";
            this.txtBadDebtBalance.ReadOnly = true;
            this.txtBadDebtBalance.Size = new System.Drawing.Size(70, 20);
            this.txtBadDebtBalance.TabIndex = 93;
            this.txtBadDebtBalance.TabStop = false;
            this.txtBadDebtBalance.Visible = false;
            // 
            // txtBadDebtCharges
            // 
            this.txtBadDebtCharges.BackColor = System.Drawing.SystemColors.Window;
            this.txtBadDebtCharges.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBadDebtCharges.Location = new System.Drawing.Point(207, 12);
            this.txtBadDebtCharges.MaxLength = 10;
            this.txtBadDebtCharges.Name = "txtBadDebtCharges";
            this.txtBadDebtCharges.ReadOnly = true;
            this.txtBadDebtCharges.Size = new System.Drawing.Size(74, 20);
            this.txtBadDebtCharges.TabIndex = 94;
            this.txtBadDebtCharges.TabStop = false;
            this.txtBadDebtCharges.Visible = false;
            // 
            // lBadDebtCharges
            // 
            this.lBadDebtCharges.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBadDebtCharges.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lBadDebtCharges.Location = new System.Drawing.Point(153, 12);
            this.lBadDebtCharges.Name = "lBadDebtCharges";
            this.lBadDebtCharges.Size = new System.Drawing.Size(48, 19);
            this.lBadDebtCharges.TabIndex = 96;
            this.lBadDebtCharges.Text = "Charges";
            this.lBadDebtCharges.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lBadDebtCharges.Visible = false;
            // 
            // lBadDebtBalance
            // 
            this.lBadDebtBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lBadDebtBalance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lBadDebtBalance.Location = new System.Drawing.Point(8, 12);
            this.lBadDebtBalance.Name = "lBadDebtBalance";
            this.lBadDebtBalance.Size = new System.Drawing.Size(49, 19);
            this.lBadDebtBalance.TabIndex = 95;
            this.lBadDebtBalance.Text = "Balance";
            this.lBadDebtBalance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lBadDebtBalance.Visible = false;
            // 
            // btnPaymentList
            // 
            this.btnPaymentList.Enabled = false;
            this.btnPaymentList.Image = ((System.Drawing.Image)(resources.GetObject("btnPaymentList.Image")));
            this.btnPaymentList.Location = new System.Drawing.Point(258, 48);
            this.btnPaymentList.Name = "btnPaymentList";
            this.btnPaymentList.Size = new System.Drawing.Size(32, 32);
            this.btnPaymentList.TabIndex = 80;
            this.btnPaymentList.Visible = false;
            this.btnPaymentList.Click += new System.EventHandler(this.btnPaymentList_Click);
            // 
            // txtSegmentName
            // 
            this.txtSegmentName.BackColor = System.Drawing.SystemColors.Window;
            this.txtSegmentName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSegmentName.Location = new System.Drawing.Point(202, 98);
            this.txtSegmentName.Name = "txtSegmentName";
            this.txtSegmentName.ReadOnly = true;
            this.txtSegmentName.Size = new System.Drawing.Size(88, 20);
            this.txtSegmentName.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.label8.Location = new System.Drawing.Point(149, 101);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 103;
            this.label8.Text = "Segment";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnLaunchTally
            // 
            this.btnLaunchTally.Image = ((System.Drawing.Image)(resources.GetObject("btnLaunchTally.Image")));
            this.btnLaunchTally.Location = new System.Drawing.Point(233, 178);
            this.btnLaunchTally.Name = "btnLaunchTally";
            this.btnLaunchTally.Size = new System.Drawing.Size(69, 23);
            this.btnLaunchTally.TabIndex = 100;
            this.btnLaunchTally.Click += new System.EventHandler(this.btnLaunchTally_Click);
            // 
            // gbSPA
            // 
            this.gbSPA.Controls.Add(this.label7);
            this.gbSPA.Controls.Add(this.label6);
            this.gbSPA.Controls.Add(this.txtExpiryDate);
            this.gbSPA.Controls.Add(this.txtSPAInstalment);
            this.gbSPA.Enabled = false;
            this.gbSPA.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.gbSPA.Location = new System.Drawing.Point(6, 263);
            this.gbSPA.Name = "gbSPA";
            this.gbSPA.Size = new System.Drawing.Size(296, 37);
            this.gbSPA.TabIndex = 97;
            this.gbSPA.TabStop = false;
            this.gbSPA.Text = "Special Arrangement";
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(140, 14);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(65, 19);
            this.label7.TabIndex = 97;
            this.label7.Text = "Expiry Date";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(6, 14);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 19);
            this.label6.TabIndex = 96;
            this.label6.Text = "Instalment";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtExpiryDate
            // 
            this.txtExpiryDate.BackColor = System.Drawing.SystemColors.Window;
            this.txtExpiryDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtExpiryDate.Location = new System.Drawing.Point(207, 13);
            this.txtExpiryDate.MaxLength = 10;
            this.txtExpiryDate.Name = "txtExpiryDate";
            this.txtExpiryDate.ReadOnly = true;
            this.txtExpiryDate.Size = new System.Drawing.Size(74, 20);
            this.txtExpiryDate.TabIndex = 95;
            this.txtExpiryDate.TabStop = false;
            // 
            // txtSPAInstalment
            // 
            this.txtSPAInstalment.BackColor = System.Drawing.SystemColors.Window;
            this.txtSPAInstalment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSPAInstalment.Location = new System.Drawing.Point(70, 14);
            this.txtSPAInstalment.MaxLength = 10;
            this.txtSPAInstalment.Name = "txtSPAInstalment";
            this.txtSPAInstalment.ReadOnly = true;
            this.txtSPAInstalment.Size = new System.Drawing.Size(70, 20);
            this.txtSPAInstalment.TabIndex = 94;
            this.txtSPAInstalment.TabStop = false;
            // 
            // lFreeInstalment
            // 
            this.lFreeInstalment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lFreeInstalment.Location = new System.Drawing.Point(3, -1);
            this.lFreeInstalment.Name = "lFreeInstalment";
            this.lFreeInstalment.Size = new System.Drawing.Size(193, 16);
            this.lFreeInstalment.TabIndex = 92;
            this.lFreeInstalment.Text = "** FREE Instalment Available **";
            this.lFreeInstalment.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lFreeInstalment.Visible = false;
            // 
            // btnCopyAgreementTotal
            // 
            this.btnCopyAgreementTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopyAgreementTotal.Location = new System.Drawing.Point(235, 15);
            this.btnCopyAgreementTotal.Name = "btnCopyAgreementTotal";
            this.btnCopyAgreementTotal.Size = new System.Drawing.Size(48, 24);
            this.btnCopyAgreementTotal.TabIndex = 90;
            this.btnCopyAgreementTotal.Text = "Copy";
            this.btnCopyAgreementTotal.Visible = false;
            this.btnCopyAgreementTotal.Click += new System.EventHandler(this.btnCopyAgreementTotal_Click);
            // 
            // btnCopyInstallment
            // 
            this.btnCopyInstallment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCopyInstallment.Location = new System.Drawing.Point(235, 120);
            this.btnCopyInstallment.Name = "btnCopyInstallment";
            this.btnCopyInstallment.Size = new System.Drawing.Size(48, 24);
            this.btnCopyInstallment.TabIndex = 91;
            this.btnCopyInstallment.Text = "Copy";
            this.btnCopyInstallment.Click += new System.EventHandler(this.btnCopyInstallment_Click);
            // 
            // lToFollow
            // 
            this.lToFollow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lToFollow.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lToFollow.Location = new System.Drawing.Point(58, 78);
            this.lToFollow.Name = "lToFollow";
            this.lToFollow.Size = new System.Drawing.Size(64, 19);
            this.lToFollow.TabIndex = 0;
            this.lToFollow.Text = "To Follow";
            this.lToFollow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtToFollow
            // 
            this.txtToFollow.BackColor = System.Drawing.SystemColors.Window;
            this.txtToFollow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtToFollow.Location = new System.Drawing.Point(123, 78);
            this.txtToFollow.MaxLength = 10;
            this.txtToFollow.Name = "txtToFollow";
            this.txtToFollow.ReadOnly = true;
            this.txtToFollow.Size = new System.Drawing.Size(97, 20);
            this.txtToFollow.TabIndex = 0;
            this.txtToFollow.TabStop = false;
            // 
            // lFullyDelivered
            // 
            this.lFullyDelivered.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lFullyDelivered.Location = new System.Drawing.Point(130, 79);
            this.lFullyDelivered.Name = "lFullyDelivered";
            this.lFullyDelivered.Size = new System.Drawing.Size(127, 19);
            this.lFullyDelivered.TabIndex = 0;
            this.lFullyDelivered.Text = "Fully Delivered";
            this.lFullyDelivered.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lFullyDelivered.Visible = false;
            // 
            // lAccountStatus
            // 
            this.lAccountStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lAccountStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAccountStatus.Location = new System.Drawing.Point(22, 99);
            this.lAccountStatus.Name = "lAccountStatus";
            this.lAccountStatus.Size = new System.Drawing.Size(100, 16);
            this.lAccountStatus.TabIndex = 0;
            this.lAccountStatus.Text = "Account Status";
            this.lAccountStatus.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lDueDate
            // 
            this.lDueDate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lDueDate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lDueDate.Location = new System.Drawing.Point(42, 143);
            this.lDueDate.Name = "lDueDate";
            this.lDueDate.Size = new System.Drawing.Size(80, 16);
            this.lDueDate.TabIndex = 0;
            this.lDueDate.Text = "Due Date";
            this.lDueDate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtDateFInstalment
            // 
            this.txtDateFInstalment.BackColor = System.Drawing.SystemColors.Window;
            this.txtDateFInstalment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDateFInstalment.Location = new System.Drawing.Point(123, 142);
            this.txtDateFInstalment.MaxLength = 20;
            this.txtDateFInstalment.Name = "txtDateFInstalment";
            this.txtDateFInstalment.ReadOnly = true;
            this.txtDateFInstalment.Size = new System.Drawing.Size(97, 20);
            this.txtDateFInstalment.TabIndex = 0;
            this.txtDateFInstalment.TabStop = false;
            // 
            // txtCurrentStatus
            // 
            this.txtCurrentStatus.BackColor = System.Drawing.SystemColors.Window;
            this.txtCurrentStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCurrentStatus.Location = new System.Drawing.Point(123, 99);
            this.txtCurrentStatus.MaxLength = 2;
            this.txtCurrentStatus.Name = "txtCurrentStatus";
            this.txtCurrentStatus.ReadOnly = true;
            this.txtCurrentStatus.Size = new System.Drawing.Size(24, 20);
            this.txtCurrentStatus.TabIndex = 0;
            this.txtCurrentStatus.TabStop = false;
            // 
            // lSettlement
            // 
            this.lSettlement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lSettlement.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lSettlement.Location = new System.Drawing.Point(2, 185);
            this.lSettlement.Name = "lSettlement";
            this.lSettlement.Size = new System.Drawing.Size(120, 16);
            this.lSettlement.TabIndex = 0;
            this.lSettlement.Text = "Settlement Figure";
            this.lSettlement.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtArrears
            // 
            this.txtArrears.BackColor = System.Drawing.SystemColors.Window;
            this.txtArrears.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtArrears.Location = new System.Drawing.Point(123, 36);
            this.txtArrears.MaxLength = 10;
            this.txtArrears.Name = "txtArrears";
            this.txtArrears.ReadOnly = true;
            this.txtArrears.Size = new System.Drawing.Size(97, 20);
            this.txtArrears.TabIndex = 0;
            this.txtArrears.TabStop = false;
            // 
            // txtOutstandingBalance
            // 
            this.txtOutstandingBalance.BackColor = System.Drawing.SystemColors.Window;
            this.txtOutstandingBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOutstandingBalance.Location = new System.Drawing.Point(123, 57);
            this.txtOutstandingBalance.MaxLength = 10;
            this.txtOutstandingBalance.Name = "txtOutstandingBalance";
            this.txtOutstandingBalance.ReadOnly = true;
            this.txtOutstandingBalance.Size = new System.Drawing.Size(97, 20);
            this.txtOutstandingBalance.TabIndex = 0;
            this.txtOutstandingBalance.TabStop = false;
            // 
            // lArrears
            // 
            this.lArrears.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lArrears.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lArrears.Location = new System.Drawing.Point(50, 37);
            this.lArrears.Name = "lArrears";
            this.lArrears.Size = new System.Drawing.Size(72, 16);
            this.lArrears.TabIndex = 0;
            this.lArrears.Text = "Arrears";
            this.lArrears.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lOutstandingBalance
            // 
            this.lOutstandingBalance.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lOutstandingBalance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lOutstandingBalance.Location = new System.Drawing.Point(10, 57);
            this.lOutstandingBalance.Name = "lOutstandingBalance";
            this.lOutstandingBalance.Size = new System.Drawing.Size(112, 19);
            this.lOutstandingBalance.TabIndex = 0;
            this.lOutstandingBalance.Text = "Outstanding Balance";
            this.lOutstandingBalance.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtAgreementTotal
            // 
            this.txtAgreementTotal.BackColor = System.Drawing.SystemColors.Window;
            this.txtAgreementTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtAgreementTotal.Location = new System.Drawing.Point(123, 15);
            this.txtAgreementTotal.MaxLength = 10;
            this.txtAgreementTotal.Name = "txtAgreementTotal";
            this.txtAgreementTotal.ReadOnly = true;
            this.txtAgreementTotal.Size = new System.Drawing.Size(97, 20);
            this.txtAgreementTotal.TabIndex = 0;
            this.txtAgreementTotal.TabStop = false;
            // 
            // txtRebate
            // 
            this.txtRebate.BackColor = System.Drawing.SystemColors.Window;
            this.txtRebate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtRebate.Location = new System.Drawing.Point(123, 163);
            this.txtRebate.MaxLength = 10;
            this.txtRebate.Name = "txtRebate";
            this.txtRebate.ReadOnly = true;
            this.txtRebate.Size = new System.Drawing.Size(97, 20);
            this.txtRebate.TabIndex = 0;
            this.txtRebate.TabStop = false;
            // 
            // txtSettlement
            // 
            this.txtSettlement.BackColor = System.Drawing.SystemColors.Window;
            this.txtSettlement.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSettlement.Location = new System.Drawing.Point(123, 184);
            this.txtSettlement.MaxLength = 10;
            this.txtSettlement.Name = "txtSettlement";
            this.txtSettlement.ReadOnly = true;
            this.txtSettlement.Size = new System.Drawing.Size(97, 20);
            this.txtSettlement.TabIndex = 0;
            this.txtSettlement.TabStop = false;
            // 
            // lInstalmentAmount
            // 
            this.lInstalmentAmount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lInstalmentAmount.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lInstalmentAmount.Location = new System.Drawing.Point(2, 121);
            this.lInstalmentAmount.Name = "lInstalmentAmount";
            this.lInstalmentAmount.Size = new System.Drawing.Size(120, 19);
            this.lInstalmentAmount.TabIndex = 0;
            this.lInstalmentAmount.Text = "Instalment Amount";
            this.lInstalmentAmount.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lRebate
            // 
            this.lRebate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lRebate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lRebate.Location = new System.Drawing.Point(50, 164);
            this.lRebate.Name = "lRebate";
            this.lRebate.Size = new System.Drawing.Size(72, 16);
            this.lRebate.TabIndex = 0;
            this.lRebate.Text = "Rebate";
            this.lRebate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lAgreementTotal
            // 
            this.lAgreementTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lAgreementTotal.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lAgreementTotal.Location = new System.Drawing.Point(23, 16);
            this.lAgreementTotal.Name = "lAgreementTotal";
            this.lAgreementTotal.Size = new System.Drawing.Size(100, 16);
            this.lAgreementTotal.TabIndex = 0;
            this.lAgreementTotal.Text = "Agreement Total";
            this.lAgreementTotal.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtInstalment
            // 
            this.txtInstalment.BackColor = System.Drawing.SystemColors.Window;
            this.txtInstalment.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInstalment.Location = new System.Drawing.Point(123, 121);
            this.txtInstalment.MaxLength = 10;
            this.txtInstalment.Name = "txtInstalment";
            this.txtInstalment.ReadOnly = true;
            this.txtInstalment.Size = new System.Drawing.Size(97, 20);
            this.txtInstalment.TabIndex = 0;
            this.txtInstalment.TabStop = false;
            // 
            // tpAllocation
            // 
            this.tpAllocation.Controls.Add(this.gbTallymanAlloc);
            this.tpAllocation.Controls.Add(this.gbCourtsAlloc);
            this.tpAllocation.Location = new System.Drawing.Point(0, 25);
            this.tpAllocation.Name = "tpAllocation";
            this.tpAllocation.Selected = false;
            this.tpAllocation.Size = new System.Drawing.Size(308, 303);
            this.tpAllocation.TabIndex = 1;
            this.tpAllocation.Title = "Allocation";
            // 
            // gbTallymanAlloc
            // 
            this.gbTallymanAlloc.Controls.Add(this.txtSegmentId);
            this.gbTallymanAlloc.Controls.Add(this.lSegmentId);
            this.gbTallymanAlloc.Location = new System.Drawing.Point(8, 24);
            this.gbTallymanAlloc.Name = "gbTallymanAlloc";
            this.gbTallymanAlloc.Size = new System.Drawing.Size(280, 112);
            this.gbTallymanAlloc.TabIndex = 7;
            this.gbTallymanAlloc.TabStop = false;
            this.gbTallymanAlloc.Text = "Tallyman Allocation";
            // 
            // txtSegmentId
            // 
            this.txtSegmentId.BackColor = System.Drawing.SystemColors.Window;
            this.txtSegmentId.Location = new System.Drawing.Point(120, 45);
            this.txtSegmentId.MaxLength = 10;
            this.txtSegmentId.Name = "txtSegmentId";
            this.txtSegmentId.ReadOnly = true;
            this.txtSegmentId.Size = new System.Drawing.Size(80, 23);
            this.txtSegmentId.TabIndex = 6;
            this.txtSegmentId.TabStop = false;
            // 
            // lSegmentId
            // 
            this.lSegmentId.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.lSegmentId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lSegmentId.Location = new System.Drawing.Point(32, 45);
            this.lSegmentId.Name = "lSegmentId";
            this.lSegmentId.Size = new System.Drawing.Size(80, 16);
            this.lSegmentId.TabIndex = 5;
            this.lSegmentId.Text = "Segment Id";
            this.lSegmentId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // gbCourtsAlloc
            // 
            this.gbCourtsAlloc.Controls.Add(this.lEmpType);
            this.gbCourtsAlloc.Controls.Add(this.lEmpName);
            this.gbCourtsAlloc.Controls.Add(this.txtEmployeeName);
            this.gbCourtsAlloc.Controls.Add(this.txtEmployeeType);
            this.gbCourtsAlloc.Controls.Add(this.txtEmployeeNo);
            this.gbCourtsAlloc.Controls.Add(this.lEmployeeNo);
            this.gbCourtsAlloc.Location = new System.Drawing.Point(8, 24);
            this.gbCourtsAlloc.Name = "gbCourtsAlloc";
            this.gbCourtsAlloc.Size = new System.Drawing.Size(280, 136);
            this.gbCourtsAlloc.TabIndex = 6;
            this.gbCourtsAlloc.TabStop = false;
            this.gbCourtsAlloc.Text = "CoSACS Allocation";
            // 
            // lEmpType
            // 
            this.lEmpType.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.lEmpType.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lEmpType.Location = new System.Drawing.Point(16, 89);
            this.lEmpType.Name = "lEmpType";
            this.lEmpType.Size = new System.Drawing.Size(64, 16);
            this.lEmpType.TabIndex = 4;
            this.lEmpType.Text = "Type";
            this.lEmpType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lEmpName
            // 
            this.lEmpName.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.lEmpName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lEmpName.Location = new System.Drawing.Point(16, 57);
            this.lEmpName.Name = "lEmpName";
            this.lEmpName.Size = new System.Drawing.Size(64, 16);
            this.lEmpName.TabIndex = 5;
            this.lEmpName.Text = "Name";
            this.lEmpName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtEmployeeName
            // 
            this.txtEmployeeName.BackColor = System.Drawing.SystemColors.Window;
            this.txtEmployeeName.Location = new System.Drawing.Point(88, 56);
            this.txtEmployeeName.MaxLength = 80;
            this.txtEmployeeName.Name = "txtEmployeeName";
            this.txtEmployeeName.ReadOnly = true;
            this.txtEmployeeName.Size = new System.Drawing.Size(184, 23);
            this.txtEmployeeName.TabIndex = 6;
            this.txtEmployeeName.TabStop = false;
            // 
            // txtEmployeeType
            // 
            this.txtEmployeeType.BackColor = System.Drawing.SystemColors.Window;
            this.txtEmployeeType.Location = new System.Drawing.Point(88, 89);
            this.txtEmployeeType.MaxLength = 30;
            this.txtEmployeeType.Name = "txtEmployeeType";
            this.txtEmployeeType.ReadOnly = true;
            this.txtEmployeeType.Size = new System.Drawing.Size(112, 23);
            this.txtEmployeeType.TabIndex = 1;
            this.txtEmployeeType.TabStop = false;
            // 
            // txtEmployeeNo
            // 
            this.txtEmployeeNo.BackColor = System.Drawing.SystemColors.Window;
            this.txtEmployeeNo.Location = new System.Drawing.Point(88, 25);
            this.txtEmployeeNo.MaxLength = 10;
            this.txtEmployeeNo.Name = "txtEmployeeNo";
            this.txtEmployeeNo.ReadOnly = true;
            this.txtEmployeeNo.Size = new System.Drawing.Size(80, 23);
            this.txtEmployeeNo.TabIndex = 2;
            this.txtEmployeeNo.TabStop = false;
            // 
            // lEmployeeNo
            // 
            this.lEmployeeNo.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.lEmployeeNo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lEmployeeNo.Location = new System.Drawing.Point(8, 25);
            this.lEmployeeNo.Name = "lEmployeeNo";
            this.lEmployeeNo.Size = new System.Drawing.Size(72, 16);
            this.lEmployeeNo.TabIndex = 3;
            this.lEmployeeNo.Text = "Employee No";
            this.lEmployeeNo.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tpTransactions
            // 
            this.tpTransactions.Controls.Add(this.lTransactionCount);
            this.tpTransactions.Controls.Add(this.cbNonPrinted);
            this.tpTransactions.Controls.Add(this.dgTransactionList);
            this.tpTransactions.Location = new System.Drawing.Point(0, 25);
            this.tpTransactions.Name = "tpTransactions";
            this.tpTransactions.Selected = false;
            this.tpTransactions.Size = new System.Drawing.Size(308, 303);
            this.tpTransactions.TabIndex = 2;
            this.tpTransactions.Title = "Transactions";
            this.tpTransactions.Enter += new System.EventHandler(this.tpTransactions_Enter);
            // 
            // lTransactionCount
            // 
            this.lTransactionCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTransactionCount.Location = new System.Drawing.Point(192, 8);
            this.lTransactionCount.Name = "lTransactionCount";
            this.lTransactionCount.Size = new System.Drawing.Size(104, 16);
            this.lTransactionCount.TabIndex = 0;
            this.lTransactionCount.Text = "0 Transactions";
            this.lTransactionCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lTransactionCount.Visible = false;
            // 
            // cbNonPrinted
            // 
            this.cbNonPrinted.Location = new System.Drawing.Point(24, 8);
            this.cbNonPrinted.Name = "cbNonPrinted";
            this.cbNonPrinted.Size = new System.Drawing.Size(144, 16);
            this.cbNonPrinted.TabIndex = 0;
            this.cbNonPrinted.TabStop = false;
            this.cbNonPrinted.Text = "Non-printed only";
            this.cbNonPrinted.CheckedChanged += new System.EventHandler(this.cbNonPrinted_CheckedChanged);
            // 
            // dgTransactionList
            // 
            this.dgTransactionList.CaptionVisible = false;
            this.dgTransactionList.DataMember = "";
            this.dgTransactionList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgTransactionList.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgTransactionList.Location = new System.Drawing.Point(0, 55);
            this.dgTransactionList.Name = "dgTransactionList";
            this.dgTransactionList.ReadOnly = true;
            this.dgTransactionList.Size = new System.Drawing.Size(308, 248);
            this.dgTransactionList.TabIndex = 0;
            this.dgTransactionList.TabStop = false;
            // 
            // tpPaymentHolidays
            // 
            this.tpPaymentHolidays.Controls.Add(this.groupBox1);
            this.tpPaymentHolidays.Controls.Add(this.dgPaymentHolidays);
            this.tpPaymentHolidays.Location = new System.Drawing.Point(0, 25);
            this.tpPaymentHolidays.Name = "tpPaymentHolidays";
            this.tpPaymentHolidays.Selected = false;
            this.tpPaymentHolidays.Size = new System.Drawing.Size(308, 303);
            this.tpPaymentHolidays.TabIndex = 3;
            this.tpPaymentHolidays.Title = "Payment Holidays";
            this.tpPaymentHolidays.Enter += new System.EventHandler(this.tpPaymentHolidays_Enter);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chxPaymentHolidayCancelled);
            this.groupBox1.Controls.Add(this.numMinimumPayments);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numPaymentHolidaysLeft);
            this.groupBox1.Controls.Add(this.btnPaymentHoliday);
            this.groupBox1.Controls.Add(this.txtNewDueDate);
            this.groupBox1.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 144);
            this.groupBox1.TabIndex = 101;
            this.groupBox1.TabStop = false;
            // 
            // chxPaymentHolidayCancelled
            // 
            this.chxPaymentHolidayCancelled.Enabled = false;
            this.chxPaymentHolidayCancelled.Location = new System.Drawing.Point(138, 100);
            this.chxPaymentHolidayCancelled.Name = "chxPaymentHolidayCancelled";
            this.chxPaymentHolidayCancelled.Size = new System.Drawing.Size(16, 24);
            this.chxPaymentHolidayCancelled.TabIndex = 97;
            this.chxPaymentHolidayCancelled.Visible = false;
            // 
            // numMinimumPayments
            // 
            this.numMinimumPayments.BackColor = System.Drawing.SystemColors.Window;
            this.numMinimumPayments.Enabled = false;
            this.numMinimumPayments.Location = new System.Drawing.Point(120, 51);
            this.numMinimumPayments.Name = "numMinimumPayments";
            this.numMinimumPayments.Size = new System.Drawing.Size(48, 21);
            this.numMinimumPayments.TabIndex = 99;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(13, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(104, 31);
            this.label3.TabIndex = 95;
            this.label3.Text = "Payment Holidays Remaining";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(13, 86);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 16);
            this.label2.TabIndex = 92;
            this.label2.Text = "Revised Due Date";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Visible = false;
            // 
            // label5
            // 
            this.label5.Enabled = false;
            this.label5.Location = new System.Drawing.Point(117, 17);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(122, 31);
            this.label5.TabIndex = 100;
            this.label5.Text = "Minimum Payments Required";
            this.label5.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label4
            // 
            this.label4.Enabled = false;
            this.label4.Location = new System.Drawing.Point(160, 95);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 33);
            this.label4.TabIndex = 98;
            this.label4.Text = "Payment Holiday Cancelled";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label4.Visible = false;
            // 
            // numPaymentHolidaysLeft
            // 
            this.numPaymentHolidaysLeft.BackColor = System.Drawing.SystemColors.Window;
            this.numPaymentHolidaysLeft.Enabled = false;
            this.numPaymentHolidaysLeft.Location = new System.Drawing.Point(16, 51);
            this.numPaymentHolidaysLeft.Name = "numPaymentHolidaysLeft";
            this.numPaymentHolidaysLeft.Size = new System.Drawing.Size(48, 21);
            this.numPaymentHolidaysLeft.TabIndex = 94;
            // 
            // btnPaymentHoliday
            // 
            this.btnPaymentHoliday.Enabled = false;
            this.btnPaymentHoliday.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPaymentHoliday.Location = new System.Drawing.Point(218, 48);
            this.btnPaymentHoliday.Name = "btnPaymentHoliday";
            this.btnPaymentHoliday.Size = new System.Drawing.Size(59, 24);
            this.btnPaymentHoliday.TabIndex = 91;
            this.btnPaymentHoliday.Text = "Process";
            this.btnPaymentHoliday.Click += new System.EventHandler(this.btnPaymentHoliday_Click);
            // 
            // txtNewDueDate
            // 
            this.txtNewDueDate.BackColor = System.Drawing.SystemColors.Window;
            this.txtNewDueDate.Location = new System.Drawing.Point(16, 105);
            this.txtNewDueDate.MaxLength = 10;
            this.txtNewDueDate.Name = "txtNewDueDate";
            this.txtNewDueDate.ReadOnly = true;
            this.txtNewDueDate.Size = new System.Drawing.Size(96, 21);
            this.txtNewDueDate.TabIndex = 93;
            this.txtNewDueDate.Visible = false;
            // 
            // dgPaymentHolidays
            // 
            this.dgPaymentHolidays.CaptionVisible = false;
            this.dgPaymentHolidays.DataMember = "";
            this.dgPaymentHolidays.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgPaymentHolidays.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPaymentHolidays.Location = new System.Drawing.Point(0, 151);
            this.dgPaymentHolidays.Name = "dgPaymentHolidays";
            this.dgPaymentHolidays.ReadOnly = true;
            this.dgPaymentHolidays.Size = new System.Drawing.Size(308, 152);
            this.dgPaymentHolidays.TabIndex = 96;
            this.dgPaymentHolidays.TabStop = false;
            // 
            // lblAgrmtNo
            // 
            this.lblAgrmtNo.AutoSize = true;
            this.lblAgrmtNo.Location = new System.Drawing.Point(166, 19);
            this.lblAgrmtNo.Name = "lblAgrmtNo";
            this.lblAgrmtNo.Size = new System.Drawing.Size(75, 13);
            this.lblAgrmtNo.TabIndex = 1;
            this.lblAgrmtNo.Text = "Agreement No";
            this.lblAgrmtNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lCombinedRF
            // 
            this.lCombinedRF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lCombinedRF.Location = new System.Drawing.Point(45, 17);
            this.lCombinedRF.Name = "lCombinedRF";
            this.lCombinedRF.Size = new System.Drawing.Size(216, 16);
            this.lCombinedRF.TabIndex = 0;
            this.lCombinedRF.Text = "Combined Ready Finance Accounts";
            this.lCombinedRF.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lCombinedRF.Visible = false;
            // 
            // menuControl1
            // 
            this.menuControl1.AnimateStyle = Crownwood.Magic.Menus.Animation.System;
            this.menuControl1.AnimateTime = 100;
            this.menuControl1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.menuControl1.Direction = Crownwood.Magic.Common.Direction.Horizontal;
            this.menuControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.menuControl1.Font = new System.Drawing.Font("Tahoma", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World, ((byte)(0)));
            this.menuControl1.HighlightTextColor = System.Drawing.SystemColors.MenuText;
            this.menuControl1.Location = new System.Drawing.Point(0, 0);
            this.menuControl1.Name = "menuControl1";
            this.menuControl1.Size = new System.Drawing.Size(298, 25);
            this.menuControl1.Style = Crownwood.Magic.Common.VisualStyle.IDE;
            this.menuControl1.TabIndex = 4;
            this.menuControl1.TabStop = false;
            this.menuControl1.Text = "menuControl1";
            // 
            // menuFile
            // 
            this.menuFile.Description = "MenuItem";
            this.menuFile.Text = "&File";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            // 
            // menuPrint
            // 
            this.menuPrint.Description = "MenuItem";
            this.menuPrint.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] {
            this.menuTaxInvoice,
            this.menuPaymentCard,
            this.menuStatement,
            this.menuPaymentCardDueDate});
            this.menuPrint.Text = "&Print";
            // 
            // menuTaxInvoice
            // 
            this.menuTaxInvoice.Description = "MenuItem";
            this.menuTaxInvoice.Enabled = false;
            this.menuTaxInvoice.Text = "&Tax Invoice";
            this.menuTaxInvoice.Click += new System.EventHandler(this.menuTaxInvoice_Click);
            // 
            // menuPaymentCard
            // 
            this.menuPaymentCard.Description = "MenuItem";
            this.menuPaymentCard.Enabled = false;
            this.menuPaymentCard.Text = "New &Payment Card";
            this.menuPaymentCard.Click += new System.EventHandler(this.menuPaymentCard_Click);
            // 
            // menuStatement
            // 
            this.menuStatement.Description = "MenuItem";
            this.menuStatement.Enabled = false;
            this.menuStatement.Text = "&Statement of Account";
            this.menuStatement.Click += new System.EventHandler(this.menuStatement_Click);
            // 
            // menuPaymentCardDueDate
            // 
            this.menuPaymentCardDueDate.Description = "MenuItem";
            this.menuPaymentCardDueDate.Text = "Payment Card Due Date";
            this.menuPaymentCardDueDate.Click += new System.EventHandler(this.menuPaymentCardDueDate_Click);
            // 
            // menuCashTill
            // 
            this.menuCashTill.Description = "MenuItem";
            this.menuCashTill.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] {
            this.menuOpenCashTill});
            this.menuCashTill.Text = "Cash Till";
            // 
            // menuOpenCashTill
            // 
            this.menuOpenCashTill.Description = "MenuItem";
            this.menuOpenCashTill.Text = "Open Cash Till";
            this.menuOpenCashTill.Click += new System.EventHandler(this.menuOpenCashTill_Click);
            // 
            // menuWarrantRenewals
            // 
            this.menuWarrantRenewals.Description = "MenuItem";
            this.menuWarrantRenewals.Enabled = false;
            this.menuWarrantRenewals.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] {
            this.menuCheckExpiringWarranties});
            this.menuWarrantRenewals.Text = "Warranty Renewals";
            this.menuWarrantRenewals.Visible = false;
            // 
            // menuCheckExpiringWarranties
            // 
            this.menuCheckExpiringWarranties.Description = "MenuItem";
            this.menuCheckExpiringWarranties.Text = "Check Expiring Warranties";
            this.menuCheckExpiringWarranties.Visible = false;
            this.menuCheckExpiringWarranties.Click += new System.EventHandler(this.menuCheckExpiringWarranties_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Description = "MenuItem";
            this.menuHelp.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] {
            this.menuLaunchHelp});
            this.menuHelp.Text = "&Help";
            // 
            // menuLaunchHelp
            // 
            this.menuLaunchHelp.Description = "MenuItem";
            this.menuLaunchHelp.Text = "&About this Screen";
            this.menuLaunchHelp.Click += new System.EventHandler(this.menuLaunchHelp_Click);
            // 
            // menuSESPopup
            // 
            this.menuSESPopup.Description = "MenuItem";
            this.menuSESPopup.MenuCommands.AddRange(new Crownwood.Magic.Menus.MenuCommand[] {
            this.menuSES});
            this.menuSESPopup.Text = "&SES Items";
            // 
            // menuSES
            // 
            this.menuSES.Description = "MenuItem";
            this.menuSES.Text = "Show SES Items";
            this.menuSES.Click += new System.EventHandler(this.menuSES_Click);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // errorProviderStoreCard
            // 
            this.errorProviderStoreCard.ContainerControl = this;
            // 
            // addressTab1
            // 
            this.addressTab1.Location = new System.Drawing.Point(545, -25);
            this.addressTab1.Name = "addressTab1";
            this.addressTab1.ReadOnly = true;
            this.addressTab1.SimpleAddress = true;
            this.addressTab1.Size = new System.Drawing.Size(184, 111);
            this.addressTab1.TabIndex = 0;
            this.addressTab1.TabStop = false;
            this.addressTab1.drptitleC.Visible = false;
            this.addressTab1.CFirstname.Visible = false;
            this.addressTab1.CLastname.Visible = false;
            // 
            // Payment
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(792, 477);
            this.Controls.Add(this.gbNewPayment);
            this.Controls.Add(this.gbSelectedAccount);
            this.Controls.Add(this.gbCustomer);
            this.Controls.Add(this.gbAccountList);
            this.Name = "Payment";
            this.Text = "Payments";
            this.Load += new System.EventHandler(this.Payment_Load);
            this.gbCustomer.ResumeLayout(false);
            this.gbCustomer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.storecardimage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LoyaltyLogo_pb)).EndInit();
            this.gbAccountList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgAccountList)).EndInit();
            this.gbNewPayment.ResumeLayout(false);
            this.gbNewPayment.PerformLayout();
            this.gbSelectedAccount.ResumeLayout(false);
            this.gbSelectedAccount.PerformLayout();
            this.tcAccount.ResumeLayout(false);
            this.tpDetails.ResumeLayout(false);
            this.tpDetails.PerformLayout();
            this.gbPromiseToPay.ResumeLayout(false);
            this.gbPromiseToPay.PerformLayout();
            this.gbBadDebt.ResumeLayout(false);
            this.gbBadDebt.PerformLayout();
            this.gbSPA.ResumeLayout(false);
            this.gbSPA.PerformLayout();
            this.tpAllocation.ResumeLayout(false);
            this.gbTallymanAlloc.ResumeLayout(false);
            this.gbTallymanAlloc.PerformLayout();
            this.gbCourtsAlloc.ResumeLayout(false);
            this.gbCourtsAlloc.PerformLayout();
            this.tpTransactions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgTransactionList)).EndInit();
            this.tpPaymentHolidays.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMinimumPayments)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numPaymentHolidaysLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgPaymentHolidays)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderStoreCard)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        // Local Procedures

        private bool SlipPrinterOK()
        {
            Function = "Payment Screen: Check Slip Printer connected";

            while (1 == 1)
                try
                {
                    // Early warning if receipt printer not available
                    ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SLIPCHECK");
                    this._slipRequired = true;
                    ReceiptPrinter rp = null;
                    rp = new ReceiptPrinter(this);
                    rp.OpenPrinter();

                    // Check for paper
                    if (rp.SlpEmpty)
                    {
                        rp.ClosePrinter();
                        ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SLIPPAPEROUT");
                        DialogResult userRequest = ShowInfo("M_SLIPPAPER", MessageBoxButtons.AbortRetryIgnore);
                        if (userRequest == DialogResult.Abort)
                        {
                            return false;
                        }
                        else if (userRequest == DialogResult.Ignore)
                        {
                            this._slipRequired = false;
                            return true;
                        }
                        // Otherwise Retry
                    }
                    else
                    {
                        rp.ClosePrinter();
                        ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SLIPOK");
                        return true;
                    }
                }
                catch
                {
                    ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SLIPNOCONNECT");
                    DialogResult userRequest = ShowInfo("M_SLIPCONNECT", MessageBoxButtons.AbortRetryIgnore);
                    if (userRequest == DialogResult.Abort)
                    {
                        ((MainForm)FormRoot).lDownloading.Visible = false;
                        ((MainForm)FormRoot).pbDownloading.Visible = false;
                        return false;
                    }
                    else if (userRequest == DialogResult.Ignore)
                    {
                        ((MainForm)FormRoot).lDownloading.Visible = false;
                        ((MainForm)FormRoot).pbDownloading.Visible = false;
                        this._slipRequired = false;
                        return true;
                    }
                    // Otherwise Retry
                }

        }  // End of SlipPrinterOK

        private void ResetScreen()
        {

            // Set the screen for initial entry and assuming non RF accounts
            errorProviderStoreCard.Clear();
            // Unlock any accounts previously locked by this screen
            this.UnlockAccounts();
            btnStoreCardManualEntry.Enabled = mtb_CardNo.ReadOnly = ((bool)Country[CountryParameterNames.StoreCardPasswordforManualEntry]);


            // Reset change events first before clearing all controls
            this._userChanged = false;
            this._lastCustId = "";
            this._lastAccountNo = "";
            this._lastCombinedRF = false;
            this._lastReceiptNo = "";
            this._lastPayMethod = "";
            this._lastCardRowNo = 1;
            this._lastTotalAmount = -1;
            this._lastFee = 0;
            this._lastPayAmount = 0;
            this._returnedChequeAuthorisedBy = 0; // CR 543 Peter Chong [27-Sep-2006]

            // Clear all controls
            ClearControls(this.Controls);
            ((MainForm)this.FormRoot).statusBar1.Text = "";

            // Initial custom settings

            // Customer group
            this.gbCustomer.Enabled = true;
            this._curSundryCredit = false;
            this.txtCustId.ReadOnly = false;
            this.txtAccountNo.ReadOnly = false;
            this.txtAccountNo.PreventPaste = false;
            this.btnSearchAccount.Enabled = true;
       //     this.btnServiceSearch.Enabled = true;
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.BackColor = SystemColors.Window;
            this.lNextPayDue.Enabled = true;
            this.txtNextPaymentDue.Enabled = true;
            this.lPrivilegeClub.Visible = false;
            this.addressTab1.txtAddress1.BackColor = SystemColors.Window;
            this.addressTab1.cmbVillage.BackColor = SystemColors.Window;
            this.lTransactionCount.Visible = false;
            this.lServiceRequestNo.Visible = false;
            // Account list group
            this.gbAccountList.Enabled = false;
            this.dgAccountList.TableStyles.Clear();
            this.dgAccountList.DataSource = null;
            this.dgAccountList.ResetText();

            // Selected Account group
            this.gbSelectedAccount.Enabled = false;
            this._curAccount = null;
            this.tpDetails.Selected = true;
            this._curRFTransactionSet = null;
            this._curAccountTransactionSet = null;
            this._paymentSet = null;
            this._curReceipt = null;
            this._debitFee = false;
            this._calculatedFee = 0;
            this._curChequeClearance = DateTime.MinValue;
            this.SetRFDisplay(false);
            this.lArrears.Text = GetResource("T_ARREARS");
            this.txtArrears.ForeColor = Color.Black;
            this.txtCurrentStatus.BackColor = Color.White;
            this.txtRebate.Enabled = false;
            this.lRebate.Enabled = false;
            this.txtSettlement.Enabled = false;
            this.lSettlement.Enabled = false;
            this.lFreeInstalment.Visible = false;
            this.lFullyDelivered.Visible = false;
            this.tpAllocation.Enabled = false;
            this.menuPaymentCard.Enabled = false;
            this.txtBadDebtBalance.Visible = false;
            this.txtBadDebtCharges.Visible = false;
            this.lBadDebtBalance.Visible = false;
            this.lBadDebtCharges.Visible = false;
            this.gbBadDebt.Visible = false;     //CR1084
            this.gbPromiseToPay.Visible = false;    //CR1084

            // Payment group
            this.gbNewPayment.Enabled = false;
            this.lTotalAmount.Visible = false;
            this.txtTotalAmount.Visible = false;
            this.lFee.Visible = false;
            this.txtFee.Visible = false;
            this.txtPayAmount.ReadOnly = false;
            this.txtPayAmount.Enabled = true;
            this.SetPayMethod(false);
            //this.txtCardRowNo.Value = 1;           //IP - 18/05/12 - #9445 - CR1239
            this.lblCashLoan.Visible = false;       // #8486 jec 27/10/11

            //this.SetCardPrint();                  //IP - 18/05/12 - #9445 - CR1239
            menuTaxInvoice.Enabled = false;
            menuStatement.Enabled = false;

            //PaymentHoliday group
            txtNewDueDate.BackColor = SystemColors.Window;
            dgPaymentHolidays.DataSource = null;
            _paymentHolidays = null;

            // Initial focus
            this.txtCustId.Focus();
            // Enable change events
            this._userChanged = true;
            CheckLogo(false);
            txtNextPaymentDue.BackColor = System.Drawing.SystemColors.Window;      //CR1084

            errorProvider1.SetError(txtPayAmount, "");

            errorProvider1.SetError(btnStoreCardManualEntry, ""); //IP - 02/12/10 - Store Card
            mtb_CardNo.Text = string.Empty;          //IP - 02/12/10 - Store Card
            mtb_CardNo.ResetText();                     //IP - 25/01/11 - Sprint 5.9 - #2785
            btn_calc.Enabled = false;
            cbMiniStat.Checked = PrintAutomaticMiniStatement;   //IP - 18/05/12 - #9445 - CR1239

        }  // End of ResetScreen

        private bool ValidMoneyField(TextBox moneyField, out decimal moneyValue)
        {
            // Check a blank or zero money value entered
            moneyValue = 0.0M;
            moneyField.Text = moneyField.Text.Trim();
            if (!IsStrictMoney(moneyField.Text))
            {
                ShowInfo("M_NUMERIC");
                // Trap the focus in this field
                moneyField.Focus();
                return false;
            }

            // Reformat
            moneyValue = MoneyStrToDecimal(moneyField.Text);
            moneyField.Text = moneyValue.ToString(DecimalPlaces);

            return true;
        }  // End of ValidMoneyField

        private void SetRFDisplay(bool DisplayRF)
        {
            // Display the account number or the 'RF Combined' header
            this._combinedRF = DisplayRF;
            this.lAcctNo.Visible = !DisplayRF;
            this.txtSelectedAcctNo.Visible = !DisplayRF;
            this.lCombinedRF.Visible = DisplayRF;

            //IP - 05/01/12 - #9380
            var set = false;

            if (drpPayMethod.SelectedIndex >= 0)
            {
                set = PayMethod.IsPayMethod(Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()), PayMethod.StoreCard);
            }

            // For RF disable the fee field - fees are only changed in the popup
            //SetFeeforStoreCard(PayMethod.IsPayMethod(Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()), PayMethod.StoreCard), DisplayRF);
            SetFeeforStoreCard(set, DisplayRF);

            this.btnPaymentList.Visible = DisplayRF;
            this.btnPaymentList.Enabled = DisplayRF;

            // Display the Available Spend for RF
            // M186 - Require the Available Spend field to be displayed on the payments 
            //		  screen instead of Add To Potential for RF accounts
            // Note: this is based on the account type only, which may not be part of
            //       a Combined RF set

            bool displayAvailableSpend = false;

            if (this._curAccount != null)
                displayAvailableSpend = (this._curAccount[CN.AccountType].ToString().Trim() == AT.ReadyFinance);

            if (displayAvailableSpend)
            {
                lAddTo.Text = GetResource("T_AVAILABLESPEND");
                txtAddToAmount.Text = this._availableSpend.ToString(DecimalPlaces);
            }
            else
            {
                lAddTo.Text = GetResource("T_ADDTOPOTENTIAL");
                txtAddToAmount.Text = this._addToAmount.ToString(DecimalPlaces);
            }
        }  // End of SetRFDisplay

        private void SetNonPrinted(bool nonPrintedOnly)
        {
            string rowFilter = "";
            this.cbNonPrinted.Checked = nonPrintedOnly;
            if (nonPrintedOnly)
            {
                rowFilter = CN.TransPrinted + " = 'N'";
            }
            ((DataView)dgTransactionList.DataSource).RowFilter = rowFilter;
            this.lTransactionCount.Text = Convert.ToString(((DataView)dgTransactionList.DataSource).Count) + GetResource("T_TRANSACTIONS");
        }  // End of SetNonPrinted

        private void SetPayMethod(bool accountSelected)
        {

            // Make sure this event has been fired when it is useful
            if (drpPayMethod.SelectedIndex < 0 ||
                drpPayMethod.SelectedValue.ToString() == this._lastPayMethod)
            {
                return;
            }


            this._lastPayMethod = drpPayMethod.SelectedValue.ToString();
            int curPayMethod = Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString());

            bool payByEntered = (curPayMethod != 0);
            // For cash payments disable the bank details and display the change field
            bool payByCash = PayMethod.IsPayMethod(curPayMethod, PayMethod.Cash);
            // When paying by cheque enable the Print Account Number button
            bool payByCheque = PayMethod.IsPayMethod(curPayMethod, PayMethod.Cheque);
            // When paying with a card enable the Card Type
            bool payByCard = (PayMethod.IsPayMethod(curPayMethod, PayMethod.CreditCard) || PayMethod.IsPayMethod(curPayMethod, PayMethod.DebitCard));
            // When paying with a Gift Voucher disable the payment amounts
            bool payByGift = PayMethod.IsPayMethod(curPayMethod, PayMethod.GiftVoucher);
            // All methods of Foreign payments must be able to give change
            bool payByForeign = (curPayMethod >= CAT.FPMForeignCurrency);

            bool payByScoreCard = (PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard)); //IP - 29/11/10 - Score Card

            this.txtTotalAmount.ReadOnly = payByGift;
            this.txtPayAmount.ReadOnly = payByGift;
            this.lTendered.Visible = payByCash || payByForeign;
            this.txtTendered.Visible = payByCash || payByForeign;
            this.lChange.Visible = payByCash || payByForeign;
            this.txtChange.Visible = payByCash || payByForeign;
            this.lCardNo.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque); //IP- 23/05/12 - #10173 - added check for payByCheque //IP - 29/11/10 - Score Card - Added check on pay method ScoreCard
            this.txtCardNo.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque) && !payByGift; //IP- 23/05/12 - #10173 //IP - 05/01/12 - #8860
            this.txtCardNo.Visible = !payByCash && !payByScoreCard && payByEntered && !payByCard && (!payByForeign || payByCheque);//IP- 23/05/12 - #10173 //IP - 25/01/11 - Sprint 5.9 - #2785 - Only make visible if payment method is cheque
            this.mtb_CardNo.Visible = !payByCash && !payByScoreCard && !payByCheque && payByEntered && !payByForeign && !payByGift; //IP - 05/01/12 - #8860  //IP - 25/01/11 - Sprint 5.9 - #2785 - Only make visible if payment method is credit/debit card
            this.lBank.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque); //IP- 23/05/12 - #10173
            this.drpBank.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque) && !payByGift; //IP- 23/05/12 - #10173   //IP - 05/01/12 - #8860
            this.lBankAcctNo.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque);
            this.txtBankAcctNo.Enabled = !payByCash && !payByScoreCard && payByEntered && (!payByForeign || payByCheque) && !payByGift; //IP- 23/05/12 - #10173 //IP - 05/01/12 - #8860
            this.btnPrintAcctNo.Enabled = payByCheque;
            this.lCardType.Enabled = payByCard;
            this.drpCardType.Enabled = payByCard;
            this.btnPay.Enabled = !payByScoreCard;      // #10126

            // When paying by cheque warn the user of each delivery date that
            // is before the earliest cheque clearance date.
            if (this._userChanged && payByCheque)
            {
                // Get a list of any items to be delivered before cheque clearance
                DataSet deliverySet = PaymentManager.CheckDeliveryDate(this.txtSelectedAcctNo.Text, Config.CountryCode, out this._curChequeClearance, out error);

                if (error.Length > 0)
                {
                    ShowError(error);
                    return;
                }

                // Set row filter to remove line items not required.
                string rowFilter = CN.Price + " > 0 and " +
                    CN.DateReqDel + " <= '" + this._curChequeClearance.ToShortDateString() + "' and " +
                    CN.DateReqDel + " not = '" + DateTime.MinValue.AddYears(1899).ToShortDateString() + "' and " +
                    CN.Qtydiff + " = 'N'";

                DataView deliveryView = deliverySet.Tables[0].DefaultView;
                deliveryView.RowFilter = rowFilter;

                if (deliveryView.Count > 0)
                {
                    //ShowInfo("M_CHEQUECLEARANCE", new Object[]{deliveryView[0][CN.ItemNo].ToString().Trim(), deliveryView[0][CN.ItemDescr1].ToString().Trim(), this._curChequeClearance.ToShortDateString()}, MessageBoxButtons.YesNo);

                    foreach (DataRowView deliveryRow in deliveryView)
                    {
                        // Display a confirmation prompt for each item delivered before cheque clearance
                        if (DialogResult.No == ShowInfo("M_CHEQUECLEARANCE", new Object[] { deliveryRow[CN.ItemNo].ToString().Trim(), deliveryRow[CN.ItemDescr1].ToString().Trim(), this._curChequeClearance.ToShortDateString() }, MessageBoxButtons.YesNo))
                        {
                            this.drpPayMethod.SelectedIndex = 0;
                            return;
                        }
                        return;
                    }
                }

                //If all items confirmed check returned cheque history			
                //CR 543 Cancelled Cheque History - Peter Chong [25-Sep-2006]

                //Check the account type.  Dialog only to be shown if Cash or HP
                string accType = this._curAccount[CN.AccountType].ToString().Trim();
                if (accType != AT.Special)
                {
                    //Check to see if returned cheques exceed specified criteria.
                    bool authorisationRequired = false;
                    DataSet dsReturnedCheques = base.PaymentManager.GetPaymentReturnedCheques(txtCustId.Text, out authorisationRequired, out this.error);

                    if (error.Length > 0)
                    {
                        ShowError(error);
                        return;
                    }

                    if (dsReturnedCheques != null)
                    {
                        if (authorisationRequired && dsReturnedCheques.Tables[TN.ReturnedCheques].Rows.Count > 0)
                        {
                            ChequeAuthorisationPopUp chequeAuthPopop = new ChequeAuthorisationPopUp(this.FormRoot, this);
                            //Display a dialog
                            chequeAuthPopop.ReturnedCheques = dsReturnedCheques;
                            chequeAuthPopop.ShowDialog(this);

                            if (!chequeAuthPopop.Authorised)
                            {

                                //Set the payment option back to cash
                                drpPayMethod.SelectedValue = PayMethod.Cash;
                                //UAT Issue 53 - request to make payment field disabled if cheque authorisation cancelled
                                if (chequeAuthPopop.Cancelled == true)
                                {
                                    txtPayAmount.Enabled = false;
                                }
                                return;
                            }
                            else
                            {
                                //If Authorised allow payment
                                this._returnedChequeAuthorisedBy = chequeAuthPopop.AuthorisingUser;
                            }


                        }
                    }

                }

            }

            // Might need to use the Foreign Currency Calculator
            if (!accountSelected && payByForeign)
            {
                // Call the Foreign Currency Calculator popup form
                this.ExchangeCalculator();
                this.btnExchange.Visible = true;
            }
            else
            {
                this.btnExchange.Visible = false;
            }

            // Securitised accounts cannot pay with a Gift Voucher
            if (payByGift && this._curAccount[CN.Securitised].ToString() == "Y")
            {
                ShowInfo("M_NOGIFTMETHOD");
                this.drpPayMethod.SelectedIndex = 0;
            }
            else
            {

                /* throw up the gift voucher popup if necessary */
                if (!accountSelected && payByGift)
                {
                    GiftVoucher gv = new GiftVoucher(this, FormRoot, false);
                    gv.ShowDialog();
                    if (GiftVoucherValue == 0)
                        drpPayMethod.SelectedIndex = 0;
                    else
                    {
                        txtTotalAmount.Text = this.txtTendered.Text = GiftVoucherValue.ToString(DecimalPlaces);
                        this.txtTotalAmount_Leave(null, null);
                        this.btnCopyInstallment.Visible = false;
                    }
                    this.btnCopyAgreementTotal.Visible = false;
                }
                else
                {
                    _giftVoucherValue = 0;
                    CourtsVoucher = true;
                    VoucherReference = "";
                    VoucherAuthorisedBy = 0;
                    VoucherCompanyAcctNo = "";
                    this.btnCopyInstallment.Visible = true;
                    if (_curAccount != null)
                        if (Convert.ToString(_curAccount[CN.AccountType]).Trim() == AT.Cash)
                        {
                            this.btnCopyAgreementTotal.Visible = true;
                            btnCopyAgreementTotal.BringToFront();
                        }

                }
            }

            //if (drpPayMethod.Text == StoreCard)

            SetStoreCardControls(PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard));

        }  // End of SetPayMethod

        private void ExchangeCalculator()
        {
            // Call the Foreign Currency Calculator popup form
            int curPayMethod = Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString());
            ExchangeCalculator ExchangeCalculatorPopup = new ExchangeCalculator(this.FormRoot, this, curPayMethod, MoneyStrToDecimal(this.txtTendered.Text, DecimalPlaces));
            ExchangeCalculatorPopup.ShowDialog();
            if (ExchangeCalculatorPopup.convert)
            {
                this._lastPayMethod = ExchangeCalculatorPopup.payMethod;
                this.drpPayMethod.SelectedValue = this._lastPayMethod;
                this.txtTendered.Text = ExchangeCalculatorPopup.newAmount.ToString(DecimalPlaces);
                // RM - 26/08/09 71004 set the pay amount and total amount so these do not need to be entered and change is calculated correctly
                this.txtPayAmount.Text = this.txtTendered.Text;
                this.txtTotalAmount.Text = this.txtPayAmount.Text;
                // Invoke leave method
                this.txtTendered_Validating(new object(), new System.ComponentModel.CancelEventArgs());
            }
        }

        //IP - 18/05/12 - #9445 - CR1239 - No longer require payment card
        //private void SetCardPrint()
        //{
        //    if (this._curAccount != null)
        //    {
        //        bool canPrintCard = AT.IsCreditType(_curAccount[CN.AccountType].ToString().Trim());

        //        this.cbCardPrint.Checked = (canPrintCard && this.cbCardPrint.Checked);
        //        this.cbCardPrint.Enabled = canPrintCard;
        //        this.lCardPrint.Enabled = canPrintCard;
        //        this.menuPaymentCard.Enabled = canPrintCard;
        //    }
        //    this.lCardRowNo.Enabled = this.cbCardPrint.Checked;
        //    this.txtCardRowNo.Enabled = this.cbCardPrint.Checked;
        //    this.btnUpdateCard.Enabled = this.cbCardPrint.Checked;
        //}  // End of SetCardPrint

        //private int MaxCardLines()
        //{
        //    if (PaymentCardType == 1)
        //    {
        //        // Long card type (30 lines)
        //        return CardType.rowsLong;
        //    }
        //    else
        //    {
        //        // Short card type (27 lines)
        //        return CardType.rowsShort;
        //    }
        //}  // End of MaxCardLines

        //IP - 18/05/12 - #9445 - CR1239 - No longer require payment card
        //private void CheckCardRow()
        //{
        //    // Make sure this event has been fired when it is useful
        //    if (!this._userChanged ||
        //        this.txtCardRowNo.Value == this._lastCardRowNo)
        //    {
        //        return;
        //    }
        //    this._lastCardRowNo = this.txtCardRowNo.Value;

        //    int maxLines = this.MaxCardLines();

        //    if (this.txtCardRowNo.Value < 1 || this.txtCardRowNo.Value > maxLines)
        //    {
        //        ShowInfo("M_CARDPRINT", new Object[] { maxLines });
        //        if (this.txtCardRowNo.Value < 1)
        //        {
        //            this.txtCardRowNo.Value = 1;
        //        }
        //        else
        //        {
        //            this.txtCardRowNo.Value = maxLines;
        //        }
        //        this.txtCardRowNo.Focus();
        //    }

        //}  // End of CheckCardRow

        private void CheckReceiptAllocation()
        {
            if (this._curReceipt == null) return;

            if (this._curReceipt[CN.EmployeeNo].ToString().Trim().Length != 0 &&
                this.txtEmployeeNo.Text.Trim().Length != 0)
            {
                // Check the Account and the Receipt are allocated to the same employee
                int curReceiptEmployeeNo = Convert.ToInt32(this._curReceipt[CN.EmployeeNo].ToString());
                int curAccountEmployeeNo = Convert.ToInt32(this.txtEmployeeNo.Text);
                if (curReceiptEmployeeNo != curAccountEmployeeNo)
                {
                    if (DialogResult.No == ShowInfo("M_DIFFERENTALLOCATION", new Object[] { this.txtReceiptNo.Text, curReceiptEmployeeNo, curAccountEmployeeNo }, MessageBoxButtons.YesNo))
                    {
                        this.txtReceiptNo.Text = "";
                        this._lastReceiptNo = "";
                        this.txtReceiptNo.Focus();
                    }
                }
            }
        }  // End of CheckReceiptAllocation

        public bool LoadData(string CustId, bool notifyFreeInstalment)
        {
            int lockCount = 0;
            string statusText = GetResource("M_ACCOUNTSZERO");
            DataSet accountSet = null;
            DataView AccountListView = null;
            otherArrears = false;    //CR1084
            errorProviderStoreCard.Clear();
            // Make sure the screen is reset
            //ResetScreen();
            LoyaltyGetCharges(CustId);
            txtCustId.Text = CustId;
            ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_LOADINGDATA");

            PaymentCardType = AccountManager.GetPaymentCardType((short)Convert.ToInt32(Config.BranchCode), StaticDataManager.GetServerDate(), out error);

            // Load the Customer details and the list of Account details
            accountSet = PaymentManager.GetPaymentAccounts(txtCustId.Text, Config.CountryCode, true, out this._addToAmount, out error);
            txtAddToAmount.Text = this._addToAmount.ToString(DecimalPlaces);

            if (error.Length > 0)
            {
                ShowError(error);
                return false;
            }


            foreach (DataTable PaymentDetails in accountSet.Tables)
            {
                if (PaymentDetails.TableName == TN.Customer)
                {
                    // Check the customer exists
                    if (PaymentDetails.Rows.Count == 0)
                    {
                        ShowInfo("M_NOSUCHCUSTOMER");
                        this.ResetScreen();
                        this.txtCustId.Focus();
                        return false;
                    }

                    // Display Customer details
                    txtCustomerName.Text = PaymentDetails.Rows[0][CN.Title].ToString() + ' ' +
                        PaymentDetails.Rows[0][CN.FirstName].ToString() + ' ' +
                        PaymentDetails.Rows[0][CN.LastName].ToString();

                    // The user can change the name printed for Sundry Credit accounts
                    if (PaymentDetails.Rows[0][CN.LastName].ToString() == "SUNDRY CREDIT")
                    {
                        this._curSundryCredit = true;
                        this.txtCustomerName.ReadOnly = false;
                        this.txtCustomerName.BackColor = Color.Salmon;
                    }

                    if (PaymentDetails.Rows[0][CN.PrivilegeClub].ToString() == "1")
                    {
                        if ((bool)Country[CountryParameterNames.TierPCEnabled])
                        {
                            lPrivilegeClub.Text = PaymentDetails.Rows[0][CN.PrivilegeClubDesc].ToString();
                        }
                        lPrivilegeClub.Visible = true;
                    }

                    // When address is blank display "AWAITING CUSTOMER ADDRESS DETAILS"
                    if ((PaymentDetails.Rows[0][CN.cusaddr1].ToString().Trim() +
                        PaymentDetails.Rows[0][CN.cusaddr2].ToString().Trim() +
                        PaymentDetails.Rows[0][CN.cusaddr3].ToString().Trim() +
                        PaymentDetails.Rows[0][CN.cuspocode].ToString().Trim()) == "")
                    {
                        addressTab1.txtAddress1.Text = GetResource("M_ADDRESSBLANK1");
                        addressTab1.cmbVillage.Text = GetResource("M_ADDRESSBLANK2");
                        addressTab1.txtAddress1.BackColor = Color.Salmon;
                        addressTab1.cmbVillage.BackColor = Color.Salmon;
                    }
                    else
                    {
                        addressTab1.txtAddress1.Text = PaymentDetails.Rows[0][CN.cusaddr1].ToString();
                        if (addressTab1.cmbVillage.Items.Count > 0 &&
                                   PaymentDetails.Rows[0][CN.cusaddr1] != DBNull.Value) // Address Standardization CR2019 - 025
                        {
                            var villageIndex = addressTab1.cmbVillage.FindStringExact(PaymentDetails.Rows[0][CN.cusaddr2].ToString());
                            if (villageIndex != -1)
                                addressTab1.cmbVillage.SelectedIndex = villageIndex;
                            else
                                addressTab1.cmbVillage.SelectedText = PaymentDetails.Rows[0][CN.cusaddr2].ToString();
                        }
                        else if (PaymentDetails.Rows[0][CN.cusaddr1] != DBNull.Value)
                        {
                            addressTab1.cmbVillage.Text = string.Empty;
                            addressTab1.cmbVillage.SelectedText = PaymentDetails.Rows[0][CN.cusaddr2].ToString(); // Address Standardization CR2019 - 025
                        }
                        if (addressTab1.cmbRegion.Items.Count > 0 &&
                            PaymentDetails.Rows[0][CN.cusaddr3] != DBNull.Value) // Address Standardization CR2019 - 025
                        {
                            var regionIndex = addressTab1.cmbRegion.FindStringExact(PaymentDetails.Rows[0][CN.cusaddr3].ToString());
                            if (regionIndex != -1)
                                addressTab1.cmbRegion.SelectedIndex = regionIndex;
                            else
                                addressTab1.cmbRegion.SelectedText = PaymentDetails.Rows[0][CN.cusaddr3].ToString();
                        }
                        else if (PaymentDetails.Rows[0][CN.cusaddr3] != DBNull.Value) // Address Standardization CR2019 - 025
                        {
                            addressTab1.cmbRegion.Text = string.Empty;
                            addressTab1.cmbRegion.SelectedText = PaymentDetails.Rows[0][CN.cusaddr3].ToString();
                        }
                        addressTab1.txtPostCode.Text = PaymentDetails.Rows[0][CN.cuspocode].ToString();
                            if (!Convert.IsDBNull(PaymentDetails.Rows[0][CN.Latitude]) && !Convert.IsDBNull(PaymentDetails.Rows[0][CN.Longitude])) // Address Standardization CR2019 - 025
                            addressTab1.txtCoordinate.Text = string.Format("{0},{1}", PaymentDetails.Rows[0]["Latitude"].ToString(), PaymentDetails.Rows[0]["Longitude"].ToString());
                        else
                            addressTab1.txtCoordinate.Text = string.Empty;
                        addressTab1.Enable = false; // Address Standardization CR2019 - 025                 
                    }
                }

                else if (PaymentDetails.TableName == TN.Accounts)
                {
                    // Display list of Customer Accounts
                    statusText = PaymentDetails.Rows.Count + GetResource("M_ACCOUNTSLISTED");

                    // Add ratio columns for RF spread
                    PaymentDetails.Columns.AddRange(
                        new DataColumn[] { new DataColumn(CN.RatioPay, Type.GetType("System.Decimal")) });

                    // Check for accounts already locked
                    foreach (DataRow accountRow in PaymentDetails.Rows)
                    {
                        if (accountRow[CN.LockedBy].ToString().Length > 0)
                            lockCount++;
                    }
                    if (lockCount > 0)
                    {
                        ShowInfo("M_LOCKEDACCOUNTS", new Object[] { lockCount, this.txtCustId.Text });
                    }

                    AccountListView = new DataView(PaymentDetails);
                    AccountListView.AllowNew = false;
                    AccountListView.Sort = CN.acctno + " ASC ";
                    LoyaltyUpdateHCC(ref AccountListView);
                    dgAccountList.DataSource = AccountListView;

                    decimal totalArrears = 0;
                    DateTime nextDue = new DateTime(3000, 1, 1);
                    foreach (DataRowView accountRow in AccountListView)
                    {
                        // Round all numerc values to avoid rounding errors later on
                        accountRow[CN.AgreementTotal] = Math.Round((decimal)accountRow[CN.AgreementTotal], this._precision);
                        accountRow[CN.Arrears] = Math.Round((decimal)accountRow[CN.Arrears], this._precision);
                        accountRow[CN.OutstandingBalance] = Math.Round((decimal)accountRow[CN.OutstandingBalance], this._precision);
                        accountRow[CN.InstalAmount] = Math.Round((decimal)accountRow[CN.InstalAmount], this._precision);
                        accountRow[CN.Rebate] = Math.Round((decimal)accountRow[CN.Rebate], this._precision);
                        accountRow[CN.CollectionFee] = Math.Round((decimal)accountRow[CN.CollectionFee], this._precision);
                        accountRow[CN.SettlementFigure] = Math.Round((decimal)accountRow[CN.SettlementFigure], this._precision);
                        accountRow[CN.ToFollowAmount] = Math.Round((decimal)accountRow[CN.ToFollowAmount], this._precision);
                        accountRow[CN.CalculatedFee] = Math.Round((decimal)accountRow[CN.CalculatedFee], this._precision);
                        accountRow[CN.BailiffFee] = Math.Round((decimal)accountRow[CN.BailiffFee], this._precision);
                        accountRow[CN.BDWBalance] = Math.Round((decimal)accountRow[CN.BDWBalance], this._precision);
                        accountRow[CN.BDWCharges] = Math.Round((decimal)accountRow[CN.BDWCharges], this._precision);

                        // Total arrears
                        totalArrears += (decimal)accountRow[CN.Arrears];
                        //Other accounts in arrears     CR1084 jec
                        if ((decimal)accountRow[CN.Arrears] > 0 && (string)accountRow[CN.acctno] != txtAccountNo.Text.Replace("-", ""))
                        {
                            otherArrears = true;
                        }

                        // Get earliest due date
                        if ((DateTime)accountRow[CN.DateFirst] != new DateTime(1900, 1, 1) &&
                            (DateTime)accountRow[CN.DateFirst] < nextDue)
                            nextDue = (DateTime)accountRow[CN.DateFirst];
                    }
                    txtTotalArrears.Text = totalArrears.ToString(DecimalPlaces);

                    if (totalArrears < 0.0M)
                        this.txtTotalArrears.ForeColor = Color.Green;
                    else if (totalArrears > 0.0M)
                        this.txtTotalArrears.ForeColor = Color.Red;
                    else
                        this.txtTotalArrears.ForeColor = Color.Black;

                    if (dgAccountList.TableStyles.Count == 0)
                    {
                        DataGridTableStyle tabStyle = new DataGridTableStyle();
                        tabStyle.MappingName = AccountListView.Table.TableName;

                        int numCols = AccountListView.Table.Columns.Count;

                        //add an extra column at the end of our customers table
                        AccountListView.Table.Columns.Add("Icon");
                        DataGridTextBoxColumn aColumnTextColumn;
                        for (int i = 0; i < numCols; ++i)
                        {
                            if (i == 0)  //add an unbound stand-alone icon column
                            {
                                DataGridIconColumn iconColumn = new DataGridIconColumn(imageList1.Images[0], CN.LockedBy, "");
                                iconColumn.HeaderText = "";
                                iconColumn.MappingName = "Icon";
                                iconColumn.Width = imageList1.Images[0].Size.Width;
                                tabStyle.GridColumnStyles.Add(iconColumn);
                            }

                            aColumnTextColumn = new DataGridTextBoxColumn();
                            aColumnTextColumn.HeaderText = AccountListView.Table.Columns[i].ColumnName;
                            aColumnTextColumn.MappingName = AccountListView.Table.Columns[i].ColumnName;
                            tabStyle.GridColumnStyles.Add(aColumnTextColumn);
                        }

                        dgAccountList.TableStyles.Clear();
                        dgAccountList.TableStyles.Add(tabStyle);
                        dgAccountList.DataSource = AccountListView;

                        // Hidden columns
                        if (lockCount == 0) tabStyle.GridColumnStyles["Icon"].Width = 0;
                        tabStyle.GridColumnStyles[CN.LockedBy].Width = 0;
                        tabStyle.GridColumnStyles[CN.SegmentID].Width = 0;
                        tabStyle.GridColumnStyles[CN.EmployeeNo].Width = 0;
                        tabStyle.GridColumnStyles[CN.EmployeeName].Width = 0;
                        tabStyle.GridColumnStyles[CN.Status].Width = 0;
                        tabStyle.GridColumnStyles[CN.DateFirst].Width = 0;
                        tabStyle.GridColumnStyles[CN.DeliveredIndicator].Width = 0;
                        tabStyle.GridColumnStyles[CN.DeliveryFlag].Width = 0;
                        tabStyle.GridColumnStyles[CN.ToFollowAmount].Width = 0;
                        tabStyle.GridColumnStyles[CN.DebitAccount].Width = 0;
                        tabStyle.GridColumnStyles[CN.CalculatedFee].Width = 0;
                        tabStyle.GridColumnStyles[CN.RatioPay].Width = 0;
                        tabStyle.GridColumnStyles[CN.FreeInstalment].Width = 0;
                        tabStyle.GridColumnStyles[CN.Securitised].Width = 0;
                        tabStyle.GridColumnStyles[CN.PaymentHoliday].Width = 0;
                        tabStyle.GridColumnStyles[CN.AgrmtNo].Width = 0;
                        tabStyle.GridColumnStyles[CN.PaymentHolidayMin].Width = 0;
                        tabStyle.GridColumnStyles[CN.PaymentCardLine].Width = 0;
                        tabStyle.GridColumnStyles[CN.DateAcctOpen].Width = 0;
                        tabStyle.GridColumnStyles[CN.ServiceRequestNoStr].Width = 0;
                        tabStyle.GridColumnStyles[CN.Internal].Width = 0;

                        // Displayed columns
                        tabStyle.GridColumnStyles[CN.acctno].Width = 100;
                        tabStyle.GridColumnStyles[CN.acctno].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.acctno].HeaderText = GetResource("T_ACCOUNTNO");

                        tabStyle.GridColumnStyles[CN.AccountType].Width = 30;
                        tabStyle.GridColumnStyles[CN.AccountType].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.AccountType].HeaderText = GetResource("T_TYPE");

                        tabStyle.GridColumnStyles[CN.AgreementTotal].Width = 90;
                        tabStyle.GridColumnStyles[CN.AgreementTotal].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.AgreementTotal].HeaderText = GetResource("T_AGREETOTAL");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.AgreementTotal]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.Arrears].Width = 90;
                        tabStyle.GridColumnStyles[CN.Arrears].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.Arrears].HeaderText = GetResource("T_ARREARS");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.Arrears]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.OutstandingBalance].Width = 90;
                        tabStyle.GridColumnStyles[CN.OutstandingBalance].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.OutstandingBalance].HeaderText = GetResource("T_OUTBAL");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.OutstandingBalance]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.InstalAmount].Width = 90;
                        tabStyle.GridColumnStyles[CN.InstalAmount].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.InstalAmount].HeaderText = GetResource("T_INSTAL");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.InstalAmount]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.Rebate].Width = 90;
                        tabStyle.GridColumnStyles[CN.Rebate].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.Rebate].HeaderText = GetResource("T_REBATE");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.Rebate]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.CollectionFee].Width = 90;
                        tabStyle.GridColumnStyles[CN.CollectionFee].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.CollectionFee].HeaderText = GetResource("T_FEE");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.CollectionFee]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.SettlementFigure].Width = 90;
                        tabStyle.GridColumnStyles[CN.SettlementFigure].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.SettlementFigure].HeaderText = GetResource("T_SETTLEMENT");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.SettlementFigure]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.BailiffFee].Width = 90;
                        tabStyle.GridColumnStyles[CN.BailiffFee].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.BailiffFee].HeaderText = GetResource("T_BAILFEE");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.BailiffFee]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.BDWBalance].Width = 90;
                        tabStyle.GridColumnStyles[CN.BDWBalance].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.BDWBalance].HeaderText = GetResource("T_BDWBALANCE");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.BDWBalance]).Format = DecimalPlaces;

                        tabStyle.GridColumnStyles[CN.BDWCharges].Width = 90;
                        tabStyle.GridColumnStyles[CN.BDWCharges].ReadOnly = true;
                        tabStyle.GridColumnStyles[CN.BDWCharges].HeaderText = GetResource("T_BDWCHARGES");
                        ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.BDWCharges]).Format = DecimalPlaces;

                        // Loop through the controls that belong to the datagrid
                        // to enable the scroll bars, as they are being disabled,
                        // possibly becasue of the length of the results grid.
                        foreach (Control c in dgAccountList.Controls)
                        {
                            if (c.GetType().Name == "HScrollBar" || c.GetType().Name == "VScrollBar")
                            {
                                c.Enabled = true;
                            }
                        }
                    }
                }
            }

            // Retrieve the Available Spend
            DataSet RFCombinedSet = CustomerManager.GetRFCombinedDetails(this.txtCustId.Text, out error);

            if (error.Length > 0)
            {
                ShowError(error);
            }
            else
            {
                this._availableSpend = Convert.ToDecimal(RFCombinedSet.Tables[TN.RFDetails].Rows[0][CN.AvailableCredit]);
                DateTime dueDate = new DateTime(3000, 1, 1);
                dueDate = Convert.ToDateTime(RFCombinedSet.Tables[TN.RFDetails].Rows[0][CN.DateNextPaymentDue]);

                if (dueDate != new DateTime(1900, 1, 1))
                {
                    txtNextPaymentDue.Text = dueDate.ToShortDateString();  // UAT 65 RD 06/09/06
                    this.lNextPayDue.Enabled = true;
                    txtNextPaymentDue.Enabled = true;
                }
                else
                {
                    // Dim a blank due date
                    txtNextPaymentDue.Text = "";
                    this.lNextPayDue.Enabled = false;
                    txtNextPaymentDue.Enabled = false;
                }
            }

            // Disable the Customer fields and enable the Account list
            this.txtCustId.ReadOnly = true;
            this.txtAccountNo.ReadOnly = true;
            this.txtAccountNo.PreventPaste = true;
            this.btnSearchAccount.Enabled = false;
        //    this.btnServiceSearch.Enabled = false;
            this.gbAccountList.Enabled = true;
            this.menuTaxInvoice.Enabled = true;
            menuStatement.Enabled = true;
            ((MainForm)this.FormRoot).statusBar1.Text = statusText;

            // Check for any free instalments due
            if (notifyFreeInstalment) this.NotifyFreeInstalments();

            WarrantySESPopup.ShowSESPopUp(CustId, this, false);
            int noSESPrompts = Convert.ToInt32(Country[CountryParameterNames.WarrantySESPrompts]);

            //if (noSESPrompts > 0)
            //{
            //    int daysAllowedAfterDelivery = Convert.ToInt32(Country[CountryParameterNames.WarrantyDays]);
            //    DataSet ds = CustomerManager.GetWarrantySecondEffortSolicitationItems(CustId, noSESPrompts, daysAllowedAfterDelivery, out Error);
            //    if (Error.Length > 0) ShowError(Error);


            //    if (ds.Tables.IndexOf(TN.WarrantyList) >= 0)
            //        if (ds.Tables[TN.WarrantyList].Rows.Count > 0)
            //        {
            //            WarrantySESPopup pu = new WarrantySESPopup(ds);
            //            pu.ShowDialog(this);
            //        }
            //}

            return true;

        }  // End of LoadData

        private bool CheckSelectAccount(int index)
        {
            bool check = false;
            DataView accountList = (DataView)dgAccountList.DataSource;
            string acctno = ((DataView)dgAccountList.DataSource)[index][CN.acctno].ToString();

            if (Convert.ToBoolean(Country[CountryParameterNames.LoyaltyScheme]))
            {
                if (LoyaltyAcct != null && LoyaltyAcct != "" && acctno != LoyaltyAcct && LoyaltyAmount != 0)
                {
                    MessageBox.Show("Payment on Home Club Account is required to be paid first.", "Please pay Home Club Account", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    for (int i = 0; i < accountList.Count; i++)
                    {
                        //accountRow = accountList[i].Row;
                        //accountRow[CN.RatioPay] = 0;

                        if (LoyaltyAcct == accountList[i].Row[CN.AccountNumber].ToString())    //UAT137 jec 20/05/10
                        {
                            dgAccountList.Select(i);
                        }
                        else
                        {
                            dgAccountList.UnSelect(i);
                        }
                    }
                }
                else
                {
                    if (acctno == LoyaltyAcct && LoyaltyAmount == 0)
                    {
                        MessageBox.Show("Payment on Home Club Account can not be paid when balance is not outstanding.", "Please pay Home Club Account", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        check = SelectAccount(index);
                        errorProvider1.SetError(txtPayAmount, "");
                    }
                }
            }
            return check;
        }

        private void NotifyFreeInstalments()
        {
            // Prompt the user with any free instalments due
            if ((bool)Country[CountryParameterNames.TierPCEnabled])
            {
                DataView accountList = (DataView)dgAccountList.DataSource;
                DataTable freeList = new DataTable();
                freeList.Columns.AddRange(
                    new DataColumn[] {	new DataColumn(CN.AccountNo),
										 new DataColumn(CN.OutstandingBalance,  Type.GetType("System.Decimal")),
										 new DataColumn(CN.Rebate,				Type.GetType("System.Decimal")),
										 new DataColumn(CN.SettlementFigure,	Type.GetType("System.Decimal")),
										 new DataColumn(CN.InstalAmount,		Type.GetType("System.Decimal")),
										 new DataColumn(CN.Status),
										 new DataColumn(CN.FreeInstalment,		Type.GetType("System.Decimal")),
										 new DataColumn(CN.ToDelete)
									 });

                // Check each account for a free instalment amount
                for (int i = 0; i < accountList.Count; i++)
                {
                    if ((int)accountList[i].Row[CN.FreeInstalment] >= 0.01M)
                    {
                        DataRow newRow = freeList.NewRow();
                        newRow[CN.AccountNo] = accountList[i][CN.acctno];
                        newRow[CN.OutstandingBalance] = accountList[i][CN.OutstandingBalance];
                        newRow[CN.Rebate] = accountList[i][CN.Rebate];
                        newRow[CN.SettlementFigure] = accountList[i][CN.SettlementFigure];
                        newRow[CN.InstalAmount] = accountList[i][CN.InstalAmount];
                        newRow[CN.Status] = accountList[i][CN.Status];
                        newRow[CN.FreeInstalment] = accountList[i][CN.FreeInstalment];
                        newRow[CN.ToDelete] = "N";
                        freeList.Rows.Add(newRow);
                    }
                }

                if (freeList.Rows.Count > 0)
                {
                    // Call the free instalment list popup
                    FreeInstalmentList FreeInstalmentListPopup = new FreeInstalmentList(this.txtCustId.Text, this.txtCustomerName.Text, freeList, this.FormRoot, this.FormParent);
                    FreeInstalmentListPopup.ShowDialog();
                    // Refresh the data
                    this.LoadData(txtCustId.Text, false);
                }
            }
        }  // End of NotifyFreeInstalments


        private bool SelectAccount(int piIndex)
        {
            int selectedCount = 0;
            decimal totAgreementTotal = 0.0M;
            decimal totArrears = 0.0M;
            decimal totOutstandingBalance = 0.0M;
            decimal totToFollow = 0.0M;
            decimal totInstalment = 0.0M;
            decimal totRebate = 0.0M;
            decimal totSettlement = 0.0M;
            decimal totBadDebtBalance = 0.0M;
            decimal totBadDebtCharges = 0.0M;
            int maxCurStatus = -2;
            int curStatus = -2;
            DateTime maxDateFirst = DateTime.MinValue;
            bool totFullyDelivered = false;
            bool totFreeInstalment = false;
            DataView accountList = (DataView)dgAccountList.DataSource;
            DataRow newAccount = accountList[piIndex].Row;


            // Disable the Account Details and Payment group boxes
            this.gbSelectedAccount.Enabled = false;
            this.gbNewPayment.Enabled = false;

            // Mark this row with the current row arrow
            this.dgAccountList.CurrentRowIndex = piIndex;
            CheckPaymentCalc(piIndex);          // #9919 jec 18/04/12

            // Check this account is not locked
            if (newAccount[CN.LockedBy].ToString().Length > 0)
            {
                ShowInfo("M_ACCOUNTLOCKED", new Object[] { newAccount[CN.acctno].ToString(), this.txtCustId.Text });
                return false;
            }
            // Only enable Tallyman launch if tallyman account  //UAT12 jec 16/03/10
            if (newAccount[CN.TallymanAcct].ToString() == "1")
            {
                btnLaunchTally.Enabled = true;
            }
            else
            {
                btnLaunchTally.Enabled = false;
            }

            // Ready Finance accounts are paid collectively so select all of the
            // RF accounts that have been delivered (i.e. Outstanding Balance > 0).
            // An RF account that has not been delivered will be treated as a
            // normal account to allow a deposit to be paid against this individual
            // account before delivery.
            this._combinedRF = (newAccount[CN.AccountType].ToString().Trim() == AT.ReadyFinance
                && (decimal)newAccount[CN.OutstandingBalance] >= 0.01M
                && newAccount[CN.DeliveryFlag].ToString() == "Y");

            //this.txtCardRowNo.Value = ((int)newAccount[CN.PaymentCardLine] + 1);                           //IP - 18/05/12 - #9445 - CR1239

            this.txtSegmentName.Text = newAccount[CN.SegmentName].ToString();   //CR1072 

            if (this._combinedRF)
            {
                // Check all delivered RF accounts in the list are not locked
                for (int i = 0; i < accountList.Count; i++)
                {
                    if (accountList[i].Row[CN.AccountType].ToString().Trim() == AT.ReadyFinance
                        && (decimal)accountList[i].Row[CN.OutstandingBalance] >= 0.01M
                        && accountList[i].Row[CN.DeliveryFlag].ToString() == "Y"
                        && accountList[i].Row[CN.LockedBy].ToString().Length > 0)
                    {
                        // This account is locked, so none of the set can be selected
                        ShowInfo("M_RFACCOUNTLOCKED", new Object[] { accountList[i].Row[CN.acctno].ToString(), this.txtCustId.Text });
                        return false;
                    }
                }

                // Select all delivered RF accounts in the list
                for (int i = 0; i < accountList.Count; i++)
                {
                    if (accountList[i].Row[CN.AccountType].ToString().Trim() == AT.ReadyFinance
                        && (decimal)accountList[i].Row[CN.OutstandingBalance] >= 0.01M
                        && accountList[i].Row[CN.DeliveryFlag].ToString() == "Y")
                    {
                        dgAccountList.Select(i);
                    }
                    else
                    {
                        dgAccountList.UnSelect(i);      //UAT1 jec 22/04/10
                    }
                }

                // If the RF set has been selected again then do not reset anything else
                if (this._lastCombinedRF)
                {
                    // Enable the Account Details and Payment group boxes
                    this.gbSelectedAccount.Enabled = true;
                    this.gbNewPayment.Enabled = true;
                    this.tpDetails.Selected = true;
                    return true;
                }
            }
            else
            {
                // Do not set up the same account again
                if (newAccount[CN.AccountNumber].ToString() == this._lastAccountNo)
                {
                    // Enable the Account Details and Payment group boxes
                    this.gbSelectedAccount.Enabled = true;
                    this.gbNewPayment.Enabled = true;
                    return true;
                }
            }


            // Select the new account
            this._lastAccountNo = newAccount[CN.AccountNumber].ToString();
            this._lastCombinedRF = this._combinedRF;
            this._curAccount = newAccount;
            dgAccountList.Select(piIndex);
            this.txtSelectedAcctNo.Text = this._curAccount[CN.AccountNumber].ToString();
            txtAccountNo.Text = this.txtSelectedAcctNo.Text;
            this._paymentSet = null;


            //IP - 21/05/12 - #10147 
            if (Convert.ToString(this._curAccount[CN.AccountType]).Trim() == AT.Cash)
            {
                cbMiniStat.Enabled = false;
                cbMiniStat.Checked = false;
            }
            else
            {
                cbMiniStat.Enabled = true;
                cbMiniStat.Checked = PrintAutomaticMiniStatement;
            }

            // Set the display for RF or not RF
            this.SetRFDisplay(this._combinedRF);

            // Display account details for one or more selected accounts
            DataRow accountRow;
            decimal maxTotalPay = 0.0M;
            //decimal maxTotalFee = 0.0M;
            //decimal curBDW = 0.0M;
            selectedCount = 0;
            for (int i = 0; i < accountList.Count; i++)
            {
                accountRow = accountList[i].Row;
                accountRow[CN.RatioPay] = 0;

                if (!this._combinedRF)      //IP - 02/06/10 - UAT(249) UAT(5.2.1.0) Log
                {
                    if (this._lastAccountNo == accountList[i].Row[CN.AccountNumber].ToString())    //UAT137 jec 20/05/10
                    {
                        dgAccountList.Select(i);
                    }
                    else
                    {
                        dgAccountList.UnSelect(i);
                    }
                }


                if (dgAccountList.IsSelected(i))
                {
                    // Sum values or find max values for all selected accounts
                    selectedCount++;
                    totAgreementTotal = Math.Round(totAgreementTotal + Convert.ToDecimal(accountRow[CN.AgreementTotal]), this._precision);
                    totArrears = Math.Round(totArrears + Convert.ToDecimal(accountRow[CN.Arrears]), this._precision);
                    totOutstandingBalance = Math.Round(totOutstandingBalance + Convert.ToDecimal(accountRow[CN.OutstandingBalance]), this._precision);
                    totToFollow = Math.Round(totToFollow + Convert.ToDecimal(accountRow[CN.ToFollowAmount]), this._precision);
                    totInstalment = Math.Round(totInstalment + Convert.ToDecimal(accountRow[CN.InstalAmount]), this._precision);
                    totRebate = Math.Round(totRebate + Convert.ToDecimal(accountRow[CN.Rebate]), this._precision);
                    totSettlement = Math.Round(totSettlement + Convert.ToDecimal(accountRow[CN.SettlementFigure]), this._precision);
                    totBadDebtBalance = Math.Round(totBadDebtBalance + Convert.ToDecimal(accountRow[CN.BDWBalance]), this._precision);
                    totBadDebtCharges = Math.Round(totBadDebtCharges + Convert.ToDecimal(accountRow[CN.BDWCharges]), this._precision);

                    if (Char.IsNumber(accountRow[CN.Status].ToString(), 0))
                        curStatus = Convert.ToInt16(accountRow[CN.Status]);
                    else if (accountRow[CN.Status].ToString() == "S")
                        curStatus = -2;
                    else if (accountRow[CN.Status].ToString() == "U")
                        curStatus = -1;
                    else
                        curStatus = 0;

                    maxCurStatus = (curStatus > maxCurStatus) ? curStatus : maxCurStatus;
                    DateTime curDateFirst = Convert.ToDateTime(accountRow[CN.DateFirst]);
                    maxDateFirst = (curDateFirst > maxDateFirst) ? curDateFirst : maxDateFirst;
                    totFullyDelivered = (totFullyDelivered || Convert.ToBoolean(accountRow[CN.DeliveredIndicator]));
                    totFreeInstalment = (totFreeInstalment || Convert.ToBoolean(accountRow[CN.FreeInstalment]));

                    // Initialise the PAYMENT ratio for each account. The ratio for each account
                    // is initially set to the instalment amount you would expect to pay when just  
                    // paying that account. This is later divided by the sum of all such amounts 
                    // for all selected accounts to give a ratio. If only one acount is selected
                    // the ratio is always one.
                    // Note this works for one or more selected accounts.
                    accountRow[CN.RatioPay] = accountRow[CN.InstalAmount];
                    maxTotalPay = maxTotalPay + (decimal)accountRow[CN.RatioPay];
                    this.txtSegmentName.Text = accountRow[CN.SegmentName].ToString();

                    //CR 802 show the service request number here, if more than one account is selected then show the last selected SR No.

                    if (!accountRow[CN.ServiceRequestNoStr].ToString().Equals(string.Empty) || !SrNo.Equals(string.Empty))
                    {
                        string srLabel = "";

                        if (!accountRow[CN.ServiceRequestNoStr].ToString().Equals(string.Empty))
                        {
                            txtAgrmtNo.Text = accountRow[CN.ServiceRequestNoStr].ToString();
                            srLabel = GetResource("L_SERVICEREQUEST") + ' ' + accountRow[CN.ServiceRequestNoStr].ToString();
                        }
                        else
                        {
                            txtAgrmtNo.Text = SrNo;
                            srLabel = GetResource("L_SERVICEREQUEST") + ' ' + SrNo;
                        }


                        if (accountRow[CN.Internal].ToString().Equals("Y"))
                            srLabel += " (Internal)";
                        if (accountRow[CN.Internal].ToString().Equals("N"))
                            srLabel += " (External)";

                        lServiceRequestNo.Visible = true;
                        lServiceRequestNo.Text = srLabel;
                    }
                    else
                        lServiceRequestNo.Visible = false;

                }
            }

            // Calculate the PAYMENT and FEE ratios
            for (int i = 0; i < accountList.Count; i++)
            {
                accountRow = accountList[i].Row;
                if (dgAccountList.IsSelected(i))
                {
                    if (maxTotalPay != 0)
                    {
                        accountRow[CN.RatioPay] = (decimal)accountRow[CN.RatioPay] / maxTotalPay;
                    }
                    else if (selectedCount > 0)
                    {
                        // Use an even spread if other figures are not available
                        accountRow[CN.RatioPay] = (1 / selectedCount);
                    }
                    else
                    {
                        accountRow[CN.RatioPay] = 0;
                    }
                }
                else
                {
                    accountRow[CN.RatioPay] = 0;
                }
            }

            // Display the summed values
            this.txtAgreementTotal.Text = totAgreementTotal.ToString(DecimalPlaces);
            this.txtArrears.Text = totArrears.ToString(DecimalPlaces);
            this.txtOutstandingBalance.Text = totOutstandingBalance.ToString(DecimalPlaces);
            this.txtToFollow.Text = totToFollow.ToString(DecimalPlaces);
            if (maxCurStatus == -2)
                this.txtCurrentStatus.Text = "S";
            else if (maxCurStatus == -1)
                this.txtCurrentStatus.Text = "U";
            else this.txtCurrentStatus.Text = maxCurStatus.ToString();
            this.txtInstalment.Text = totInstalment.ToString(DecimalPlaces);
            this.txtDateFInstalment.Text = maxDateFirst.ToShortDateString();  // UAT 65 RD 06/09/06 ToString();
            this.txtRebate.Text = totRebate.ToString(DecimalPlaces);
            this.txtSettlement.Text = totSettlement.ToString(DecimalPlaces);
            this.lFullyDelivered.Visible = totFullyDelivered;
            this.txtToFollow.Visible = !totFullyDelivered;   //CR1072
            this.lFreeInstalment.Visible = totFreeInstalment;
            this.txtBadDebtBalance.Text = totBadDebtBalance.ToString(DecimalPlaces);
            this.txtBadDebtCharges.Text = totBadDebtCharges.ToString(DecimalPlaces);
            if (this.txtAgrmtNo.Text.Trim() == "")
                this.txtAgrmtNo.Text = "1";
            this.txtAgrmtNo.Visible = false;
            this.lblAgrmtNo.Visible = false;

            if (txtNextPaymentDue.Text != "" && Convert.ToDateTime(txtNextPaymentDue.Text) < dateNow + TimeSpan.FromDays(7))     //CR1084
            {
                txtNextPaymentDue.BackColor = System.Drawing.Color.Gold;
            }
            else
            {
                txtNextPaymentDue.BackColor = System.Drawing.SystemColors.Window;
            }

            // Cash accounts have a copy Agreement Total button
            this.btnCopyAgreementTotal.Visible = (this._curAccount[CN.AccountType].ToString().Trim() == AT.Cash);

            if (totArrears < 0.0M)
            {
                // Arrears < 0 means the account is in advance
                this.lArrears.Text = GetResource("T_ADVANCE");
                this.txtArrears.ForeColor = Color.Green;
            }
            else if (totArrears > 0.0M)
            {
                // The account is in arrears
                this.lArrears.Text = GetResource("T_ARREARS");
                this.txtArrears.ForeColor = Color.Red;
            }
            else
            {
                this.lArrears.Text = GetResource("T_ARREARS");
                this.txtArrears.ForeColor = Color.Black;
            }

            // Colour code the Account Status
            switch (this.txtCurrentStatus.Text)
            {
                case "1":
                    this.txtCurrentStatus.BackColor = Color.PaleGreen;
                    break;
                case "2":
                    this.txtCurrentStatus.BackColor = Color.Yellow;
                    break;
                case "3":
                    this.txtCurrentStatus.BackColor = Color.Orange;
                    break;
                case "4":
                    this.txtCurrentStatus.BackColor = Color.Pink;
                    break;
                case "5":
                    this.txtCurrentStatus.BackColor = Color.Red;
                    break;
                case "6":
                    this.txtCurrentStatus.BackColor = Color.PaleVioletRed;
                    break;
                default:
                    this.txtCurrentStatus.BackColor = Color.White;
                    break;
            }

            // Rebate and Settlement are only used when > 0
            this.txtRebate.Enabled = (totRebate >= 0.01M);
            this.lRebate.Enabled = (totRebate >= 0.01M);
            this.txtSettlement.Enabled = (totSettlement >= 0.01M);
            this.lSettlement.Enabled = (totSettlement >= 0.01M);
            this.lBadDebtBalance.Visible = this.txtBadDebtBalance.Visible = (Math.Abs(totBadDebtBalance) >= 0.01M);
            this.lBadDebtCharges.Visible = this.txtBadDebtCharges.Visible = (Math.Abs(totBadDebtCharges) >= 0.01M);

            // Display the allocated employee
            // The selected account number will change if it is not already
            // on the preferred allocated row in the list.
            DataRow AllocatedRow = null;
            this.SelectAllocation(piIndex, out AllocatedRow);
            if (AllocatedRow != null)
            {
                this.tpAllocation.Enabled = true;
                this.txtSelectedAcctNo.Text = AllocatedRow[CN.AccountNumber].ToString();
                bool showTallyman = (int)AllocatedRow[CN.SegmentID] != 0;
                this.gbTallymanAlloc.Enabled = showTallyman;
                this.gbTallymanAlloc.Visible = showTallyman;
                this.gbCourtsAlloc.Enabled = !showTallyman;
                this.gbCourtsAlloc.Visible = !showTallyman;
                // Tallyman allocation
                // this.txtSegmentId.Text		= AllocatedRow[CN.SegmentID].ToString();

                if (Convert.ToBoolean(Country[CountryParameterNames.LinkToTallyman]))
                {
                    bool visible = false;
                    if (AllocatedRow["TallyManAcct"] != DBNull.Value)
                        visible = Convert.ToBoolean(Convert.ToInt16(AllocatedRow["TallyManAcct"]));
                    this.btnLaunchTally.Visible = visible;
                }
                // CoSACS allocation
                this.txtEmployeeName.Text = AllocatedRow[CN.EmployeeName].ToString();
                this.txtEmployeeNo.Text = AllocatedRow[CN.EmployeeNo].ToString();
              

                // The Total Amount and the Credit Fee will be displayed based on same calculation 


                this._debitFee =
                    (((int)AllocatedRow[CN.EmployeeNo] != 0 || (int)AllocatedRow[CN.SegmentID] != 0) &&
                    (int)AllocatedRow[CN.DebitAccount] == 1 &&
                    MoneyStrToDecimal(this.txtArrears.Text) >= 0.01M);
            }
            else
            {
                this.tpAllocation.Enabled = false;
                this.gbTallymanAlloc.Enabled = false;
                this.gbTallymanAlloc.Visible = false;
                this.gbCourtsAlloc.Enabled = false;
                this.gbCourtsAlloc.Visible = false;
                this.txtSegmentId.Text = "";
                this.txtEmployeeName.Text = "";
                this.txtEmployeeNo.Text = "";
                this.txtEmployeeType.Text = "";
                this._debitFee = false;
            }


            this.txtTotalAmount.Visible = this._debitFee;
            this.lTotalAmount.Visible = this._debitFee;
            SetFeeforStoreCard(PayMethod.IsPayMethod(Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()), PayMethod.StoreCard));
            this.txtPayAmount.ReadOnly = this._debitFee;
            this.txtPayAmount.Enabled = !this._debitFee;

            // The transactions for this account will be loaded
            // when the user clicks on the 'Transactions' tab page
            // so make sure the 'Transactions' page is not displayed
            this.tpDetails.Selected = true;
            this._curAccountTransactionSet = null;

            // Check whether an allocated receipt has been entered
            this.CheckReceiptAllocation();

            // Might need to check the payment method for the account
            this._lastPayMethod = "";
            this.SetPayMethod(true);

            // Might not be able to print the customer card for this account
            //this.SetCardPrint();                                                       //IP - 18/05/12 - #9445 - CR1239

            // Enable the Account Details and Payment group boxes
            this.gbSelectedAccount.Enabled = true;
            this.gbNewPayment.Enabled = true;

            if (menuWarrantRenewals.Enabled)
            {
                menuWarrantRenewals.Visible = true;
                menuCheckExpiringWarranties.Visible = true;
            }

            //IP - 18/05/12 - #9445 - CR1239
            //if (!ThermalPrintingEnabled)          
            //{
            //    #region extra bit to check for unprinted transactions
            //    /* check if there are any unprinted transactions for this account
            // * if there are optionally prompt the user to print them */
            //    if ((bool)Country[CountryParameterNames.PaymentCardPrompt] &&
            //        AT.IsCreditType(Convert.ToString(newAccount[CN.AccountType]).Trim())                    )
            //    {
            //        bool unprinted = false;
            //        bool unprinteddel = false;
            //        int printedtrans = 0;

            //        /* make sure the transactions are loaded*/
            //        LoadTransactions(true, ref unprinted, ref unprinteddel, ref printedtrans);

            //        if (unprinteddel && printedtrans <= 2)
            //        {
            //            if (DialogResult.OK == ShowInfo("M_PAYMENTCARDDUEDATE", MessageBoxButtons.OKCancel))
            //            {
            //                PrintPaymentCardDueDate(txtCustId.Text, txtSelectedAcctNo.Text);
            //            }
            //        }

            //        if (unprinted)
            //        {
            //            cbCardPrint.Checked = DialogResult.OK == ShowInfo("M_UNPRINTEDTRANSACTIONS", new object[] { newAccount[CN.AccountNumber], txtCardRowNo.Value }, MessageBoxButtons.OKCancel);
            //        }
            //    }
            //    #endregion
            //}

            StoreCardCheckAccountType(newAccount[CN.AccountType].ToString().Trim()); //IP - 29/11/10 - Store Card - Changed to pass in AccountType as incorrectly passed AccountNumber
            // #9859 only enable Calculator if selecting Storecard account  jec 11/04/12
            if (newAccount[CN.AccountType].ToString().Trim() == AT.StoreCard)
            {
                btn_calc.Enabled = true;
                lInstalmentAmount.Text = "Monthly Payment";     //#9919
            }
            else
            {
                btn_calc.Enabled = false;
                lInstalmentAmount.Text = "Instalment Amount";   //#9919
            }

            return true;

        }  // End of SelectAccount

        /* added this wrapper for when we don't care whether there are unprinted 
         * transactions or not - just so that all the other LoadTransactions calls 
         * still work */

        private void SelectAllocation(int piIndex, out DataRow AllocatedRow)
        {
            // Choose most preferred allocated employee from the
            // list of accounts currently marked as selected
            DataView accountList = (DataView)dgAccountList.DataSource;
            DataRow accountRow;
            int Preference = 9;
            AllocatedRow = null;
            for (int i = 0; i < accountList.Count; i++)
            {
                if (dgAccountList.IsSelected(i))
                {
                    accountRow = accountList[i].Row;
                    if (i == piIndex &&
                        (((int)accountRow[CN.EmployeeNo] != 0 && Convert.ToBoolean(accountRow["isBailiff"]) && !(bool)Country[CountryParameterNames.LinkToTallyman])
                            || accountRow[CN.SegmentName].ToString() != "" && (int)accountRow[CN.DebitAccount] == 1 && (bool)Country[CountryParameterNames.LinkToTallyman])) //TODO
                    {
                        // Preference 1: Bailiff or Tallyman with Fees on user clicked account
                        Preference = 1;
                        AllocatedRow = accountRow;
                    }
                    else if (Preference > 2 &&
                        (((int)accountRow[CN.EmployeeNo] != 0 && Convert.ToBoolean(accountRow["isBailiff"]) && !(bool)Country[CountryParameterNames.LinkToTallyman])
                            || (accountRow[CN.SegmentName].ToString() != "") && (int)accountRow[CN.DebitAccount] == 1 && (bool)Country[CountryParameterNames.LinkToTallyman]))
                    {
                        // Preference 2: Bailiff or Tallyman on any selected account
                        Preference = 2;
                        AllocatedRow = accountRow;
                    }
                    else if (Preference > 3 && i == piIndex &&
                        (int)accountRow[CN.EmployeeNo] != 0 &&  Convert.ToBoolean(accountRow["isTelephoneCaller"]))
                    {
                        // Preference 3: Telephone Caller on user clicked account
                        Preference = 3;
                        AllocatedRow = accountRow;
                    }
                    else if (Preference > 4 &&
                        (int)accountRow[CN.EmployeeNo] != 0 &&  Convert.ToBoolean(accountRow["isTelephoneCaller"]))
                    {
                        // Preference 4: Telephone Caller on any selected account
                        Preference = 4;
                        AllocatedRow = accountRow;
                    }
                    else if (Preference > 5 && i == piIndex &&
                        (int)accountRow[CN.EmployeeNo] != 0)
                    {
                        // Preference 5: Any employee type on user clicked account
                        Preference = 5;
                        AllocatedRow = accountRow;
                    }
                    else if (Preference > 6 &&
                        (int)accountRow[CN.EmployeeNo] != 0 )
                    {
                        // Preference 6: Any employee type on any selected account
                        Preference = 6;
                        AllocatedRow = accountRow;
                    }

                }
            }
        }  // End of SelectAllocation


        private bool LoadTransactions(bool LoadForPrint)
        {
            bool unprinted = false;
            bool unprinteddel = false;
            int printedtrans = 0;
            return LoadTransactions(LoadForPrint, ref unprinted, ref unprinteddel, ref printedtrans);
        }

        private bool LoadTransactions(bool LoadForPrint, ref bool unprinted, ref bool unprinteddel, ref int printedtrans)
        {
            DataSet TransactionSet = null;
            this._printTransactionSet = null;

            // Clear the old transaction list
            this.lTransactionCount.Visible = false;
            dgTransactionList.DataSource = null;
            dgTransactionList.ResetText();

            if (this._combinedRF)
            {
                // Customer RF transactions might already be loaded
                if (this._curRFTransactionSet == null)
                {
                    // Load the combined transactions for the set of RF Accounts
                    // summed by date and transaction type accross all delivered RF accounts.
                    TransactionSet = CustomerManager.GetRFCombinedTransactions(true, this.txtCustId.Text, out error);
                    this._curRFTransactionSet = TransactionSet;
                }
                else
                {
                    TransactionSet = this._curRFTransactionSet;
                }
                if (LoadForPrint)
                {
                    // Load the individual transactions for printing
                    // as discrete transactions for each RF account.
                    this._printTransactionSet = CustomerManager.GetRFCombinedTransactions(false, this.txtCustId.Text, out error);
                }
            }
            else
            {
                // This account transactions might already be loaded
                if (this._curAccountTransactionSet == null || LoadForPrint)
                {
                    // Load individual transactions for the selected account
                    TransactionSet = AccountManager.GetTransactions(this.txtSelectedAcctNo.Text, out error);
                    this._curAccountTransactionSet = TransactionSet;
                    this._printTransactionSet = TransactionSet;
                }
                else
                {
                    TransactionSet = this._curAccountTransactionSet;
                }
            }

            if (error.Length > 0)
            {
                ShowError(error);
                return false;
            }

            foreach (DataTable TransactionDetails in TransactionSet.Tables)
            {
                if (TransactionDetails.TableName == TN.Transactions ||
                    TransactionDetails.TableName == TN.RFTransactions)
                {
                    // Display list of Account Transactions
                    this.lTransactionCount.Text = Convert.ToString(TransactionDetails.Rows.Count) + GetResource("T_TRANSACTIONS");
                    this.lTransactionCount.Visible = true;

                    // Always recreate the table style because two different
                    // tables can be supplied as the datasource.
                    dgTransactionList.TableStyles.Clear();
                    DataView TransactionListView = new DataView(TransactionDetails);
                    TransactionListView.AllowNew = false;
                    TransactionListView.Sort = CN.DateTrans + "," + CN.TransTypeCode;
                    dgTransactionList.DataSource = TransactionListView;
                    DataGridTableStyle tabStyle = new DataGridTableStyle();
                    tabStyle.MappingName = TransactionListView.Table.TableName;
                    dgTransactionList.TableStyles.Add(tabStyle);

                    if (!this._combinedRF)
                    {
                        // Hidden columns
                        tabStyle.GridColumnStyles[CN.acctno].Width = 0;
                        tabStyle.GridColumnStyles[CN.TransRefNo].Width = 0;
                        tabStyle.GridColumnStyles[CN.PayMethod].Width = 0;
                        tabStyle.GridColumnStyles[CN.EmployeeName].Width = 0;
                        tabStyle.GridColumnStyles[CN.EmployeeNo].Width = 0;
                        tabStyle.GridColumnStyles[CN.FootNotes].Width = 0;
                    }

                    // Displayed columns
                    tabStyle.GridColumnStyles[CN.DateTrans].Width = 80;
                    tabStyle.GridColumnStyles[CN.DateTrans].HeaderText = GetResource("T_DATE");
                    tabStyle.GridColumnStyles[CN.DateTrans].Alignment = HorizontalAlignment.Left;

                    tabStyle.GridColumnStyles[CN.TransTypeCode].Width = 40;
                    tabStyle.GridColumnStyles[CN.TransTypeCode].HeaderText = GetResource("T_TYPE");
                    tabStyle.GridColumnStyles[CN.TransTypeCode].Alignment = HorizontalAlignment.Center;

                    tabStyle.GridColumnStyles[CN.TransValue].Width = 90;
                    tabStyle.GridColumnStyles[CN.TransValue].HeaderText = GetResource("T_VALUE");
                    tabStyle.GridColumnStyles[CN.TransValue].Alignment = HorizontalAlignment.Left;
                    ((DataGridTextBoxColumn)tabStyle.GridColumnStyles[CN.TransValue]).Format = DecimalPlaces;

                    tabStyle.GridColumnStyles[CN.TransPrinted].Width = 50;
                    tabStyle.GridColumnStyles[CN.TransPrinted].HeaderText = GetResource("T_PRINTED");
                    tabStyle.GridColumnStyles[CN.TransPrinted].Alignment = HorizontalAlignment.Center;

                    /* check whether there are any non printed transactions */
                    ((DataView)dgTransactionList.DataSource).RowFilter = CN.TransPrinted + " = 'N'";
                    unprinted = ((DataView)dgTransactionList.DataSource).Count > 0;
                    ((DataView)dgTransactionList.DataSource).RowFilter = "";

                    /* check whether there are any non printed delivery transactions */
                    ((DataView)dgTransactionList.DataSource).RowFilter = CN.TransPrinted + " = 'N' and " + CN.TransTypeCode + " = 'DEL'";
                    unprinteddel = ((DataView)dgTransactionList.DataSource).Count > 0;
                    ((DataView)dgTransactionList.DataSource).RowFilter = "";

                    /* record how many printed transactions there are */
                    ((DataView)dgTransactionList.DataSource).RowFilter = CN.TransPrinted + " = 'Y'";
                    printedtrans = ((DataView)dgTransactionList.DataSource).Count;
                    ((DataView)dgTransactionList.DataSource).RowFilter = "";
                }
            }

            // The user might have ticked to only display non-printed transactions
            this.SetNonPrinted(this.cbNonPrinted.Checked);

            return true;

        }  // End of LoadTransactions

        private void LoadPaymentHolidays()
        {
            bool nosuch = false;
            bool valid = true;

            DataRowView selectedRow = ((DataView)dgAccountList.DataSource)[dgAccountList.CurrentRowIndex];
            string accountNo = (string)selectedRow[CN.AccountNumber];

            /* Retrieve the payment holidays Data */
            if (_paymentHolidays == null)
            {
                _paymentHolidays = PaymentManager.GetPaymentHolidays(txtCustId.Text, out error);
                if (error.Length > 0)
                    ShowError(error);
            }

            DataView dv = _paymentHolidays.Tables[TN.PaymentHolidays].DefaultView;

            dgPaymentHolidays.DataSource = dv;

            dv.RowFilter = CN.AccountNumber + " = '" + selectedRow[CN.AccountNumber] + "'";

            short paymentHolidays = (short)selectedRow[CN.PaymentHoliday];

            this.numPaymentHolidaysLeft.Value = paymentHolidays - dv.Count;
            this.txtNewDueDate.Text = Convert.ToDateTime(this.txtDateFInstalment.Text).AddMonths(1).ToShortDateString(); // RD UAT 65 06/09/06 

            valid = numPaymentHolidaysLeft.Value > 0;

            if (dgPaymentHolidays.TableStyles.Count == 0)
            {
                DataGridTableStyle tabStyle = new DataGridTableStyle();
                tabStyle.MappingName = TN.PaymentHolidays;

                AddColumnStyle(CN.acctno, tabStyle, 0, true, "", "", HorizontalAlignment.Left);
                AddColumnStyle(CN.AgrmtNo, tabStyle, 0, true, "", "", HorizontalAlignment.Left);
                AddColumnStyle(CN.DateTaken, tabStyle, 100, true, GetResource("T_DATETAKEN"), "", HorizontalAlignment.Left);
                AddColumnStyle(CN.NewDateFirst, tabStyle, 100, true, GetResource("T_NEWDATEFIRST"), "", HorizontalAlignment.Left);
                AddColumnStyle(CN.EmployeeNo, tabStyle, 0, true, "", "", HorizontalAlignment.Left);
                AddColumnStyle(CN.EmployeeName, tabStyle, 100, true, GetResource("T_TAKENBY"), "", HorizontalAlignment.Left);

                dgPaymentHolidays.TableStyles.Add(tabStyle);
            }

            if (valid)
            {
                /* need to check that the customer has made the minimum number for payments
                * to qualify to take a payment holiday */
                short paymentHolidaysMin = (short)selectedRow[CN.PaymentHolidayMin];
                numMinimumPayments.Value = paymentHolidaysMin;

                decimal instalAmount = (decimal)selectedRow[CN.InstalAmount];
                decimal paidAmount = 0;
                LoadTransactions(false);
                foreach (DataRowView r in (DataView)dgTransactionList.DataSource)
                {
                    /* find the total value of payments on this account so far */
                    if ((string)r[CN.TransTypeCode] == TransType.Payment ||
                        (string)r[CN.TransTypeCode] == TransType.Correction ||
                        (string)r[CN.TransTypeCode] == TransType.Transfer ||
                        (string)r[CN.TransTypeCode] == TransType.TakeonTransfer ||
                        (string)r[CN.TransTypeCode] == TransType.SundryCreditTransfer ||
                        (string)r[CN.TransTypeCode] == TransType.Refund ||
                        (string)r[CN.TransTypeCode] == TransType.Return ||
                        (string)r[CN.TransTypeCode] == TransType.GiroExtra ||
                        (string)r[CN.TransTypeCode] == TransType.GiroNormal ||
                        (string)r[CN.TransTypeCode] == TransType.GiroRepresent ||
                        (string)r[CN.TransTypeCode] == TransType.Overage ||
                        (string)r[CN.TransTypeCode] == TransType.DebtPayment ||
                        (string)r[CN.TransTypeCode] == TransType.OldAddToReversal)
                    {
                        paidAmount -= (decimal)r[CN.TransValue];
                    }
                }

                if (paidAmount < instalAmount * paymentHolidaysMin)
                {
                    valid = false;		/* not enough payments have been made */
                }

                /* retrieve acctcodes and make sure that the account is not coded to 
                * prevent payment holidays */
                DataSet codes = AccountManager.GetCodesOnAccount((string)selectedRow[CN.AccountNumber], out nosuch, out error);
                if (error.Length > 0)
                    ShowError(error);
                else
                {
                    chxPaymentHolidayCancelled.Checked = false;

                    foreach (DataRow r in codes.Tables[TN.AccountCodes].Rows)
                    {
                        if ((string)r[CN.Code] == "PHC")		/* account is coded as payment holiday cancelled */
                        {
                            chxPaymentHolidayCancelled.Checked = true;
                        }
                    }
                }

                if (valid)	/* can only be changed from true to false */
                    valid = !chxPaymentHolidayCancelled.Checked;
            }

            btnPaymentHoliday.Enabled = valid;
        }

        //private void SelectAllocation(int piIndex, out DataRow AllocatedRow)
        //{
        //    // Choose most preferred allocated employee from the
        //    // list of accounts currently marked as selected
        //    DataView accountList = (DataView)dgAccountList.DataSource;
        //    DataRow accountRow;
        //    int Preference = 9;
        //    AllocatedRow = null;
        //    for (int i = 0; i < accountList.Count; i++)
        //    {
        //        if (dgAccountList.IsSelected(i))
        //        {
        //            accountRow = accountList[i].Row;
        //            if (i == piIndex &&
        //                (((int)accountRow[CN.EmployeeNo] != 0 && (string)accountRow[CN.EmployeeType] == ET.Bailiff && !(bool)Country[CountryParameterNames.LinkToTallyman])
        //                    || accountRow[CN.SegmentName].ToString() != "" && (int)accountRow[CN.DebitAccount] == 1 && (bool)Country[CountryParameterNames.LinkToTallyman])) //TODO
        //            {
        //                // Preference 1: Bailiff or Tallyman with Fees on user clicked account
        //                Preference = 1;
        //                AllocatedRow = accountRow;
        //            }
        //            else if (Preference > 2 &&
        //                (((int)accountRow[CN.EmployeeNo] != 0 && (string)accountRow[CN.EmployeeType] == ET.Bailiff && !(bool)Country[CountryParameterNames.LinkToTallyman])
        //                    || (accountRow[CN.SegmentName].ToString() != "") && (int)accountRow[CN.DebitAccount] == 1 && (bool)Country[CountryParameterNames.LinkToTallyman]))
        //            {
        //                // Preference 2: Bailiff or Tallyman on any selected account
        //                Preference = 2;
        //                AllocatedRow = accountRow;
        //            }
        //            else if (Preference > 3 && i == piIndex &&
        //                (int)accountRow[CN.EmployeeNo] != 0 && (string)accountRow[CN.EmployeeType] == ET.Telephone)
        //            {
        //                // Preference 3: Telephone Caller on user clicked account
        //                Preference = 3;
        //                AllocatedRow = accountRow;
        //            }
        //            else if (Preference > 4 &&
        //                (int)accountRow[CN.EmployeeNo] != 0 && (string)accountRow[CN.EmployeeType] == ET.Telephone)
        //            {
        //                // Preference 4: Telephone Caller on any selected account
        //                Preference = 4;
        //                AllocatedRow = accountRow;
        //            }
        //            else if (Preference > 5 && i == piIndex &&
        //                (int)accountRow[CN.EmployeeNo] != 0 && accountRow[CN.EmployeeType].ToString().Trim().Length != 0)
        //            {
        //                // Preference 5: Any employee type on user clicked account
        //                Preference = 5;
        //                AllocatedRow = accountRow;
        //            }
        //            else if (Preference > 6 &&
        //                (int)accountRow[CN.EmployeeNo] != 0 && accountRow[CN.EmployeeType].ToString().Trim().Length != 0)
        //            {
        //                // Preference 6: Any employee type on any selected account
        //                Preference = 6;
        //                AllocatedRow = accountRow;
        //            }

        //        }
        //    }
        //}  // End of SelectAllocation

        private void CalcPaymentFields(decimal curTotalAmount, decimal curCreditFee)
        {
            // Recalculate the Payment, Tendered and Change amounts for the 
            // current Total and Credit Fee amounts.
            // Note that the 'CurTotalAmount' may originate from either the
            // total pay amount if a fee is displayed, or from the net pay
            // amount when the fee is zero. In both cases this routine will
            // calculate the correct amounts.
            decimal curPayAmount = 0.0M;
            decimal curTendered = MoneyStrToDecimal(this.txtTendered.Text);
            decimal curChange = 0.0M;

            curTotalAmount = Math.Round(curTotalAmount, this._precision);
            if (this._debitFee)
                curCreditFee = Math.Round(curCreditFee, this._precision);
            else
                curCreditFee = 0.0M;

            if (curTotalAmount > curCreditFee)
            {
                curPayAmount = curTotalAmount - curCreditFee;
            }

            if (curTendered > curTotalAmount)
            {
                curChange = curTendered - curTotalAmount;
            }

            this._lastTotalAmount = curTotalAmount;
            this._lastFee = curCreditFee;
            this._lastPayAmount = curPayAmount;
            this.txtTotalAmount.Text = curTotalAmount.ToString(DecimalPlaces);
            if (txtAccountNo.Text.Length >= 12 &&             // #8534 jec 01/11/11 account number has been entered
                    txtAccountNo.Text.Replace("-", "").Substring(3, 1) != "4") //only credit accounts replace TxtFee otherwise could be store card. 
                this.txtFee.Text = curCreditFee.ToString(DecimalPlaces);
            this.txtPayAmount.Text = curPayAmount.ToString(DecimalPlaces);
            this.txtTendered.Text = curTendered.ToString(DecimalPlaces);
            this.txtChange.Text = curChange.ToString(DecimalPlaces);

            // Recalculate the Settlement when fully delivered
            bool isamortizedacct = PaymentManager.IsCashLoanAmortizedAccount(txtAccountNo.Text.Replace("-", ""));
            if (lFullyDelivered.Visible && !isamortizedacct)
            {
                decimal curSettlement =
                    MoneyStrToDecimal(this.txtOutstandingBalance.Text) -
                    MoneyStrToDecimal(this.txtRebate.Text) +
                    curCreditFee;
                this.txtSettlement.Text = curSettlement.ToString(DecimalPlaces);
            }
        }  // End of CalcPaymentFields

        private bool CheckPaymentSet()
        {
            // Check the payment list has been set up.
            // This will only create the list if it is empty.
            // An existing list must not be overwritten because the
            // user may have typed values into it.
            bool needList = false;
            if (this._paymentSet == null)
                needList = true;
            else if (!this._paymentSet.Tables.Contains(TN.Payments))
                needList = true;
            else if (this._paymentSet.Tables[TN.Payments].Rows.Count < 1)
                needList = true;

            /* list was not being refreshed if user had changed the credit fee
             * which meant the original fee was being saved - JJ */
            /* DSR - Cannot do this here because it will overwrite any
             * values the user may have typed into the list.
             * else if (this._calculatedFee != MoneyStrToDecimal(txtFee.Text))
             *	needList = true;
            */

            if (needList)
            {
                // Calculate the list of payments
                this.SetPaymentList(MoneyStrToDecimal(this.txtTotalAmount.Text));
            }

            // Check the payment split adds up to the correct total
            decimal curPayment;
            decimal totalPayment = 0.0M;
            foreach (DataRow accountRow in this._paymentSet.Tables[TN.Payments].Rows)
            {
                curPayment = (decimal)accountRow[CN.Payment];
                totalPayment = totalPayment + curPayment;
            }

            // Allow a five cent tolerance for rounding
            decimal expectedTotalPayment = MoneyStrToDecimal(this.txtTotalAmount.Text);
            return (Math.Abs(totalPayment - expectedTotalPayment) < 0.05M);

        }  // End of CheckPaymentSet


        private void SetPaymentList(decimal totPayment)
        {
            // Some countries prefer payments without cents
            int paymentPrecision = this._precision;
            if ((bool)Country[CountryParameterNames.PayWholeUnits])
                paymentPrecision = 0;

            // Create the list of spread payments
            DataSet accountSet = new DataSet();
            accountSet.Tables.Add(TN.Payments);
            accountSet.Tables[TN.Payments].Columns.AddRange(
                new DataColumn[] {	new DataColumn(CN.DateAcctOpen,			Type.GetType("System.DateTime")),
									 new DataColumn(CN.RatioPay,			Type.GetType("System.Decimal")),
									 new DataColumn(CN.LockedBy),
									 new DataColumn(CN.AccountNo),
									 new DataColumn(CN.OutstandingBalance,  Type.GetType("System.Decimal")),
									 new DataColumn(CN.Arrears,				Type.GetType("System.Decimal")),
									 new DataColumn(CN.Rebate,				Type.GetType("System.Decimal")),
									 new DataColumn(CN.SettlementFigure,	Type.GetType("System.Decimal")),
									 new DataColumn(CN.InstalAmount,		Type.GetType("System.Decimal")),
									 new DataColumn(CN.NetPayment,			Type.GetType("System.Decimal")),
									 new DataColumn(CN.CollectionFee,		Type.GetType("System.Decimal")),
									 new DataColumn(CN.BailiffFee,			Type.GetType("System.Decimal")),
									 new DataColumn(CN.Payment,				Type.GetType("System.Decimal")),
									 new DataColumn(CN.CalculatedFee,		Type.GetType("System.Decimal")),
									 new DataColumn(CN.EmployeeNo,			Type.GetType("System.Int32")),
									 new DataColumn(CN.SegmentID,			Type.GetType("System.Int32")),
									 new DataColumn(CN.Status),
									 new DataColumn(CN.ReadOnly),
									 new DataColumn(CN.DebitAccount,		Type.GetType("System.Int16")), 
									 new DataColumn(CN.BDWBalance,			Type.GetType("System.Decimal")),
									 new DataColumn(CN.BDWCharges,			Type.GetType("System.Decimal"))
								 });

            DataView accountList = (DataView)dgAccountList.DataSource;
            DataRow recentAcct = null;
            decimal totSettlement = MoneyStrToDecimal(this.txtSettlement.Text);
            decimal totOutStanding = MoneyStrToDecimal(this.txtOutstandingBalance.Text);
            decimal sumPayment = 0;

            for (int i = 0; i < accountList.Count; i++)
            {
                if ((decimal)accountList[i].Row[CN.RatioPay] > 0)
                {
                    // Work out the proportion to be paid to this account
                    DataRow accountRow = accountList[i].Row;
                    decimal curPayment = Math.Round(totPayment * (decimal)accountRow[CN.RatioPay], this._precision);
                    string readOnly = "Y";

                    // DSR 9 Sep 2004 - Users can now overpay accounts so that they could go into credit
                    // If an account is overpaid, then reserve the extra amount to pay off another account
                    if ((decimal)accountRow[CN.SettlementFigure] > 0 &&
                        (curPayment >= (decimal)accountRow[CN.SettlementFigure] || totPayment >= totSettlement))
                    {
                        // Don't pay over the Settlement Figure
                        curPayment = (decimal)accountRow[CN.SettlementFigure];
                    }
                    else
                    {
                        // The payment uses a precision which could be for whole units
                        curPayment = Math.Round(curPayment, paymentPrecision);
                    }

                    // Rounding up can cause a larger sum to be paid than is available
                    if (sumPayment + curPayment > totPayment)
                    {
                        decimal excess = (sumPayment + curPayment) - totPayment;
                        curPayment = curPayment - excess;
                    }

                    // Add up what is being paid to work out later if there is a reserve
                    // amount. Hopefully this approach will reduce rounding errors.
                    sumPayment = sumPayment + curPayment;

                    // Determine whether the fee can be entered on this row
                    if (((int)accountRow[CN.EmployeeNo] != 0 || (int)accountRow[CN.SegmentID] != 0)
                        && (int)accountRow[CN.DebitAccount] == 1
                        && MoneyStrToDecimal(accountRow[CN.Arrears].ToString()) >= 0.01M)
                    {
                        readOnly = "N";
                    }

                    // Create the payment row
                    accountSet.Tables[TN.Payments].Rows.Add(
                        new Object[] {	accountRow[CN.DateAcctOpen],
										 accountRow[CN.RatioPay],
										 accountRow[CN.LockedBy],
										 accountRow[CN.acctno],
										 accountRow[CN.OutstandingBalance],
										 accountRow[CN.Arrears],
										 accountRow[CN.Rebate],
										 accountRow[CN.SettlementFigure],
										 accountRow[CN.InstalAmount],
										 curPayment,			// Net Payment (can be changed by user when no fee)
										 0,						// Collection Fee (can be changed by user when a fee)
										 0,						// Bailiff Fee (not visible to the user)
										 curPayment,			// Total Payment (can be changed by user when a fee)
										 0,						// Calculated Collection Fee (not visible to the user)
										 accountRow[CN.EmployeeNo],
										 accountRow[CN.SegmentID],
										 accountRow[CN.Status],
										 readOnly,
										 accountRow[CN.DebitAccount],
										 Math.Round((decimal)accountRow[CN.BDWBalance], this._precision),
										 Math.Round((decimal)accountRow[CN.BDWCharges], this._precision) });

                    // Note the row of the most recent account with a payment
                    DataRow newRow = accountSet.Tables[TN.Payments].Rows[accountSet.Tables[TN.Payments].Rows.Count - 1];
                    if (recentAcct == null)
                        recentAcct = newRow;
                    else if ((DateTime)newRow[CN.DateAcctOpen] > (DateTime)recentAcct[CN.DateAcctOpen])
                        recentAcct = newRow;
                }
            }

            // Spread any reserve amount across any accounts not yet settled
            decimal reserve = totPayment - sumPayment;
            decimal oldReserve = 0;
            bool allSettled = false;
            while (reserve > 0 && reserve != oldReserve && !allSettled)
            {
                // If the reserve amount does not change then it is too small
                // to add split payments to the accounts
                oldReserve = reserve;
                sumPayment = 0;
                allSettled = true;
                foreach (DataRow accountRow in accountSet.Tables[TN.Payments].Rows)
                {
                    // Work out the proportion to be paid to this account
                    decimal curPayment = Math.Round((decimal)accountRow[CN.Payment] + (reserve * (decimal)accountRow[CN.RatioPay]), this._precision);

                    // DSR 9 Sep 2004 - Users can now overpay accounts so that they could go into credit
                    // If an account is overpaid, then reserve the extra amount to pay off another account
                    if ((decimal)accountRow[CN.SettlementFigure] > 0 && curPayment >= (decimal)accountRow[CN.SettlementFigure])
                    {
                        // Don't pay over the Settlement Figure
                        curPayment = (decimal)accountRow[CN.SettlementFigure];
                    }
                    else
                    {
                        // The payment uses a precision which could be for whole units
                        curPayment = Math.Round(curPayment, paymentPrecision);
                        allSettled = false;
                    }

                    // Rounding up can cause a larger sum to be paid than is available
                    if (sumPayment + curPayment > totPayment)
                    {
                        decimal excess = (sumPayment + curPayment) - totPayment;
                        curPayment = curPayment - excess;
                    }

                    accountRow[CN.NetPayment] = curPayment;
                    accountRow[CN.Payment] = curPayment;

                    // Add up what is being paid to work out later if there is a reserve
                    // amount. Hopefully this approach will reduce rounding errors.
                    sumPayment = sumPayment + curPayment;
                }
                // Calc new reserve if any
                reserve = totPayment - sumPayment;
            }

            // Credit any remaining reserve amount onto the account with the largest OS Balance
            // Use country precision here to try to make sure the accounts add up to the entered payment
            if (reserve > 0)
            {
                decimal maxBal = 0;
                DataRow reserveAcct = null;
                foreach (DataRow accountRow in accountSet.Tables[TN.Payments].Rows)
                {
                    if ((decimal)accountRow[CN.SettlementFigure] - (decimal)accountRow[CN.Payment] > maxBal)
                    {
                        maxBal = (decimal)accountRow[CN.SettlementFigure] - (decimal)accountRow[CN.Payment];
                        reserveAcct = accountRow;
                    }
                }
                // If all acounts are being settled, then credit the most recent acct
                if (maxBal <= 0 && recentAcct != null) reserveAcct = recentAcct;
                if (reserveAcct != null)
                {
                    reserveAcct[CN.NetPayment] = Math.Round((decimal)reserveAcct[CN.NetPayment] + reserve, this._precision);
                    reserveAcct[CN.Payment] = Math.Round((decimal)reserveAcct[CN.Payment] + reserve, this._precision);
                }
            }

            //
            // Re-calculate the fee for each account and bailiff in the set
            //
            decimal newTotalAmount = totPayment;
            decimal newFee = 0;
            this._calculatedFee = PaymentManager.CalculateCreditFee(
                ref accountSet,
                Config.CountryCode,
                TransType.Payment,
                ref newTotalAmount,
                out error);

            if (error.Length > 0)
                ShowError(error);
            else
            {
                // If the user entered a zero Total Amount update with the return value
                if (totPayment < 0.01M)
                {
                    totPayment = Math.Round(newTotalAmount, this._precision);
                }
                // The displayed fee will change to the calculated amount
                newFee = Math.Round(this._calculatedFee, this._precision);
            }

            // Recalculate other fields
            this.CalcPaymentFields(totPayment, newFee);

            this._paymentSet = accountSet;
        }  // End of SetPaymentList


        private void SetFeeList(decimal totFee)
        {
            // Fees are only spread when the user overwrites the total fee.
            // The payment list must be set up first with the payment
            // amounts and the calculated fees.
            this.CheckPaymentSet();

            DataTable accountList = this._paymentSet.Tables[TN.Payments];
            decimal totCalculatedFee = 0;
            decimal sumFee = 0;
            decimal curFee = totFee;    //CR1072 - LW69836  jec 
            decimal calculatedFee;

            // Add up all the calculated fees to work out each fraction
            int i = 0;
            while (i < accountList.Rows.Count)
                totCalculatedFee += (decimal)accountList.Rows[i++][CN.CalculatedFee];

            // Apply each fraction to each account with a fee
            i = 0;
            while (i < accountList.Rows.Count && sumFee <= totFee)      //CR1072 (70136) jec
            {
                // The row is ignored if there is no calculated fee on this row
                DataRow accountRow = accountList.Rows[i];
                //curFee = 0;           //CR1072 - LW69836  jec
                calculatedFee = (decimal)accountRow[CN.CalculatedFee];

                if (calculatedFee > 0)
                {
                    curFee = Math.Round(totFee * (calculatedFee / totCalculatedFee), this._precision);
                    sumFee += curFee;
                    // Rounding can cause the individual fees to exceed the total fee
                    if (sumFee > totFee) curFee = curFee - (sumFee - totFee);
                }

                accountRow[CN.CollectionFee] = curFee;
                accountRow[CN.NetPayment] = (decimal)accountRow[CN.Payment] - curFee;
                i++;
            }
        }  // End of SetFeeList


        private bool UnlockAccounts()
        {
            DataView accountList = (DataView)dgAccountList.DataSource;

            // If no accounts listed then nothing to unlock
            if (accountList == null) return true;

            // Unlock the accounts

            for (int i = 0; i < accountList.Count; i++)
            {
                // Only unlock accounts not already locked when this screen opened
                if (accountList[i].Row[CN.LockedBy].ToString().Length == 0)
                {
                    AccountManager.UnlockAccount(accountList[i].Row[CN.acctno].ToString(), Credential.UserId, out error);
                    if (error.Length > 0)
                    {
                        ShowError(error);
                        return false;
                    }
                }
            }
            return true;
        }  // End of UnlockAccounts


        // Form Events

        private void Payment_Load(object sender, System.EventArgs e)
        {
            // Load Customer Accounts if CustId parameter was passed
            try
            {
                Function = "Payment Screen: Form Load";
                Wait();

                this.ttPayment.SetToolTip(btnSearchAccount, GetResource("TT_ACCOUNTSEARCH"));
            //    this.ttPayment.SetToolTip(btnServiceSearch, GetResource("TT_SERVICESEARCH"));
                //this.ttPayment.SetToolTip(btnUpdateCard, GetResource("TT_PRINTCARD"));                     //IP - 18/05/12 - #9445 - CR1239
                this.ttPayment.SetToolTip(btnPaymentList, GetResource("TT_PAYMENTLIST"));
                this.ttPayment.SetToolTip(btnExchange, GetResource("TT_EXCHANGECALCULATOR"));
                this.ttPayment.SetToolTip(mtb_CardNo, GetResource("TT_CARDNO"));                 //IP - 26/01/11 - Sprint 5.9 - #2785

                //Active Action Popups
                popAdditionalInfo = Convert.ToBoolean(Country[CountryParameterNames.PopUpAdditInfo]);  //CR1084
                popCustLeftAddr = Convert.ToBoolean(Country[CountryParameterNames.PopUpCustomerLeftAddr]);  //CR1084
                popAcctsInArrears = Convert.ToBoolean(Country[CountryParameterNames.PopUpAcctsInArrears]);  //CR1084
                popPhoneOutOfService = Convert.ToBoolean(Country[CountryParameterNames.PopUpPhoneOutOfService]);  //CR1084

                #region Get drop down data
                //Get the required static data for the drop down lists

                XmlUtilities xml = new XmlUtilities();
                XmlDocument dropDowns = new XmlDocument();
                dropDowns.LoadXml("<DROP_DOWNS></DROP_DOWNS>");

                if (StaticData.Tables[TN.PayMethod] == null)
                    dropDowns.DocumentElement.AppendChild(xml.CreateDropDownNode(dropDowns, TN.PayMethod, new string[] { CAT.FintransPayMethod, "L" }));
                if (StaticData.Tables[TN.CreditCard] == null)
                    dropDowns.DocumentElement.AppendChild(xml.CreateDropDownNode(dropDowns, TN.CreditCard, new string[] { CAT.CreditCardType, "L" }));
                if (StaticData.Tables[TN.Bank] == null)
                    dropDowns.DocumentElement.AppendChild(xml.CreateDropDownNode(dropDowns, TN.Bank, null));

                if (dropDowns.DocumentElement.ChildNodes.Count > 0)
                {
                    DataSet ds = StaticDataManager.GetDropDownData(dropDowns.DocumentElement, out error);
                    if (error.Length > 0)
                        ShowError(error);
                    else
                    {
                        foreach (DataTable dt in ds.Tables)
                        {
                            StaticData.Tables[dt.TableName] = dt;
                        }
                    }
                }
                #endregion

                // Take a copy of the PayMethod table and delete 'Not Applicable'
                DataTable dtPayMethod = ((DataTable)StaticData.Tables[TN.PayMethod]).Copy(); //IP - 19/01/11 - Store Card - Sprint4 - #2669 - taking a copy of the table
                //for (int i = 0; i < dtPayMethod.Rows.Count; i++)
                //{
                //    if (((string)dtPayMethod.Rows[i][CN.Code]).Trim() == "0") // Exclude "Not Applicable"
                //    {
                //        dtPayMethod.Rows[i].Delete();
                //        break;
                //    }	
                //}

                //IP - 11/02/10 - CR1048 (Ref:3.1.2.2/3.1.2.3) Merged - Malaysia Enhancements (CR1072)
                //Only select the payment methods where the following is true.
                // B = Both, P = Payment Screen, '' = Blank(appear in both payments and cash & go)
                dtPayMethod.DefaultView.RowFilter = DefaultPaymentFilter;

                //drpPayMethod.DataSource = dtPayMethod;
                if ((bool)Country[CountryParameterNames.StoreCardEnabled] != true)
                {
                    dtPayMethod.DefaultView.RowFilter += AdditionalPaymentFilterStoreCard;
                }

                drpPayMethod.DataSource = dtPayMethod.DefaultView;
                drpPayMethod.ValueMember = CN.Code;
                drpPayMethod.DisplayMember = CN.CodeDescription;

                drpCardType.DataSource = (DataTable)StaticData.Tables[TN.CreditCard];
                drpCardType.DisplayMember = CN.CodeDescription;

                drpBank.DataSource = (DataTable)StaticData.Tables[TN.Bank];
                drpBank.DisplayMember = CN.BankName;

                if (this.txtCustId.Text.Length > 0)
                {
                    LoadData(this.txtCustId.Text, true);
                }
                else if (this.txtAccountNo.Text.Length > 1
                    && !this.txtAccountNo.Text.Equals(this._blankAcctNo))
                {
                    SearchAccountNo();
                }
                else ResetScreen();

                if (this._initPayAmount > 0)
                {
                    this.txtTotalAmount.Text = this.txtPayAmount.Text = this._initPayAmount.ToString(DecimalPlaces);
                    this.txtPayAmount.Focus();
                }

                //CR855
                // Determine if Show Photograph button should be enabled
                // Method not required to be run from here since txtCustId_Leave event will fire in same process
                //EnablePhotographButton();

                // Early warning if receipt printer not available
                Wait();
                if (!ThermalPrintingEnabled
                    && this.SlipPrinterOK() != true)
                {
                    AvoidControlDispose = true;
                    this.CloseTab();
                }

                if (((bool)Country[CountryParameterNames.StoreCardEnabled]))
                {
                    MagStripeReader = new StoreCardMagStripeReader(Country[CountryParameterNames.StoreCardMagStripeReaderName].ToString(),
                    readEvent =>
                    {
                        if (readEvent.state == ControlState.Idle && IsStoreCardPaymentSelected())
                        {
                            this.BeginInvoke(new InvokeDelegate(delegate
                            {
                                mtb_CardNo.Text = DecodeMsrTrack.Decode(readEvent.Track1);
                            }));
                        }
                    },
                    errorevent =>
                    {
                        MainForm.Current.ShowStatus("Magnetic Stripe Read failed.");
                    }
                    );
                }

                cbMiniStat.Checked = PrintAutomaticMiniStatement;                   //IP - 18/05/12 - #9445 - CR1239
                btnCustomerDetails.Enabled = Credential.HasPermission(Blue.Cosacs.Shared.CosacsPermissionEnum.PaymentsCustomerSearch);

            }
            catch (Exception ex)
            {
                Catch(ex, Function);
                ((MainForm)this.FormRoot).statusBar1.Text = "";
            }
            finally
            {
                StopWait();
            }
        }

        private void EnablePhotographButton()
        {
            try
            {
                if (txtCustId.Text != String.Empty)
                {
                    fileName = CustomerManager.GetCustomerPhoto(txtCustId.Text.Trim(), out Error);

                    string signatureFileName = String.Empty;
                    signatureFileName = CustomerManager.GetCustomerSignature(txtCustId.Text.Trim(), out Error);

                    if (fileName != String.Empty || signatureFileName != String.Empty)
                    {
                        button1.Enabled = true;
                    }
                    else
                    {
                        button1.Enabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Reset Screen";
                Wait();
                ClearLoyalty();
                this.ResetScreen();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void ClearLoyalty()
        {
            LoyaltyAmount = 0;
            LoyaltyAcct = "";
            Loyalty_lbl.Visible = false;
            LoyaltyLogo_pb.Visible = false;
        }

        private void btnExit_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Close Tab";
                Wait();
                CloseTab();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void btnSearchAccount_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Account Search";
                Wait();

                AccountSearch a = new AccountSearch();
                a.FormRoot = this.FormRoot;
                a.FormParent = this;
                ((MainForm)this.FormRoot).AddTabPage(a, 7);

                //CR855
                // Determine if Show Photograph button should be enabled
                EnablePhotographButton();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void dgAccountList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Account Details and Print Invoice are on the account list context menu
            try
            {
                //IP - 11/02/10 - CR1048 (Ref:3.1.2.2/3.1.2.3) Merged - Malaysia Enhancements (CR1072)
                //If drpPayMethod not populated do not continue to load details for the account.
                if (drpPayMethod.Items.Count > 0)
                {
                    Function = "Payment Screen: Click on Account List";
                    Wait();

                    // Do not allow the focus to land in this DataGrid
                    // because cursor key movement can only be trapped
                    // with the CurrentCellChanged event which then confuses
                    // the row selection. Row selection can only be performed
                    // with the mouse button.
                    this.txtReceiptNo.Focus();

                    int index = dgAccountList.CurrentRowIndex;
                    if (index >= 0)
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            DataGrid ctl = (DataGrid)sender;

                            MenuCommand m1 = new MenuCommand(GetResource("P_ACCOUNT_DETAILS"));
                            //MenuCommand m2 = new MenuCommand(GetResource("P_PRINT_INVOICE"));
                            m1.Click += new System.EventHandler(this.cmenuAccountDetails_Click);
                            //m2.Click += new System.EventHandler(this.cmenuPrintInvoice_Click);

                            PopupMenu popup = new PopupMenu();
                            popup.Animate = Animate.Yes;
                            popup.AnimateStyle = Animation.SlideHorVerPositive;

                            popup.MenuCommands.AddRange(new MenuCommand[] { m1 });
                            MenuCommand selected = popup.TrackPopup(ctl.PointToScreen(new Point(e.X, e.Y)));
                        }
                        else
                        {

                            // Get SPA details (if they exist) for the account - CR 852 JH 29/05/2007
                            PopulateSPADetails(dgAccountList[index, 5].ToString());
                            SelectAccount(index);
                            drpPayMethod.SelectedIndex = 0;

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void PopulateSPADetails(string acctNo)
        {
            DataSet dsSPA = new DataSet();
            dsSPA = CustomerManager.GetSPADetails(acctNo, out error);      //CR1084 jec
            if (error.Length > 0)
            {
                ShowError(error);
                StopWait();
                return;
            }

            decimal SPAInstalment = 0.0M;
            decimal PTPInstalment = 0.0M;      //CR1084 jec
            gbPromiseToPay.Visible = false;

            if (dsSPA != null)
            {
                if (dsSPA.Tables[0].Rows.Count > 0)
                {

                    foreach (DataRow row in dsSPA.Tables[0].Rows)
                    {
                        if (row[0].ToString() == acctNo)//txtAccountNo.Text.Replace("-", ""))
                        {
                            gbSPA.Enabled = true;
                            gbSPA.Visible = true;     //CR1084 jec
                            SPAInstalment = Math.Round(SPAInstalment + Convert.ToDecimal(row[1]), this._precision);
                            txtSPAInstalment.Text = SPAInstalment.ToString(DecimalPlaces);
                            txtExpiryDate.Text = ((DateTime)row[2]).ToShortDateString();
                            break;
                        }
                        else
                        {
                            gbSPA.Enabled = false;
                            gbSPA.Visible = false;     //CR1084 jec
                            txtSPAInstalment.Text = String.Empty;
                            txtExpiryDate.Text = String.Empty;
                        }
                    }
                }
                // PTP details  CR1084 jec 
                if (dsSPA.Tables[1].Rows.Count > 0)
                {
                    foreach (DataRow row in dsSPA.Tables[1].Rows)
                    {
                        if (row[0].ToString() == acctNo)
                        {
                            gbPromiseToPay.Visible = true;
                            PTPInstalment = Math.Round(PTPInstalment + Convert.ToDecimal(row[1]), this._precision);
                            txtPTPAmount.Text = PTPInstalment.ToString(DecimalPlaces);
                            txtPTPDueDate.Text = ((DateTime)row[2]).ToShortDateString();
                            break;
                        }
                        else
                        {
                            gbPromiseToPay.Visible = false;
                            txtPTPAmount.Text = String.Empty;
                            txtPTPDueDate.Text = String.Empty;
                        }
                    }
                }
            }
        }

        private void tpTransactions_Enter(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Load Transactions";

                Wait();
                // Load transactions for this account or combined RF accounts
                this.LoadTransactions(false);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void cmenuAccountDetails_Click(object sender, System.EventArgs e)
        {
            // Account details menu option
            try
            {
                Function = "Payment Screen: Account Details context menu";
                Wait();

                int index = dgAccountList.CurrentRowIndex;

                if (index >= 0)
                {
                    string acctNo = (string)dgAccountList[index, 4];
                    if (acctNo.Length != 0)
                    {
                        AccountDetails details = new AccountDetails(acctNo.Replace("-", ""), FormRoot, this);
                        ((MainForm)this.FormRoot).AddTabPage(details, 7);
                    }
                    else
                        ShowInfo("M_NOACCOUNT");
                }
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void ReceiptNo_Leave(object sender, System.EventArgs e)
        {
            // Validate a Receipt Number
            try
            {
                Function = "Payment Screen: Validate Receipt Number";

                // Make sure this event has been fired when it is useful
                this.txtReceiptNo.Text = this.txtReceiptNo.Text.Trim();
                if (!this._userChanged ||
                    this.txtReceiptNo.Text == this._lastReceiptNo)
                {
                    return;
                }
                this._lastReceiptNo = this.txtReceiptNo.Text;

                // Check a numeric Receipt No has been entered
                if (this.txtReceiptNo.Text.Length < 1)
                {
                    return;
                }
                else if (!IsNumeric(this.txtReceiptNo.Text) ||
                    this.txtReceiptNo.Text.Substring(0, 1) == "+" ||
                    this.txtReceiptNo.Text.Substring(0, 1) == "-")
                {
                    ShowInfo("M_NUMERIC");
                    this.txtReceiptNo.Text = "";
                    this._lastReceiptNo = "";
                    this.txtReceiptNo.Focus();
                    return;
                }

                Wait();
                // Load the Receipt details
                DataSet TempReceiptSet = PaymentManager.GetTempReceipt(Convert.ToInt32(this.txtReceiptNo.Text), out error);

                if (error.Length > 0)
                {
                    ShowError(error);
                    StopWait();
                    return;
                }

                foreach (DataTable TempReceiptDetails in TempReceiptSet.Tables)
                {
                    if (TempReceiptDetails.TableName == TN.TempReceipt)
                    {
                        if (TempReceiptDetails.Rows.Count > 0)
                            this._curReceipt = TempReceiptDetails.Rows[0];
                    }
                }

                if (this._curReceipt == null)
                {
                    // Receipt number not found
                    ShowInfo("M_RECEIPTNOTALLOCATED", new Object[] { this.txtReceiptNo.Text });
                    this.txtReceiptNo.Text = "";
                    this._lastReceiptNo = "";
                    this.txtReceiptNo.Focus();
                }
                else if (Convert.ToDecimal(this._curReceipt[CN.Amount].ToString()) != 0.0M)
                {
                    // Receipt already has a payment
                    ShowInfo("M_RECEIPTPAID", new Object[] { this.txtReceiptNo.Text });
                    this.txtReceiptNo.Text = "";
                    this._lastReceiptNo = "";
                    this.txtReceiptNo.Focus();
                }
                else
                {
                    this.CheckReceiptAllocation();
                }
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void txtTotalAmount_Leave(object sender, System.EventArgs e)
        {
            // Validate the Total Amount
            try
            {
                Function = "Payment Screen: Validate Total Amount";
                decimal curTotalAmount = 0.0M;
                //decimal newFee = 0.0M;

                // Check a numeric Total Amount has been entered
                if (!this._userChanged ||
                    !this.ValidMoneyField(this.txtTotalAmount, out curTotalAmount)) return;

                // Check the value has been changed
                if (curTotalAmount == this._lastTotalAmount) return;
                this._lastTotalAmount = curTotalAmount;

                Wait();

                // Allow blank and clear other fields
                if (curTotalAmount == 0.0M)
                {
                    this._lastFee = 0;
                    this._lastPayAmount = 0;
                    this.txtFee.Text = "";
                    this.txtPayAmount.Text = "";
                    this.txtChange.Text = "";
                }

                // Spread the list of payments (before calculating each fee).
                // This will load the new Fee/Commision amount for the Total Amount.
                // The calculation might be required for commission even if the fee is not displayed.
                this.SetPaymentList(curTotalAmount);

                bool iscurrentlySettled = (MoneyStrToDecimal(txtPayAmount.Text) >= MoneyStrToDecimal(txtSettlement.Text));

               // GetCustomerWarrantyRenewals(iscurrentlySettled, false);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }

        }

        private void txtFee_Leave(object sender, System.EventArgs e)
        {
            // Validate the Credit Fee
            try
            {
                Function = "Payment Screen: Validate Credit Fee";
                //bool status = true;
                decimal curFee = 0.0M;

                // Check a numeric Credit Fee has been entered
                if (!this._userChanged ||
                    !this.ValidMoneyField(this.txtFee, out curFee)) return;

                // Check the value has been changed
                if (curFee == this._lastFee) return;

                Wait();

                AuthorisePrompt ap = new AuthorisePrompt(this, lAuthorise, GetResource("M_CREDITFEE"));
                ap.ShowDialog();

                if (ap.Authorised)
                {
                    this._authorisedBy = ap.AuthorisedBy;
                }
                else
                {
                    // Revert to the previous value
                    this.txtFee.Text = this._lastFee.ToString(DecimalPlaces);
                    return;
                }

                this._lastFee = curFee;

                // Spread the list of fees
                this.SetFeeList(curFee);

                // Recalculate other fields
                this.CalcPaymentFields(MoneyStrToDecimal(this.txtTotalAmount.Text), curFee);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }

        }

        private void txtPayAmount_Leave(object sender, System.EventArgs e)
        {
            // Validate the Payment Amount
            try
            {
                errorProvider1.SetError(txtPayAmount, "");

                Function = "Payment Screen: Validate Payment Amount";
                decimal curPayAmount = 0.0M;
                //decimal noFee = 0.0M;
                bool iscurrentlySettled = false;
                int curPayMethod = Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()); //IP - 02/12/10 - Store Card

                if (PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard))
                {
                    StoreCardCheckPayButton();
                }
                // Check a numeric Payment Amount has been entered
                if (!this._userChanged ||
                    !this.txtPayAmount.Enabled ||
                    !this.ValidMoneyField(this.txtPayAmount, out curPayAmount)) return;

                // Check the value has been changed
                if (curPayAmount == this._lastPayAmount) return;
                this._lastPayAmount = curPayAmount;

                Wait();

                // Spread the list of payments.
                // The calculation might be required for commission even if the fee is not displayed.
                this.SetPaymentList(curPayAmount);

                if (MoneyStrToDecimal(txtPayAmount.Text) >= MoneyStrToDecimal(txtSettlement.Text))
                {
                    iscurrentlySettled = true;
                }

                //GetCustomerWarrantyRenewals(iscurrentlySettled, false);
                errorProvider1.SetError(txtPayAmount, "");

                if (txtSelectedAcctNo.Text == LoyaltyAcct)
                {
                    if (MoneyStrToDecimal(txtPayAmount.Text) != MoneyStrToDecimal(txtOutstandingBalance.Text))
                    {
                        txtPayAmount.Text = "";
                        errorProvider1.SetError(txtPayAmount, "You can only pay the outstanding balance for a HomeClub account.");
                    }
                }

                //var pay = 0.0m;
                // var storeCardNo = mtb_Cardno.Text.Trim();
                //if (!decimal.TryParse(txtTendered.Text, out pay) ||  errorProvider1.GetError(txtPayAmount).Trim().Length > 0)
                //{
                //    errorProvider1.SetError(txtPayAmount, "This is not a valid amount.");
                //}
                //else
                //{


                //}

            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }

        }

        //NM Commented : UAT(5.2) - 605, 629
        //private void txtTendered_Leave(object sender, System.EventArgs e) 
        private void txtTendered_Validating(object sender, CancelEventArgs e)
        {
            // Validate the Tendered Amount
            try
            {
                Function = "Payment Screen: Validate Tendered Amount";
                decimal curTendered = 0.0M;

                // Check a numeric Tendered Amount has been entered
                if (!this._userChanged ||
                    !this.ValidMoneyField(this.txtTendered, out curTendered)) return;

                // uat309 rdb 9/04/08 if tenderered amt entered for cash then pay mehtod changed, value retained
                // Default a blank Tendered amount to the Total amount

                //RM 26/08/09 71004 check if value for paymethod is >99 ie an exchange amount
                if ((drpPayMethod.Text != "Cash" && Convert.ToInt16(((System.Data.DataRowView)(drpPayMethod.SelectedItem)).Row.ItemArray[0].ToString()) <= 99) || (curTendered < 0.01M && IsStrictMoney(this.txtTotalAmount.Text)))
                {
                    // this.txtTotalAmount.Text = this.txtTendered.Text; //NM Commented : UAT(5.2) - 605, 629
                    this.txtTendered.Text = this.txtTotalAmount.Text; //IP - 28/07/09 - UAT(763) - Reversing change made by Simone for issue (71004).
                    curTendered = MoneyStrToDecimal(this.txtTendered.Text);
                }

                Wait();
                // Recalculate the Change amount
                decimal curChange = curTendered - MoneyStrToDecimal(this.txtTotalAmount.Text);
                if (curChange < 0)
                {
                    // Warn the user the Tendered amount is too small
                    curChange = 0.0M;
                    ShowInfo("M_TENDERED");
                    // Trap the focus in this field
                    this.txtTendered.Focus();
                }
                this.txtTendered.Text = curTendered.ToString(DecimalPlaces);
                this.txtChange.Text = curChange.ToString(DecimalPlaces);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void drpPayMethod_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Set Pay Method";
                Wait();

                clearPaymentFields();           //IP - 25/01/11 - Sprint 5.9 - #2785
                txtPayAmount.Enabled = true;
                SetPayMethod(false);
                SetFeeforStoreCard(PayMethod.IsPayMethod(Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()), PayMethod.StoreCard));
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        //IP - 18/05/12 - #9445 - CR1239
        //private void cbCardPrint_CheckedChanged(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Set Card Print";
        //        Wait();
        //        SetCardPrint();
        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        StopWait();
        //    }
        //}

        private void btnPay_Click(object sender, System.EventArgs e)
        {
            try
            {
                int agrmtno = Convert.ToInt32(this.txtAgrmtNo.Text.Trim());
                //IP - 18/01/11 - Store Card
                string storeCardAcctNo = string.Empty;

                Function = "Payment Screen: Pay Button";

                //Agreement Number must be entered
                if (txtAgrmtNo.Text.Trim().Length <= 0)
                {
                    ShowInfo("M_REQUIREAGREEMENTNO");
                    this.txtAgrmtNo.Focus();
                    return;
                }
                else
                {
                    agrmtno = Convert.ToInt32(txtAgrmtNo.Text.Trim());
                }
                if (agrmtno == 0)
                {
                    ShowInfo("M_REQUIREAGREEMENTNO");
                    this.txtAgrmtNo.Focus();
                    return;

                }


                // The pay method must be entered
                short curPayMethod = Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString());
                if (curPayMethod == 0)
                {
                    ShowInfo("M_REQUIREPAYMETHOD");
                    this.drpPayMethod.Focus();
                    return;
                }


                if (errorProvider1.GetError(txtPayAmount).Trim().Length != 0)
                {
                    ShowInfo("M_AmountPaid");
                    return;
                }

                if (errorProvider1.GetError(txtCardNo).Trim().Length != 0)
                {
                    ShowInfo("M_CardError");
                    return;
                }


                // When paying by card the card type must be entered
                if (this.drpCardType.SelectedIndex == 0
                    && (PayMethod.IsPayMethod(curPayMethod, PayMethod.CreditCard) || PayMethod.IsPayMethod(curPayMethod, PayMethod.DebitCard)))
                {
                    ShowInfo("M_REQUIRECARDTYPE");
                    this.drpCardType.Focus();
                    return;
                }

                // When paying by cheque the Bank Account number must be entered
                if (this.txtBankAcctNo.Text.Trim().Length == 0
                    && PayMethod.IsPayMethod(curPayMethod, PayMethod.Cheque))
                {
                    ShowInfo("M_REQUIREBANKACCOUNTNO");
                    this.txtBankAcctNo.Focus();
                    return;
                }

                // When paying by cheque/card the cheque/card number must be entered - IP - 26/01/11 - Sprint 5.9 - #2785 - Commented out. Replaced with below.
                //if (   this.txtCardNo.Text.Trim().Length == 0
                //    && (   PayMethod.IsPayMethod(curPayMethod, PayMethod.Cheque)
                //    || PayMethod.IsPayMethod(curPayMethod, PayMethod.CreditCard)
                //    || PayMethod.IsPayMethod(curPayMethod, PayMethod.DebitCard)))
                //{
                //    ShowInfo("M_REQUIRECHEQUENO");
                //    this.txtCardNo.Focus();
                //    return;
                //}

                //IP - 26/01/11 - Sprint 5.9 - #2785 - When paying by Cheque the cheque number must be entered
                if (this.txtCardNo.Text.Trim().Length == 0
                    && txtCardNo.Visible == true
                    && PayMethod.IsPayMethod(curPayMethod, PayMethod.Cheque))
                {
                    ShowInfo("M_REQUIRECHEQUENO");
                    this.txtCardNo.Focus();
                    return;
                }

                //IP - 26/01/11 - Sprint 5.9 - #2785 - When paying by Credit/Debit card the card number must be entered
                if (txtCardNo.Text.Trim().Length == 0
                   && mtb_CardNo.Visible == true
                   && (PayMethod.IsPayMethod(curPayMethod, PayMethod.CreditCard) || PayMethod.IsPayMethod(curPayMethod, PayMethod.DebitCard)))
                {
                    ShowInfo("M_REQUIRECHEQUENO");
                    this.mtb_CardNo.Focus();
                    return;
                }

                //Bug #3044
                if (mtb_CardNo.Visible &&
                    (PayMethod.IsPayMethod(curPayMethod, PayMethod.CreditCard) || PayMethod.IsPayMethod(curPayMethod, PayMethod.DebitCard)) &&
                    !mtb_CardNo.MaskCompleted)
                {
                    ShowInfo("M_INCOMPLETECARDNO");
                    mtb_CardNo.Focus();
                    return;
                }

                // The Payment Amount must be entered
                // unless a foreign currency to allow for a cheque to be cashed
                if (MoneyStrToDecimal(this.txtPayAmount.Text) < 0.01M &&
                    curPayMethod < CAT.FPMForeignCurrency)
                {
                    ShowInfo("M_REQUIREPAYAMOUNT");
                    this.txtPayAmount.Focus();
                    return;
                }

                // The Tendered Amount must be entered
                if (!this.txtTendered.Visible)
                    txtTendered_Validating(this, null);

                if (MoneyStrToDecimal(this.txtTendered.Text) < 0.01M
                    || MoneyStrToDecimal(this.txtTendered.Text) < MoneyStrToDecimal(this.txtTotalAmount.Text))
                {
                    ShowInfo("M_REQUIRETENDERED");
                    this.txtTendered.Focus();
                    return;
                }

                //SPECIAL STORECARD ROUTINE
                //if (drpPayMethod.SelectedText == StoreCard)
                if (PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard)) //IP - 02/12/10 - Store Card
                {
                    //IP - 02/12/10 - First check if there were any errors, if not continue to process transfer.
                    if (errorProvider1.GetError(btnStoreCardManualEntry).Trim().Length != 0)
                    {
                        ShowInfo("M_SoreCardErrors");
                        this.mtb_CardNo.Focus();
                        return;
                    }

                    if (errorProviderStoreCard.GetError(drpPayMethod).Trim().Length != 0)
                    {
                        ShowInfo("M_SELECTDIFFERENTPAYMETHOD");
                        this.drpPayMethod.Focus();
                        return;
                    }

                    //SaveStoreCardTransfer(txtSelectedAcctNo.Text, Convert.ToInt16(Config.BranchCode),  MoneyStrToDecimal(this.txtPayAmount.Text));
                    //this.ResetScreen();



                    LoyaltyLogo_pb.Visible = false;
                    Loyalty_lbl.Visible = false;

                }


                // The Payment must not put the account into credit
                decimal Payment = MoneyStrToDecimal(this.txtPayAmount.Text);
                decimal OutstandingBal = MoneyStrToDecimal(this.txtOutstandingBalance.Text);
                decimal Settlement = MoneyStrToDecimal(this.txtSettlement.Text);
                decimal BadDebtTotal = MoneyStrToDecimal(this.txtBadDebtBalance.Text) + MoneyStrToDecimal(this.txtBadDebtCharges.Text);
                if ((Payment > Settlement && Settlement >= 0.01M) ||
                    (Payment > BadDebtTotal && BadDebtTotal >= 0.01M))
                {
                    if (DialogResult.Cancel == ShowInfo("M_INVALIDCREDIT", MessageBoxButtons.OKCancel))
                        //ShowInfo("M_INVALIDCREDIT");
                        return;
                }

                if (this._calculatedFee != MoneyStrToDecimal(this.txtFee.Text))
                {
                    // TODO: The user has changed the Fee so a password is required
                }

                Wait();
                // Early warning if receipt printer not available
                //if ((!ThermalPrintingEnabled || this.cbCardPrint.Checked)
                //    && this.SlipPrinterOK() != true)
                if (!ThermalPrintingEnabled               //IP - 18/05/12 - #9445 - CR1239
                && this.SlipPrinterOK() != true)
                {
                    StopWait();
                    return;
                }

                //
                // Process the payment
                //

                // Disable the Account List, Account Details and Payment group boxes
                this.gbAccountList.Enabled = false;
                this.gbSelectedAccount.Enabled = false;
                this.gbNewPayment.Enabled = false;

                ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SAVINGTRANSACTION");


                // Prepare input parameters
                string curBankCode = drpBank.SelectedItem != null ? ((DataRowView)drpBank.SelectedItem)[CN.BankCode].ToString() : ""; //Bug #3040
                int curReceiptNo = 0;
                if (this.txtReceiptNo.Text.Trim().Length > 0 && IsStrictNumeric(this.txtReceiptNo.Text))
                    curReceiptNo = Convert.ToInt32(this.txtReceiptNo.Text);

                // Declare output parameters
                int curCommissionRef = 0;
                int curPaymentRef = 0;
                int curRebateRef = 0;
                decimal curRebateSum = 0;

                // Check the list of payments has been set up
                if (!this.CheckPaymentSet())
                {
                    ShowInfo("M_REQUIRETENDERED");
                    return;
                }

                // Save the payments and return the set of 
                // transactions just created.
                DataSet lastTransactionSet = lastTransactionSet = PaymentManager.SavePayment(
                    this.txtSelectedAcctNo.Text,
                    this._curSundryCredit,
                    curPayMethod,
                    this.txtCardNo.Text,
                    curBankCode,
                    this.txtBankAcctNo.Text,
                    Convert.ToInt16(Config.BranchCode),
                    this._paymentSet,
                    MoneyStrToDecimal(this.txtTendered.Text),
                    MoneyStrToDecimal(this.txtChange.Text),
                    this._authorisedBy,
                    this._curChequeClearance,
                    curReceiptNo,
                    Config.CountryCode,
                    this.VoucherReference,
                    this.CourtsVoucher,
                    this.VoucherAuthorisedBy,
                    VoucherCompanyAcctNo,
                    this._returnedChequeAuthorisedBy,
                    agrmtno,
                    storeCardValidated != null ? storeCardValidated.Acctno : string.Empty,                               //IP - 10/01/11 - Store Card
                    storeCardValidated != null ? storeCardValidated.CardNo : 0,                               //IP - 10/01/11 - Store Card
                    out curCommissionRef,
                    out curPaymentRef,
                    out curRebateRef,
                    out curRebateSum,
                    out error);

                if (error.Length > 0)
                {
                    ShowError(error);
                    StopWait();
                    return;
                }

                if (txtSelectedAcctNo.Text == LoyaltyAcct)
                {
                    CustomerManager.LoyaltyPay(txtSelectedAcctNo.Text);
                }

                if (curRebateSum != 0)
                {
                    //#14890
                    GetCustomerWarrantyRenewalsSettle(true, false, curPayMethod, Payment, lastTransactionSet);

                    ShowInfo("M_REBATEPAID");

                }
                else
                {
                    SlipPrinterReceipt(curPayMethod, Payment, lastTransactionSet);
                }

                LoyaltyLogo_pb.Visible = false;
                Loyalty_lbl.Visible = false;

            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void SlipPrinterReceipt(short curPayMethod, decimal Payment, DataSet lastTransactionSet)
        {
            if (this._slipRequired)
            {
                //66669 Check for whether or not the transaction has been committed to the database
                bool dataCommitted = true;
                foreach (DataRow row in lastTransactionSet.Tables[TN.Transactions].Rows)
                {
                    if (row[CN.Committed].ToString() == "N")
                    {
                        dataCommitted = false;
                        break;
                    }
                }

                if (dataCommitted == false)
                {
                    ShowError("The transaction has not been committed to the database. No print-out will therefore be allowed. Please try again. \n\nIf the problem persists contact CoSACS support.");
                }
                else
                {
                    // Print the payment
                    ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_TRANSACTIONSAVEDPRINT");

                    // Load the new outstanding balance
                    decimal newBalance = 0.0M;
                    decimal availableSpend = 0, creditLimit = 0;

                    if (this._combinedRF)
                    {
                        DataSet RFCombinedSet = CustomerManager.GetRFCombinedDetailsForPrint(this.txtCustId.Text, out error);
                        foreach (DataTable RFCombinedTable in RFCombinedSet.Tables)
                            if (RFCombinedTable.TableName == TN.RFDetails && RFCombinedTable.Rows.Count > 0)
                            {
                                newBalance = Convert.ToDecimal(RFCombinedTable.Rows[0][CN.TotalBalances].ToString());
                                availableSpend = Convert.ToDecimal(RFCombinedTable.Rows[0][CN.AvailableCredit].ToString());
                                creditLimit = Convert.ToDecimal(RFCombinedTable.Rows[0][CN.TotalCredit].ToString());
                            }
                    }
                    else
                    {
                        DataSet accountDetailsSet = AccountManager.GetAccountDetails(this.txtSelectedAcctNo.Text, out error);
                        foreach (DataTable accountDetailsTable in accountDetailsSet.Tables)
                            if (accountDetailsTable.TableName == TN.AccountDetails && accountDetailsTable.Rows.Count > 0)
                            {

                                newBalance = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.OutstandingBalance2].ToString());
                                if (newBalance == 0)
                                    newBalance = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.BDWBalance].ToString())
                                        + Convert.ToDecimal(accountDetailsTable.Rows[0][CN.BDWCharges].ToString());

                                availableSpend = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.AvailableSpend].ToString());
                                creditLimit = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.CreditLimit].ToString());

                                if (PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard))
                                {

                                    //IP - 12/01/10 - Store Card - Retrieve updated Store Card values
                                    //cardInfo.StoreCardLimit = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.StoreCardLimit].ToString());
                                    //cardInfo.StoreCardAvailable = Convert.ToDecimal(accountDetailsTable.Rows[0][CN.StoreCardAvailable].ToString());
                                }
                            }
                    }
                    //////if (error.Length > 0)
                    //////{
                    //////    ShowError(error);
                    //////    StopWait();
                    //////    return;
                    //////}

                    NewPrintPaymentReceipt
                        (
                           lastTransactionSet.Tables[TN.Transactions],
                           newBalance,
                           creditLimit,
                           availableSpend,
                           txtCustId.Text,
                           this.txtCustomerName.Text,
                           txtSelectedAcctNo.Text,
                           curPayMethod,
                           _combinedRF,
                           (string)this._curAccount[CN.AccountType],      //IP - 21/05/12 - #10146 
                           cbMiniStat.Checked                           //IP - 18/05/12 - #9445 - CR1239   
                       );

                    //IP - 18/06/12 - #9448 - CR1239 - Print Tax Invoice after payment processed if the following is true.
                    if (ThermalPrintingEnabled && Convert.ToBoolean(Country[CountryParameterNames.TaxInvPrintAfterPayment]))
                    {
                        menuTaxInvoice_Click(null, null);
                    }

                    //IP - 11/01/11 - Store Card - Proceed to print the Store Card Receipt if a payment was processed using a Store Card

                    if (PayMethod.IsPayMethod(curPayMethod, PayMethod.StoreCard))
                    {
                        var storeCardNum = mtb_CardNo.Text.Trim();
                        var custAddr1 = addressTab1.txtAddress1.Text;
                        var custAddr2 = addressTab1.cmbVillage.SelectedIndex != -1 ? (string)addressTab1.cmbVillage.SelectedValue  : addressTab1.cmbVillage.Text;
                        var custAddr3 = addressTab1.cmbRegion.SelectedIndex != -1 ? (string)addressTab1.cmbRegion.SelectedValue : addressTab1.cmbRegion.Text;
                        var custAddr4 = addressTab1.txtPostCode.Text;

                        var invoiceNo = txtSelectedAcctNo.Text;
                        var receiptNo = Convert.ToInt32(lastTransactionSet.Tables[0].Rows[0][CN.TransRefNo]);

                        NewPrintStoreCardReceipt(txtCustomerName.Text, custAddr1, custAddr2, custAddr3, custAddr4, invoiceNo, receiptNo, Payment, storeCardValidated);

                    }

                    //}
                }  // End of Slip Print required
 
            }
            this.ResetScreen();
        
        }



        //IP - 18/05/12 - #9445 - CR1239
        //private void txtCardRowNo_Leave(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Card Print Row No Value";

        //        Wait();
        //        // Check the row number fits on the card print type
        //        CheckCardRow();
        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        StopWait();
        //    }
        //}

        //IP - 18/05/12 - #9445 - CR1239
        //private void txtCardPrint_Click(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Cycle card print spin control";

        //        Wait();
        //        // Cycle the spin control when outside the min/max range
        //        if (this.txtCardRowNo.Value < 1)
        //            this.txtCardRowNo.Value = this.MaxCardLines();
        //        else if (this.txtCardRowNo.Value > this.MaxCardLines())
        //            this.txtCardRowNo.Value = 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        StopWait();
        //    }
        //}

        //IP - 18/05/12 - #9445 - CR1239
        //private void drpCardPrintType_SelectedIndexChanged(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Card Print Row No Index";

        //        Wait();
        //        // Check the row number fits on the card print type
        //        CheckCardRow();
        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        StopWait();
        //    }
        //}

        private void cbNonPrinted_CheckedChanged(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Display Non-Printed Transactions";

                Wait();
                this.SetNonPrinted(this.cbNonPrinted.Checked);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        public override bool ConfirmClose()
        {
            bool unlockStatus = false;

            try
            {
                Function = "Payment Screen: Confirm Close";

                Wait();
                // Unlock the accounts
                unlockStatus = this.UnlockAccounts();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }

            return unlockStatus;
        }

        //Changed to public to allow call from service search
        public void SearchCustId()
        {
            try
            {
                Function = "Payment Screen: Load on Customer Id";

                // Check a Customer ID has been entered
                // Now uses format mask - this.txtCustId.Text = this.txtCustId.Text.Trim();
                if (this.txtCustId.Text == this._lastCustId) return;

                if (this.txtCustId.Text.Length > 0)
                {
                    Wait();
                    if (LoadData(this.txtCustId.Text, true))
                    {
                        this.txtAccountNo.Text = "";
                        this._lastAccountNo = "";

                        ////IP - 29/11/10 - Store Card - Retrieve the Store Card available balance if one exists for this Customer
                        //if ((bool)Country[CountryParameterNames.StoreCardEnabled] == true)
                        //{
                        //    try
                        //    {
                        //      //  cardInfo = StoreCardCommon.ConvertStoreCard(StoreCardManager.GetStoreCardAll(Convert.ToInt64(0))); //TODO - StoreCard Was calling with Custid...
                        //        //IP - 11/01/10 - Retrieve Store Card object

                        //        //GetStoreCardAccountandBalance(this.txtCustId.Text);
                        //       // StoreCardButtonEnable(cardInfo.StoreCardAvailable);
                        //    }
                        //    catch
                        //    { //do nothing in case no rows brought back  
                        //    }
                        //}

                        if (((DataView)dgAccountList.DataSource).Count > 0)
                        {
                            // Select the first account in the list
                            this.SelectAccount(0);
                            CheckSelectAccount(0);
                            this.txtPayAmount.Focus();
                        }
                    }
                }
                this._lastCustId = this.txtCustId.Text;
            }
            catch (Exception ex)
            {
                ((MainForm)this.FormRoot).statusBar1.Text = "";
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        public void SearchAccountNo()
        {
            Function = "Payment Screen: Load on Account No";

            try
            {
                // Check an Account No has been entered
                this.txtAccountNo.Text = this.txtAccountNo.Text.Trim();
                if (this.txtAccountNo.Text == this._lastAccountNo) return;
                this._lastAccountNo = this.txtAccountNo.Text;

                if (this.txtAccountNo.Text.Length < 1
                    || this.txtAccountNo.Text.Equals(this._blankAcctNo))
                {
                    //ShowInfo("M_REQUIREACCOUNTNO");
                    //this.txtAccountNo.Focus();
                    return;
                }

                Wait();
                string searchAccountNo = this.txtAccountNo.Text;
                ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_SEARCHACCOUNT");

                // Find the Customer ID for this Account Number
                DataSet CustomerSet = CustomerManager.GetCustomerAccountsDetails(this.txtAccountNo.Text.Replace("-", ""), out error);
                if (error.Length > 0)
                {
                    ShowError(error);
                    StopWait();
                    return;
                }
                if (CustomerSet.Tables[TN.Customer].Rows.Count < 1)
                {
                    ShowInfo("M_NOSUCHACCOUNT");
                    this.ResetScreen();
                    this.txtAccountNo.Focus();
                    StopWait();
                    return;
                }

                if ((bool)Country[CountryParameterNames.StoreCardEnabled] == true)
                {
                    //GetStoreCardAccountandBalance( CustomerSet.Tables[TN.Customer].Rows[0][CN.CustomerID].ToString());
                    try
                    {
                        //cardInfo = StoreCardManager.GetStoreCardDetailsShared(string.Empty, CustomerSet.Tables[TN.Customer].Rows[0][CN.CustomerID].ToString()); //IP - 11/01/10 - Retrieve Store Card object

                        //StoreCardButtonEnable(cardInfo.StoreCardAvailable);
                        //TODO - Storecard
                    }
                    catch // (Exception ex)
                    {
                        //Catch(ex, Function);
                    }
                }

                // CR1084 set pop-ups sproc DN_CustomerGetAccountsAndDetailsSP
                for (int i = 0; i < CustomerSet.Tables[TN.Customer].Rows.Count; i++)
                {
                    if (Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["CLA"]) == "CLA")
                    {
                        custLeftAddress = true;
                        AdditionalNotes = Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["AddressNotes"]);
                    }
                    else
                    {
                        custLeftAddress = false;
                    }

                    if (Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["INFO"]) == "INFO")
                    {
                        additionalInfo = true;
                        AdditionalNotes = Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["InfoNotes"]);
                    }
                    else
                    {
                        additionalInfo = false;
                    }

                    if (Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["TOS"]) == "TOS")
                    {
                        telNotInService = true;
                        AdditionalNotes = Convert.ToString(CustomerSet.Tables[TN.Customer].Rows[i]["TOSNotes"]);

                    }
                    else
                    {
                        telNotInService = false;
                    }
                    // #8486 - Show Cash Loan Qualified message 
                    if (Convert.ToBoolean(CustomerSet.Tables[TN.Customer].Rows[i]["LoanQualified"]) == true)
                    {
                        lblCashLoan.Visible = true;
                    }
                    else
                    {
                        lblCashLoan.Visible = false;
                    }
                }

                if (LoadData(CustomerSet.Tables[TN.Customer].Rows[0][CN.CustomerID].ToString(), true))
                {
                    // Select the account that was used for the search
                    this.txtAccountNo.Text = searchAccountNo;
                    this._lastCustId = this.txtCustId.Text;
                    DataView accountList = (DataView)dgAccountList.DataSource;

                    if (accountList.Count > 0)
                    {
                        searchAccountNo = searchAccountNo.Replace("-", "");
                        int i = 0;
                        int accountIndex = -1;
                        while (i < accountList.Count && accountIndex == -1)
                        {
                            if (accountList[i].Row[CN.AccountNumber].ToString() == searchAccountNo)
                            {
                                accountIndex = i;
                            }
                            i++;
                        }
                        // Select the account from the list returned.
                        // The specified account could be cancelled or settled
                        // and therefore not returned, so select the
                        // first account in the list instead.
                        if (accountIndex == -1) accountIndex = 0;
                        this.SelectAccount(accountIndex);
                        this.CheckSelectAccount(accountIndex);
                        this.txtPayAmount.Focus();
                    }
                }

                // Get SPA details (if they exist) for the account - CR 852 JH 29/05/2007
                PopulateSPADetails(txtAccountNo.Text.Replace("-", ""));

                //Check for pop-ups here
                if (custLeftAddress && popCustLeftAddr == true)
                {
                    MessageBox.Show(GetResource("M_CUSTOMERLEFTADDR"), "Customer Left Address", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                if (otherArrears && popAcctsInArrears == true)
                {
                    MessageBox.Show(GetResource("M_CUSTOMEROTHERARREARS"), "Other Accounts in Arrears", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                if (additionalInfo && popAdditionalInfo == true)
                {
                    MessageBox.Show(GetResource("M_CUSTOMERADDITINFO"), "Addition Information Required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    btnCustomerDetails_Click(null, null);
                }

                if (telNotInService && popPhoneOutOfService == true)
                {
                    MessageBox.Show(GetResource("M_CUSTOMERTELNOTINSERVICE"), "Telephone number out of Service/Invalid number", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    btnCustomerDetails_Click(null, null);
                }

            }
            catch (Exception ex)
            {
                ((MainForm)this.FormRoot).statusBar1.Text = "";
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void txtCustId_Leave(object sender, System.EventArgs e)
        {
            if (!this.txtCustId.ReadOnly) this.SearchCustId();

            //CR855
            EnablePhotographButton();
        }

        private void txtAccountNo_Leave(object sender, System.EventArgs e)
        {
            //IP - 11/02/10 - CR1048 (Ref:3.1.2.2/3.1.2.3) Merged - Malaysia Enhancements (CR1072)
            //If drpPayMethod not populated do not load details for the account.
            if (drpPayMethod.Items.Count > 0)
            {
                if (!this.txtAccountNo.ReadOnly) this.SearchAccountNo();

                //CR855
                //EnablePhotographButton();

                /*if (this.txtAccountNo.UnformattedText != "000000000000")
                {
                    menuWarrantRenewals.Visible = true;
                    menuCheckExpiringWarranties.Visible = true;
                }*/
            }

        }

        private void btnCopyInstallment_Click(object sender, System.EventArgs e)
        {
            // DSR 20/5/2003 Use total amount in case the fee is displayed
            this.txtTotalAmount.Text = this.txtInstalment.Text;

            // Invoke leave method
            this.txtTotalAmount_Leave(new object(), new System.EventArgs());
        }

        private void btnCopyAgreementTotal_Click(object sender, System.EventArgs e)
        {
            // DSR 20/5/2003 Use total amount in case the fee is displayed
            this.txtTotalAmount.Text = this.txtAgreementTotal.Text;

            // Invoke leave method
            this.txtTotalAmount_Leave(new object(), new System.EventArgs());
        }

        private void menuTaxInvoice_Click(object sender, System.EventArgs e)
        {
            int noPrints = 0;
            try
            {
                Wait();

                if (this._curAccount != null)
                {
                    XmlNode lineItems = AccountManager.GetLineItems((string)this._curAccount[CN.AccountNumber], 1,
                        Convert.ToString(this._curAccount[CN.AccountType]).Trim(),
                        Config.CountryCode, Convert.ToInt16(Config.BranchCode), out error);
                    if (error.Length > 0)
                        ShowError(error);
                    else
                    {
                        bool taxExempt = AccountManager.IsTaxExempt((string)this._curAccount[CN.AccountNumber], Convert.ToInt32(this._curAccount[CN.AgrmtNo]).ToString(), out Error);
                        NewPrintTaxInvoice((string)this._curAccount[CN.AccountNumber], Convert.ToInt32(this._curAccount[CN.AgrmtNo]),
                            (string)this._curAccount[CN.AccountType],
                            txtCustId.Text, false, false, null, 0, 0,
                            lineItems, 0, ((CommonForm)FormRoot).CreateBrowserArray(1)[0], ref noPrints, false, true, Credential.UserId,
                            Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code]), null, taxExempt);
                    }
                }
            }
            catch (Exception ex)
            { Catch(ex, "menuTaxInvoice_Click"); }
            finally
            { StopWait(); }
        }

        private void menuStatement_Click(object sender, System.EventArgs e)
        {
            try
            {
                Wait();

                CountryParameter.StatementPrintTypes statementPrintType = (CountryParameter.StatementPrintTypes)Country.GetCountryParameterValue<int>(CountryParameterNames.Printing.StatementPrintType);
                DataRow selectedRow = ((DataView)dgAccountList.DataSource).Table.Rows[dgAccountList.CurrentRowIndex];

                if (!Config.ThermalPrintingEnabled
                    || statementPrintType == CountryParameter.StatementPrintTypes.LaserPrinter)
                {
                    if (dgAccountList.CurrentRowIndex >= 0)
                    {
                        PrintStatementOfAccount(((CommonForm)FormRoot).CreateBrowserArray(1)[0], (string)selectedRow[CN.AccountNumber], txtCustId.Text, (string)selectedRow[CN.AccountType]);
                    }
                }
                else if (statementPrintType == CountryParameter.StatementPrintTypes.ReceiptPrinter)
                    ((MainForm)this.FormRoot).OpenStatementsTab((string)selectedRow[CN.AccountNumber], txtCustId.Text);
            }
            catch (Exception ex)
            {
                Catch(ex, "menuStatement_Click");
            }
            finally
            {
                StopWait();
            }
        }

        //IP - 18/05/12 - #9445 - CR1239
        //private void btnUpdateCard_Click(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Update Payment Card Button";

        //        Wait();
        //        // Early warning if receipt printer not available
        //        if (!this.SlipPrinterOK())
        //        {
        //            StopWait();
        //            return;
        //        }

        //        // Print the payment
        //        ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_PRINTCARDUPDATE");

        //        // Reload the transactions for printing
        //        this.LoadTransactions(true);

        //        // Print Payment Card
        //        txtCardRowNo.Value = PrintPaymentCardTransactions(
        //            this._printTransactionSet.Tables[TN.Transactions],
        //            Convert.ToInt16(this.txtCardRowNo.Value),
        //            Convert.ToDecimal(StripCurrency(this.txtOutstandingBalance.Text)),
        //       txtCustId.Text, txtSelectedAcctNo.Text, 0, false);

        //        // Reload the transactions after printing
        //        // Must reload because any failed transactions will still be set to 'N'
        //        this.SetNonPrinted(false);
        //        this._curRFTransactionSet = null;
        //        this.LoadTransactions(true);

        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        ((MainForm)this.FormRoot).statusBar1.Text = "";
        //        StopWait();
        //    }

        //}

        private void btnPrintAcctNo_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Update Payment Card Button";

                Wait();
                PrintAcctNoOnCheque(this.txtSelectedAcctNo.Text);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void btnPaymentList_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Payment List popup button";

                Wait();

                // Check the list of payments has been set up
                this.CheckPaymentSet();

                // Call the payment list popup form
                PaymentList PaymentListPopup = new PaymentList(this.txtCustId.Text, this.txtCustomerName.Text, this.txtFee.Visible, this._paymentSet, this.FormRoot, this.FormParent);
                PaymentListPopup.ShowDialog();
                this._paymentSet = PaymentListPopup.PaymentSet;
                // 69113 There is a situation where the cashier can have a fee authorised
                // go in and change the fee, not authorise it and the authorisedBy value
                // is overwritten by 0 so never set authorisedBy to 0
                if (PaymentListPopup.authorisedBy != 0)
                    this._authorisedBy = PaymentListPopup.authorisedBy;
                // The return parameters are expected to be rounded to DecimalPlaces
                this.CalcPaymentFields(PaymentListPopup.totPayment, PaymentListPopup.totFee);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void menuPaymentCard_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Print New Payment Card";

                Wait();
                // Early warning if receipt printer not available
                if (!this.SlipPrinterOK() || !this._slipRequired)
                {
                    StopWait();
                    return;
                }

                ((MainForm)this.FormRoot).statusBar1.Text = GetResource("M_PRINTCARDNEW", new object[] { this.txtSelectedAcctNo.Text });
                PrintPaymentCard(this.txtCustId.Text, this.txtSelectedAcctNo.Text);
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                ((MainForm)this.FormRoot).statusBar1.Text = "";
                StopWait();
            }
        }

        private void dgAccountList_CurrentCellChanged(object sender, System.EventArgs e)
        {
            // Do not allow the focus to land in this DataGrid
            // because cursor key movement can only be trapped
            // with the CurrentCellChanged event which then confuses
            // the row selection. Row selection can only be performed
            // with the mouse button.
            this.txtReceiptNo.Focus();
            int index = dgAccountList.CurrentRowIndex;
            CheckSelectAccount(index);
            CheckPaymentCalc(index);
            txtPayAmount.Text = "";
            txtTendered.Text = "";
            txtChange.Text = "";
        }

        private void btnExchange_Click(object sender, System.EventArgs e)
        {
            try
            {
                Function = "Payment Screen: Open Exchange Calculator";
                Wait();

                // Call the Foreign Currency Calculator popup form
                this.ExchangeCalculator();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
            }
        }

        private void menuOpenCashTill_Click(object sender, System.EventArgs e)
        {
            try
            {
                Wait();

                OpenCashDrawer(true);

            }
            catch (Exception ex)
            {
                Catch(ex, "menuOpenCashTill_Click");
            }
            finally
            {
                StopWait();
            }
        }

        private void tpPaymentHolidays_Enter(object sender, System.EventArgs e)
        {
            try
            {
                Wait();

                LoadPaymentHolidays();
            }
            catch (Exception ex)
            {
                Catch(ex, "tpPaymentHolidays_Enter");
            }
            finally
            {
                StopWait();
            }
        }

        private void btnPaymentHoliday_Click(object sender, System.EventArgs e)
        {
            try
            {
                Wait();

                DataRowView selectedRow = ((DataView)dgAccountList.DataSource)[dgAccountList.CurrentRowIndex];
                string acctno = (string)selectedRow[CN.AccountNumber];
                int agrmtno = (int)selectedRow[CN.AgrmtNo];
                DateTime newdatefirst = Convert.ToDateTime(txtNewDueDate.Text);

                PaymentManager.SavePaymentHoliday(acctno, agrmtno, newdatefirst, DateTime.Now, out error);
                if (error.Length > 0)
                    ShowError(error);
                else
                {
                    btnClear_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Catch(ex, "btnPaymentHoliday_Click");
            }
            finally
            {
                StopWait();
            }
        }

        private void gbNewPayment_Enter(object sender, System.EventArgs e)
        {

        }

        private void menuPaymentCardDueDate_Click(object sender, System.EventArgs e)
        {
            try
            {
                Wait();
                // Early warning if receipt printer not available
                if (!this.SlipPrinterOK() || !this._slipRequired)
                {
                    StopWait();
                    return;
                }
                PrintPaymentCardDueDate(this.txtCustId.Text, this.txtSelectedAcctNo.Text);
            }
            catch (Exception ex)
            {
                Catch(ex, "menuPaymentCardDueDate_Click");
            }
            finally
            {
                StopWait();
            }
        }

        private void menuCheckExpiringWarranties_Click(object sender, System.EventArgs e)
        {
            GetCustomerWarrantyRenewals(false, true);
        }

        private void GetCustomerWarrantyRenewals(bool isCurrentlySettled, bool isMenuCall)
        {
            string custId = txtCustId.Text;
            //DataSet ds = AccountManager.GetWarrantyRenewals(this.txtSelectedAcctNo.Text, isCurrentlySettled, isMenuCall, ref custId, out error);
            DataSet ds =null;
            DataView warrantyrenewals = null;

            //check on web
            Client.Call(new GetWarrantyRenewalsRequest
            {
                AccountNumber=this.txtSelectedAcctNo.Text
            }, response =>
            {
                ds=response.WarrantyRenewal;
                if (ds.Tables.Count > 0)
                { 
                    warrantyrenewals = ds.Tables[0].DefaultView;
                    ShowRenewals(isMenuCall, warrantyrenewals);
                }

            }, this);

        }

        private void GetCustomerWarrantyRenewalsSettle(bool isCurrentlySettled, bool isMenuCall, short curPayMethod, decimal Payment, 
                                                        DataSet lastTransactionSet)
        {
            string custId = txtCustId.Text;
            //DataSet ds = AccountManager.GetWarrantyRenewals(this.txtSelectedAcctNo.Text, isCurrentlySettled, isMenuCall, ref custId, out error);
            DataSet ds = null;
            DataView warrantyrenewals = null;

            //check on web
            Client.Call(new GetWarrantyRenewalsRequest
            {
                AccountNumber = this.txtSelectedAcctNo.Text
            }, response =>
            {
                ds = response.WarrantyRenewal;
                if (ds.Tables.Count > 0)
                {
                    warrantyrenewals = ds.Tables[0].DefaultView;
                    ShowRenewals(isMenuCall, warrantyrenewals);
                }

                SlipPrinterReceipt(curPayMethod, Payment, lastTransactionSet);

            }, this);

        }

        private void ShowRenewals(bool isMenuCall, DataView warrantyrenewals)
        {
            if (warrantyrenewals.Count > 0)
            {
                if (menuWarrantRenewals.Enabled)
                {
                    //Launch a pop-up holding related items
                    WarrantyRenewals wr = new WarrantyRenewals(warrantyrenewals, txtCustId.Text, this.FormRoot, this);
                    wr.ShowDialog();		//launch as a modal dialog
                }
                else
                {
                    string msgTxt = "";
                    foreach (DataRowView addedItems in (DataView)warrantyrenewals)
                    {
                        msgTxt += (string)addedItems[CN.ItemNo] + " - ";                        //IP - 16/06/11 - CR1212 - RI - Changed back to ItemNo
                        msgTxt += (string)addedItems[CN.Description] + '\n';
                    }
                    ShowInfo("M_WARRANTYRENEWALPROMPT", new object[] { msgTxt }, MessageBoxButtons.OK);
                }
            }
            else if (isMenuCall)
            {
                ShowInfo("M_NOWARRANTYRENEWALPROMPT", MessageBoxButtons.OK);
            }
        }

        
        private void menuLaunchHelp_Click(object sender, System.EventArgs e)
        {
            Payment_HelpRequested(this, null);
        }

        private void Payment_HelpRequested(object sender, System.Windows.Forms.HelpEventArgs hlpevent)
        {
            string fileName = this.Name + ".htm";
            LaunchHelp(fileName);
        }

        //private void btnServiceSearch_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Function = "Payment Screen: Service Search";
        //        Wait();

        //        SERVICE.SR_ServiceSearch ss = new SERVICE.SR_ServiceSearch(this.FormRoot, this.FormParent);
        //        ss.FormRoot = this.FormRoot;
        //        ss.FormParent = this;
        //        ((MainForm)this.FormRoot).AddTabPage(ss, 7);
        //    }
        //    catch (Exception ex)
        //    {
        //        Catch(ex, Function);
        //    }
        //    finally
        //    {
        //        StopWait();
        //    }
        //}

        private void menuSES_Click(object sender, EventArgs e)
        {
            if (!txtCustId.Text.Trim().Equals(string.Empty))
                WarrantySESPopup.ShowSESPopUp(txtCustId.Text.Trim(), this, true);
        }

        //CR855
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Function = "button1_Click";
                Wait();

                CustomerPhotograph customerPhoto = new CustomerPhotograph(txtCustId.Text.Trim(), fileName, this.FormRoot, this);
                customerPhoto.ShowDialog();
            }
            catch (Exception ex)
            {
                Catch(ex, Function);
            }
            finally
            {
                StopWait();
                Function = "End of button1_Click";
            }
        }

        private void btnLaunchTally_Click(object sender, EventArgs e)
        {
            // check if tallyman instance already running, if so do not launch new instance
            bool tallyDetected = false;

            ShellWindows SW = new ShellWindowsClass();
            string Temp;

            foreach (InternetExplorer TempIE in SW)
            {
                Temp = Path.GetFileNameWithoutExtension(TempIE.FullName).ToLower();
                if (Temp.Equals("iexplore"))
                {

                    if (TempIE.LocationURL.IndexOf("srvmaltlm1:8080/collections") > -1)
                    {
                        tallyDetected = true;
                        break;
                    }
                }
            }
            if (!tallyDetected)
                System.Diagnostics.Process.Start("iexplore.exe", "http://srvmaltlm1:8080/collections");

        }

        //IP - 18/02/10 - CR1072 - UAT(77) - Payment Fixes from 4.3 - Merge
        private void txtCustId_KeyDown(object sender, KeyEventArgs e)
        {
            btnSearchAccount.Enabled = false;
        }

        //IP - 18/02/10 - CR1072 - UAT(77) - Payment Fixes from 4.3 - Merge
        private void txtAccountNo_KeyDown(object sender, KeyEventArgs e)
        {
            btnSearchAccount.Enabled = false;
        }

        // load Customer details  CR1084 jec
        private void btnCustomerDetails_Click(object sender, EventArgs e)
        {
            try
            {
                Wait();
                if (dgAccountList.VisibleRowCount != 0 && dgAccountList.DataSource != null)         //UAT107 jec 01/11/10
                {
                    BasicCustomerDetails details = null;
                    if (txtSelectedAcctNo.Text == string.Empty)// load up an account so customer details can be loaded ok. 
                        SelectAccount(0);

                    details = new BasicCustomerDetails(true, txtCustId.Text, txtSelectedAcctNo.Text, "H", "", FormRoot, this);

                    ((MainForm)this.FormRoot).AddTabPage(details, 10);
                    ((MainForm)details.FormRoot).statusBar1.Text = AdditionalNotes;
                    details.loaded = true;
                }
            }
            catch (Exception ex)
            {
                Catch(ex, "btnReferences_Click");
            }
            finally
            {
                StopWait();
            }

        }


        //IP - 25/01/11 - Sprint 5.9 - Method to clear payment fields when changing payment methods.
        private void clearPaymentFields()
        {
            //IP - 31/01/11 - Sprint 5.9 - #3050 - selected index should be set to 0 if datasource is not null
            if (drpCardType.DataSource != null)
            {
                drpCardType.SelectedIndex = 0;
            }

            mtb_CardNo.ResetText();

            //IP - 31/01/11 - Sprint 5.9 - #3050 - selected index should be set to 0 if datasource is not null
            if (drpBank.DataSource != null)
            {
                drpBank.SelectedIndex = 0;
            }

            txtBankAcctNo.Text = string.Empty;
            txtBankAcctNo.ReadOnly = false;
            txtBankAcctNo.BackColor = Color.White;
            txtCardNo.Text = string.Empty;
            lBankAcctNo.Text = "Bank Account No";
        }

        //IP - 26/01/11 - Sprint 5.9 - #2785
        private void mtb_Cardno_TextChanged(object sender, EventArgs e)
        {
            //Only retrieve the last 4 digits and set the txtCardNo as the contents of this control are used to save card/cheque details.
            txtCardNo.Text = mtb_CardNo.Text; //.Replace('-', ' ').Replace('X', ' ').Trim();
            //errorProviderStoreCard.Clear();       // #10126
            if (PayMethod.IsPayMethod(Convert.ToInt16(((DataRowView)drpPayMethod.SelectedItem)[CN.Code].ToString()), PayMethod.StoreCard))
            {
                txtFee.Text = string.Empty;
                if (mtb_CardNo.Text.Length == 16)
                {
                    ValidateStoreCard();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //ffgg 

            AuthoriseCheck Auth = new AuthoriseCheck("Payment", "mtb_Cardno");

            mtb_CardNo.Visible = true;

            if (!Auth.ControlPermissionCheck(Credential.User).HasValue)
            {
                //    tcMain.SelectedTab = tpEmployee;
                Auth.ShowDialog();
                if (Auth.IsAuthorised)
                {
                    mtb_CardNo.ReadOnly = false;
                    mtb_CardNo.Focus();
                }
                else
                    mtb_CardNo.ReadOnly = true;
            }
            else
            {
                mtb_CardNo.ReadOnly = false;
                mtb_CardNo.Focus();
            }
        }





    }
}
