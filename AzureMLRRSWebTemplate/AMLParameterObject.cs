using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;

namespace AzureMLRRSWebTemplate
{
    public class AMLParameterObject
    {
        [XmlElement("Url")]
        public string Url;

        [XmlElement("APIKey")]
        public string APIKey;

        [XmlElement("Title")]
        public string Title;

        [XmlElement("Description")]
        public string Description;

        [XmlElement("InputParameter")]
        public List<AMLParam> listInputParameter = new List<AMLParam>();

        [XmlElement("OutputParameter")]
        public List<AMLParam> listOutputParameter = new List<AMLParam>();

        [XmlElement("Copyright")]
        public string Copyright = "";

        public AMLParameterObject()
        {
        }

        public bool ExportInputParameter(string OutputPath)
        {
            bool flag;
            try
            {
                bool flag1 = false;
                Encoding encoding = Encoding.GetEncoding("UTF-8");
                XmlSerializerNamespaces xmlSerializerNamespace = new XmlSerializerNamespaces();
                xmlSerializerNamespace.Add("", "");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(AMLParameterObject));
                StreamWriter streamWriter = new StreamWriter(OutputPath, flag1, encoding);
                try
                {
                    xmlSerializer.Serialize(streamWriter, this, xmlSerializerNamespace);
                }
                finally
                {
                    if (streamWriter != null)
                    {
                        ((IDisposable)streamWriter).Dispose();
                    }
                }
                flag = true;
            }
            catch (Exception exception)
            {
                flag = false;
            }
            return flag;
        }

        private string ExtractString(string full, string start, string end)
        {
            string str;
            int num = full.IndexOf(start);
            if (num != -1)
            {
                int num1 = full.IndexOf(end, num + start.Length + 1);
                str = (num1 != -1 ? full.Substring(num + start.Length, num1 - num - start.Length) : "");
            }
            else
            {
                str = "";
            }
            return str;
        }

        private List<string> GetPaths(string json, string ExecuteionPath)
        {
            List<string> strs;
            List<string> strs1 = new List<string>();
            JToken jTokens = JObject.Parse(json).SelectToken(ExecuteionPath);
            JToken item = jTokens["properties"];
            if (item != null)
            {
                foreach (KeyValuePair<string, JToken> keyValuePair in JObject.Parse(item.ToString()))
                {
                    if (keyValuePair.Value["items"]["$ref"] != null)
                    {
                        strs1.Add(keyValuePair.Value["items"]["$ref"].ToString().Replace("#/", "").Replace("/", "."));
                    }
                }
                strs = strs1;
            }
            else
            {
                strs = null;
            }
            return strs;
        }

        public bool ImportInputParameter(string InputPath)
        {
            bool flag;
            TextReader streamReader = new StreamReader(InputPath);
            try
            {
                AMLParameterObject aMLParameterObject = (AMLParameterObject)(new XmlSerializer(typeof(AMLParameterObject))).Deserialize(streamReader);
                this.Url = aMLParameterObject.Url;
                this.APIKey = aMLParameterObject.APIKey;
                this.Title = aMLParameterObject.Title;
                this.Description = aMLParameterObject.Description;
                this.Copyright = aMLParameterObject.Copyright;
                this.listInputParameter = aMLParameterObject.listInputParameter;
                this.listOutputParameter = aMLParameterObject.listOutputParameter;
                streamReader.Close();
                flag = true;
            }
            catch (Exception exception)
            {
                streamReader.Close();
                flag = false;
            }
            return flag;
        }

        private List<AMLParam> ParseMLParmeter(string jsonStr, string ParameterXPath)
        {
            List<AMLParam> aMLParams;
            try
            {
                List<AMLParam> aMLParams1 = new List<AMLParam>();
                JToken jTokens = JObject.Parse(jsonStr).SelectToken(ParameterXPath);
                JArray jArrays = JArray.Parse(jTokens["required"].ToString());
                JObject jObjects = JObject.Parse(jTokens["properties"].ToString());
                jArrays.ToList<JToken>();
                foreach (KeyValuePair<string, JToken> keyValuePair in jObjects)
                {
                    AMLParam aMLParam = new AMLParam()
                    {
                        Name = keyValuePair.Key,
                        Type = keyValuePair.Value["type"].ToString(),
                        Format = (keyValuePair.Value["format"] != null ? keyValuePair.Value["format"].ToString() : ""),
                        Description = (keyValuePair.Value["description"] != null ? keyValuePair.Value["description"].ToString() : ""),
                        DefaultValue = (keyValuePair.Value["default"] != null ? keyValuePair.Value["default"].ToString() : "")
                    };
                    string[] strArrays = aMLParam.Description.Split(new char[] { '|' });
                    if ((int)strArrays.Length == 2)
                    {
                        aMLParam.Alias = strArrays[0];
                        aMLParam.Description = strArrays[1];
                    }
                    if (aMLParam.Type == "bool")
                    {
                        aMLParam.StrEnum.Add("true");
                        aMLParam.StrEnum.Add("false");
                    }
                    if (keyValuePair.Value["enum"] != null)
                    {
                        JArray jArrays1 = JArray.Parse(keyValuePair.Value["enum"].ToString());
                        aMLParam.StrEnum = jArrays1.ToObject<List<string>>();
                        if (string.IsNullOrEmpty(aMLParam.DefaultValue))
                        {
                            aMLParam.DefaultValue = aMLParam.StrEnum[0];
                        }
                    }
                    else if ((aMLParam.Type == "integer" ? true : aMLParam.Type == "number"))
                    {
                        if (string.IsNullOrEmpty(aMLParam.DefaultValue))
                        {
                            aMLParam.DefaultValue = "0";
                        }
                        else if (aMLParam.Type == "bool" && string.IsNullOrEmpty(aMLParam.DefaultValue))
                        {
                            aMLParam.DefaultValue = "true";
                        }
                    }
                    aMLParams1.Add(aMLParam);
                }
                aMLParams = aMLParams1;
            }
            catch (Exception exception)
            {
                aMLParams = null;
            }
            return aMLParams;
        }

