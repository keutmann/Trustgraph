using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext Data { get; set; }

        GraphQueryContext Query(GraphQuery query);
    }
}