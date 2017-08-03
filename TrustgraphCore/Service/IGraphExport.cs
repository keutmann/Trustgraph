using TrustchainCore.Model;

namespace TrustgraphCore.Service
{
    public interface IGraphExport
    {
        PackageModel GetFullGraph();
    }
}