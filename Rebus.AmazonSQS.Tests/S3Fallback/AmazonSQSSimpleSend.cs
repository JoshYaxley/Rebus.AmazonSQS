using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using NUnit.Framework;
using Rebus.Config;
using Rebus.Tests.Contracts.Transports;
using Rebus.Transport;

namespace Rebus.AmazonSQS.Tests.S3Fallback
{
    [TestFixture, Category(Category.S3Fallback)]
    public class AmazonSqsSimpleSend : BasicSendReceive<AmazonSqsWithS3FallbackTransportFactory>
    {
    }
}