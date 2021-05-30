using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Sceelix.Designer.Utils
{
    public class SceelixWebClient : WebClient
    {
        static SceelixWebClient()
        {
            //ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            //http://stackoverflow.com/questions/20064505/requesting-html-over-https-with-c-sharp-webclient
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
        }



        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslpolicyerrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'", certificate.Subject, sslpolicyerrors.ToString());

            return false;
        }



        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = 3000;
            return w;
        }
    }
}