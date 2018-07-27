using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS.Model;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Logging;
using Rebus.Messages;
using Rebus.Tests.Contracts;
using Rebus.Tests.Contracts.Transports;
using Rebus.Transport;

#pragma warning disable 1998

namespace Rebus.AmazonSQS.Tests.S3Fallback
{
    [TestFixture, Category(Category.S3Fallback)]
    public class OnlyUseS3IfOverThreshold : SqsFixtureBase
    {
        AmazonSqsWithS3FallbackTransportFactory _factory;
        CancellationToken _cancellationToken;

        protected override void SetUp()
        {
            _cancellationToken = new CancellationTokenSource().Token;
            _factory = new AmazonSqsWithS3FallbackTransportFactory();
        }

        protected override void TearDown()
        {
            CleanUpDisposables();

            _factory.CleanUp();
        }

        [Test]
        public async Task HeaderPresentIfOverThreshold()
        {
            var input1QueueName = TestConfig.GetName("input1");
            var input2QueueName = TestConfig.GetName("input2");

            var input1 = _factory.Create(input1QueueName, 1000);
            var input2 = _factory.Create(input2QueueName, 1000);

            var messageContents = string.Concat(Enumerable.Repeat("0", 2000));

            await WithContext(async context =>
            {
                await input1.Send(input2QueueName, MessageWith(messageContents), context);
            });

            await WithContext(async context =>
            {
                var transportMessage = await input2.Receive(context, _cancellationToken);

                Assert.That(transportMessage.Headers.ContainsKey(AmazonSQSTransport.S3FallbackHeader));
            });
        }

        [Test]
        public async Task HeaderAbsentIfUnderThreshold()
        {
            var input1QueueName = TestConfig.GetName("input1");
            var input2QueueName = TestConfig.GetName("input2");

            var input1 = _factory.Create(input1QueueName, 1000);
            var input2 = _factory.Create(input2QueueName, 1000);

            var messageContents = string.Concat(Enumerable.Repeat("0", 100));

            await WithContext(async context =>
            {
                await input1.Send(input2QueueName, MessageWith(messageContents), context);
            });

            await WithContext(async context =>
            {
                var transportMessage = await input2.Receive(context, _cancellationToken);

                Assert.That(!transportMessage.Headers.ContainsKey(AmazonSQSTransport.S3FallbackHeader));
            });
        }
    }
}
