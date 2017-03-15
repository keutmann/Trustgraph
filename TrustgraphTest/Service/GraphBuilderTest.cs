using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustgraphCore.Data;
using TrustgraphCore.Service;

namespace TrustgraphTest.Service
{
    [TestFixture]
    public class GraphBuilderTest
    {
        [Test]
        public void Build1()
        {
            var trust = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            var trusts = new List<TrustModel>();
            trusts.Add(trust);
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);

            builder.Build(trusts);

            Assert.IsTrue(graphService.Graph.Nodes.Count == 2);
            Assert.IsTrue(graphService.Graph.NodeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.SubjectTypesIndex.ContainsKey("person"));
            Assert.IsTrue(graphService.Graph.ScopeIndex.Count == 2);
            Assert.IsTrue(graphService.Graph.ScopeIndex.ContainsKey("global"));
        }
    }
}
