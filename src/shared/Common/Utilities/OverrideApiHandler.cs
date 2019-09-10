// -----------------------------------------------------------------------
// <copyright file="OverrideApiHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.CCME.Assessment.Utilities
{
    public class OverrideApiHandler : DelegatingHandler
    {
        private string apiVersion;
        public OverrideApiHandler(string apiVersion)
            : base()
        {
            this.apiVersion = apiVersion;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.RequestUri = new UriBuilder(
                request.RequestUri.Scheme,
                request.RequestUri.Host)
            {
                Path = request.RequestUri.LocalPath,
                Query = $"api-version={this.apiVersion}"
            }.Uri;
            return base.SendAsync(request, cancellationToken);
        }
    }
}
