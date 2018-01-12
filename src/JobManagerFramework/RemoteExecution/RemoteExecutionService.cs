using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using GME.MGA.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace JobManagerFramework.RemoteExecution
{
    public class RemoteExecutionService
    {
        const int REQUEST_TIMEOUT = 10 * 60 * 1000;

        private string BaseUri { get; }

        private string Username { get; }

        private string Password { get; }

        public RemoteExecutionService(string baseUri, string username, string password)
        {
            BaseUri = baseUri;
            Username = username;
            Password = password;
        }

        public bool PingServer()
        {
            var result = GetJson("/api/client/ping");

            JToken token = null;
            if (result.TryGetValue("result", out token) && token.Value<string>() == "ok")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public RemoteJob GetJobInfo(string jobId)
        {
            try
            {
                return Get<RemoteJob>("/api/client/job/" + jobId);
            }
            catch (RequestFailedException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new ObjectNotFoundException("Job not found");
                }
                else
                {
                    throw;
                }
            }
        }

        public bool CancelJob(string jobId)
        {
            var result = GetJson("/api/client/job/" + jobId + "/cancel", Method.POST);

            JToken token = null;
            if (result.TryGetValue("cancelled", out token) && token.Value<bool>() == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string UploadArtifact(Stream fileStream)
        {
            var result = PutFile("/api/client/uploadArtifact", "artifact", fileStream);

            return result["hash"].Value<string>();
        }

        public void DownloadArtifact(string hash, Stream fileWriter)
        {
            GetFile("/api/client/downloadArtifact/" + hash, fileWriter);
        }

        public string CreateJob(string runCommand, string workingDirectory, string runZipId, string labels)
        {
            var result = PutObjectAsJson("/api/client/createJob", new RemoteJobRequest {runCommand = runCommand, workingDirectory = workingDirectory, runZipId = runZipId, labels = labels});

            return result["id"].Value<string>();
        }

        private void GetFile(string path, Stream fileWriter)
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.GET;

            request.ResponseWriter = responseStream => responseStream.CopyTo(fileWriter);

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                // Success response handled by ResponseWriter
            }
            else
            {
                var responseBody = response.Content;
                var responseMessage = "Unknown error occurred";
                try
                {
                    var jsonData = JObject.Parse(responseBody);
                    responseMessage = jsonData["message"].Value<string>();
                }
                catch (Exception)
                {
                    // Failed to parse JSON or didn't contain "message" property
                }
                throw new RequestFailedException(response.StatusCode, responseMessage);
            }
        }

        private JObject GetJson(string path, Method method = Method.GET)
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = method;

            var response = client.Execute(request);
            return GetJsonFromResponse(response);
        }

        private T Get<T>(string path) where T: new()
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.GET;

            var response = client.Execute<T>(request);
            return GetDataFromResponse(response);
        }

        private static T GetDataFromResponse<T>(IRestResponse<T> response) where T : new()
        {
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new RequestFailedException(response.StatusCode, response.ErrorMessage);
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            else
            {
                var responseBody = response.Content;
                var responseMessage = "Unknown error occurred";
                try
                {
                    var jsonData = JObject.Parse(responseBody);
                    responseMessage = jsonData["message"].Value<string>();
                }
                catch (Exception)
                {
                    // Failed to parse JSON or didn't contain "message" property
                }
                throw new RequestFailedException(response.StatusCode, responseMessage);
            }
        }

        private JObject PutFile(string path, string parameterName, Stream fileStream)
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.PUT;

            request.AddFile(parameterName, fileStream.CopyTo, "upload");
            request.AlwaysMultipartFormData = true;

            var response = client.Execute(request);
            return GetJsonFromResponse(response);
        }

        private JObject PutObjectAsJson(string path, object obj)
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.PUT;

            request.RequestFormat = DataFormat.Json;
            request.AddBody(obj);

            var response = client.Execute(request);
            return GetJsonFromResponse(response);
        }

        private static JObject GetJsonFromResponse(IRestResponse response)
        {
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = response.Content;
                var jsonData = JObject.Parse(data);
                return jsonData;
            }
            else
            {
                var responseBody = response.Content;
                var responseMessage = "Unknown error occurred";
                try
                {
                    var jsonData = JObject.Parse(responseBody);
                    responseMessage = jsonData["message"].Value<string>();
                }
                catch (Exception)
                {
                    // Failed to parse JSON or didn't contain "message" property
                }
                throw new RequestFailedException(response.StatusCode, responseMessage);
            }
        }

        public class RequestFailedException : Exception
        {
            public HttpStatusCode StatusCode { get; }

            public RequestFailedException(HttpStatusCode statusCode, string message) : base(string.Format("{0}: {1}", statusCode, message))
            {
                StatusCode = statusCode;
            }
        }

        public class ObjectNotFoundException : RequestFailedException
        {
            public ObjectNotFoundException(string message) : base(HttpStatusCode.NotFound, message) { }

            public ObjectNotFoundException() : this("Not Found") { }
        }

        public enum RemoteJobState
        {
            Created = 0,
            Running = 1,
            Succeeded = 2,
            Failed = 3,
            Cancelled = 4
        }

        public class RemoteJob
        {
            public string RunCommand { get; set; }
            public string WorkingDirectory { get; set; }
            public string RunZipId { get; set; }
            public string Owner { get; set; }
            public DateTime CreationTime { get; set; }
            public RemoteJobState Status { get; set; }
            public string Uid { get; set; }
            public string ResultZipId { get; set; }
            public List<string> Labels { get; set; }
        }

        private class RemoteJobRequest
        {
            public string runCommand { get; set; }
            public string workingDirectory { get; set; }
            public string runZipId { get; set; }
            public string labels { get; set; }
        }
    }
}
