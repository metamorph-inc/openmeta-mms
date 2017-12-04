using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using JobManagerFramework.Jenkins;

namespace JobManagerFramework.RemoteExecution
{
    class WebHelpers
    {
        private const int HTTP_WEB_REQUEST_TIMEOUT = 10 * 60 * 1000;

        public static string SendPostRequest(
            string url,
            string content = "",
            string contentType = null)
        {
            HttpWebResponse response;

            string ret;
            try
            {
                ret = SendPostRequest(url, content, contentType, out response);
                if (response != null)
                    (response as IDisposable).Dispose();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                throw;
            }

            return ret;
        }
        public static string SendPostRequest(
            string url,
            string content,
            string contentType,
            out HttpWebResponse response,
            bool isLogging = true)
        {
            string logMessage = string.Empty;
            string responseFromServer = null;
            response = null;

            // Create a request using a URL that can receive a post. 
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (request as IDisposable)
            {
                request.Timeout = HTTP_WEB_REQUEST_TIMEOUT;
                request.KeepAlive = false;
                request.Method = "POST";
                
                // Create POST data and convert it to a byte array.
                byte[] byteArray = Encoding.UTF8.GetBytes(content);
                // Set the ContentType property of the WebRequest.
                request.ContentType = contentType;
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                using (Stream dataStream = request.GetRequestStream())
                {
                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                // Get the response.
                try
                {
                    response = request.GetResponse() as HttpWebResponse;
                }
                catch (WebException e)
                {
                    if (isLogging)
                    {
                        Trace.TraceError(e.ToString());

                        using (response = e.Response as HttpWebResponse)
                        {
                            logMessage = response.Method + " " + request.Address + " " + (int)response.StatusCode;
                            Trace.TraceWarning(logMessage);

                            using (Stream data = response.GetResponseStream())
                            {
                                string text = new StreamReader(data).ReadToEnd();
                                logMessage = "\t" + text.Replace("\n", "");
                                Trace.TraceWarning(logMessage);
                            }
                        }
                        throw;
                    }
                    else
                    {
                        using (response = e.Response as HttpWebResponse)
                            Trace.TraceError("Exception occured (" + (int)response.StatusCode + ") but logging is disabled");
                        throw;
                    }
                }

                if (isLogging)
                {
                    logMessage = response.Method + " " + request.Address + " " + (int)response.StatusCode;
                    Trace.TraceInformation(logMessage);
                }

                try
                {
                    // Display the status.
                    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        // Open the stream using a StreamReader for easy access.
                        using (StreamReader reader = new StreamReader(dataStream))
                        {
                            // Read the content.
                            responseFromServer = reader.ReadToEnd();
                            // Display the content.
                            //Console.WriteLine(responseFromServer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (isLogging)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                    else
                    {
                        Trace.TraceError("Exception occured but logging is disabled");
                    }

                    (response as IDisposable).Dispose();
                    response = null;
                    throw;
                }
            }
            return responseFromServer;
        }

        public static void SendGetRequest(string url, out HttpWebResponse response, bool isLogging = true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            using (request as IDisposable)
            {
                request.Timeout = HTTP_WEB_REQUEST_TIMEOUT;
                request.KeepAlive = false;

                // Get the response.
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException e)
                {
                    int statusCode = 0;
                    if (e.Response != null && e.Response is HttpWebResponse)
                    {
                        statusCode = (int)((HttpWebResponse)e.Response).StatusCode;
                    }
                    IDisposable disp = e.Response as IDisposable;
                    if (disp != null)
                        disp.Dispose();

                    throw new WebExceptionWithStatusCode(e.Message, statusCode);
                }
            }
        }

        /// <summary>
        /// Sends a get request to a given url.
        /// </summary>
        /// <param name="url">Server's url</param>
        /// <param name="isLogging">Puts all exceptions and requests into the log file.</param>
        /// <param name="rethrow">Rethrows the exception that occured.</param>
        /// <returns></returns>
        public static string SendGetRequest(string url, bool isLogging = true, bool rethrow = true)
        {
            string responseFromServer = null;
            HttpWebResponse response;

            try
            {
                SendGetRequest(url, out response, isLogging);
                using (response)
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.
                    using (StreamReader reader = new StreamReader(dataStream))
                    {
                        // Read the content.
                        responseFromServer = reader.ReadToEnd();
                        // Display the content.
                        //Console.WriteLine(responseFromServer);
                    }
                }
            }
            catch (Exception ex)
            {
                if (isLogging)
                {
                    Trace.TraceInformation("GET " + url);
                    Trace.TraceError(ex.ToString());
                }

                if (rethrow)
                {
                    throw;
                }
            }

            return responseFromServer;
        }
    }
}
