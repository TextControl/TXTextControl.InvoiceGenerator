using s2industries.ZUGFeRD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceGenerator
{
    public class XRechnung
    {
        public string InvoiceNumber { get; set; } = Guid.NewGuid().ToString();
        public DateTime InvoiceDate { get; set; } = DateTime.Now;
        public CurrencyCodes Currency { get; set; } = CurrencyCodes.EUR;
        public string OrderNumber { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string PaymentTerms { get; set; } = "Zahlbar innerhalb 30 Tagen netto bis 04.07.2023";
        public DateTime PaymentDueDate { get; set; } = DateTime.Now.AddDays(30);
        public Seller Seller { get; set; }
        public Buyer Buyer { get; set; }
        public List<LineItem> LineItems { get; set; }
        public decimal TotalNetAmount
        {
            get
            {
                return LineItems.Sum(x => x.LineTotal);
            }
            set
            {
                TotalNetAmount = value;
            }
        }
        public decimal TotalTaxAmount
        {
            get
            {
                return LineItems.Sum(x => x.TaxAmount);
            }
            set
            {
                TotalTaxAmount = value;
            }
        }
        public decimal TotalGrossAmount
        {
            get
            {
                return LineItems.Sum(x => x.Total);
            }
            set
            {
                TotalGrossAmount = value;
            }
        }
        public decimal TotalAllowanceChargeAmount { get; set; } = 0;
        public decimal TotalChargeAmount { get; set; } = 0;
        public decimal TotalPrepaidAmount { get; set; } = 0;
        public decimal TotalRoundingAmount { get; set; } = 0;
        public decimal TotalDueAmount {
            get
            {
                return TotalGrossAmount - TotalPrepaidAmount + TotalTaxAmount;
            }
        }

        // create XML from XRechnung object with s2industries.ZUGFeRD
        public string CreateXML()
        {
            InvoiceDescriptor desc = InvoiceDescriptor.CreateInvoice(this.InvoiceNumber, this.InvoiceDate, this.Currency);
            desc.ReferenceOrderNo = this.OrderNumber;
            desc.SetBuyer(this.Buyer.Name, this.Buyer.ZipCode, this.Buyer.City, this.Buyer.Street, CountryCodes.DE, this.Buyer.Phone);
            desc.AddBuyerTaxRegistration(this.Buyer.VATID, TaxRegistrationSchemeID.VA);
            desc.SetBuyerContact(this.Buyer.Contact);
            desc.SetBuyerOrderReferenceDocument(this.Buyer.OrderReferenceDocument, this.Buyer.OrderReferenceDocumentDate);

            desc.SetSeller(this.Seller.Name, this.Seller.ZipCode, this.Seller.City, this.Seller.Street, CountryCodes.DE, this.Seller.Phone);
            desc.AddSellerTaxRegistration(this.Seller.VATID, TaxRegistrationSchemeID.VA);
            desc.SetSellerContact(this.Seller.Contact, this.Seller.OrganizationUnit, this.Seller.Email, this.Seller.Phone);

            desc.ActualDeliveryDate = this.DeliveryDate;

            desc.SetTotals(
                this.TotalNetAmount,
                this.TotalChargeAmount,
                this.TotalAllowanceChargeAmount,
                this.TotalGrossAmount,
                this.TotalTaxAmount,
                this.TotalNetAmount + this.TotalTaxAmount,
                this.TotalPrepaidAmount,
                this.TotalDueAmount);

            desc.SetTradePaymentTerms(this.PaymentTerms, this.PaymentDueDate);

            foreach (LineItem lineItem in this.LineItems)
            {
                desc.AddTradeLineItem(lineItem.Name, lineItem.Description, lineItem.Unit, lineItem.Quantity, lineItem.UnitPrice + (lineItem.UnitPrice * lineItem.TaxPercent / 100), lineItem.UnitPrice, lineItem.Quantity, lineItem.TaxType, lineItem.TaxCategory, lineItem.TaxPercent);
            }

            desc.AddApplicableTradeTax(this.TotalNetAmount, 19m, TaxTypes.VAT, TaxCategoryCodes.S);

            desc.SetPaymentMeans(PaymentMeansTypeCodes.ClearingBetweenPartners);

            // new memory stream
            MemoryStream stream = new MemoryStream();

            desc.Save(stream, ZUGFeRDVersion.Version21, Profile.XRechnung);

            // FileStream to XML
            stream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(stream);
            string xmlZugferd = reader.ReadToEnd();
            reader.Close();

            return xmlZugferd;
        }
    }

    public class LineItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public QuantityCodes Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal {
            get
            {
                return Quantity * UnitPrice;
            }
            set
            {
                LineTotal = value;
            }
        }
        public decimal TaxAmount
        {
            get
            {
                return LineTotal * (TaxPercent / 100);
            }
            set
            {
                TaxAmount = value;
            }
        }
        public decimal Total
        {
            get
            {
                return LineTotal;
            }
            set
            {
                Total = value;
            }
        }
        public TaxTypes TaxType { get; set; }
        public TaxCategoryCodes TaxCategory { get; set; }
        public decimal TaxPercent { get; set; }
    }   

    public class Buyer
    {
        public string Name { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public CountryCodes Country { get; set; }
        public string VATID { get; set; }
        public TaxRegistrationSchemeID TaxRegistrationSchemeID { get; set; }
        public string Contact { get; set; }
        public string OrganizationUnit { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string OrderReferenceDocument { get; set; }
        public DateTime OrderReferenceDocumentDate { get; set; }
    }

    public class Seller : Buyer
    {
        public string TaxNumber { get; set; }
        public string TaxNumberType { get; set; }
    }
}
