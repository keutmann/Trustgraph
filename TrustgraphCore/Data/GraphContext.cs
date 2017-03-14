using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustgraphCore.Configuration;
using TrustgraphCore.Model;

namespace TrustgraphCore.Data
{
    [IOC(LifeCycle = IOCLifeCycleType.Singleton)]
    public class GraphContext : IGraphContext
    {
        private GraphModel _graph;
        public GraphModel Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        public HashSet<string> FilesLoaded  { get; set; }

        public GraphContext()
        {
            Graph = new GraphModel();
            FilesLoaded = new HashSet<string>();
        }

        public EdgeModel CreateEdgeModel(SubjectModel subject, int timestamp)
        {
            var edge = new EdgeModel();
            edge.SubjectId = EnsureNode(subject.Id);
            edge.SubjectType = EnsureSubjectType(subject.IdType);
            edge.Scope = EnsureScopeIndex(subject.Scope);
            edge.Activate = (int)subject.Activate;
            edge.Expire = (int)subject.Expire;
            edge.Cost = (short)subject.Cost;
            edge.Timestamp = timestamp;
            edge.Claim = ClaimStandardModel.Parse(subject.Claim);

            return edge;
        }

        public int EnsureNode(byte[] id)
        {

            if (!Graph.IssuerIdIndex.ContainsKey(id))
            {
                var index = Graph.Nodes.Count;
                Graph.IssuerIdIndex.Add(id, index);
                Graph.Nodes.Add(new NodeModel());

                return index;
            }

            return Graph.IssuerIdIndex[id];
        }

        public short EnsureSubjectType(string subjectType)
        {

            if (!Graph.SubjectTypesIndex.ContainsKey(subjectType))
            {
                var index = (short)Graph.SubjectTypesIndex.Count;
                Graph.SubjectTypesIndex.Add(subjectType, index);

                return index;
            }

            return (short)Graph.SubjectTypesIndex[subjectType];
        }

        public short EnsureScopeIndex(string scope)
        {
            if (!Graph.ScopeIndex.ContainsKey(scope))
            {
                var index = (short)Graph.ScopeIndex.Count;
                Graph.ScopeIndex.Add(scope, index);

                return index;
            }

            return (short)Graph.ScopeIndex[scope];
        }

    }
}
