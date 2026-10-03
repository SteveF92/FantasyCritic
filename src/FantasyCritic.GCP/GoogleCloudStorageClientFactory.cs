using System;
using FantasyCritic.Lib.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.GCP;

public static class GoogleCloudStorageClientFactory
{
    public static StorageClient Create(string credentialConfiguration)
    {
        //A developer's machine, signed in with gcloud auth application-default login.
        if (string.IsNullOrWhiteSpace(credentialConfiguration) || MissingConfiguration.IsPlaceholder(credentialConfiguration))
        {
            return StorageClient.Create();
        }

        //Google asks that a credential configuration be validated before use.
        var credential = GoogleCredential.FromJson(credentialConfiguration);
        if (credential.UnderlyingCredential is not AwsExternalAccountCredential)
        {
            throw new InvalidOperationException(
                $"The Google Cloud credential configuration is a {credential.UnderlyingCredential.GetType().Name}, not AWS Workload Identity Federation.");
        }

        return StorageClient.Create(credential);
    }
}
