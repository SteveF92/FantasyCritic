using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.S3;
using Amazon.S3.Transfer;
using CsvHelper;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using FantasyCritic.Lib.Statistics;
using Newtonsoft.Json;
using NLog;

namespace FantasyCritic.AWS
{
    public class LambdaHypeFactorService : IHypeFactorService
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly string _region;
        private readonly string _bucket;

        public LambdaHypeFactorService(string region, string bucket)
        {
            _region = region;
            _bucket = bucket;
        }

        public async Task<HypeConstants> GetHypeConstants(IEnumerable<MasterGameYear> allMasterGameYears)
        {
            var dataStream = CreateDataStream(allMasterGameYears);
            await UploadMasterGameYearStats(dataStream);

            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();
            InvokeRequest request = new InvokeRequest()
            {
                FunctionName = "getHypeFactor",
                InvocationType = InvocationType.RequestResponse
            };

            InvokeResponse response = await lambdaClient.InvokeAsync(request);
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                _logger.Error(response.HttpStatusCode);
                _logger.Error(response.FunctionError);
                _logger.Error(response.LogResult);
                throw new Exception("Lambda function failed.");
            }

            var stringReader = new StreamReader(response.Payload);
            var responseString = stringReader.ReadToEnd().Trim('"').Replace("\\", "");
            var entity = JsonConvert.DeserializeObject<HypeConstantsEntity>(responseString);
            return entity.ToDomain();
        }

        private MemoryStream CreateDataStream(IEnumerable<MasterGameYear> allMasterGameYears)
        {
            IEnumerable<MasterGameYearScriptInput> outputModels = allMasterGameYears.Select(x => new MasterGameYearScriptInput(x));
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(outputModels);

            return memoryStream;
        }

        private async Task UploadMasterGameYearStats(MemoryStream stream)
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
