using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;


namespace Addressable_Test
{
    public class FTPCDNStorageAuthentication : MonoBehaviour
    {
        public string FTPHost = "ftp://3.1.213.158/addressable";
        public string FTPUserName = "long.chau-ftp";
        public string FTPPassword = "qvRGjkCtb2m4d5eF";
        public string FilePath;
        //ftp://3.1.213.158/addressable/[BuildTarget]
        /// <summary>
        /// The API access key used for authentication
        /// </summary>
        public string ApiAccessKey { get; private set; }

        /// <summary>
        /// The name of the storage zone we are working on
        /// </summary>
        public string StorageZoneName { get; private set; }

        /// <summary>
        /// The HTTP Client used for the API communication
        /// </summary>
        private HttpClient _http = null;

        private void Start()
        {
            //SetupFTPAuthentication("", "", "", );
            SetupFTPAuthentication();
        }

        /// <summary>
        /// Open the FTP and upload the file asynchronously.
        /// Async help the main process run smoothly, not to be blocked by this heavy process.
        /// </summary>
        /// <param name="filePath"></param>
        private async void AsyncUploadFile(string filePath)
        {
            try
            {
                Uri uri = new Uri(FTPHost + new FileInfo(filePath).Name);

                Debug.Log(uri);

                FtpWebRequest ftp = (FtpWebRequest)WebRequest.Create(uri);
                ftp.Credentials = new NetworkCredential(FTPUserName, FTPPassword);

                ftp.KeepAlive = true;
                ftp.UseBinary = true;
                ftp.Method = WebRequestMethods.Ftp.UploadFile;

                FileStream fs = File.OpenRead(filePath);
                byte[] buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length);
                fs.Close();

                Stream ftpstream = ftp.GetRequestStream();
                await ftpstream.WriteAsync(buffer, 0, buffer.Length);
                ftpstream.Close();
            }
            catch (Exception ex)
            {
                throw new FTP_CDNStorageException(ex.Message);
            }
        }

        [ContextMenu("SetupFTPAuthentication")]
        public void SetupFTPAuthentication()
        {
            try
            {
                Uri uri = new Uri(FTPHost);

                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(uri);
                ftpReq.Credentials = new NetworkCredential(FTPUserName, FTPPassword);
                ftpReq.Method = WebRequestMethods.Ftp.ListDirectory;

                FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                Debug.Log($"List directories: {reader.ReadToEnd()}");
            }
            catch (Exception ex)
            {
                throw new FTP_CDNStorageException(ex.Message);
            }
        }

        /// <summary>
        /// Initializes a new instance of the BunnyCDNStorage class 
        /// </summary>
        /// <param name="storageZoneName">The name of the storage zone to connect to</param>
        /// <param name="apiAccessKey">The API key to authenticate with</param>
        public void SetupFTPAuthentication(string storageZoneName, string apiAccessKey, string mainReplicationRegion = "de", HttpMessageHandler handler = null)
        {
            this.ApiAccessKey = apiAccessKey;
            this.StorageZoneName = storageZoneName;

            // Initialize the HTTP Client
            _http = handler != null ? new HttpClient(handler) : new HttpClient();
            _http.Timeout = new TimeSpan(0, 0, 120);
            _http.DefaultRequestHeaders.Add("AccessKey", this.ApiAccessKey);
            _http.BaseAddress = new Uri(this.GetBaseAddress(mainReplicationRegion));
        }

        #region Delete
        /// <summary>
        /// Delete an object at the given path. If the object is a directory, the contents will also be deleted.
        /// </summary>
        public async Task DeleteObjectAsync(string path)
        {
            var normalizedPath = this.NormalizePath(path);
            try
            {
                await _http.DeleteAsync(normalizedPath);
            }
            catch (WebException ex)
            {
                throw this.MapResponseToException((HttpStatusCode)(int)ex.Status, path);
            }
        }
        #endregion

        #region List
        /// <summary>
        /// Get the list of storage objects on the given path
        /// </summary>
        //public async Task<ArrayStorageObject> GetStorageObjectsAsync(string path)
        //{
        //    var normalizedPath = this.NormalizePath(path, true);
        //    var response = await _http.GetAsync(normalizedPath);

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var responseJson = await response.Content.ReadAsStringAsync();
        //        Debug.Log($"{responseJson}");
        //        string JSONToParse = "{\"storageObjects\":" + responseJson + "}";  // Wrap JSON. Make the array json into the root of an object.
        //        var arrayStorageObjs = JsonUtility.FromJson<ArrayStorageObject>(JSONToParse);
        //        return arrayStorageObjs;
        //    }
        //    else
        //    {
        //        throw this.MapResponseToException(response.StatusCode, normalizedPath);
        //    }
        //}
        #endregion

        #region Upload
        /// <summary>
        /// Upload an object from a stream
        /// </summary>
        public async Task UploadAsync(Stream stream, string path)
        {
            var normalizedPath = this.NormalizePath(path, false);
            using (var content = new StreamContent(stream))
            {
                var response = await _http.PutAsync(normalizedPath, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw this.MapResponseToException(response.StatusCode, normalizedPath);
                }
            }
        }

