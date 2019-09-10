// -----------------------------------------------------------------------
// <copyright file="StorageAccess.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics;
using System.IO;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.CCME.Assessment.Hosts.DAL
{
    internal static class StorageAccess
    {
        private static readonly CloudBlobContainer ReportsContainer;

        static StorageAccess()
        {
            if (!CloudStorageAccount.TryParse(ConfigHelper.StorageAccountConnectionString, out var storageAccount))
            {
                Trace.TraceError(@"Invalid storage account connection string.");
                return;
            }

            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(@"reports");
            container.CreateIfNotExists();

            ReportsContainer = container;
        }

        public static string UploadFile(string localFilePath)
        {
            var fileName = Path.GetFileName(localFilePath);
            var fileBlob = ReportsContainer.GetBlockBlobReference(fileName);

            fileBlob.UploadFromFile(localFilePath);

            File.Delete(localFilePath);

            return fileName;
        }

        public static void DownloadFile(string fileName, Stream stream)
        {
            var fileBlob = ReportsContainer.GetBlockBlobReference(fileName);
            fileBlob.DownloadToStream(stream);
        }

        public static void RemoveFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var fileBlob = ReportsContainer.GetBlockBlobReference(fileName);
            fileBlob.DeleteIfExists();
        }
    }
}