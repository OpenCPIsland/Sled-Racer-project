using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using DI.HTTP.Security.Pinning;

namespace DI.HTTP.Security
{
    public class CertificatePolicyHandler
    {
        private static CertificatePolicyHandler instance = null;

        private IPinset pinset = null;

        private CertificatePolicyHandler()
        {
            setPinset(DefaultPinsetFactory.getFactory().getPinset());

            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
        }

        public IPinset getPinset()
        {
            return pinset;
        }

        public void setPinset(IPinset pinset)
        {
            this.pinset = pinset;
        }

        public static CertificatePolicyHandler getPolicyHandler()
        {
            if (instance == null)
            {
                instance = new CertificatePolicyHandler();
            }
            return instance;
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Only for demo/dev! Accepts all certificates.
            // Replace with pinning logic if needed.
            return true;
        }
    }
}