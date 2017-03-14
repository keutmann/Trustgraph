using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrustgraphTest
{
    [TestFixture]
    public class DummyTest
    {

        [Test]
        public void DicTest()
        {
            Dictionary<byte[], int> IssuerIdIndex = new Dictionary<byte[], int>();
            var key = new byte[] { 0, 1 };
            IssuerIdIndex.Add(key, 100);

            Assert.IsNotNull(IssuerIdIndex[key]);
            Assert.IsNull(IssuerIdIndex[new byte[] { 1, 1, 1 }]);

        }
    }
}
