using NBitcoin;
using NBitcoin.Crypto;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Business;
using TrustchainCore.Configuration;
using TrustchainCore.Model;

namespace TrustgraphTest
{
    public class TrustBuilder
    {
        public static TrustModel CreateTrust(string issuerName, string subjectName, JObject claim)
        {
            var issuerKey = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes(issuerName)));
            var subjectKey = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes(subjectName)));
            var serverKey = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes("server")));

            var trust = new TrustModel();
            trust.Head = new HeadModel
            {
                Version = "standard 0.1.0",
                Script = "btc-pkh"
            };
            trust.Server = new ServerModel();
            trust.Server.Id = serverKey.PubKey.GetAddress(App.BitcoinNetwork).Hash.ToBytes();
            trust.Issuer = new IssuerModel();
            trust.Issuer.Id = issuerKey.PubKey.GetAddress(App.BitcoinNetwork).Hash.ToBytes();
            var subjects = new List<SubjectModel>();
            subjects.Add(new SubjectModel
            {
                Id = subjectKey.PubKey.GetAddress(App.BitcoinNetwork).Hash.ToBytes(),
                IdType = "person",
                Claim = (claim != null) ? claim : new JObject(
                    new JProperty("trust", "true")
                    ),
                Scope = "global"
            });
            trust.Issuer.Subjects = subjects.ToArray();

            var binary = new TrustBinary(trust);
            trust.TrustId = TrustECDSASignature.GetHashOfBinary(binary.GetIssuerBinary());
            var trustHash = new uint256(trust.TrustId);
            trust.Issuer.Signature = issuerKey.SignCompact(trustHash);

            return trust;
        }

        public static JObject CreateTrustTrue()
        {
            return new JObject(
                    new JProperty("trust", true)
                    );
        }

        public static JObject CreateRating(byte value)
        {
            return new JObject(
                    new JProperty("Rating", value)
                    );
        }

    }
}
