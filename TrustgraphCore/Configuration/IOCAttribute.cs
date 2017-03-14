using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrustpathCore.Configuration
{
    public enum IOCLifeCycleType
    {
        Singleton,
        PerThread,
        PerResolve,
        PerRequest
    }


    public class IOCAttribute : Attribute
    {
        public IOCLifeCycleType LifeCycle { get; set; }
    }
}
