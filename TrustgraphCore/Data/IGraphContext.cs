using TrustchainCore.Model;
using TrustgraphCore.Model;

namespace TrustgraphCore.Data
{
    public interface IGraphContext
    {
        GraphModel Graph { get; set; }

        EdgeModel CreateEdgeModel(SubjectModel subject, int timestamp);
        void InitSubjectModel(SubjectModel node, EdgeModel edge);
        int EnsureNode(byte[] id);
        int EnsureScopeIndex(string scope);
        int EnsureSubjectType(string subjectType);
    }
}