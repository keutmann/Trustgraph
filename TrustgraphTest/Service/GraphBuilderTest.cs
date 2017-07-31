using NBitcoin;
using NBitcoin.Crypto;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustgraphCore.Service;

namespace TrustgraphTest.Service
{
    [TestFixture]
    public class GraphBuilderTest
    {

        [Test]
        public void NodeIndex()
        {
            var keyA = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes("A"))).PubKey.GetAddress(Network.TestNet).Hash.ToBytes();
            var keyB = new Key(Hashes.SHA256(Encoding.UTF8.GetBytes("B"))).PubKey.GetAddress(Network.TestNet).Hash.ToBytes();

            var graphService = new GraphContext();
            graphService.Graph.AddressIndex.Add(keyA, 0);
            graphService.Graph.AddressIndex.Add(keyB, 1);

            Assert.IsTrue(graphService.Graph.AddressIndex.ContainsKey(keyA));
            Assert.IsTrue(graphService.Graph.AddressIndex.ContainsKey(keyB));
            Assert.IsTrue(graphService.Graph.AddressIndex[keyA] == 0);
            Assert.IsTrue(graphService.Graph.AddressIndex[keyB] == 1);
        }



        [Test]
        public void BuildEdge1()
        {
            var trust = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            trust.Issuer.Subjects[0].Activate = (uint)DateTime.Now.UnixTimestamp()-1000;
            trust.Issuer.Subjects[0].Expire = (uint)DateTime.Now.UnixTimestamp()+1000;
            trust.Issuer.Subjects[0].Cost = 112;

            var trusts = new List<TrustModel>();
            trusts.Add(trust);
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Address.Count == 2);
            Assert.IsTrue(graphService.Graph.Address[0].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Types == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].SubjectId == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Cost == trust.Issuer.Subjects[0].Cost);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Activate == trust.Issuer.Subjects[0].Activate);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Expire == trust.Issuer.Subjects[0].Expire);

            Assert.IsTrue(graphService.Graph.AddressIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }

        [Test]
        public void BuildEdge2()
        {
            var trusts = new List<TrustModel>();
            trusts.Add(TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue()));

            Assert.AreEqual(trusts[0].Issuer.Subjects[0].Id, trusts[1].Issuer.Id);

            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Address.Count == 3);
            Assert.IsTrue(graphService.Graph.Address[0].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].SubjectId == 1);

            Assert.IsTrue(graphService.Graph.Address[1].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].SubjectId == 2);

            Assert.IsTrue(graphService.Graph.AddressIndex.Count == 3);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }

        [Test]
        public void BuildEdge3()
        {
            var trusts = new List<TrustModel>();
            trusts.Add(TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("C", "A", TrustBuilder.CreateTrustTrue()));
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Address.Count == 3);
            Assert.IsTrue(graphService.Graph.Address[0].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].SubjectId == 1);

            Assert.IsTrue(graphService.Graph.Address[1].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].SubjectId == 2);


            Assert.IsTrue(graphService.Graph.Address[2].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].SubjectId == 0);


            Assert.IsTrue(graphService.Graph.AddressIndex.Count == 3);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }

        [Test]
        public void BuildEdge4()
        {
            var trusts = new List<TrustModel>();
            trusts.Add(TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("A", "C", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("C", "A", TrustBuilder.CreateTrustTrue()));
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Address.Count == 3);

            Assert.IsTrue(graphService.Graph.Address[0].Edges.Length == 2);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].SubjectId == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[1].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[1].SubjectId == 2);

            Assert.IsTrue(graphService.Graph.Address[1].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].SubjectId == 2);

            Assert.IsTrue(graphService.Graph.Address[2].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].SubjectId == 0);

            Assert.IsTrue(graphService.Graph.AddressIndex.Count == 3);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }

        [Test]
        public void BuildEdge5()
        {
            var trusts = new List<TrustModel>();
            trusts.Add(TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("A", "C", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("C", "A", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("D", "E", TrustBuilder.CreateTrustTrue()));
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Address.Count == 5);

            Assert.IsTrue(graphService.Graph.Address[0].Edges.Length == 2);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[0].SubjectId == 1);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[1].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[0].Edges[1].SubjectId == 2);

            Assert.IsTrue(graphService.Graph.Address[1].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[1].Edges[0].SubjectId == 2);

            Assert.IsTrue(graphService.Graph.Address[2].Edges.Length == 1);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].Claim.Flags == ClaimType.Trust);
            Assert.IsTrue(graphService.Graph.Address[2].Edges[0].SubjectId == 0);

            Assert.IsTrue(graphService.Graph.AddressIndex.Count == 5);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }
    }
}
