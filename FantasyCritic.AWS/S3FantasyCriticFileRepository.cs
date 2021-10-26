using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Internal;
using Amazon.S3;
using Amazon.S3.Transfer;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.AWS
{
    public class S3FantasyCriticFileRepository : IFantasyCriticFileRepository
    {
        private readonly string _region;
        private readonly string _bucket;

        public S3FantasyCriticFileRepository(string region, string bucket)
        {
            _region = region;
            _bucket = bucket;
        }

        public async Task UploadMasterGameYearStats(MemoryStream stream)
        {
            stream.Position = 0;
            using (IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(_region)))
            using (TransferUtility transferUtility = new TransferUtility(s3Client))
            {
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                {
                    BucketName = _bucket,
                    Key = "HypeFactor/LiveData.csv",
                    InputStream = stream
                };
                await transferUtility.UploadAsync(fileTransferUtilityRequest);
            }
        }
    }
}
