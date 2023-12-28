using InvoiceGenerator;
using Newtonsoft.Json;
using s2industries.ZUGFeRD;
using System.Text;
using TXTextControl.DocumentServer;

XRechnung xRechnung = new XRechnung()
{
    InvoiceNumber = "471102",
    InvoiceDate = new DateTime(2013, 6, 5),
    Currency = CurrencyCodes.EUR,
    OrderNumber = "AB-312",
    DeliveryDate = new DateTime(2013, 6, 3),
    PaymentTerms = "Zahlbar innerhalb 30 Tagen netto bis 04.07.2023",
    PaymentDueDate = new DateTime(2023, 07, 04),
    Seller = new Seller()
    {
        Name = "Lieferant GmbH",
        Street = "Lieferantenstraße",
        ZipCode = "80333",
        City = "München",
        VATID = "DE123456789",
        TaxRegistrationSchemeID = TaxRegistrationSchemeID.VA,
        Contact = "Max Mustermann",
        Phone = "+4942142706710",
        Email = "max@mustermann.de"
    },
    Buyer = new Buyer()
    {
        Name = "Kunden Mitte AG",
        Street = "Kundenstraße",
        ZipCode = "69876",
        City = "Frankfurt",
        VATID = "DE234567890",
        TaxRegistrationSchemeID = TaxRegistrationSchemeID.VA,
        Contact = "Hans Muster",
        OrganizationUnit = "Einkauf",
        Email = "hans@muster.de",
        Phone = "22342424",
        OrderReferenceDocument = "2013-471102",
        OrderReferenceDocumentDate = new DateTime(2013, 5, 5)
    },
    LineItems = new List<LineItem>()
    {
        new LineItem()
        {
            Name = "Item name",
            Description = "Detail description",
            Quantity = 1,
            Unit = QuantityCodes.H87,
            UnitPrice = 10m,
            TaxCategory = TaxCategoryCodes.S,
            TaxType = TaxTypes.VAT,
            TaxPercent = 19
        }
    }
};

// generate JSON from XRechnung object with newtonsoft.json
string json = JsonConvert.SerializeObject(xRechnung);

string xmlZugferd = xRechnung.CreateXML();

// load Xml file
string metaData = File.ReadAllText("metadata.xml");

TXTextControl.SaveSettings saveSettings = new TXTextControl.SaveSettings();

// create a new embedded file
var zugferdInvoice = new TXTextControl.EmbeddedFile(
   "ZUGFeRD-invoice.xml",
   Encoding.UTF8.GetBytes(xmlZugferd),
   metaData);

zugferdInvoice.Description = "ZUGFeRD-invoice";
zugferdInvoice.Relationship = "Alternative";
zugferdInvoice.MIMEType = "application/xml";
zugferdInvoice.LastModificationDate = DateTime.Now;

// set the embedded files
saveSettings.EmbeddedFiles = new TXTextControl.EmbeddedFile[] { zugferdInvoice };

using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl())
{
    tx.Create();
    // load the document
    tx.Load("template.tx", TXTextControl.StreamType.InternalUnicodeFormat);

    // merge the data
    using (MailMerge mm = new MailMerge())
    {
        mm.TextComponent = tx;
        mm.MergeObject(xRechnung);
    }

    // save the document
    tx.Save("test.pdf", TXTextControl.StreamType.AdobePDFA, saveSettings);
}

