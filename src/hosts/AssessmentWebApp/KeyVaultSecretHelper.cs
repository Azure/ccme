// -----------------------------------------------------------------------
// <copyright file="KeyVaultSecretHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Azure.CCME.Assessment.Hosts
{
    internal static class KeyVaultSecretHelper
    {
        public static async Task<Dictionary<string, string>> LoadAsync(
            string keyVaultBaseUri,
            string applicationId,
            string appCertThumbprint)
        {
            try
            {
                using (var client = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(
                        (authority, resource, scope) => GetToken(applicationId, appCertThumbprint, authority, resource, scope))))
                {
                    var secretItems = await client.GetSecretsAsync(keyVaultBaseUri);
                    var tasks = secretItems.Select(async item => await client.GetSecretAsync(item.Identifier.Identifier));
                    var secrets = (await Task.WhenAll(tasks))
                        .ToDictionary(
                            secret => secret.SecretIdentifier.Name,
                            secret => secret.Value);

                    Trace.TraceInformation(FormattableString.Invariant($"Secrets loaded from keyVault: {string.Join(", ", secrets.Keys)}"));
                    return secrets;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(FormattableString.Invariant($"Failed to load secret from keyVault {keyVaultBaseUri}: {ex}"));
                return new Dictionary<string, string>();
            }
        }

        private static async Task<string> GetToken(
            string applicationId,
            string thumbprint,
            string authority,
            string resource,
            string scope)
        {
            var authenticationContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var clientCredential = new ClientAssertionCertificate(applicationId, GetCertificate(thumbprint));
            var result = await authenticationContext.AcquireTokenAsync(resource, clientCredential);
            return result.AccessToken;
        }

        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);

                var collection = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    thumbprint,
                    false);

                return collection[0];
            }
        }
    }
}