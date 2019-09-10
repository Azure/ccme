// -----------------------------------------------------------------------
// <copyright file="ManagerException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.CCME.Assessment.Managers
{
    /// <summary>
    /// Defines the manager exception class.
    /// </summary>
    /// <seealso cref="Exception" />
    [Serializable]
    public class ManagerException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerException"/> 
        /// class.
        /// </summary>
        public ManagerException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerException"/>
        /// class.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public ManagerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerException"/> 
        /// class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception.</param>
        public ManagerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ManagerException"/>
        /// class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized
        /// object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual
        /// information about the source or destination.
        /// </param>
        protected ManagerException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
            this.SourceSubscriptionId =
                info.GetString(nameof(this.SourceSubscriptionId));
            this.TargetEnvironment = info.GetString(nameof(this.TargetEnvironment));
        }

        /// <summary>
        /// Gets or sets the source subscription identifier.
        /// </summary>
        /// <value>
        /// The source subscription identifier.
        /// </value>
        public string SourceSubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the target environment.
        /// </summary>
        /// <value>
        /// The target environment.
        /// </value>
        public string TargetEnvironment { get; set; }

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
            info.AddValue(
                nameof(this.SourceSubscriptionId),
                this.SourceSubscriptionId);
            info.AddValue(nameof(this.TargetEnvironment), this.TargetEnvironment);
        }
    }
}
