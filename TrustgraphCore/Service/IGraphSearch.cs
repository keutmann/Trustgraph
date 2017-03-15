using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext Data { get; set; }

        ResultContext Query(GraphQuery query);
    }
}