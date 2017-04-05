using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustgraphCore.Data;
using TrustgraphCore.Service;
using TrustgraphServer;
using Microsoft.Practices.Unity;
using TrustchainCore.Business;
using System.Threading;

namespace TrustgraphTest.Service
{
    [TestFixture]
    public class TrustgraphServiceTest
    {


        [Test]
        public void Start()
        {
            AppDirectory.Setup();

            var service = new TrustgraphService();

            //var t = Task.Run(() =>
            //{
                service.Start();
            //});

            //t.Wait();

            IGraphContext container = UnitySingleton.Container.Resolve<IGraphContext>();
           
            //Assert.IsTrue(container.Graph.Nodes.Count > 1);
        }


    }
}
