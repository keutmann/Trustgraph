using System.Collections.Generic;
using TrustchainCore.Model;
using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphBuilder
    {
        IGraphContext Context { get; set; }

        IGraphBuilder Append(PackageModel package);
        IGraphBuilder Build(IEnumerable<TrustModel> trusts);
    }
}