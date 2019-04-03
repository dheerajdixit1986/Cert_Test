﻿using System;

using System.Collections.Generic;

using System.Diagnostics;

using System.Linq;

using System.Net;

using System.Net.Http;

using System.Security.Cryptography.X509Certificates;

using System.Web.Configuration;

using System.Web.Http;

using System.Web.Http.Controllers;

using System.Web.Http.Filters;

namespace Cert_Test.Controllers
{
    [RequireSpecificCert]

    public class ValuesController : ApiController

    {
        // GET api/values
        public IHttpActionResult Get()
        {
            return Ok("It works!");
        }
        public class RequireSpecificCertAttribute : AuthorizationFilterAttribute

        {
            public override void OnAuthorization(HttpActionContext actionContext)

            {
                if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)

                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)

                    {
                        ReasonPhrase = "HTTPS Required"
                    };
                 }
                else
                {
                    X509Certificate2 cert = actionContext.Request.GetClientCertificate();
                    X509Certificate2 cert2 = actionContext.RequestContext.ClientCertificate;
                    if (cert == null)

                    {
                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)

                        {
                            ReasonPhrase = "Client Certificate Required"

                        };
                    }
                    else
                    {
                        X509Chain chain = new X509Chain();
                        
                        //Needed because the error "The revocation function was unable to check revocation for the certificate" happened to me otherwise

                        chain.ChainPolicy = new X509ChainPolicy()

                        {
                            RevocationMode = X509RevocationMode.NoCheck,
                        };
                        try

                        {

                            var chainBuilt = chain.Build(cert);

                            Debug.WriteLine(string.Format("Chain building status: {0}", chainBuilt));
                            
                            var validCert = CheckCertificate(chain, cert);
                           
                            if (chainBuilt == false || validCert == false)

                            {

                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)

                                {
                                    ReasonPhrase = "Client Certificate not valid"
                                };
                                
                                foreach (X509ChainStatus chainStatus in chain.ChainStatus)

                                {
                                    Debug.WriteLine(string.Format("Chain error: {0} {1}", chainStatus.Status, chainStatus.StatusInformation));
                                }

                            }

                        }

                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }

                    }
                    base.OnAuthorization(actionContext);

                }

            }
            private bool CheckCertificate(X509Chain chain, X509Certificate2 cert)

            {

                var rootThumbprint = WebConfigurationManager.AppSettings["rootThumbprint"].ToUpper().Replace(" ", string.Empty);
                var clientThumbprint = WebConfigurationManager.AppSettings["clientThumbprint"].ToUpper().Replace(" ", string.Empty);
                
                //Check that the certificate have been issued by a specific Root Certificate
                var validRoot = chain.ChainElements.Cast<X509ChainElement>().Any(x => x.Certificate.Thumbprint.Equals(rootThumbprint, StringComparison.InvariantCultureIgnoreCase));
                //Check that the certificate thumbprint matches our expected thumbprint

                var validCert = cert.Thumbprint.Equals(clientThumbprint, StringComparison.InvariantCultureIgnoreCase);
                
                return validRoot && validCert;
            }

        }

    }

}