// -----------------------------------------------------------------------
// <copyright file="TokenWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Tokens
{
    public class TokenWrapper
    {
        public string Token { get; set; }
        public string UserObjectId { get; set; }
        public DateTime EndTime { get; set; }

        public TokenWrapper(string token, string userObjectId, DateTime endTime)
        {
            this.Token = token;
            this.UserObjectId = userObjectId;
            this.EndTime = endTime;
        }

        public void ResetToken(string token, DateTime endTime)
        {
            this.Token = token;
            this.EndTime = endTime;
        }
    }
}
