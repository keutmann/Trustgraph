using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Business;
using TrustchainCore.Data;
using TrustchainCore.Extensions;
using TrustchainCore.Model;

namespace TrustgraphCore.Service
{
    public class TrustLoader : ITrustLoader
    {
        public IGraphBuilder Builder { get; set; }

        public TrustLoader(IGraphBuilder builder)
        {
            Builder = builder;
        }


        public void LoadFile(string filename)
        {
            IEnumerable<TrustModel> trusts = null;
            var info = new FileInfo(filename);

            if ("json".EqualsIgnoreCase(info.Extension))
                trusts = LoadJson(info);
            else
                if("db".EqualsIgnoreCase(info.Extension))
                trusts = LoadSQLite(info);

            Builder.Build(trusts);
        }

        private IEnumerable<TrustModel> LoadSQLite(FileInfo info)
        {
            using (var db = TrustchainDatabase.Open(info.FullName))
            {
                return db.Trust.Select();
            }
        }

        private IEnumerable<TrustModel> LoadJson(FileInfo info)
        {
            //var json = File.ReadAllText(info.FullName);
            //var trust = TrustManager.Deserialize(json);
            return null;
        }
    }
}
