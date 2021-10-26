using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using FantasyCritic.Lib.Domain;
using FantasyCritic.Lib.Interfaces;
using Newtonsoft.Json;

namespace FantasyCritic.AWS
{
    public class LambdaService : IPythonService
    {
        public async Task<HypeConstants> GetHypeConstants()
        {
            AmazonLambdaClient lambdaClient = new AmazonLambdaClient();
            InvokeRequest request = new InvokeRequest()
            {
                FunctionName = "getHypeFactor",
                InvocationType = InvocationType.RequestResponse
            };

            InvokeResponse response = await lambdaClient.InvokeAsync(request);
            var stringReader = new StreamReader(response.Payload);
            var responseString = stringReader.ReadToEnd().Trim('"').Replace("\\", "");
            var entity = JsonConvert.DeserializeObject<HypeConstantsEntity>(responseString);
            return entity.ToDomain();
        }
    }
}
