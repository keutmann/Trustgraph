using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustgraphCore.Data;
using TrustgraphCore.Model;

namespace TrustgraphCore.Service
{
    public class GraphDump : IGraphDump
    {
        private IGraphContext Context;

        public GraphDump(IGraphContext context)
        {
            Context = context;
        }

        public GraphResult GetFullGraph()
        {
            var result = new GraphResult();
            result.Nodes = new List<SubjectNode>();

            foreach (var node in Context.Graph.Address)
            {
                var subject = new SubjectNode();
                subject.Id = node.Id;
                subject.Children = new List<SubjectNode>();
                if (node.Edges != null)
                {
                    foreach (var edge in node.Edges)
                    {
                        var child = new SubjectNode();
                        Context.InitSubjectModel(child, edge);
                        subject.Children.Add(child);
                    }
                }

                result.Nodes.Add(subject);
            }
            return result;
        }

    }
}
