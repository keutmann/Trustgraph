using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Model;
using TrustgraphCore.Model;
using TrustchainCore.Extensions;
using System.Diagnostics;
using Newtonsoft.Json;
using TrustchainCore.IOC;

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
            edge.Activate = subject.Activate;
            edge.Expire = subject.Expire;
            edge.Cost = (short)subject.Cost;
            edge.Timestamp = timestamp;
            edge.Claim = ClaimStandardModel.Parse(subject.Claim);

            return edge;
        }

        public void InitSubjectModel(SubjectModel node, EdgeModel edge)
        {
            node.Id = Graph.NodeIndexReverse[edge.SubjectId];
            node.IdType = Graph.SubjectTypesIndexReverse[edge.SubjectType];
            node.Scope = Graph.ScopeIndexReverse[edge.Scope];
            node.Activate = edge.Activate;
            node.Expire = edge.Expire;
            node.Cost = edge.Cost;
            node.Timestamp = edge.Timestamp;
            node.Claim = edge.Claim.ConvertToJObject();
        }

        public int EnsureNode(byte[] id)
        {
            if (Graph.NodeIndex.ContainsKey(id))
                return Graph.NodeIndex[id];

            var index = Graph.Nodes.Count;
            Graph.NodeIndex.Add(id, index);
            Graph.Nodes.Add(new NodeModel());
            Graph.NodeIndexReverse.Add(index, id); // Revert from the internal index to the id address

            return index;
        }

        public int EnsureSubjectType(string subjectType)
        {

            if (!Graph.SubjectTypesIndex.ContainsKey(subjectType))
            {
                var index = (short)Graph.SubjectTypesIndex.Count;
                Graph.SubjectTypesIndex.Add(subjectType, index);
                Graph.SubjectTypesIndexReverse.Add(index, subjectType);

                return index;
            }

            return (short)Graph.SubjectTypesIndex[subjectType];
        }

        public int EnsureScopeIndex(string scope)
        {
            if (!Graph.ScopeIndex.ContainsKey(scope))
            {
                var index = Graph.ScopeIndex.Count;
                Graph.ScopeIndex.Add(scope, index);
                Graph.ScopeIndexReverse.Add(index, scope);

                return index;
            }

            return (short)Graph.ScopeIndex[scope];
        }

    }
}