        public string ReadSwagger()
        {
            try
            {
                //string workspaceID = ExtractString(Url, "/workspaces/", "/").Trim('/');
                //if (string.IsNullOrEmpty(workspaceID))
                //    return "Extract workspaces ID Error!!! Please check the API Post URL";
                string serviceID = ExtractString(Url, "/services/", "/").Trim('/');
                if (string.IsNullOrEmpty(serviceID))
                    return "Extract service ID Error!!! Please check the API Post URL";
                string basicLink = Url.Substring(0, Url.IndexOf("execute"));
                var swaggerUrl = $"{basicLink}/swagger.json";
                return ReadSwagger(swaggerUrl);   
            }
            catch (Exception exception)
            {
                return $"Read Swagger Error !!!";
            }
        }

        public string ReadSwagger(string swaggerUrl)
        {
            List<string> paths;
            List<string> strs;
            string str;
            this.listInputParameter.Clear();
            this.listOutputParameter.Clear();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (object param0, X509Certificate param1, X509Chain param2, SslPolicyErrors param3) => true;
                WebClient webClient = new WebClient()
                {
                    Encoding = Encoding.UTF8
                };
                string swaggerJson = webClient.DownloadString(swaggerUrl);
                try
                {
                    this.Title = JObject.Parse(swaggerJson).SelectToken("info.title").ToString();
                }
                catch (Exception exception)
                {
                    str = "Cannot get Service Name !!!";
                    return str;
                }
                try
                {
                    this.Description = JObject.Parse(swaggerJson).SelectToken("info.description").ToString();
                }
                catch (Exception exception1)
                {
                }
                try
                {
                    paths = this.GetPaths(swaggerJson, "definitions.ExecutionInputs");
                }
                catch (Exception exception2)
                {
                    str = "Cannot get List of Input Paramters !!!";
                    return str;
                }
                try
                {
                    strs = this.GetPaths(swaggerJson, "definitions.ExecutionOutputs");
                }
                catch (Exception exception3)
                {
                    str = "Cannot get List of Output Paramters !!!";
                    return str;
                }
                if (paths != null)
                {
                    foreach (string path in paths)
                    {
                        this.listInputParameter.AddRange(this.ParseMLParmeter(swaggerJson, path));
                    }
                }
                if (strs != null)
                {
                    foreach (string path1 in strs)
                    {
                        this.listOutputParameter.AddRange(this.ParseMLParmeter(swaggerJson, path1));
                    }
                }
                if (!(this.listInputParameter == null ? false : this.listOutputParameter != null))
                {
                    str = "";
                }
                else if (this.listOutputParameter.Count > this.listInputParameter.Count)
                {
                    for (int i = 0; i < this.listInputParameter.Count; i++)
                    {
                        this.listOutputParameter[i].Enable = false;
                    }
                    str = "";
                }
                else
                {
                    str = "";
                }
            }
            catch (Exception exception4)
            {
                str = $"Get Service Document Error from {swaggerUrl} !!! Please check the configured API Post URL";
            }

            return str;
        }

        public string ReadSwagger(string workspace, string webservice)
        {
            List<string> paths;
            List<string> strs;
            string str;
            this.listInputParameter.Clear();
            this.listOutputParameter.Clear();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = (object param0, X509Certificate param1, X509Chain param2, SslPolicyErrors param3) => true;
                WebClient webClient = new WebClient()
                {
                    Encoding = Encoding.UTF8
                };
                string swagger = webClient.DownloadString(string.Format("https://ussouthcentral.services.azureml.net/workspaces/{0}/services/{1}/swagger.json", workspace, webservice));
                try
                {
                    this.Title = JObject.Parse(swagger).SelectToken("info.title").ToString();
                }
                catch (Exception exception)
                {
                    str = "Cannot get Service Name !!!";
                    return str;
                }
                try
                {
                    this.Description = JObject.Parse(swagger).SelectToken("info.description").ToString();
                }
                catch (Exception exception1)
                {
                }
                try
                {
                    paths = this.GetPaths(swagger, "definitions.ExecutionInputs");
                }
                catch (Exception exception2)
                {
                    str = "Cannot get List of Input Paramters !!!";
                    return str;
                }
                try
                {
                    strs = this.GetPaths(swagger, "definitions.ExecutionOutputs");
                }
                catch (Exception exception3)
                {
                    str = "Cannot get List of Output Paramters !!!";
                    return str;
                }
                if (paths != null)
                {
                    foreach (string path in paths)
                    {
                        this.listInputParameter.AddRange(this.ParseMLParmeter(swagger, path));
                    }
                }
                if (strs != null)
                {
                    foreach (string path1 in strs)
                    {
                        this.listOutputParameter.AddRange(this.ParseMLParmeter(swagger, path1));
                    }
                }
                if (!(this.listInputParameter == null ? false : this.listOutputParameter != null))
                {
                    str = "";
                }
                else if (this.listOutputParameter.Count > this.listInputParameter.Count)
                {
                    for (int i = 0; i < this.listInputParameter.Count; i++)
                    {
                        this.listOutputParameter[i].Enable = false;
                    }
                    str = "";
                }
                else
                {
                    str = "";
                }
            }
            catch (Exception exception4)
            {
                str = "Get Service Document Error !!! Please check the API Post URL";
            }
            return str;
        }
    }
}