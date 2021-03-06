﻿using NBitcoin;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustchainCore.Extensions;
using TrustgraphCore.Data;
using TrustgraphCore.Model;
using TrustgraphCore.Service;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace TrustgraphTest.Service
{
    [TestFixture]
    public class GraphSearchTest
    {

        [Test]
        public void Search1()
        {
            var trust = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            var trusts = new List<TrustModel>();
            trusts.Add(trust);

            var search = BuildSearch(trusts);
            var query = new RequestQuery();

            query.Issuers = new List<byte[]>();
            query.Issuers.Add(trust.Issuer.Id);
            query.Subjects = new List<SubjectQuery>();
            query.Subjects.Add(new SubjectQuery { Id = trust.Issuer.Subjects[0].Id, Type = trust.Issuer.Subjects[0].IdType });
            query.Scope = trust.Issuer.Subjects[0].Scope;
            query.Claim = trust.Issuer.Subjects[0].Claim;

            var json = JsonConvert.SerializeObject(query, Formatting.Indented);
            Console.WriteLine(json);

            var result = search.Query(query);
            Assert.NotNull(result.Nodes);
            PrintResult(result.Nodes, search.GraphService, 1);
        }

        [Test]
        public void Search2()
        {
            var trust1 = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            var trust2 = TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue());

            var sb = new StringBuilder();
            sb.Append("/api/query/");

            var trusts = new List<TrustModel>();
            trusts.Add(trust1);
            trusts.Add(trust2);

            var search = BuildSearch(trusts);
            var query = new RequestQuery();

            query.Issuers = new List<byte[]>();
            query.Issuers.Add(trust1.Issuer.Id);
            query.Subjects = new List<SubjectQuery>();
            query.Subjects.Add(new SubjectQuery { Id = trust2.Issuer.Subjects[0].Id, Type = trust2.Issuer.Subjects[0].IdType });

            query.Scope = trust2.Issuer.Subjects[0].Scope;
            query.Claim = trust2.Issuer.Subjects[0].Claim;

            var json = JsonConvert.SerializeObject(query, Formatting.Indented);
            Console.WriteLine(json);
            
            var result = search.Query(query);
            Assert.NotNull(result.Nodes);

            //Console.WriteLine("Start id: "+search.GraphService.Graph.IdIndex[0].ConvertToHex()); // A
            PrintResult(result.Nodes, search.GraphService, 1);
            PrintJson(result.Nodes);
        }

        [Test]
        public void Search3()
        {
            var trust1 = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            var trust2 = TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue());
            var trust3 = TrustBuilder.CreateTrust("C", "D", TrustBuilder.CreateTrustTrue());
            var trusts = new List<TrustModel>();
            trusts.Add(trust1);
            trusts.Add(trust2);
            trusts.Add(trust3);
            trusts.Add(TrustBuilder.CreateTrust("B", "E", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("E", "D", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("B", "F", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("F", "G", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("G", "D", TrustBuilder.CreateTrustTrue())); // Long way, no trust


            var search = BuildSearch(trusts);
            var query = new RequestQuery();

            query.Issuers = new List<byte[]>();
            query.Issuers.Add(trust1.Issuer.Id);
            query.Subjects = new List<SubjectQuery>();
            query.Subjects.Add(new SubjectQuery { Id = trust3.Issuer.Subjects[0].Id, Type = trust3.Issuer.Subjects[0].IdType });

            query.Scope = trust2.Issuer.Subjects[0].Scope;
            query.Claim = trust2.Issuer.Subjects[0].Claim;

            var result = search.Query(query);
            Assert.NotNull(result.Nodes);
            //Assert.IsTrue(result.Node.Children.Count == 1);
            //Assert.IsTrue(result.Node.Children[0].Children.Count == 1);

            //Console.WriteLine("Start id: " + search.GraphService.Graph.IdIndex[0].ConvertToHex()); // A
            PrintResult(result.Nodes, search.GraphService, 0);
        }

        [Test]
        public void SearchRating1()
        {
            var trusts = new List<TrustModel>();

            var trustsource = TrustBuilder.CreateTrust("A", "B", TrustBuilder.CreateTrustTrue());
            trusts.Add(trustsource);
            trusts.Add(TrustBuilder.CreateTrust("B", "C", TrustBuilder.CreateTrustTrue()));
            var trusttarget = TrustBuilder.CreateTrust("C", "D", TrustBuilder.CreateRating(100));
            trusts.Add(trusttarget);
            trusts.Add(TrustBuilder.CreateTrust("B", "E", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("E", "D", TrustBuilder.CreateRating(50)));
            trusts.Add(TrustBuilder.CreateTrust("B", "F", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("F", "G", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("G", "D", TrustBuilder.CreateRating(50)));
            trusts.Add(TrustBuilder.CreateTrust("A", "H", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("H", "G", TrustBuilder.CreateTrustTrue()));
            trusts.Add(TrustBuilder.CreateTrust("G", "D", TrustBuilder.CreateRating(50)));

            var search = BuildSearch(trusts);
            var query = new RequestQuery();

            var claim = TrustBuilder.CreateRating(0); // 0 is not used!

            query.Issuers = new List<byte[]>();
            query.Issuers.Add(trustsource.Issuer.Id);
            query.Subjects = new List<SubjectQuery>();
            query.Subjects.Add(new SubjectQuery { Id = trusttarget.Issuer.Subjects[0].Id, Type = trusttarget.Issuer.Subjects[0].IdType });

            query.Scope = trusttarget.Issuer.Subjects[0].Scope;
            query.Claim = claim;

            var result = search.Query(query);
            Assert.NotNull(result.Nodes);
            //Assert.IsTrue(result.Node.Children.Count == 1);
            //Assert.IsTrue(result.Node.Children[0].Children.Count == 0);

            //Console.WriteLine("Start id: "+search.GraphService.Graph.IdIndex[0].ConvertToHex()); // A
            PrintResult(result.Nodes, search.GraphService, 1);
            PrintJson(result.Nodes);
        }

        private void PrintJson(List<SubjectNode> nodes)
        {
            var json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            Console.WriteLine(json);
        }

        private void PrintResult(List<SubjectNode> nodes, IGraphContext service, int level)
        {
            foreach (var node in nodes)
            {
                PrintResult(node, service, level);
            }
        }

        private void PrintResult(SubjectNode node, IGraphContext service, int level)
        {
            if (node.Nodes == null)
            {
                Console.Write("".PadLeft(level, '-'));
                Console.WriteLine("Issuer: {1} trust subject {2}", level, node.NodeIndex, node.Id.ConvertToHex());
                return;
            }

            foreach (var child in node.Nodes)
            {
                Console.Write("".PadLeft(level, '-'));
                Console.WriteLine("Issuer: {1} trust subject {2}", level, node.NodeIndex, child.NodeIndex);

                PrintResult(child, service, level + 1);
            }
        }


        private GraphSearch BuildSearch(IEnumerable<TrustModel> trusts)
        {
            var graphService = new GraphContext();
            var builder = new GraphBuilder(graphService);
            builder.Build(trusts);


            var search = new GraphSearch(graphService);
            return search;
        }
    }
}