        /// <summary>
        /// Upload a local file to the storage
        /// </summary>
        public async Task UploadAsync(string localFilePath, string path)
        {
            var normalizedPath = this.NormalizePath(path, false);
            using (var fileStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 64))
            {
                using (var content = new StreamContent(fileStream))
                {
                    var response = await _http.PutAsync(normalizedPath, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw this.MapResponseToException(response.StatusCode, normalizedPath);
                    }
                }
            }
        }
        #endregion

        #region Download
        /// <summary>
        /// Download the object to a local file
        /// </summary>
        /// <param name="path">path</param>
        /// <returns></returns>
        public async Task DownloadObjectAsync(string path, string localFilePath)
        {

            var normalizedPath = this.NormalizePath(path);
            try
            {
                using (var stream = await this.DownloadObjectAsStreamAsync(normalizedPath))
                {
                    // Create a buffered stream to speed up the download
                    using (var bufferedStream = new BufferedStream(stream, 1024 * 64))
                    {
                        using (var fileStream = new FileStream(localFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 1024 * 64))
                        {
                            bufferedStream.CopyTo(fileStream, 1024 * 64);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                throw this.MapResponseToException((HttpStatusCode)(int)ex.Status, path);
            }
        }

        /// <summary>
        /// Return a stream with the contents of the object
        /// </summary>
        /// <param name="path">path</param>
        /// <returns></returns>
        public async Task<Stream> DownloadObjectAsStreamAsync(string path)
        {
            try
            {
                var normalizedPath = this.NormalizePath(path, false);
                return await _http.GetStreamAsync(normalizedPath);
            }
            catch (WebException ex)
            {
                throw this.MapResponseToException((HttpStatusCode)(int)ex.Status, path);
            }
        }
        #endregion

        #region Utils
        /// <summary>
        /// Map the API response to the correct BunnyCDNStorageExecption
        /// </summary>
        /// <param name="statusCode">The StatusCode returned by the API</param>
        /// <param name="path">The called path</param>
        private FTP_CDNStorageException MapResponseToException(HttpStatusCode statusCode, string path)
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                return new FTP_CDNStorageFileNotFoundException(path);
            }
            else if (statusCode == HttpStatusCode.Unauthorized)
            {
                return new BunnyCDNStorageAuthenticationException(this.StorageZoneName, this.ApiAccessKey);
            }
            else
            {
                return new FTP_CDNStorageException("An unknown error has occured during the request.");
            }
        }

        /// <summary>
        /// Normalize a path string
        /// </summary>
        /// <returns></returns>
        private string NormalizePath(string path, bool? isDirectory = null)
        {
            if (!path.StartsWith($"/{this.StorageZoneName}/") && !path.StartsWith($"{this.StorageZoneName}/"))
            {
                throw new FTP_CDNStorageException($"Path validation failed. File path must begin with /{this.StorageZoneName}/.");
            }

            path = path.Replace("\\", "/");
            if (isDirectory != null)
            {
                if (isDirectory.Value)
                {
                    if (!path.EndsWith("/"))
                    {
                        path = path + "/";
                    }
                }
                else
                {
                    if (path.EndsWith("/") && path != "/")
                    {
                        throw new FTP_CDNStorageException("The requested path is invalid.");
                    }
                }
            }
            // Remove double slashes
            while (path.Contains("//"))
            {
                path.Replace("//", "/");
            }
            // Remove the starting slash
            if (path.StartsWith("/"))
            {
                path = path.Remove(0, 1);
            }

            return path;
        }

        /// <summary>
        /// Get the base HTTP URL address of the storage endpoint
        /// </summary>
        /// <param name="mainReplicationRegion">The master region zone code</param>
        /// <returns></returns>
        private string GetBaseAddress(string mainReplicationRegion)
        {
            if (mainReplicationRegion == "" || mainReplicationRegion.ToLower() == "de")
            {
                return "https://storage.bunnycdn.com/";
            }

            return $"https://{mainReplicationRegion}.storage.bunnycdn.com/";
        }
        #endregion
    }

    /// <summary>
    /// An exception thrown by BunnyCDNStorage
    /// </summary>
    public class FTP_CDNStorageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the BunnyCDNStorageException class
        /// </summary>
        /// <param name="message"></param>
        public FTP_CDNStorageException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// An exception thrown by BunnyCDNStorage
    /// </summary>
    public class FTP_CDNStorageFileNotFoundException : FTP_CDNStorageException
    {
        /// <summary>
        /// Initialize a new instance of the BunnyCDNStorageFileNotFoundException class
        /// </summary>
        /// <param name="path">The path that is not found</param>
        public FTP_CDNStorageFileNotFoundException(string path) : base($"Could not find part of the object path: {path}")
        {

        }
    }

    /// <summary>
    /// An exception thrown by BunnyCDNStorage caused by authentication failure
    /// </summary>
    public class BunnyCDNStorageAuthenticationException : FTP_CDNStorageException
    {
        /// <summary>
        /// Initialize a new instance of the BunnyCDNStorageAuthenticationException class
        /// </summary>
        /// <param name="path">The path that is not found</param>
        public BunnyCDNStorageAuthenticationException(string storageZoneName, string accessKey) : base($"Authentication failed for storage zone '{storageZoneName}' with access key '{accessKey}'.")
        {

        }
    }
}