using System;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SQS;
using Rebus.Bus;
using Rebus.Transport;

namespace Rebus.Config
{
    /// <summary>
    /// Holds all of the exposed options which can be applied when using the SQS transport.
    /// </summary>
    public class AmazonSQSTransportOptions
    {
        const string SqsClientContextKey = "SQS_Client";
        const string S3ClientContextKey = "S3_Client";
        const string TransferUtilityContextKey = "S3_Transfer_Utility";

        /// <summary>
        /// Sets the WaitTimeSeconds on the ReceiveMessage. The default setting is 1, which enables long
        /// polling for a single second. The number of seconds can be set up to 20 seconds. 
        /// In case no long polling is desired, then set the value to 0.
        /// </summary>
        public int ReceiveWaitTimeSeconds { get; set; }

        /// <summary>
        /// Configures whether SQS's built-in deferred messages mechanism is to be used when you <see cref="IBus.Defer"/> messages.
        /// Defaults to <code>true</code>.
        /// Please note that SQS's mechanism is only capably of deferring messages up 900 seconds, so you might need to
        /// set <see cref="UseNativeDeferredMessages"/> to <code>false</code> and then use a "real" timeout manager like e.g.
        /// one that uses SQL Server to store timeouts.
        /// </summary>
        public bool UseNativeDeferredMessages { get; set; }

        /// <summary>
        /// Configures whether Rebus is in control to create queues or not. If set to false, Rebus expects that the queue's are already created. 
        /// Defaults to <code>true</code>.
        /// </summary>
        public bool CreateQueues { get; set; }

        /// <summary>
        /// Configures whether Rebus should use an S3 bucket as a fallback for when messages are too big to fit on an SQS queue
        /// </summary>
        public S3FallbackOptions S3Fallback { get; set; }



        /// <summary>
        /// Default constructor of the exposed SQS transport options.
        /// </summary>
        public AmazonSQSTransportOptions()
        {
            ReceiveWaitTimeSeconds = 1;
            UseNativeDeferredMessages = true;
            CreateQueues = true;
            S3Fallback = new S3FallbackOptions
            {
                Enabled = false,
                ByteThreshold = 200_000,
                AmazonS3Config = new AmazonS3Config(),
                DefaultUploadRequest = new TransferUtilityUploadRequest(),
                DefaultOpenStreamRequest = new TransferUtilityOpenStreamRequest()
            };

            GetOrCreateSqsClient = (context, credentials, config) =>
            {
                return context.GetOrAdd(SqsClientContextKey, () =>
                {
                    var amazonSqsClient = new AmazonSQSClient(credentials, config);
                    context.OnDisposed(amazonSqsClient.Dispose);
                    return amazonSqsClient;
                });
            };

            GetOrCreateS3Client = (context, credentials, config) =>
            {
                return context.GetOrAdd(S3ClientContextKey, () =>
                {
                    var amazonS3Client = new AmazonS3Client(credentials, config);
                    context.OnDisposed(amazonS3Client.Dispose);
                    return amazonS3Client;
                });
            };

            GetOrCreateTransferUtility = (context, credentials, config) =>
            {
                return context.GetOrAdd(TransferUtilityContextKey, () =>
                {
                    var amazonS3Client = GetOrCreateS3Client(context, credentials, config);

                    var transferUtility = new TransferUtility(amazonS3Client);
                    context.OnDisposed(transferUtility.Dispose);
                    return transferUtility;
                });
            };
        }

        /// <summary>
        /// Function that is getting or creating the <cref type="IAmazonSQS"/> object that will be used for SQS.
        /// </summary>
        /// <returns>The <cref type="IAmazonSQS"/> object to be used for SQS.</returns>
        public Func<ITransactionContext, AWSCredentials, AmazonSQSConfig, IAmazonSQS> GetOrCreateSqsClient;

        /// <summary>
        /// Function that is getting or creating the <cref type="IAmazonS3"/> object that will be used for S3.
        /// </summary>
        /// <returns>The <cref type="IAmazonS3"/> object to be used for S3.</returns>
        public Func<ITransactionContext, AWSCredentials, AmazonS3Config, IAmazonS3> GetOrCreateS3Client;

        /// <summary>
        /// Function that is getting or creating the <cref type="ITransferUtility"/> object that will be used for S3 file transfers.
        /// </summary>
        /// <returns>The <cref type="ITransferUtility"/> object to be used for S3 file transfers.</returns>
        public Func<ITransactionContext, AWSCredentials, AmazonS3Config, ITransferUtility> GetOrCreateTransferUtility;
    }

    /// <summary>
    /// Options controlling if/how Rebus should use an S3 bucket as a fallback for when messages are too big to fit on an SQS queue.
    /// </summary>
    public class S3FallbackOptions
    {
        /// <summary>
        /// Enables the S3 fallback.
        /// Defaults to <code>false</code>
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The number of bytes a message's size must reach to be eligible for S3 fallback.
        /// SQS has a message size limit of 256KB, so this threshold should be less than this.
        /// Defaults to <code>200_000</code>
        /// </summary>
        public int ByteThreshold { get; set; }

        /// <summary>
        /// The AmazonS3Config to use when creating the AmazonS3Client. Set region here.
        /// </summary>
        public AmazonS3Config AmazonS3Config { get; set; }

        /// <summary>
        /// The default TransferUtilityUploadRequest object to use when uploading to S3.
        /// Key will be overridden at point of upload.
        /// Set BucketName here.
        /// </summary>
        public TransferUtilityUploadRequest DefaultUploadRequest { get; set; }

        /// <summary>
        /// The default TransferUtilityOpenStreamRequest object to use when reading from S3.
        /// Key and BucketName will be overridden at point of read.
        /// </summary>
        public TransferUtilityOpenStreamRequest DefaultOpenStreamRequest { get; set; }
    }
}
