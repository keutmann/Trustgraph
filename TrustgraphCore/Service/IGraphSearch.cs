using TrustgraphCore.Data;
using TrustgraphCore.Model;

namespace TrustgraphCore.Service
{
    public interface IGraphSearch
    {
        IGraphContext GraphService { get; set; }

        QueryContext Query(RequestQuery query);
    }
}