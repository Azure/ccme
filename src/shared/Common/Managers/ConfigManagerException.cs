// -----------------------------------------------------------------------
// <copyright file="ConfigManagerException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    /// <summary>
    /// Defines the configuration manager exception class.
    /// </summary>
    /// <seealso cref="ManagerException" />
    [Serializable]
    public class ConfigManagerException : ManagerException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ConfigManagerException"/> class.
        /// </summary>
        public ConfigManagerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ConfigManagerException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public ConfigManagerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ConfigManagerException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public ConfigManagerException(string message, Exception inner)
          : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ConfigManagerException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.
        /// </param>
        protected ConfigManagerException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            this.ConfigProviderType = info.GetString(nameof(this.ConfigProviderType));
        }

        /// <summary>
        /// Gets or sets the type of the configuration provider.
        /// </summary>
        /// <value>
        /// The type of the configuration provider.
        /// </value>
        public string ConfigProviderType { get; set; }

        /// <summary>
        /// When overridden in a derived class, sets the
        /// <see cref="SerializationInfo" /> with information about the
        /// exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.
        /// </param>
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(this.ConfigProviderType), this.ConfigProviderType);
        }
    }
}
