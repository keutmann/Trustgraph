using TrustgraphCore.Data;
using TrustgraphCore.Model;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext GraphService { get; set; }

        ResultContext Query(GraphQuery query);
        ResultContext BuildResultContext(QueryContext context);
    }
}