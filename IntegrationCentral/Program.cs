using IntegrationCentral;
using IntegrationCentral.Services.Finance;

var pay = new PayFastService();

var payfast = new PayFastService()
{
    MerchantId = "10027557",
    MerchantKey = "z7kgwwlhpj96y",
    ItemName = "Shongololo Token",
    ItemDescription = "Shongololo Token - in-app tokens that can be redeemed for access to features offered in applications offered by The Other Bhengu (PTY) Ltd (trading as The Geek).",
    Amount = "100",
    FirstName = "Thamsanqa",
    LastName = "Bengu",
    EmailAddress = "tbengu@gmail.com",
    PaymentId = "001",
    NotifyUrl = "https://thejobcenter.co.za/payments/notification/index.html",
    Passphrase = "Bh3nguNgc0l0siMsh1b3TestAP1",
    ProcessAction = "payment"

};


//Ad-Hoc Payment
var response = payfast.Process();
var x = response.ToString();
Console.WriteLine(x);