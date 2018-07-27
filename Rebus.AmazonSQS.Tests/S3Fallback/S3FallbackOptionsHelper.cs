using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Rebus.Config;

namespace Rebus.AmazonSQS.Tests.S3Fallback
{
    public static class S3FallbackOptionsHelper
    {
        public static AmazonSQSTransportOptions FallbackWithThreshold(int byteThreshold = 0) => new AmazonSQSTransportOptions
        {
            S3Fallback = new S3FallbackOptions
            {
                Enabled = true,
                ByteThreshold = byteThreshold,
                AmazonS3Config = new AmazonS3Config
                {
                    RegionEndpoint = AmazonSqsTransportFactory.ConnectionInfo.RegionEndpoint
                },
                DefaultUploadRequest = new TransferUtilityUploadRequest
                {
                    BucketName = AmazonSqsTransportFactory.ConnectionInfo.BucketName
                }
            }
        };
    }
}
