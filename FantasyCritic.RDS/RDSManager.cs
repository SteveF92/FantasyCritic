using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.RDS;
using Amazon.RDS.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal.Auth;
using NodaTime;
using FantasyCritic.Lib.Interfaces;

namespace FantasyCritic.RDS
{
    public class RDSManager : IRDSManager
    {
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _region;
        private readonly string _instanceName;

        public RDSManager(string accessKey, string secretKey, string region, string instanceName)
        {
            _accessKey = accessKey;
            _secretKey = secretKey;
            _region = region;
            _instanceName = instanceName;
        }

        public async Task SnapshotRDS(Instant snapshotTime)
        {
            var credentials = new BasicAWSCredentials(_accessKey, _secretKey);
            var region = Amazon.RegionEndpoint.GetBySystemName(_region);
            AmazonRDSClient rdsClient = new AmazonRDSClient(credentials, region);

            CreateDBSnapshotRequest request = new CreateDBSnapshotRequest("AutoSnap - " + snapshotTime, _instanceName);
            await rdsClient.CreateDBSnapshotAsync(request, CancellationToken.None);
        }
    }
}
