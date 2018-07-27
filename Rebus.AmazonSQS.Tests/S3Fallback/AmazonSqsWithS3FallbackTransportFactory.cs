using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Rebus.Config;
using Rebus.Tests.Contracts.Transports;
using Rebus.Transport;

namespace Rebus.AmazonSQS.Tests.S3Fallback
{
    public class AmazonSqsWithS3FallbackTransportFactory : ITransportFactory
    {
        readonly AmazonSqsTransportFactory _sqsFactory;

        public AmazonSqsWithS3FallbackTransportFactory()
        {
            _sqsFactory = new AmazonSqsTransportFactory();
        }

        public ITransport CreateOneWayClient()
        {
            return _sqsFactory.Create(null, TimeSpan.FromSeconds(30), S3FallbackOptionsHelper.AlwaysFallback);
        }

        public ITransport Create(string inputQueueAddress)
        {
            return _sqsFactory.Create(inputQueueAddress, TimeSpan.FromSeconds(30), S3FallbackOptionsHelper.AlwaysFallback);
        }

        public void CleanUp()
        {
            _sqsFactory.CleanUp();
        }
    }
}
