using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.IO;

namespace Cert_Test.Controllers
{
    [RoutePrefix("api/certificatetest")]

    public class CertificateTestController : ApiController

    {
        public IHttpActionResult Get()
        {
            var handler = new WebRequestHandler();

            handler.ClientCertificateOptions = ClientCertificateOption.Manual;

            handler.ClientCertificates.Add(GetClientCertificate());

            handler.UseProxy = false;

            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Host", "localhost:44399");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.86 Safari/537.36");
            
            var result = client.GetAsync("https://localhost:44399//api//values").GetAwaiter().GetResult();
            //var result = client.GetAsync("https://localhost:44399//api//values").GetAwaiter().GetResult();
            //https://localhost:44399/
            var resultString = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return Ok(resultString);
        }
        
        private static X509Certificate GetClientCert()

        {
            X509Store store = null;
            try

            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

                var certificateSerialNumber = "DF285CF2AE54574D2C61762AF44FBAF46BC8D8A0".ToUpper().Replace(" ", string.Empty);
                var cert = store.Certificates.Cast<X509Certificate>().FirstOrDefault(x => x.GetSerialNumberString().Equals(certificateSerialNumber, StringComparison.InvariantCultureIgnoreCase));

                return cert;
            }

            finally

            {
                store.Close();

            }

        }

        private static X509Certificate2 GetClientCertificate()

        {
            X509Store userCaStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try

            {

                userCaStore.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificatesInStore = userCaStore.Certificates;
                X509Certificate2Collection findResult = certificatesInStore.Find(X509FindType.FindByThumbprint, "DF285CF2AE54574D2C61762AF44FBAF46BC8D8A0", true);

                X509Certificate2 clientCertificate = null;
                if (findResult.Count == 1)
                {
                    clientCertificate = findResult[0];
                }
                else
                {
                    throw new Exception("Unable to locate the correct client certificate.");

                }

                return clientCertificate;

            }

            catch

            {

                throw;

            }

            finally

            {

                userCaStore.Close();

            }

        }

    }

}