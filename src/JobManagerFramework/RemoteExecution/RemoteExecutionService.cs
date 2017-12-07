using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using GME.MGA.Core;
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

        public RemoteJob GetJobInfo(string jobId)
        {
            return Get<RemoteJob>("/api/client/job/" + jobId);
        }

        private JObject GetJson(string path)
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.GET;

            var response = client.Execute(request);
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
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ObjectNotFoundException("Job not found");
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

        private T Get<T>(string path) where T: new()
        {
            var client = new RestClient(BaseUri);
            client.Authenticator = new HttpBasicAuthenticator(Username, Password);

            var request = new RestRequest(path);
            request.Method = Method.GET;

            var response = client.Execute<T>(request);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.Data;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ObjectNotFoundException("Job not found");
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
            public string RunZipId { get; set; }
            public string Owner { get; set; }
            public DateTime CreationTime { get; set; }
            public RemoteJobState Status { get; set; }
            public string Uid { get; set; }
            public string ResultZipId { get; set; }
        }
    }
}
