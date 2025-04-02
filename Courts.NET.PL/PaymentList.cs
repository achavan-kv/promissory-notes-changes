using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using STL.Common.Static;
using STL.Common.Constants.ColumnNames;
using STL.Common.Constants.TableNames;
using Crownwood.Magic.Menus;
using STL.Common.Constants.EmployeeTypes;
using STL.Common.Constants.FTransaction;




namespace STL.PL
{
	/// <summary>
	/// Popup prompt used by the Payment screen to list the payments across
	/// all Ready Finance (RF) accounts for the same customer. The payment
	/// screen will automatically spread the total payment across the set of
	/// RF accounts in proportion to the relative size of the normal
	/// instalments on these accounts. This popup prompt allows the user to
	/// adjust the individual payment amounts. Any account that is allocated
	/// and in arrears will have the calculated fee amount displayed. When the
	/// payment amount is changed, then the fee is automatically re-calculated
	/// for that account. The user may also change the individual fee amounts.
	/// The total payment and fee amounts are returned to the Payment screen.
	/// </summary>
	public class PaymentList : CommonForm
	{
		// The values displayed and returned by this form must be rounded
		// to Country.DecimalPlaces. If they are not the calling form might
		// recalculate and overwrite all of the user entered values.
		// 
		private new string Error = "";
		private int _precision = 2;
		private bool validateColumn = true;

		private System.Windows.Forms.DataGrid dgPaymentList;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.Label lCustomerName;
		private System.Windows.Forms.TextBox txtCustId;
		private System.Windows.Forms.TextBox txtCustomerName;
		private System.Windows.Forms.Label lCustomerId;
		private System.Windows.Forms.Button btnOK;
		private System.ComponentModel.IContainer components;

		private bool _showFee = false;
		private DataSet OrigPaymentSet = null;
		public  DataSet PaymentSet = null;
		public  decimal totPayment = 0;
		private System.Windows.Forms.Label lAuthorise;
		public  decimal totFee = 0;
		public  int authorisedBy = 0;

		public PaymentList(Form root, Form parent)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			FormRoot = root;
			FormParent = parent;
		}

		public PaymentList(string piCustId, string piCustomerName, bool piShowFee, DataSet piPaymentSet, Form root, Form parent)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// After InitializeComponent call
			//
			FormRoot = root;
			FormParent = parent;

			if (IsNumeric(DecimalPlaces.Substring(1,1)))
			{
				this._precision = System.Convert.ToInt32(DecimalPlaces.Substring(1,1));
			}

