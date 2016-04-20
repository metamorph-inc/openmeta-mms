using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace MfgBom.OctoPart
{

    [Serializable]
    public class OctopartQueryException : Exception
    {
        public OctopartQueryException()
            : base()
        { }

        public OctopartQueryException(string message)
            : base(message)
        { }

        protected OctopartQueryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class OctopartQueryServerException : OctopartQueryException
    {
        public OctopartQueryServerException()
            : base()
        { }

        public OctopartQueryServerException(string message)
            : base(message)
        { }

        protected OctopartQueryServerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    public class OctopartQueryRateException : OctopartQueryException
    {
        public OctopartQueryRateException()
            : base()
        { }

        public OctopartQueryRateException(string message)
            : base(message)
        { }

        protected OctopartQueryRateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }


    public class Querier
    {
        public String apiKey { get; private set; }
        public Querier(String ApiKey)
        {
            this.apiKey = ApiKey;
        }

        public String QueryMpn(String mpn, bool exact_only, List<string> includes, bool grab_first)
        {
            var query = new List<dynamic>()
            {
                new Dictionary<string, string>()
                { { "mpn", mpn } }
            };

            string octopartUrlBase = "http://octopart.com/api/v3";
            string octopartUrlEndpoint = "parts/match";

            // Create the search request
            string queryString = (new JavaScriptSerializer()).Serialize(query);
            var client = new RestClient(octopartUrlBase);
            var req = CreateRestRequestBasedOnIncludes(octopartUrlEndpoint,
                                                       queryString,
                                                       exact_only,
                                                       includes);

            // Perform the search and obtain results
            var data = client.Execute(req).Content;
            var response = JsonConvert.DeserializeObject<dynamic>(data);

            var classResponse = response["__class__"];
            if (classResponse == "PartsMatchResponse")
            {
                if (response["results"][0]["hits"] == 0 && !String.IsNullOrWhiteSpace(mpn))
                {
                    throw new OctoPart.OctopartQueryException(String.Format("OctoPart MPN number {0} was not found in database", mpn));
                
                }
                                
                // If more than 1 match found, shouldn't just return the first
                //   as it might not be the correct one.
                if (!grab_first && response["results"][0]["hits"] > 1)
                {
                    return "toomany";
                }
                return response["results"][0]["items"][0].ToString();
            }
            else
            {
                if (classResponse == "ClientErrorResponse")
                {
                    var message = response["message"].ToString();
                    if (message == "Access denied by rate limiter")
                    {
                        throw new OctopartQueryRateException(message);
                    }
                    else
                    {
                        throw new OctopartQueryException(message);
                    }
                }
                else if (classResponse == "ServerErrorResponse ")
                {
                    var message = response["message"].ToString();
                    throw new OctopartQueryServerException(message);
                }
                else
                {
                    throw new OctopartQueryException(response.ToString());
                }
            }
        }

        public String QueryCategory(String uid)
        {
            var query = new List<dynamic>()
            {
                new Dictionary<string, string>()
                { { "uid", uid } }
            };

            string octopartUrlBase = "http://octopart.com/api/v3";
            string octopartUrlEndpoint = "categories/" + uid;

            // Create the search request
            string queryString = (new JavaScriptSerializer()).Serialize(query);
            var client = new RestClient(octopartUrlBase);
            var req = new RestRequest(octopartUrlEndpoint, Method.GET)
                                .AddParameter("apikey", this.apiKey)
                                .AddParameter("queries", queryString);

            // Perform the search and obtain results
            var data = client.Execute(req).Content;
            var response = JsonConvert.DeserializeObject<dynamic>(data);

            var classResponse = response["__class__"];
            if (classResponse == "Category")
            {
                return response.ToString();
            }
            else
            {
                if (classResponse == "ClientErrorResponse")
                {
                    var message = response["message"].ToString();
                    if (message == "Access denied by rate limiter")
                    {
                        throw new OctopartQueryRateException(message);
                    }
                    else
                    {
                        throw new OctopartQueryException(message);
                    }
                }
                else if (classResponse == "ServerErrorResponse ")
                {
                    var message = response["message"].ToString();
                    throw new OctopartQueryServerException(message);
                }
                else
                {
                    throw new OctopartQueryException(response.ToString());
                }
            }
        }

        public IRestRequest CreateRestRequestBasedOnIncludes(string octopartUrlEndpoint, 
                                                     string queryString,
                                                     bool exact_only,
                                                     List<string> parameters)
        {
            var req = new RestRequest(octopartUrlEndpoint, Method.GET)
                        .AddParameter("apikey", this.apiKey)
                        .AddParameter("queries", queryString)
                        .AddParameter("exact_only", exact_only);

            if (parameters.Contains("specs"))
            {
                req.AddParameter("include[]", "specs");
            }
            if (parameters.Contains("descriptions"))
            {
                req.AddParameter("include[]", "descriptions");
            }
            if (parameters.Contains("datasheets"))
            {
                req.AddParameter("include[]", "datasheets");
            }
            if (parameters.Contains("imagesets"))
            {
                req.AddParameter("include[]", "imagesets");
            }
            if (parameters.Contains("category_uids"))
            {
                req.AddParameter("include[]", "category_uids");
            }

            return req;
        }
    }
}
