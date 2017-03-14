using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustgraphCore.Model;

namespace TrustgraphTest
{
    [TestFixture]
    public class DummyTest
    {

        [Test]
        public void CopyTest()
        {
            
            var i = 0;
            for (; i < 10; i++)
            {
                Console.WriteLine(i);
                if (i == 9)
                    break;
            }

            Console.WriteLine("Final: "+i);
            Assert.IsTrue(i == 9);
        }
    }
}
