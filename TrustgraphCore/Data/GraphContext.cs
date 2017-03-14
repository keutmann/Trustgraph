﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustpathCore.Configuration;
using TrustpathCore.Model;

namespace TrustpathCore.Data
{
    [IOC(LifeCycle = IOCLifeCycleType.Singleton)]
    public class GraphContext : IGraphContext
    {
        private GraphModel _graph;
        public GraphModel Graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        public HashSet<string> FilesLoaded  { get; set; }

        public GraphContext()
        {
            Graph = new GraphModel();
            FilesLoaded = new HashSet<string>();
        }


    }
}
