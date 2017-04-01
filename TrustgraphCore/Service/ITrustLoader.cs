namespace TrustgraphCore.Service
{
    public interface ITrustLoader
    {
        IGraphBuilder Builder { get; set; }

        void LoadFile(string filename);
    }
}