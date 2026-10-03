using System;
using FantasyCritic.Lib.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;

namespace FantasyCritic.GCP;

public static class GoogleCloudStorageClientFactory
{
    /// <summary>
    /// Signs in with a Workload Identity Federation credential configuration: Google trusts the host's AWS role, so no
    /// Google key exists anywhere. Without one, Application Default Credentials (gcloud auth application-default login)
    /// are used, which is for a developer's machine.
    /// </summary>
    public static StorageClient Create(string credentialConfiguration)
    {
        if (string.IsNullOrWhiteSpace(credentialConfiguration) || MissingConfiguration.IsPlaceholder(credentialConfiguration))
        {
            return StorageClient.Create();
        }

        //Google asks that a credential configuration be checked before use. This one comes from our own secret store, but
        //anything other than AWS federation would mean it is not the one we made.
        var credential = GoogleCredential.FromJson(credentialConfiguration);
        if (credential.UnderlyingCredential is not AwsExternalAccountCredential)
        {
            throw new InvalidOperationException(
                $"The Google Cloud credential configuration is a {credential.UnderlyingCredential.GetType().Name}, not AWS Workload Identity Federation.");
        }

        return StorageClient.Create(credential);
    }
}
