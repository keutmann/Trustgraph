using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext Context { get; set; }

        GraphQueryResult Query(GraphQuery query);
    }
}