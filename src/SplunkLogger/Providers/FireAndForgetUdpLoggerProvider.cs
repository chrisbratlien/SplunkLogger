using Microsoft.Extensions.Logging;
using Splunk.Configurations;
using Splunk.Loggers;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace Splunk.Providers
{
    /// <summary>
    /// This class is used to provide a Splunk Socket Udp logger for each categoryName.
    /// </summary>
    public class FireAndForgetUdpLoggerProvider : ILoggerProvider
    {
        readonly ILoggerFormatter loggerFormatter;
        readonly SplunkLoggerConfiguration configuration;
        readonly ConcurrentDictionary<string, ILogger> loggers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> class.
        /// </summary>
        /// <param name="configuration">Splunk configuration instance for Socket.</param>
        /// <param name="loggerFormatter">Formatter instance.</param>
        public FireAndForgetUdpLoggerProvider(SplunkLoggerConfiguration configuration, ILoggerFormatter loggerFormatter = null)
        {
            this.loggerFormatter = loggerFormatter;
            this.configuration = configuration;
            loggers = new ConcurrentDictionary<string, ILogger>();
        }

        /// <summary>
        /// Create a <see cref="T:Splunk.Loggers.UdpLogger"/> instance to the category name provided.
        /// </summary>
        /// <returns><see cref="T:Splunk.Loggers.UdpLogger"/> instance.</returns>
        /// <param name="categoryName">Category name.</param>
        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, CreateLoggerInstance(categoryName));
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> so the garbage collector can reclaim the memory
        /// that the <see cref="T:Splunk.Providers.SplunkUdpLoggerProvider"/> was occupying.</remarks>
        public void Dispose() 
        {
            loggers.Clear();
        }

        ILogger CreateLoggerInstance(string categoryName)
        {
            /* 
            Note: for fire-and-forget to work properly:
            1. don't pass arguments to the UdpClient() constructor 
            2. Don't call udpClient.Client.Connect() before any Send() calls
            3. supply Host and Port as 3rd and 4th args to Send()
            */
            var udpClient = new UdpClient();
            return new FireAndForgetUdpLogger(configuration.SocketConfiguration, categoryName, udpClient, loggerFormatter);
        }
    }
}