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
    public class CanSendTooBigMessages : FixtureBase
    {
        readonly Encoding _defaultEncoding = Encoding.UTF8;

        ITransportFactory _factory;
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
        public async Task CanSendAndReceive()
        {
            var input1QueueName = TestConfig.GetName("input1");
            var input2QueueName = TestConfig.GetName("input2");

            var input1 = _factory.Create(input1QueueName);
            var input2 = _factory.Create(input2QueueName);

            var messageContents = string.Concat(Enumerable.Repeat("DET HER ER BARE EN NORMAL STRENG", 10000));

            await WithContext(async context =>
            {
                await input1.Send(input2QueueName, MessageWith(messageContents), context);
            });

            await WithContext(async context =>
            {
                var transportMessage = await input2.Receive(context, _cancellationToken);
                var stringBody = GetStringBody(transportMessage);

                Assert.That(stringBody, Is.EqualTo(messageContents));
            });
        }

        async Task WithContext(Func<ITransactionContext, Task> contextAction, bool completeTransaction = true)
        {
            using (var scope = new RebusTransactionScope())
            {
                await contextAction(scope.TransactionContext);

                if (completeTransaction)
                {
                    await scope.CompleteAsync();
                }
            }
        }

        TransportMessage MessageWith(string stringBody)
        {
            var headers = new Dictionary<string, string>
            {
                {Headers.MessageId, Guid.NewGuid().ToString()}
            };
            var body = _defaultEncoding.GetBytes(stringBody);
            return new TransportMessage(headers, body);
        }

        string GetStringBody(TransportMessage transportMessage)
        {
            if (transportMessage == null)
            {
                throw new InvalidOperationException("Cannot get string body out of null message!");
            }

            return _defaultEncoding.GetString(transportMessage.Body);
        }
    }
}
