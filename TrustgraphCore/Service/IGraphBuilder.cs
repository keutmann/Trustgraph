using System.Collections.Generic;
using TrustchainCore.Model;
using TrustgraphCore.Data;

namespace TrustgraphCore.Service
{
    public interface IGraphBuilder
    {
        IGraphContext Context { get; set; }

        void Build(IEnumerable<TrustModel> trusts);
    }
}