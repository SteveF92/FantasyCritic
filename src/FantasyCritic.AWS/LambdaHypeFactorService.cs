using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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

namespace FantasyCritic.AWS;

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
        string fileName = "LiveData.csv";
        File.Delete(fileName);
        CreateCSVFile(allMasterGameYears, fileName);
        await UploadMasterGameYearStats(fileName);

        var requestParams = new
        {
            bucket = _bucket,
            key = "HypeFactor/LiveData.csv"
        };

        var jsonRequest = JsonConvert.SerializeObject(requestParams);

        AmazonLambdaClient lambdaClient = new AmazonLambdaClient();
        InvokeRequest request = new InvokeRequest()
        {
            FunctionName = "getHypeFactor",
            InvocationType = InvocationType.RequestResponse,
            Payload = jsonRequest
        };

        InvokeResponse response = await lambdaClient.InvokeAsync(request);
        if (response.HttpStatusCode != HttpStatusCode.OK || !string.IsNullOrWhiteSpace(response.FunctionError))
        {
            _logger.Error(response.HttpStatusCode);
            _logger.Error(response.FunctionError);
            _logger.Error(response.LogResult);
            throw new Exception("Lambda function failed.");
        }

        var stringReader = new StreamReader(response.Payload);
        var responseString = stringReader.ReadToEnd().Trim('"').Replace("\\", "");
        var entity = JsonConvert.DeserializeObject<HypeConstantsEntity>(responseString);
        if (entity is null)
        {
            throw new Exception("Invalid hype constants.");
        }

        return entity.ToDomain();
    }

    private static void CreateCSVFile(IEnumerable<MasterGameYear> allMasterGameYears, string fileName)
    {
        IEnumerable<MasterGameYearScriptInput> outputModels = allMasterGameYears.Select(x => new MasterGameYearScriptInput(x));
        using var fileStream = File.OpenWrite(fileName);
        using var streamWriter = new StreamWriter(fileStream);
        using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
        csv.WriteRecords((IEnumerable)outputModels);
    }

    private async Task UploadMasterGameYearStats(string fileName)
    {
        using IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(_region));
        using TransferUtility transferUtility = new TransferUtility(s3Client);
        var fileTransferUtilityRequest = new TransferUtilityUploadRequest
        {
            BucketName = _bucket,
            Key = "HypeFactor/LiveData.csv",
            FilePath = fileName
        };
        await transferUtility.UploadAsync(fileTransferUtilityRequest);
    }
}
