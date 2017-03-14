using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustgraphCore.Data;
using TrustgraphCore.Model;

namespace TrustgraphCore.Service
{

    public class GraphQuery
    {
        public byte[] Issuer;
        public byte[] Subject;
        public string SubjectType;
        public string Scope;
        public int Activate;
        public int Expire;
        public JObject Claim;
    }

    public class GraphQueryResult
    {
        public List<EdgeModel> Edges;
        public string Result;

        public GraphQueryResult()
        {
            Result = DateTime.Now.ToLongTimeString();
        }
    }

    public class GraphSearch : IGraphSearch
    {
        public IGraphContext Context { get; set; }

        public GraphSearch(IGraphContext context)
        {
            Context = context;
        }

        public GraphQueryResult Query(GraphQuery query)
        {

            return new GraphQueryResult();
        }

        private EdgeModel CreateEgdeQuery(GraphQuery query)
        {
            var edge = new EdgeModel();

            edge.SubjectId = Context.EnsureNode(query.Subject);
            edge.SubjectType = Context.EnsureSubjectType(query.SubjectType);

            edge.Scope = (Context.Graph.ScopeIndex.ContainsKey(query.Scope) ? Context.Graph.ScopeIndex[query.Scope] : (short)-1;
            edge.Activate = query.Activate;
            edge.Expire = query.Expire;
            edge.Claim = ClaimStandardModel.Parse(query.Claim);

            return edge;
        }
    }
}
