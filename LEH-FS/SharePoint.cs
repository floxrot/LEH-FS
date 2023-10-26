using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace LEH_FS
{
    public class SharePoint
    {
        public static async string GetSharePointAccessToken(string sharePointDomain, string tenantId, string clientId,
            string clientSecret)
        {
            try
            {
                var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/token";

                var request = (HttpWebRequest)WebRequest.Create(tokenEndpoint);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                var postData = new NameValueCollection
                {
                    {"grant_type", "client_credentials"},
                    {"client_id", clientId},
                    {"client_secret", clientSecret},
                    {"resource", "00000003-0000-0ff1-ce00-000000000000/" + sharePointDomain + "@" +tenantId}
                };

                var postString = string.Join("&", Array.ConvertAll(postData.AllKeys, key => $"{key}={Uri.EscapeDataString(postData[key])}"));
                var postBytes = Encoding.ASCII.GetBytes(postString);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postBytes, 0, postBytes.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

                using (JsonDocument doc = JsonDocument.Parse(responseString))
                {
                    return doc.RootElement.GetProperty("access_token").GetString();
                }
            }
            catch (WebException webEx)
            {
                var response = (HttpWebResponse)webEx.Response;
                if (response != null)
                {
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            throw new Exception("Invalid credentials or permissions.");
                        case HttpStatusCode.BadRequest:
                            throw new Exception("Invalid request.");
                        default:
                            throw new Exception($"Server response: {response.StatusCode}");
                    }
                }
                else
                {
                    throw new Exception("An error occurred: " + webEx.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred: " + ex.Message);
            }



            
            // string access_token = string.Empty;
            // WebRequest request = WebRequest.Create("https://accounts.accesscontrol.windows.net/" + tenantId + "/tokens/OAuth/2");
            // request.Method = "POST";
            
            // string postData = "grant_type = client_credentials" +
            //                   "&client_id = " +WebUtility.UrlEncode(clientId + "@" + tenantId) +
            //                   " & client_secret = " +WebUtility.UrlEncode(clientSecret) +
            //                   " & resource = " +WebUtility.UrlEncode("00000003-0000-0ff1-ce00-000000000000/" + sharePointDomain + "@" +tenantId);
            // byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // request.ContentType = "application / x - www - form - urlencoded";
            // request.ContentLength = byteArray.Length;
            // Stream dataStream = request.GetRequestStream();
            // dataStream.Write(byteArray, 0, byteArray.Length);
            // dataStream.Close();
            
            // try
            // {
            //     using (WebResponse response = request.GetResponse())
            //     {
            //         // Console.WriteLine("Status of Token Request: " +((HttpWebResponse) response).StatusDescription);
            //         
            //         dataStream = response.GetResponseStream();
            //         StreamReader reader = new StreamReader(dataStream);
            //         string responseFromServer = reader.ReadToEnd();
            //         reader.Close();
            //         dataStream.Close();
            //         const string accessToken = "access_token\":\"";
            //         int clientIndex = responseFromServer.IndexOf(accessToken, StringComparison.Ordinal);
            //         int accessTokenIndex = clientIndex + accessToken.Length;
            //         access_token = responseFromServer.Substring(accessTokenIndex,
            //             (responseFromServer.Length - accessTokenIndex - 2));
            //         return access_token;
            //     }
            // }
            // catch (WebException wex)
            // {
            //     HttpWebResponse httpResponse = wex.Response as HttpWebResponse;
            //     // Console.WriteLine("Access token not retrieved: " + httpResponse.StatusDescription);
            //     throw;
            // }
            // catch (Exception)
            // {
            //     // Something else happened. Rethrow or log.
            //     throw;
            // }
        }

        public static bool DoesFolderExist(string accessToken, string sharePointDomain, string sharePointSite,
            string sharePointLibrary, Folder folder)
        {
            string site = "/sites/" + sharePointSite + "/";
            if(site == "/sites//")
            {
                site = "/sites/";
            }
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://" + sharePointDomain + site + "_api/web/GetFolderByServerRelativeUrl('" + sharePointLibrary + "/" + folder.Name + "')");
            request.Method = "POST";
            request.Accept = "application/json;odata=verbose";
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
            request.ContentLength = 0;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    return true;
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse httpResponse = wex.Response as HttpWebResponse;
                if (httpResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }
            
        }

        public static void CreateFolder(string accessToken, string sharePointDomain, string sharePointSite,
            string sharePointLibrary, Folder folder)
        {
            string site = "/sites/" + sharePointSite + "/";
            if(site == "/sites//")
            {
                site = "/sites/";
            }
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create("https://" + sharePointDomain + site + "_api/Web/Folders/add('" + sharePointLibrary + "/" + folder.Name + "')");
            request.Method = "POST";
            request.Accept = "application/json;odata=verbose";
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + accessToken);
            request.ContentLength = 0;
            try
            {
                using (WebResponse response = request.GetResponse())
                {
                    // Do something if the resource has changed.leh_fs_job!return;
                }
            }
            catch (WebException wex)
            {
                HttpWebResponse httpResponse = wex.Response as HttpWebResponse;
                if (httpResponse.StatusCode == HttpStatusCode.NotModified)
                {
                    // resource was not modified.
                }
                throw;
            }
            catch (Exception)
            {
                // Something else happened. Rethrow or log.
                throw;
            }

            if (folder.Subfolders.Count > 0)
            {
                foreach (Folder subFolder in folder.Subfolders)
                {
                    CreateFolder(accessToken, sharePointDomain, sharePointSite,
                        sharePointLibrary, folder);
                }
            }

        }
        
    }

    public class Folder
    {
        public string Name { get; set; }
        public List<Folder> Subfolders { get; set; }

        public Folder()
        {
            Subfolders = new List<Folder>();
        }

        public Folder(string name) : this()
        {
            Name = name;
        }

        public void AddSubfolder(Folder folder)
        {
            Subfolders.Add(folder);
        }
    }
    
    public class FolderParser
    {
        public static Folder ParseFromXml(string rootFolderName, string xmlContent)
        {
            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                return new Folder(rootFolderName);
            }

            try
            {
                XDocument doc = XDocument.Parse(xmlContent);
                Folder rootFolder = new Folder(rootFolderName);
                foreach (var subElement in doc.Root.Elements())
                {
                    rootFolder.AddSubfolder(ParseElement(subElement));
                }
                return rootFolder;
            }
            catch (Exception)
            {
                // Bei einem Fehler im XML-Format wird ein leeres Root-Folder-Objekt zurückgegeben
                return new Folder(rootFolderName);
            }
        }

        private static Folder ParseElement(XElement element)
        {
            Folder folder = new Folder(element.Name.LocalName);
            foreach (var subElement in element.Elements())
            {
                folder.AddSubfolder(ParseElement(subElement));
            }
            return folder;
        }
    }
        
        // Beispiel zur Verwendung:
        // string rootFolderName = "YourRootFolderName";
        // string xmlInput = "..."; // Ihr XML-String ohne den Root-Ordner
        // var rootFolder = FolderParser.ParseFromXml(rootFolderName, xmlInput);
        /*
        <Folder1>
            <Subfolder1-1>
                <SubSubfolder1-1-1 />
                <SubSubfolder1-1-2 />
                <SubSubfolder1-1-3 />
            </Subfolder1-1>
            <Subfolder1-2>
                <SubSubfolder1-2-1 />
                <SubSubfolder1-2-2 />
                <SubSubfolder1-2-3 />
            </Subfolder1-2>
            <Subfolder1-3>
                <SubSubfolder1-3-1 />
                <SubSubfolder1-3-2 />
                <SubSubfolder1-3-3 />
            </Subfolder1-3>
        </Folder1>
        */
}