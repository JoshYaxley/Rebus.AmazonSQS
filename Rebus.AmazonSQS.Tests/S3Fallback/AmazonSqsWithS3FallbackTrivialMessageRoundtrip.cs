using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Tests.Contracts;

namespace Rebus.AmazonSQS.Tests.S3Fallback
{
    [Category(Category.S3Fallback)]
    public class AmazonSqsWithS3FallbackTrivialMessageRoundtrip : AmazonSqsTrivialMessageRoundtrip
    {
        protected override void SetUp()
        {
            BrilliantQueueName = TestConfig.GetName("roundtrippin");
            Transport = AmazonSqsTransportFactory.CreateTransport(BrilliantQueueName, TimeSpan.FromSeconds(30), S3FallbackOptionsHelper.FallbackWithThreshold());
            Transport.Purge();
        }
    }
}
