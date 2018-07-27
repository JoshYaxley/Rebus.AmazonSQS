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

        public ITransport CreateOneWayClient(int byteThreshold = 0)
        {
            return _sqsFactory.Create(null, TimeSpan.FromSeconds(30), S3FallbackOptionsHelper.FallbackWithThreshold(byteThreshold));
        }

        public ITransport Create(string inputQueueAddress, int byteThreshold = 0)
        {
            return _sqsFactory.Create(inputQueueAddress, TimeSpan.FromSeconds(30), S3FallbackOptionsHelper.FallbackWithThreshold(byteThreshold));
        }

        public ITransport CreateOneWayClient()
        {
            return CreateOneWayClient(0);
        }

        public ITransport Create(string inputQueueAddress)
        {
            return CreateOneWayClient(0);
        }

        public void CleanUp()
        {
            _sqsFactory.CleanUp();
        }
    }
}
