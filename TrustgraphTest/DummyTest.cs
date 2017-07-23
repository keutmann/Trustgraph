using NBitcoin.Crypto;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrustchainCore.Collections.Generic;
using TrustchainCore.Service;
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

            Console.WriteLine("Final: " + i);
            Assert.IsTrue(i == 9);
        }

        [Test]
        public void Mem()
        {
            Console.WriteLine(String.Format("Before {0} mb", (int)(GC.GetTotalMemory(true) / 1024 / 1024)));

            //var dd = new Dictionary<int, VisitItem>();
            var dd = new List<VisitItem>(1000);
            for (int i = 1; i <= 1000000; i++)
            {
                // Add 4 byte int
                // Add 12 bytes VisitItem
                // Time 1 mill
                // E = 

                //dd.Add(i, new VisitItem());
                dd.Add(new VisitItem());

                if ((i % 100000) == 0)
                    Console.Write(string.Format("\r{0}%", i - 100000));
            }
            Console.WriteLine("\r100%");
            Console.WriteLine(String.Format("After {0} mb", (int)(GC.GetTotalMemory(true) / 1024 / 1024)));
        }

        [Test]
        public void ByteComparerTest()
        {
            Console.WriteLine(String.Format("Before {0} mb", (int)(GC.GetTotalMemory(true) / 1024 / 1024)));
            var max = 100000;
            var step = max / 10;

            var ids = new List<byte[]>();
            for (int i = 1; i <= max; i++)
            {

                var id = Hashes.SHA256(Encoding.UTF8.GetBytes(i.ToString()));
                ids.Add(id);

                if ((i % step) == 0)
                    Console.Write(string.Format("\r{0}%", i - step));
            }
            Console.WriteLine("\r100%");


            var dd = new Dictionary<byte[], byte[]>(ByteComparer.Standard);
            foreach (var id in ids)
            {
                dd.Add(id, id);
            }

            using (var timer = new TimeMe("Test ByteComparer Safe"))
            {
                foreach (var id in ids)
                {
                    var result = dd[id];
                    var t = result[0];
                }
            }

            var item = new VisitItem(-1, -1);
            using (var timer = new TimeMe("Test List Index"))
            {
                var visited = new VisitItem[max*10];
                for (int i = 0; i < max*10; i++)
                {
                        visited[i] = item; 
                }
            }

            Console.WriteLine(String.Format("After {0} mb", (int)(GC.GetTotalMemory(true) / 1024 / 1024)));

        }
    }
}
