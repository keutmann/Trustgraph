/// <summary>
/// https://petermeinl.wordpress.com/2015/05/18/tamed-filesystemwatcher/
/// License: New BSD License
/// Modified namespace to match project.
/// </summary>
using System;

namespace TrustgraphCore.IO
{
    public class EventQueueOverflowException : Exception
    {
        public EventQueueOverflowException()
            : base() { }

        public EventQueueOverflowException(string message)
            : base(message) { }
    }
}
