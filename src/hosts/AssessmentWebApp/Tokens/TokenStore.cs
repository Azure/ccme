// -----------------------------------------------------------------------
// <copyright file="TokenStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Azure.CCME.Assessment.Hosts.Tokens
{
    public class TokenStore
    {
        private static TokenStore instance = null;
        private static readonly object Padlock = new object();
        private const int IntervalMinutes = 10;
        private const int TimeoutMinutes = 60;

        private Dictionary<int, TokenWrapper> accessTokenStore = new Dictionary<int, TokenWrapper>();
        private Timer timer = new Timer(Update, null, TimeSpan.Zero, TimeSpan.FromMinutes(IntervalMinutes));

        public TokenStore()
        {
        }

        public static TokenStore Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (Padlock)
                    {
                        if (instance == null)
                        {
                            instance = new TokenStore();
                        }
                    }
                }

                return instance;
            }
        }

        private static void Update(object state)
        {
            lock (Padlock)
            {
                foreach (var key in Instance.accessTokenStore.Where(kpv => kpv.Value.EndTime.AddMinutes(TimeoutMinutes) <= DateTime.UtcNow).Select(p => p.Key).ToList())
                {
                    Instance.accessTokenStore.Remove(key);
                }
            }
        }

        public void AddToken(int taskId, string token, string userObjectId)
        {
            lock (Padlock)
            {
                TokenWrapper tokenWrapper;

                if (this.accessTokenStore.TryGetValue(taskId, out tokenWrapper))
                {
                    tokenWrapper.ResetToken(token, DateTime.UtcNow.AddMinutes(IntervalMinutes));
                }
                else
                {
                    tokenWrapper = new TokenWrapper(token, userObjectId, DateTime.UtcNow.AddMinutes(IntervalMinutes));
                    this.accessTokenStore.Add(taskId, tokenWrapper);
                }
            }
        }

        public string GetTokenByTaskId(int taskId)
        {
            lock (Padlock)
            {
                TokenWrapper tokenWrapper;

                if (this.accessTokenStore.TryGetValue(taskId, out tokenWrapper))
                {
                    return tokenWrapper.Token;
                }
            }

            return null;
        }

        public void RemoveTokenWrapperByTaskId(int taskId)
        {
            lock (Padlock)
            {
                this.accessTokenStore.Remove(taskId);
            }
        }

        public void RemoveTokenWrapperByUserObjectId(string userObjectId)
        {
            lock (Padlock)
            {
                foreach (var key in this.accessTokenStore.Where(kvp => kvp.Value.UserObjectId == userObjectId).Select(p => p.Key).ToList())
                {
                    this.accessTokenStore.Remove(key);
                }
            }
        }
    }
}
