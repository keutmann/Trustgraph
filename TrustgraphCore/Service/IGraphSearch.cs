using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext GraphService { get; set; }

        ResultContext Query(GraphQuery query);
    }
}