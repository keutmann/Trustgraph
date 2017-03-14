using TrustpathCore.Data;

namespace TrustpathCore.Service
{
    public interface IGraphService
    {
        IGraphContext Context { get; set; }

        GraphQueryResult Resolve(GraphQuery query);
    }
}