// -----------------------------------------------------------------------
// <copyright file="DbTokenCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Microsoft.Azure.CCME.Assessment.Hosts.DAL;
using Microsoft.Azure.CCME.Assessment.Hosts.DAL.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Identity
{
    internal sealed class DbTokenCache : TokenCache
    {
        private readonly string tenantId;

        private readonly string userObjectId;

        private UserTokenCache userTokenCache;

        public DbTokenCache(string tenantId, string userObjectId)
        {
            this.tenantId = tenantId;
            this.userObjectId = userObjectId;

            this.BeforeAccess +=
                args =>
                {
                    if (this.userTokenCache == null)
                    {
                        this.userTokenCache =
                            DataAccess.GetUserTokenCache(tenantId, userObjectId);
                    }
                    else
                    {
                        var newVersion = DataAccess.GetNewVersionUserTokenCache(
                            tenantId,
                            userObjectId,
                            this.userTokenCache.LastModifedTime);

                        if (newVersion != null)
                        {
                            this.userTokenCache = newVersion;
                        }
                    }

                    this.Deserialize(this.userTokenCache?.CacheBytes);
                };

            this.AfterAccess +=
                args =>
                {
                    if (!this.HasStateChanged)
                    {
                        return;
                    }

                    this.userTokenCache =
                        DataAccess.GetUserTokenCache(
                            this.tenantId,
                            this.userObjectId);

                    if (this.userTokenCache == null)
                    {
                        this.userTokenCache =
                            new UserTokenCache
                            {
                                UserObjectId = this.userObjectId,
                                TenantId = this.tenantId
                            };
                    }

                    this.userTokenCache.CacheBytes = this.Serialize();
                    this.userTokenCache.LastModifedTime = DateTime.UtcNow;

                    DataAccess.UpdateUserTokenCache(this.userTokenCache);

                    this.HasStateChanged = false;
                };

            this.userTokenCache =
                DataAccess.GetUserTokenCache(
                    this.tenantId,
                    this.userObjectId);
            this.Deserialize(this.userTokenCache?.CacheBytes);
        }

        public override void Clear()
        {
            base.Clear();

            DataAccess.ClearUserTokenCache();
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
        }

        public override IEnumerable<TokenCacheItem> ReadItems()
        {
            foreach (var item in base.ReadItems())
            {
                if (item.ExpiresOn < DateTimeOffset.UtcNow)
                {
                    this.DeleteItem(item);
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}