using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PayFast;
using PayFast.AspNetCore;
using System.Net.Security;
using Microsoft.AspNetCore.Http;
using System.Collections.Specialized;
using RestSharp;
using System.Threading;
using IntegrationCentral.Services.Vending.Mobile;

namespace IntegrationCentral.Services.Finance
{
    public class PayFastService : IPayfastService
    {
        //The object model Payfast is expecting

        public string MerchantId = "";
        public string MerchantKey = "";
        public string ItemName = "";
        public string ItemDescription = "";
        public string Amount = "";
        public string FirstName = "";
        public string LastName = "";
        public string EmailAddress = "";
        public string PaymentId = "";
        public string NotifyUrl = "";
        public string Passphrase = "";
        public string Signature = "";
        public string ProcessAction = "";
        public bool isSuccessful = false;

        public bool Process()
        {

            switch (ProcessAction)
            {
                case "ping":
                    ProcessRestPingApiPayment();
                    isSuccessful = true;
                    break;
                case "processpayment":
                    ProcessPayment();
                    isSuccessful = true;
                    break;
                case "payment":
                    ProcessRestAdHocPayment();
                    isSuccessful = true;
                    break;
                case "adhoctokensubscription":
                    ProcessRestAdhocTokenizedSubscription();
                    isSuccessful = true;
                    break;
                case "fetchtokensubscription":
                    ProcessRestFetchTokenizedSubscription();
                    isSuccessful = true;
                    break;
                case "canceltokensubscription":
                    ProcessRestCancelTokenizedSubscription();
                    isSuccessful = true;
                    break;
                case "fetchsubscription":
                    ProcessRestFetchSubscription();
                    isSuccessful = true;
                    break;
                case "cancelsubscription":
                    ProcessRestCancelSubscription();
                    isSuccessful = true;
                    break;
                case "updatesubscription":
                    ProcessRestUpdateSubscription();
                    isSuccessful = true;
                    break;
                case "historybydaterange":
                    ProcessRestTransactionHistoryByDateRange();
                    isSuccessful = true;
                    break;
                case "historydaily":
                    ProcessRestTransactionHistoryDaily();
                    isSuccessful = true;
                    break;
                case "historyweekly":
                    ProcessRestTransactionHistoryDaily();
                    isSuccessful = true;
                    break;
                case "historymonthly":
                    ProcessRestTransactionHistoryDaily();
                    isSuccessful = true;
                    break;
                case "historyquery":
                    ProcessRestTransactionHistoryDaily();
                    isSuccessful = true;
                    break;
                default:
                    isSuccessful = false;
                    break;
            }

            return isSuccessful;
        }
        public HttpWebResponse ProcessPayment()
        {
            //Post Url to use to test payments ("sandbox")
            //HttpWebResponse httpResponse = null;
            var url = "https://sandbox.payfast.co.za/onsite/process";
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Method = "POST";

            //Payfast JSON Body            
            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var queryStringArray = ParseQueryString(data);

            var sortedData = queryStringArray.OrderBy(n => n);

            //Payfast Headers
            httpRequest.Accept = "*/*";
            httpRequest.Headers["cache-control"] = "no-cache";
            httpRequest.Headers["Connection"] = "keep-alive";
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Headers["Host"] = "transaction.local";
            httpRequest.Headers["accept-encoding"] = "gzip, deflate";
            httpRequest.Headers["content-length"] = data.Length.ToString();

            //var certificate = new X509Certificate2();
            //var certificatePath = HostingEnvironment.MapPath(@"/App_Data/Certificates/" + httpRequest.ClientCertificates.Insert(0, certificate.);
            //try
            //{
            //    certificate.Import(certificatePath, avsRequest.Credentials.CertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
            //}
            //catch (CryptographicException cex)
            //{
            //    return new AVSError($"Error importing the Certificate: {cex.Message} <br/><br/>");
            //}

            //try
            //{
            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            NEVER_EAT_POISON_Disable_CertificateValidation();

            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            //}
            //catch(Exception ex) 
            //{
            //    Console.WriteLine(ex.Message);
            //}
            return httpResponse;
        }
        public string ProcessRestPingApiPayment()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response.Content);
            return response.Content;
        }
        public string ProcessRestAdHocPayment()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/onsite/process")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 10000
            };

            var client = new RestClient(options);

            var method = Method.Post;

            RestRequest request = new RestRequest();
            request.Method = method;
            request.AddHeader("Accept","*/*");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("Connection","keep-alive");
            request.AddHeader("ContentType","application/x-www-form-urlencoded");
            request.AddHeader("Host","thejobcenter.co.za");
            request.AddHeader("accept-encoding", "gzip, deflate");
            request.AddHeader("content-length",  data.Length.ToString());
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            request.AddBody(data, "application/x-www-form-urlencoded");
            var response = client.Post(request); 
            Console.WriteLine(response.Content);
            return response.Content;
        }
        public string ProcessRestAdhocTokenizedSubscription()
        {
            //Payfast JSON Body

            #region Creating sgnature data


            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            #endregion

            //var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestFetchTokenizedSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestCancelTokenizedSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestUpdateSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestFetchSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestCancelSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestPauseSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestUnPauseSubscription()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestTransactionHistoryByDateRange()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestTransactionHistoryDaily()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestTransactionHistoryWeekly()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestTransactionHistoryMonthly()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }
        public string ProcessRestTransactionHistoryQuery()
        {
            //Payfast JSON Body            
            var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

            //Sort "data" array alphabetically
            var dictionaryData = ParseQueryString(signatureData);

            var sortedData = new SortedDictionary<string, string>(dictionaryData);

            var md5HashString = string.Empty;

            //for (int i = 0; i < sortedData.Count; i++) 
            //{
            //    md5Hash += CreateMD5(sortedData[0,i].ToString());
            //}

            foreach (KeyValuePair<string, string> item in sortedData)
            {
                md5HashString += item.ToString();
            }
            md5HashString = md5HashString.Replace("[", "");
            md5HashString = md5HashString.Replace("]", "");

            var md5Hash = CreateMD5(md5HashString);

            Signature = md5Hash.ToString();

            var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

            var options = new RestClientOptions("https://sandbox.payfast.co.za/ping?testing=true")
            {
                ThrowOnAnyError = true,
                MaxTimeout = 1000
            };

            var client = new RestClient(options);

            var method = Method.Get;

            RestRequest request = new RestRequest();
            request.AddHeader("merchant-id", MerchantId);
            request.AddHeader("version", "v1");
            request.AddHeader("timestamp", DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"));
            request.AddHeader("signature", Signature);
            var response = client.Get(request);
            //var response = client.Execute(request);
            Console.WriteLine(response);
            return response.Content;
        }

        //public async Task<HttpWebResponse> ProcessPayment()
        //{
        //    //Post Url to use to test payments ("sandbox")
        //    //HttpWebResponse httpResponse = null;
        //    var url = "https://sandbox.payfast.co.za/onsite/process";
        //    var httpRequest = (HttpWebRequest)WebRequest.Create(url);
        //    httpRequest.Method = "POST";

        //    //Payfast JSON Body            
        //    var signatureData = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&passphrase={Passphrase}";

        //    //Sort "data" array alphabetically
        //    var dictionaryData = ParseQueryString(signatureData);

        //    var sortedData = new SortedDictionary<string, string>(dictionaryData);

        //    var md5HashString = string.Empty;

        //    //for (int i = 0; i < sortedData.Count; i++) 
        //    //{
        //    //    md5Hash += CreateMD5(sortedData[0,i].ToString());
        //    //}

        //    foreach (KeyValuePair < string, string> item in sortedData)
        //    {
        //        md5HashString += item.ToString();
        //    }
        //    md5HashString = md5HashString.Replace("[", "");
        //    md5HashString = md5HashString.Replace("]", "");

        //    var md5Hash = CreateMD5(md5HashString);

        //    Signature = md5Hash.ToString();

        //    var data = $@"merchant_id={MerchantId}&merchant_key={MerchantKey}&item_name={ItemName}&item_description={ItemDescription}&amount={Amount}&name_first={FirstName}&name_last={LastName}&email_address={EmailAddress}&m_payment_id={PaymentId}&notify_url={NotifyUrl}&testing=true&passphrase={Passphrase}&signature={Signature}";

        //    //Payfast Headers
        //    httpRequest.Accept = "*/*";
        //    httpRequest.Headers["cache-control"] = "no-cache";
        //    httpRequest.Headers["Connection"] = "keep-alive";
        //    httpRequest.ContentType = "application/x-www-form-urlencoded";
        //    httpRequest.Headers["Host"] = "transaction.local";
        //    httpRequest.Headers["accept-encoding"] = "gzip, deflate";
        //    httpRequest.Headers["merchant-id"] = "10027557";
        //    httpRequest.Headers["timestamp"] = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss[+HH:MM]"); //yyyy-MM-ddThh:mm:ss[+HH:MM]
        //    httpRequest.Headers["passphrase"] = Passphrase;
        //    httpRequest.Headers["version"] = "v1";
        //    httpRequest.Headers["signature"] = Signature;
        //    httpRequest.Headers["content-length"] = data.Length.ToString();

        //    //var certificate = new X509Certificate2();
        //    //var certificatePath = HostingEnvironment.MapPath(@"/App_Data/Certificates/" + httpRequest.ClientCertificates.Insert(0, certificate.);
        //    //try
        //    //{
        //    //    certificate.Import(certificatePath, avsRequest.Credentials.CertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet);
        //    //}
        //    //catch (CryptographicException cex)
        //    //{
        //    //    return new AVSError($"Error importing the Certificate: {cex.Message} <br/><br/>");
        //    //}

        //    //try
        //    //{
        //        using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
        //        {
        //            streamWriter.Write(data);
        //        }

        //        NEVER_EAT_POISON_Disable_CertificateValidation();

        //    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
        //    //}
        //    //catch(Exception ex) 
        //    //{
        //    //    Console.WriteLine(ex.Message);
        //    //}
        //    return httpResponse;
        //}

        public static Dictionary<string, string> ParseQueryString(string queryString)
        {
            var nvc = HttpUtility.ParseQueryString(queryString);
            return nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                )
                {
                    return true;
                };
        }
    }


    //In your interface call it as a task
    public interface IPayfastService
    {
        public HttpWebResponse ProcessPayment();
        public string ProcessRestPingApiPayment();

        public string ProcessRestAdHocPayment();

        public string ProcessRestAdhocTokenizedSubscription();

        public string ProcessRestFetchTokenizedSubscription();

        public string ProcessRestCancelTokenizedSubscription();

        public string ProcessRestUpdateSubscription();

        public string ProcessRestFetchSubscription();

        public string ProcessRestCancelSubscription();

        public string ProcessRestPauseSubscription();

        public string ProcessRestUnPauseSubscription();

        public string ProcessRestTransactionHistoryByDateRange();

        public string ProcessRestTransactionHistoryDaily();

        public string ProcessRestTransactionHistoryWeekly();

        public string ProcessRestTransactionHistoryMonthly();

        public string ProcessRestTransactionHistoryQuery();
    }
    // then just call it from the corresponding page e.gvar a = await payfast.ProcessPayment();

    public interface IRestResponse
    {
        public string ProcessRestPayment();
    }
}
