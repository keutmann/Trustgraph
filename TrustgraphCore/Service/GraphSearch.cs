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
        public byte[] source;
        public byte[] target;
        public string SubjectType;
        public string Scope;
        public int Activate;
        public int Expire;
        public ClaimStandardModel Claim;
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
    }
}
