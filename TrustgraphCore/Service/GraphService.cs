using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustpathCore.Data;
using TrustpathCore.Model;

namespace TrustpathCore.Service
{

    public class GraphQuery
    {
        public byte[] source;
        public byte[] target;
        public string SubjectType;
        public string Claim;
        public int Activate;
        public int Expire;
        public string Scope;
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

    public class GraphService : IGraphService
    {
        public IGraphContext Context { get; set; }

        public GraphService(IGraphContext context)
        {
            Context = context;
        }

        public GraphQueryResult Resolve(GraphQuery query)
        {
            return new GraphQueryResult();
        }
    }
}
