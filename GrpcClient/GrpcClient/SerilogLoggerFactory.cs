using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using Serilog.Extensions.Logging;

namespace GrpcClient
{
    public class SerilogLoggerFactory : ILoggerFactory
    {
        private readonly SerilogLoggerProvider _provider;
        public SerilogLoggerFactory(Serilog.ILogger logger, bool dispose = false )
        {
            _provider = new SerilogLoggerProvider(logger, dispose);
        }

        public void Dispose() => _provider.Dispose();
        

        public ILogger CreateLogger(string categoryName)
        {
            return _provider.CreateLogger(categoryName);
        }

        public void AddProvider(ILoggerProvider provider)
        {
            //Only Serilog provider is allowed
            SelfLog.WriteLine($"Ignoring add logger provider: {provider}");
        }
    }
}