			// Copy the data in case this popup is cancelled
			this.OrigPaymentSet = piPaymentSet.Copy();
			this.PaymentSet = piPaymentSet;
			this.txtCustId.Text = piCustId;
			this.txtCustomerName.Text = piCustomerName;
			this._showFee = piShowFee;
			this.LoadData();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PaymentList));
            this.dgPaymentList = new System.Windows.Forms.DataGrid();
            this.btnOK = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.lCustomerName = new System.Windows.Forms.Label();
            this.txtCustId = new System.Windows.Forms.TextBox();
            this.txtCustomerName = new System.Windows.Forms.TextBox();
            this.lCustomerId = new System.Windows.Forms.Label();
            this.lAuthorise = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgPaymentList)).BeginInit();
            this.SuspendLayout();
            // 
            // dgPaymentList
            // 
            this.dgPaymentList.CaptionText = "Payments List";
            this.dgPaymentList.DataMember = "";
            this.dgPaymentList.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgPaymentList.Location = new System.Drawing.Point(8, 40);
            this.dgPaymentList.Name = "dgPaymentList";
            this.dgPaymentList.Size = new System.Drawing.Size(760, 216);
            this.dgPaymentList.TabIndex = 0;
            this.dgPaymentList.TabStop = false;
            this.dgPaymentList.Enter += new System.EventHandler(this.dgPaymentList_Enter);
            this.dgPaymentList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dgPaymentList_MouseUp);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(264, 264);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(424, 264);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            // 
            // lCustomerName
            // 
            this.lCustomerName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lCustomerName.Location = new System.Drawing.Point(256, 8);
            this.lCustomerName.Name = "lCustomerName";
            this.lCustomerName.Size = new System.Drawing.Size(88, 16);
            this.lCustomerName.TabIndex = 10;
            this.lCustomerName.Text = "Customer Name";
            this.lCustomerName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtCustId
            // 
            this.txtCustId.BackColor = System.Drawing.SystemColors.Window;
            this.txtCustId.Location = new System.Drawing.Point(112, 8);
            this.txtCustId.MaxLength = 20;
            this.txtCustId.Name = "txtCustId";
            this.txtCustId.ReadOnly = true;
            this.txtCustId.Size = new System.Drawing.Size(120, 20);
            this.txtCustId.TabIndex = 9;
            // 
            // txtCustomerName
            // 
            this.txtCustomerName.BackColor = System.Drawing.SystemColors.Window;
            this.txtCustomerName.Location = new System.Drawing.Point(352, 8);
            this.txtCustomerName.MaxLength = 80;
            this.txtCustomerName.Name = "txtCustomerName";
            this.txtCustomerName.ReadOnly = true;
            this.txtCustomerName.Size = new System.Drawing.Size(312, 20);
            this.txtCustomerName.TabIndex = 7;
            this.txtCustomerName.TabStop = false;
            // 
            // lCustomerId
            // 
            this.lCustomerId.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lCustomerId.Location = new System.Drawing.Point(32, 8);
            this.lCustomerId.Name = "lCustomerId";
            this.lCustomerId.Size = new System.Drawing.Size(72, 16);
            this.lCustomerId.TabIndex = 8;
            this.lCustomerId.Text = "Customer ID";
            this.lCustomerId.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lAuthorise
            // 
            this.lAuthorise.Enabled = false;
            this.lAuthorise.Location = new System.Drawing.Point(704, 8);
            this.lAuthorise.Name = "lAuthorise";
            this.lAuthorise.Size = new System.Drawing.Size(16, 16);
            this.lAuthorise.TabIndex = 15;
            // 
            // PaymentList
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(778, 293);
            this.ControlBox = false;
            this.Controls.Add(this.lAuthorise);
            this.Controls.Add(this.lCustomerName);
            this.Controls.Add(this.txtCustId);
            this.Controls.Add(this.txtCustomerName);
            this.Controls.Add(this.lCustomerId);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dgPaymentList);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PaymentList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CoSACS Payment";
            ((System.ComponentModel.ISupportInitialize)(this.dgPaymentList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		// Local Procedures
		private void AddColumnStyle(string columnName, DataGridTableStyle tabStyle,
			int width, bool readOnly, string headerText, string format)
		{
			DataGridTextBoxColumn aColumnTextColumn = new DataGridTextBoxColumn();
			aColumnTextColumn.MappingName = columnName;
			aColumnTextColumn.Width = width;
			aColumnTextColumn.ReadOnly = readOnly;
			aColumnTextColumn.HeaderText = headerText;
			aColumnTextColumn.Format = format;
			tabStyle.GridColumnStyles.Add(aColumnTextColumn);
		}

		private void LoadData ()
		{
			try
			{
				Function = "Payment List Screen: Load Data";
				Wait();

				DataView PaymentListView = null;
				string statusText = "";
				int lockCount = 0;

				foreach (DataTable PaymentDetails in PaymentSet.Tables)
				{
					if (PaymentDetails.TableName == TN.Payments)
					{
						// Add a validation event
						//dgPaymentList.CurrentCellChanged += new EventHandler(dgPaymentList_CurCellChange);
						PaymentDetails.ColumnChanging += new DataColumnChangeEventHandler(this.PaymentDetails_ColumnChanging);
						
						//
						// Display the list of Customer Payments
						//

						statusText = PaymentDetails.Rows.Count + GetResource("M_ACCOUNTSLISTED");

						// Create a view for the DataGrid
						PaymentListView = new DataView(PaymentDetails);
						PaymentListView.AllowNew = false;
						PaymentListView.Sort = CN.AccountNo + " ASC ";
						dgPaymentList.CausesValidation = false;
						dgPaymentList.DataSource = PaymentListView;

						if (dgPaymentList.TableStyles.Count == 0)
						{
							// Create the table style for the DataGrid
							DataGridTableStyle tabStyle = new DataGridTableStyle();
							tabStyle.MappingName = PaymentListView.Table.TableName;

							// Add an icon column if any accounts are locked
							foreach (DataRow accountRow in PaymentDetails.Rows)
							{
								// Check for accounts already locked
								if (accountRow[CN.LockedBy].ToString().Length > 0)
									lockCount++;
							}

							if (lockCount > 0)
							{
								if (!PaymentDetails.Columns.Contains("Icon"))
								{
									// Add an icon column to mark locked accounts
									PaymentDetails.Columns.Add("Icon");
								}

								// Add an unbound stand-alone icon column
								DataGridIconColumn iconColumn = new DataGridIconColumn(imageList1.Images[0], CN.LockedBy, "");
								iconColumn.MappingName = "Icon";
								iconColumn.HeaderText = "";
								iconColumn.Width = imageList1.Images[0].Size.Width;
								tabStyle.GridColumnStyles.Add(iconColumn);
							}

							//
							// Style each column that needs to be displayed
							//

							// Normal columns
							AddColumnStyle(CN.AccountNo,			tabStyle, 100, true,  GetResource("T_ACCOUNTNO"),	"");
							AddColumnStyle(CN.OutstandingBalance,	tabStyle,  90, true,  GetResource("T_OUTBAL"),		DecimalPlaces);
							AddColumnStyle(CN.Arrears,				tabStyle,  90, true,  GetResource("T_ARREARS"),		DecimalPlaces);
							AddColumnStyle(CN.SettlementFigure,		tabStyle,  90, true,  GetResource("T_SETTLEMENT"),	DecimalPlaces);
							AddColumnStyle(CN.InstalAmount,			tabStyle,  90, true,  GetResource("T_INSTALLMENT"),	DecimalPlaces);
							AddColumnStyle(CN.Payment,              tabStyle,  90, false, GetResource("T_PAYMENT"),     DecimalPlaces);

							if (_showFee)
							{

								// The Fee column needs different readonly properties on different rows
								DataGridEditColumn aColumnEditColumn;
								aColumnEditColumn = new DataGridEditColumn(CN.ReadOnly, "Y");
								aColumnEditColumn.MappingName = CN.CollectionFee;
								aColumnEditColumn.HeaderText = GetResource("T_FEE");
								aColumnEditColumn.Width = 90;
								aColumnEditColumn.ReadOnly = false;
								aColumnEditColumn.Format = DecimalPlaces;
								tabStyle.GridColumnStyles.Add(aColumnEditColumn);

								// Display the Net Payment when displaying the Fee
								AddColumnStyle(CN.NetPayment,		tabStyle,  90, true,  GetResource("T_NETPAYMENT"),	DecimalPlaces);
							}

							dgPaymentList.TableStyles.Clear();
							dgPaymentList.TableStyles.Add(tabStyle);
							dgPaymentList.DataSource = PaymentListView;

						}
					}
				}
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				StopWait();
			}
		}

		private bool ValidMoneyField(string moneyField, out decimal moneyValue)
		{
			// Check a blank or zero money value entered
			moneyValue = 0.0M;
			moneyField = moneyField.Trim();
			if (!IsStrictMoney(moneyField))
			{
				ShowInfo("M_NUMERIC");
				return false;
			}

			// Reformat
			moneyValue = MoneyStrToDecimal(moneyField);
			moneyField = moneyValue.ToString(DecimalPlaces);

			return true;
		}  // End of ValidMoneyField


		// Form Events
		private void dgPaymentList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			try
			{
				Function = "Payment List Screen: Click on Account List";
				Wait();

				int index = dgPaymentList.CurrentRowIndex;
				if (index >= 0)
				{
					// Check this account is not locked
					DataView PaymentList = (DataView)dgPaymentList.DataSource;
					DataRow  newPayment  = PaymentList[index].Row;
					if (newPayment[CN.LockedBy].ToString().Length > 0)
					{
						this.txtCustId.Focus();
						dgPaymentList.UnSelect(index);
						ShowInfo("M_ACCOUNTLOCKED", new Object[]{newPayment[CN.AccountNo].ToString(), this.txtCustId.Text});
					}
				}
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				StopWait();
			}
		}

		private void dgPaymentList_Enter(object sender, System.EventArgs e)
		{
			try
			{
				Function = "Payment List Screen: Tab into Account List";
				Wait();

				int index = dgPaymentList.CurrentRowIndex;
				if (index >= 0)
				{
					// Check this account is not locked
					DataView PaymentList = (DataView)dgPaymentList.DataSource;
					DataRow  newPayment;
					bool rowLocked = true;

					// Skip rows that are locked
					dgPaymentList.UnSelect(index);
					while (rowLocked && index < PaymentList.Count)
					{
						newPayment = PaymentList[index].Row;
						rowLocked = newPayment[CN.LockedBy].ToString().Length > 0;
						index++;
					}

					if (rowLocked)
					{
						// All remaining rows were also locked so skip to the CustId
						this.txtCustId.Focus();
					}
					else
					{
						// Select the next row not locked
						dgPaymentList.Select(index-1);
					}
				}
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				StopWait();
			}

		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			try
			{
				Function = "Payment List Screen: OK button";
				Wait();

				// Validate the Fees and Payments
				this.totFee = 0;
				this.totPayment = 0;
				foreach (DataTable PaymentDetails in PaymentSet.Tables)
				{
					if (PaymentDetails.TableName == TN.Payments)
					{
						foreach (DataRow paymentRow in PaymentDetails.Rows)
						{
							if (   (decimal)paymentRow[CN.CollectionFee] < 0
								|| (decimal)paymentRow[CN.CollectionFee] > (decimal)paymentRow[CN.Payment]
								|| (decimal)paymentRow[CN.Payment] < 0)
								// DSR 13/9/04 - users can now overpay accounts putting them into credit
								// || ((decimal)paymentRow[CN.Payment] > (decimal)paymentRow[CN.SettlementFigure] && (decimal)paymentRow[CN.SettlementFigure] > 0))
							{
								ShowInfo("M_VALIDPAYMENT");
								return;
							}

							// Sum the Fees and Payments
							totFee = totFee + (decimal)paymentRow[CN.CollectionFee];
							totPayment = totPayment + (decimal)paymentRow[CN.Payment];
						}
					}
				}
						
                // cancel on pop up will fire twice next time we enter this screen
                // because we are going to add this event handler anyway so best get
                // rid of the old hook up here
                this.PaymentSet.Tables[TN.Payments].ColumnChanging -= new DataColumnChangeEventHandler(this.PaymentDetails_ColumnChanging);
					
				Close();
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				StopWait();
			}
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			try
			{
				Function = "Payment List Screen: Cancel button";
				Wait();

				// Changes are lost
				this.PaymentSet = this.OrigPaymentSet;

				// Sum the Fees and Payments
				this.totFee = 0;
				this.totPayment = 0;
				foreach (DataTable PaymentDetails in PaymentSet.Tables)
				{
					if (PaymentDetails.TableName == TN.Payments)
					{
						foreach (DataRow paymentRow in PaymentDetails.Rows)
						{
							totFee = totFee + (decimal)paymentRow[CN.CollectionFee];
							totPayment = totPayment + (decimal)paymentRow[CN.Payment];
						}
					}
				}

				Close();
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				StopWait();
			}
		}


		// Validate a DataGrid cell
		/*
		protected void dgPaymentList_CurCellChange(object sender, EventArgs e)

		{
			// Get the co-ordinates of the focused cell
			int gridColumn = dgPaymentList.CurrentCell.ColumnNumber;
			int gridRow    = dgPaymentList.CurrentCell.RowNumber;
			DataView PaymentList = (DataView)dgPaymentList.DataSource;
			DataRow  curPayment	 = PaymentList[gridRow].Row;

			if (gridColumn == 10)
			{
				// Validate Fee between zero and Payment
				if ((decimal)curPayment[CN.CollectionFee] < 0
					|| (decimal)curPayment[CN.CollectionFee] > (decimal)curPayment[CN.Payment])
				{
					ShowInfo("M_VALIDFEE");
					dgPaymentList.CurrentCell = new DataGridCell(gridRow, 10);
					return;
				}

				// Recalculate the Net Payment
				curPayment[CN.NetPayment] = (decimal)curPayment[CN.Payment] - (decimal)curPayment[CN.CollectionFee];
			}

			if (gridColumn == 11)
			{
				// Validate Payment between zero and Settlement
				if ((decimal)curPayment[CN.Payment] < 0
					|| (decimal)curPayment[CN.Payment] > (decimal)curPayment[CN.SettlementFigure]))
				{
					ShowInfo("M_VALIDFEE");
					dgPaymentList.CurrentCell = new DataGridCell(gridRow, 11);
					return;
				}

				// TODO: Recalculate the Fee

				// Recalculate the Net Payment
				curPayment[CN.NetPayment] = (decimal)curPayment[CN.Payment] - (decimal)curPayment[CN.CollectionFee];
			}
		}
		*/

		
		protected void PaymentDetails_ColumnChanging(object sender, System.Data.DataColumnChangeEventArgs e)
		{
			// Validate the Fee and Payment amounts
			bool status = true;
			try
			{
				Function = "Payment List Screen: Validate Fee and Payment Amounts";

				if (validateColumn)
				{
					Wait();
					// Don't validate columns with this event that were changed by this event
					validateColumn = false;

					decimal newAmount = 0.0M;
					if (!this.ValidMoneyField(e.ProposedValue.ToString(), out newAmount))
					{
						dgPaymentList.CurrentCell = new DataGridCell(dgPaymentList.CurrentCell.RowNumber, dgPaymentList.CurrentCell.ColumnNumber);
						return;
					}

					// Write back the rounded value
					e.ProposedValue = newAmount;

					// Get the current row
					DataRow  curPayment	 = e.Row;

					if (e.Column.ColumnName == CN.CollectionFee && curPayment[CN.ReadOnly].ToString() == "N")
					{
						// Validate Fee between zero and Payment
						if (newAmount < 0 || newAmount > (decimal)curPayment[CN.Payment])
						{
							ShowInfo("M_VALIDFEE");
							e.ProposedValue = (decimal)curPayment[CN.CollectionFee];
							dgPaymentList.CurrentCell = new DataGridCell(dgPaymentList.CurrentCell.RowNumber, 5);
							return;
						}

						AuthorisePrompt ap = new AuthorisePrompt(this, lAuthorise, GetResource("M_CREDITFEE"));
						ap.ShowDialog();
					
						if (ap.Authorised)
						{
							this.authorisedBy = ap.AuthorisedBy;
							status = true;
						}
						else
						{
							e.ProposedValue = (decimal)curPayment[CN.CollectionFee];
							dgPaymentList.CurrentCell = new DataGridCell(dgPaymentList.CurrentCell.RowNumber, 5);
							status = false;
						}

						// Recalculate the Net Payment
						if(status)
							curPayment[CN.NetPayment] = (decimal)curPayment[CN.Payment] - newAmount;
					}

					if (e.Column.ColumnName == CN.Payment)
					{
						// Validate Payment between zero and Settlement
						if (newAmount < 0)
							// DSR 13/9/04 - users can now overpay accounts putting them into credit
							//|| (newAmount > (decimal)curPayment[CN.SettlementFigure] && (decimal)curPayment[CN.SettlementFigure] > 0))
						{
							ShowInfo("M_VALIDPAYMENT");
							e.ProposedValue = (decimal)curPayment[CN.Payment];
							dgPaymentList.CurrentCell = new DataGridCell(dgPaymentList.CurrentCell.RowNumber, 4);
							return;
						}

						decimal payAmount = newAmount;
						decimal newFee = 0.0M;
						decimal bailiffFee = 0.0M;

						if (payAmount >= 0.01M
							&& (decimal)curPayment[CN.Arrears] >= 0.01M
							&& IsStrictNumeric(curPayment[CN.EmployeeNo].ToString())
							&& curPayment[CN.EmployeeNo].ToString().Length > 0)
						{
							// Load the new Fee amount for the Payment Amount
							// Note the fee is only calculated when there is an allocated employee and
							// the employee number and arrears fields therefore contain non-blank
							// numeric values. Employee No is zero for Tallyman.
							int debitAccount = 0;
							int segmentId = 0;
							int empeeNo = Convert.ToInt32(curPayment[CN.EmployeeNo].ToString());

							PaymentManager.CalculateCreditFee(
								curPayment[CN.AccountNo].ToString(),            // Acct No
								Config.CountryCode,								// Country Code
								TransType.Payment,								// Payment Type
								ref empeeNo,									// Allocated Courts Person
								(decimal)curPayment[CN.Arrears],				// Arrears
                                false,                                          // reverse calc #13746
								ref payAmount,									// Payment Amount
								out newFee,
								out bailiffFee,
								out debitAccount,
								out segmentId,
								out Error);

							if (Error.Length > 0)
							{
								ShowError(Error);
								newFee = 0;
							}
						}
						curPayment[CN.CollectionFee] = Math.Round(newFee, this._precision);
						curPayment[CN.CalculatedFee] = Math.Round(newFee, this._precision);
						curPayment[CN.BailiffFee] = Math.Round(bailiffFee, this._precision);

						// Recalculate the Net Payment
						curPayment[CN.NetPayment] = newAmount - newFee;
					}
				}
			}
			catch(Exception ex)
			{
				Catch(ex, Function);
			}
			finally
			{
				validateColumn = true;
				StopWait();
			}
				
		}
		
	}
}
