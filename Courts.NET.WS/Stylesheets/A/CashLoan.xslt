<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <meta http-equiv="X-UA-Compatible" content="IE=edge" />
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <style type="text/css">
          body, table {font-family:Helvetica, Arial, sans-serif; font-size:12px; }
          .header { text-align: center; font-weight: bold; }
          .section-title { font-weight: bold; margin-top: 20px; }
          .page-break { page-break-after: always; }
          table { border-collapse: collapse; }
          td, th { padding: 4px; border: 1px solid #000; }
          ol {
          counter-reset: item;
          }

          ol::after {
          content: "";
          display: block;
          height: 1em; /* This creates the extra blank line; adjust height as needed */
          }

          li {
          list-style-type: none; /* Hide default bullets/numbers */
          }

          li::before {
          content: counters(item, ".") " ";
          counter-increment: item;
          font-weight: bold;
          margin-right: 0.5em; /* some spacing after the number */
          }

          li::after {
          content: "";
          display: block;
          height: 1em; /* This creates the extra blank line; adjust height as needed */
          }

          .section-title {
          font-weight: bold; /* or any style you prefer for headings */
          }


        </style>
      </head>
      <body>
        <xsl:apply-templates select="AGREEMENTS" />
      </body>
    </html>
  </xsl:template>


  <xsl:template match="AGREEMENTS">
    <xsl:apply-templates select="AGREEMENT" />
    <P></P>
  </xsl:template>

  <xsl:template match="AGREEMENT">
    <xsl:apply-templates select="PAGE" />
    <P></P>
  </xsl:template>

  <xsl:template match="PAGE">
    <div style="position:relative">
      <!-- Header -->
      <p class="header" style="font-family:Helvetica, font-size:16px;">UNICOMER (ANTIGUA AND BARBUDA) LIMITED </p>
      <p class="header" style="font-size:11px; text-align: right;">
        ACCOUNT NUMBER <xsl:value-of select="HEADER/ACCTNO" />
      </p>
      <p class="header" style="font-size:11px; font-weight:bold;">LOAN AGREEMENT</p>

      <xsl:variable name="date" select="../FOOTER/LOANAGREEMENTDATE"/>

      <!-- Extract day, month, year using substring -->
      <xsl:variable name="day" select="substring($date, 1, 2)" />
      <xsl:variable name="monthNumber" select="substring($date, 4, 2)" />
      <xsl:variable name="year" select="substring($date, 7, 4)" />

      <!-- Convert day to ordinal -->
      <xsl:variable name="dayOrdinal">
        <xsl:choose>
          <xsl:when test="$day = '01' or $day = '21' or $day = '31'">
            <xsl:value-of select="number($day)" />st
          </xsl:when>
          <xsl:when test="$day = '02' or $day = '22'">
            <xsl:value-of select="number($day)" />nd
          </xsl:when>
          <xsl:when test="$day = '03' or $day = '23'">
            <xsl:value-of select="number($day)" />rd
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="number($day)" />th
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <!-- Convert numeric month to month name -->
      <xsl:variable name="monthName">
        <xsl:choose>
          <xsl:when test="$monthNumber = '01'">January</xsl:when>
          <xsl:when test="$monthNumber = '02'">February</xsl:when>
          <xsl:when test="$monthNumber = '03'">March</xsl:when>
          <xsl:when test="$monthNumber = '04'">April</xsl:when>
          <xsl:when test="$monthNumber = '05'">May</xsl:when>
          <xsl:when test="$monthNumber = '06'">June</xsl:when>
          <xsl:when test="$monthNumber = '07'">July</xsl:when>
          <xsl:when test="$monthNumber = '08'">August</xsl:when>
          <xsl:when test="$monthNumber = '09'">September</xsl:when>
          <xsl:when test="$monthNumber = '10'">October</xsl:when>
          <xsl:when test="$monthNumber = '11'">November</xsl:when>
          <xsl:when test="$monthNumber = '12'">December</xsl:when>
        </xsl:choose>
      </xsl:variable>

      <!-- Agreement Intro -->
      <p>
        THIS LOAN AGREEMENT is made the <xsl:value-of select="$dayOrdinal" /> day of <xsl:value-of select="$monthName" /> <xsl:text> </xsl:text> <xsl:value-of select="$year" />.
      </p>
      <p>
        BETWEEN UNICOMER (ANTIGUA AND BARBUDA) LIMITED, trading as Courts Ready Cash, a limited liability company and licensed money services business whose registered office is All Saints Road & Vivian Richards Street, St John’s (hereinafter called "the Lender”), AND <xsl:value-of select="HEADER/NAME" /> <xsl:choose>
          <xsl:when test="../FOOTER/ISCOMPANY='1'">a limited liability company </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="../FOOTER/OCCUPATION"/> who identified himself/herself by presenting his/her <xsl:value-of select="../FOOTER/DOCUMENTTYPE"/> bearing the Number <xsl:value-of select="../FOOTER/DOCUMENTNUMBER"/> etc.
          </xsl:otherwise>
        </xsl:choose> <xsl:choose>
          <xsl:when test="../FOOTER/ISCOMPANY='1'">whose registered address </xsl:when>
          <xsl:otherwise> whose address </xsl:otherwise>
        </xsl:choose> is <xsl:value-of select="HEADER/ADDR1" /><xsl:text> </xsl:text> <xsl:value-of select="HEADER/ADDR2" /> <xsl:text> </xsl:text> <xsl:value-of select="HEADER/ADDR3" />(hereinafter jointly and severally called “the Borrower(s)”).
      </p>

      <!-- WHEREAS Clause -->
      <p class="section-title">WHEREAS:</p>
      <p>
        The Lender has agreed to lend, and the Borrower(s) has agreed to borrow, a loan of <xsl:value-of select="HEADER/LOANAMOUNTWORDS"/> (EC$<xsl:value-of select="HEADER/LOANAMOUNT"/>) on the following terms and conditions.
      </p>
      <p class="section-title">NOW THEREFORE this Agreement witnesses as follows:</p>

      <!-- Loan Section -->
      <ol>
        <li>
          <span class="section-title">The Loan</span>
          <ol>
            <li>
              Subject to the terms and conditions hereinafter contained the Lender agrees to lend to the Borrower(s) the sum of <xsl:value-of select="HEADER/LOANAMOUNTWORDS"/> (EC$<xsl:value-of select="HEADER/LOANAMOUNT"/>) (hereinafter called “the Loan”).
            </li>
            <li>
              For cash value received of the Loan, the Borrower(s) jointly and severally agree(s) to repay the Loan including interest as provided in <b>Clause 2</b> below and in the manner outlined in <b>Schedule I</b> attached hereto, together with the fees stated herein.
            </li>
            <li>
              The attached <b>Schedule II – Fact Sheet</b> summarises the key terms of this Loan Agreement.
            </li>
          </ol>
        </li>

        <!-- Interest Section -->
        <li>
          <span class="section-title">Interest</span>
          <ol>
            <li>
              The Borrower(s) shall pay to the Lender interest on the principal sum of the Loan from the date of disbursement at the rate of <b>
                <xsl:value-of select="../FOOTER/INTERESTRATEWORD"/>) per cent (<xsl:value-of select="../FOOTER/INTERESTRATE"/>)
              </b> per annum. The effective annual interest rate for the Loan is <xsl:value-of select="../FOOTER/INTERESTRATE"/>.
            </li>
            <li>
              All interest hereunder shall be calculated on an add-on basis at the applicable rate as aforesaid and on the basis of a year of 365 days.
            </li>
          </ol>
        </li>

        <!-- Security Section -->
        <li>
          <span class="section-title">Security</span>
          <ol>
            <li>
              The Loan will be unsecured, but the Lender shall have the right to call for a Promissory Note to evidence the sum outstanding at any time.
            </li>
            <li>
              The Borrower(s) shall be liable to pay the stamping and any other cost associated with perfecting the Promissory Note.
            </li>
          </ol>
        </li>

        <!-- Repayment Section -->
        <li>
          <span class="section-title">Repayment</span>
          <ol>
            <li>
              The Loan principal shall be repaid together with any accrued and unpaid interest and fees due thereon by <xsl:value-of select="../FOOTER/INSTALNOWORD"/> (<xsl:value-of select="../FOOTER/INSTALNO"/>) equal monthly installments of <xsl:value-of select="../FOOTER/FIRSTINSTWORD" /> (EC$<xsl:value-of select="../FOOTER/FIRSTINST" />) which shall include the annual interest of <xsl:value-of select="../FOOTER/INTERESTRATE"/>. Each installment shall be due and payable on the same date in each month as the date on which the Loan was disbursed (the “Disbursement Date”) commencing the next calendar month after the said Disbursement Date. See Schedule I – Schedule of Payments attached hereto.
            </li>
            <li>
              All installments will be made to the address of the Lender or such place as the Lender may specify until the Loan together with interest and related fees are paid in full.
            </li>
          </ol>
        </li>

        <!-- Prepayment Section -->
        <li>
          <span class="section-title">Prepayment</span>
          <ol>
            <li>
              The Borrower(s) may, without penalty, make early payments, pay more than the amount of any scheduled installment, or repay the entire loan (including interest and fees) before the final payment is due.
            </li>
            <li>
              For the avoidance of doubt, since interest will be calculated on an add-on basis, if the entire Loan (including interest and fees) is paid before the final payment is due, the Borrower(s) shall not be entitled to any reimbursement from the Lender.
            </li>
          </ol>
        </li>

        <!-- Late Interest & Fee Section -->
        <li>
          <span class="section-title">Late Interest &amp; Fee</span>
          <ol>
            <li>
              If an installment that is due is unpaid the Borrower(s) shall be liable to pay late interest at the applicable daily rate of <xsl:value-of select="../FOOTER/DAILYLATEINTEREST" />% of the outstanding amount, until settlement of said outstanding amount in full both before and after Judgment if applicable.
            </li>
            <li>
              In addition to the late interest payable above, if an installment that is due is unpaid the Borrower(s) shall be liable to pay a late fee which will amount to <xsl:value-of select="../FOOTER/LATEFEE" />% of the total arrears of both principal and interest both before and after Judgment if applicable.
            </li>
          </ol>
        </li>

        <!-- Default & Collection Section -->
        <li>
          <span class="section-title">Default &amp; Collection</span>
          <ol>
            <li>
              If payments are not made on or before their due dates and the Loan falls into default the Borrower may be required to pay the entire unpaid principal balance and any accrued interest and fees at once. Notice to pay all the unpaid balance, interest and fees shall be forwarded to the Borrower(s)’ within ten (10) days of the default and in accordance with clause 14 below.
            </li>
            <li>
              If the Loan, any interest, and fees are referred to an attorney or collection agency or agent for collection due to the Borrower(s)’ default, the Borrower shall be liable to pay all attorney fees, court costs, or any fees due to said agency/ agent.
            </li>
          </ol>
        </li>

        <!-- Administrative/Processing Fee Section -->
        <li>
          <span class="section-title">Administrative/ Processing Fee</span>
        </li>
        <p>
          In consideration of the Lender’s agreement to grant the Loan, the Borrower shall pay to the Lender a non‑refundable Administrative/ Processing Fee in the amount <xsl:value-of select="../FOOTER/PROCESSINGFEEWORD" />(EC$<xsl:value-of select="../FOOTER/PROCESSINGFEE" />), which shall be deducted from the proceeds of the Loan.
        </p>

        <!-- Treatment of Personal Data Section -->
        <li>
          <span class="section-title">Treatment of Personal Data</span>
          <ol>
            <li>
              I/WE hereby provide my/our consent for the Lender to share my/our personal data with its affiliates or affiliate (that is, any person, firm, corporation, association, organization, or unincorporated trade or business that, now or hereafter, directly or indirectly, controls, is controlled by, or is under common control with the Lender including without limitation, any subsidiary of the Lender), business partners and credit bureaus/institutions for Know‑Your‑Customer purposes, which due diligence data is required for any contract(s) and/or financial or other business dealings or arrangements which the Lender may have with an affiliate.
              <p>
                Yes No
              </p>
            </li>
            <li>
              I/WE hereby provide my/our consent for the Lender to share my/our contact information with affiliates of the Lender for the purpose of promoting or directly marketing products and/or services of the Lender and/or any of its affiliates to me/us and the Lender and/or any of its affiliates may communicate directly with me/us through various channels (whether electronic or otherwise) using this contact information.
              <p>
                Yes No
              </p>
            </li>
          </ol>
        </li>

        <!-- Insurance Section -->
        <li>
          <span class="section-title">Insurance</span>
        </li>
        <p>
          The Borrower(s) hereby agrees to purchase insurance with <xsl:value-of select="../FOOTER/INSURANCECOMPANYNAME"/> (“the insurer”) under Policy No. <xsl:value-of select="../FOOTER/POLICYNUMBER"/> (“the Policy”). The premium payable under the Policy is <xsl:value-of select="../FOOTER/POLICYPREMINUMPERCENTAGE"/>% of the principal sum of the Loan plus VAT and shall be deducted from the proceeds of the Loan. Under the Policy the Borrower(s)’ obligations to repay the Loan shall be insured and in the case of a successful claim all the Borrower(s)’ outstanding payments shall be paid by the insurer to the Lender. Claim settlement is subject to the terms and conditions of the Policy, full details of which are contained in the Payment Protection Plan documents issued to the Borrower(s).
        </p>

        <!-- Stamping Fees Section -->
        <li>
          <span class="section-title">Stamping Fees</span>
        </li>
        <p>
          In addition to the provisions made in Clause 3 hereof, the Borrower(s) shall be liable to pay the stamping and any other cost associated with perfecting this Loan Agreement, which shall be deducted from the proceeds of the Loan.
        </p>

        <!-- Assignment/Transfer Section -->
        <li>
          <span class="section-title">Assignment/ Transfer</span>
          <ol>
            <li>
              The Borrower(s) may not assign or transfer any of its rights or obligations under this Agreement.
            </li>
            <li>
              The Lender may at any time assign, sell, or otherwise transfer all or any of its rights and obligations under this Agreement or other related document and the Borrower(s) shall enter into all documents specified by the Lender to be necessary to give effect to any such assignment or transfer.
            </li>
          </ol>
        </li>

        <!-- Electronic Form and Signatures Section -->
        <li>
          <span class="section-title">Electronic Form and Signatures</span>
        </li>
        <p>
          The parties agree that this Agreement and any other ancillary documents to be delivered in connection herewith may be electronically signed, and any electronic signatures appearing on this Agreement, or such other ancillary documents, are the same as handwritten signatures for the purposes of validity, enforceability, and admissibility.
        </p>

        <!-- Right to Set Off Section -->
        <span class="section-title">Right to Set Off</span>
        </li>
        <p>
          If the Borrower(s) fails to make any payment or payments due and owing under this Agreement, the Lender and each of its affiliates is authorized at any time and from time to time, to the fullest extent permitted by law, to set off and apply any and all amounts due from the Lender or any of its affiliates to the Borrower's credit or account against the said payment or payments, irrespective of whether the Lender has made demand under this Agreement. The Lender's rights hereunder are in addition to other rights and remedies which the Lender may have.
        </p>

        <!-- Notices Section -->
        <li>
          <span class="section-title">Notices</span>
        </li>
        <p>
          Any Notice required by this agreement may be delivered to the Borrower personally or by post or electronic mail or by text message, without proof of receipt provided that the method of delivery is based on information on file for the Borrower.
        </p>

        <!-- Miscellaneous Section -->
        <li>
          <span class="section-title">Miscellaneous</span>
          <ol>
            <li>
              This Agreement shall be governed by and construed in accordance with the laws of Antigua and Barbuda.
            </li>
            <li>
              If any provision of this Agreement is found to be illegal or invalid, such provision shall be deemed to be severed and deleted and the remaining provisions shall be valid and enforceable.
            </li>
            <li>
              This Agreement constitutes the entire agreement between the parties with respect to the subject matter hereof and supersedes all other prior agreements and understandings, both written and oral, between the parties with respect to the subject matter hereof.
            </li>
            <li>
              Notice shall be mailed by registered post to the last known address on record of the principal Borrower.
            </li>
          </ol>
        </li>
      </ol>

      <!-- Blank Page Indicator -->

      <p style="margin-top:20px; text-align:center;">[INTENTIONALLY LEFT BLANK]</p>
      <div class="page-break"></div>

      <!-- Execution Section -->
      <p class="section-title" style="text-align:center;">IN WITNESS WHEREOF the parties have executed this Agreement as of the date first hereinbefore written.</p>
      <p>
        EXECUTED by or on behalf of the Borrower(s)<br/>
        <xsl:value-of select="HEADER/NAME" /><br/>
        ____________________________________<br/>
        Witness
      </p>
      <p>
        EXECUTED on behalf of the Lender 	)<br/>
        UNICOMER 							)<br/>
        (ANTIGUA AND BARBUDA) 				) __________________________________<br/>
        LIMITED								)<br/>
        by									)<br/>
        its Authorised Representative		)<br/>
        )<br/>
        <br/>
        ____________________________________<br/>
        Witness
      </p>

      <!-- SCHEDULE I -->
      <p class="section-title" style="text-align:center;">SCHEDULE I</p>
      <p class="section-title" style="text-align:left;">LOAN SUMMARY</p>
      <table width="100%">
        <tr>
          <td>
            <strong>Loan Amount</strong>
          </td>
          <td>
            <xsl:value-of select="HEADER/LOANAMOUNT"/>
          </td>
        </tr>
        <tr>
          <td>
            <strong>Annual Interest Rate</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/INTERESTRATE"/>
          </td>
        </tr>
        <tr>
          <td>
            <strong>Loan Period</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/INSTALNO" /> months
          </td>
        </tr>
        <tr>
          <td>
            <strong>Start Date of Loan</strong>
          </td>
          <td>
            <xsl:value-of select="(../FOOTER/INSTALTABLE/INSTALMENTDATE)[1]" />
          </td>
        </tr>
        <tr>
          <td>
            <strong>Monthly Payment</strong>
          </td>
          <td>
            $<xsl:value-of select="../FOOTER/FIRSTINST" />
          </td>
        </tr>
        <tr>
          <td>
            <strong>No. of Payments</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/INSTALNO" />
          </td>
        </tr>
        <tr>
          <td>
            <strong>Total Interest</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/TOTALINTEREST" />%
          </td>
        </tr>
        <tr>
          <td>
            <strong>Total Cost of Loan</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/TOTAL" />
          </td>
        </tr>
        <tr>
          <td>
            <strong>Total Cost of Loan (%)</strong>
          </td>
          <td>
            <xsl:value-of select="../FOOTER/TOTALCOSTOFLOAN" />%
          </td>
        </tr>
      </table>

      <!-- SCHEDULE II -->
      <p class="section-title" style="text-align:center;">SCHEDULE OF PAYMENTS</p>
      <xsl:choose>
        <xsl:when test="../FOOTER/COUNTRYCODE='Z'">

          <table width="100%">
            <tr>
              <th>Pmt. No.</th>
              <th>Payment Date</th>
              <th>Beginning Balance</th>
              <th>Amortized Scheduled Payment</th>
              <th>Principal</th>
              <th>Interest</th>
              <th>Other Charges/ Fees</th>
              <th>Ending Balance</th>
              <th>Cumulative Interest</th>
            </tr>
            <tr>
            </tr>
          </table>

        </xsl:when>
        <xsl:otherwise>

          <table border="1" style="border-collapse:collapse;" width="100%">
            <tr valign="top">
              <td width="50%">
                Installment Amount<br/>
              </td>
              <td width="50%">
                Date<br/>
              </td>
            </tr>
            <br/>
            <tr>
              <xsl:for-each select="../FOOTER/INSTALTABLE">
                <tr>
                  <td>
                    <xsl:value-of select="INSTALMENTAMOUNT"/>
                  </td>
                  <td>
                    <xsl:value-of select="INSTALMENTDATE"/>
                  </td>
                </tr>
              </xsl:for-each>
            </tr>
          </table>

        </xsl:otherwise>
      </xsl:choose>
      <!-- SCHEDULE III -->
      <p class="section-title" style="text-align:center;">SCHEDULE II – FACT SHEET: KEY TERMS OF THE LOAN AGREEMENT</p>
      <table width="100%">
        <tr>
          <th>Clause</th>
          <th>Description</th>
          <th>Terms</th>
        </tr>
        <tr>
          <td>1</td>
          <td>Loan Amount</td>
          <td>
            EC$<xsl:value-of select="HEADER/LOANAMOUNT"/>
          </td>
        </tr>
        <tr>
          <td>2</td>
          <td>Annual Interest Rate</td>
          <td>
            <xsl:value-of select="../FOOTER/INTERESTRATE"/><br/>
            All interest shall be calculated on the reducing balance at the applicable rate for each interest period based on the actual days elapsed and a year of 365 days.
          </td>
        </tr>
        <tr>
          <td>3</td>
          <td>Security</td>
          <td>
            3.1. The Loan will be unsecured, but the Lender shall have the right to call for a Promissory Note to evidence the sum outstanding at any time.
            <P style="text-align:center;">
              OR</p>
              The Borrower(s)’ obligation to repay the loan and any accrued interest shall be secured by [description of security documents] over [description of collateral].

              3.2. The Borrower(s) shall be liable to pay the stamping and any other cost associated with perfecting the Promissory Note.
              <P style="text-align:center;">
                OR</p>
                The Borrower(s) shall be liable to pay the stamping and any other cost associated with perfecting [description of security documents].

              </td>
        </tr>
        <tr>
          <td>4</td>
          <td>Repayment</td>
          <td>
            <p>
              Repayment period – <xsl:value-of select="../FOOTER/INSTALNO" /> months
            </p>
            <p>
              Monthly installments – EC$<xsl:value-of select="../FOOTER/FIRSTINST" />
            </p>
            <p>
              Annual interest rate – <xsl:value-of select="../FOOTER/INTERESTRATE"/> Each installment due and payable on the same date in each month as the date on which the Loan was disbursed (the “Disbursement Date”) commencing the next calendar month after the said Disbursement Date.
            </p>
          </td>
        </tr>
        <tr>
          <td>5</td>
          <td>Late Interest and Fee</td>
          <td>
            If an installment that is due is unpaid the Borrower(s) shall be liable to pay:
            •	Late interest at the applicable daily rate of <xsl:value-of select="../FOOTER/DAILYLATEINTEREST" />% of the outstanding amount, until settlement of said outstanding amount in full; AND
            •	a late fee which will amount to <xsl:value-of select="../FOOTER/LATEFEE" />% of the total arrears of both principal and interest.
          </td>
        </tr>
        <tr>
          <td>6</td>
          <td>Default and Collection</td>
          <td>
            6.1. If the Loan falls into default the Borrower may be required to pay the entire unpaid principal balance and any accrued interest and fees at once. Notice to pay all the unpaid balance, interest and fees shall be forwarded to the Borrower(s)’ last known address within ten (10) days of the default.

            6.2. If the Loan, any interest, and fees are referred to an attorney or collection agency or agent for collection due to the Borrower(s)’ default, the Borrower shall be liable to pay all attorney fees, court costs, or any fees due to said agency/ agent.
          </td>
        </tr>
        <tr>
          <td>7</td>
          <td>Administrative/ Processing Fee</td>
          <td>
            The Borrower shall pay to the Lender a non-refundable Administrative/ Processing Fee in the amount <xsl:value-of select="../FOOTER/PROCESSINGFEEWORD" /> (EC$<xsl:value-of select="../FOOTER/PROCESSINGFEE" />), which shall be deducted from the proceeds of the Loan.
          </td>
        </tr>
        <tr>
          <td>8</td>
          <td>Insurance</td>
          <td>
            The Borrower(s) hereby agrees to purchase insurance with <xsl:value-of select="../FOOTER/INSURANCECOMPANYNAME"/> (“the insurer”) under Policy No. <xsl:value-of select="../FOOTER/POLICYNUMBER"/> (“the Policy”). The premium payable under the Policy is <xsl:value-of select="../FOOTER/POLICYPREMINUMPERCENTAGE"/>% of the principal sum of the Loan plus VAT and shall be deducted from the proceeds of the Loan.
          </td>
        </tr>
        <tr>
          <td>9</td>
          <td>Stamping Fees</td>
          <td>Stamping and any other cost associated with perfecting this Loan Agreement, (shall be deducted from the proceeds of the Loan) to be borne by the Borrower.</td>
        </tr>
      </table>

    </div>
    <div class="page-break"></div>
  </xsl:template>
</xsl:stylesheet>
