/// <summary>
/// https://petermeinl.wordpress.com/2015/05/18/tamed-filesystemwatcher/
/// License: New BSD License
/// Modified namespace to match project.
/// </summary>
using System;
using System.ComponentModel;

namespace TrustgraphCore.IO
{
    public class FileWatcherErrorEventArgs : HandledEventArgs
    {
        public readonly Exception Error;
        public FileWatcherErrorEventArgs(Exception exception)
        {
            this.Error = exception;
        }
    }

}