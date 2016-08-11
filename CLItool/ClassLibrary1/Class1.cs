﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http; // if you want text formatting helpers (recommended)
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Web.Script.Serialization;

namespace MobileHelixUtility
{
    // just for getting started
    public class Class1
    {
        private String version = "0.20";
        public String getVersion(){
            return version;
        }

    }

    public class doWork
    {
        byte[] cert;
        private String certpass = null;
        private String host = null;
        private String ctrl_port = "8082";
        private String apps_host = null;
        private String apps_port = "8282";
        private string client = "whiteandcaselink";
        private string region = "Default";
        String[] session = null;
        String[] currentRoot = null;

        //this one is used by the CLI - since we can keep the session[] in memory while the program runs
        public doWork(string theRegion, string theClient, byte[] certificate, string certPassword, string h, string port, string appsh, string appsport, string user, string pass)
        {
            if (theClient != null)
                client = theClient;
            if (theRegion != null)
                region = theRegion;
            
            cert = certificate;
            certpass = certPassword;
            host = h;
            apps_host = appsh;
            ctrl_port = port;
            apps_port = appsport;
            session = getSession(user, pass);
        }

        // this one is used by the webapp - it needs to provide session[] each time the page reloads
        public doWork(string[] sess, byte[] certificate, string certPassword, string h, string port, string appsh, string appsport, string user, string pass)
        {
            cert = certificate;
            certpass = certPassword;
            host = h;
            ctrl_port = port;
            apps_host = appsh;
            apps_port = appsport;
            session = sess;
        }

        public doWork()
        {
            //cert will stay null and will throw an exception in session..
            // shouldn't get here as cert path is a required parameter in Options.
        }

        public string getFileProperties(string sLocation, string sDocID, string sVersionNbr)
        {
            string sRetVal = string.Empty;

            try
            {
                String uri = "https://" + apps_host + ":" + apps_port + "/clientws/files/getfileproperties";
                
                /*
                string sPost = "{";
                sPost += "\"digest\": \"" + sLocation + "\",";
                sPost += "\"id\": \"" + sDocID + "\",";
                sPost += "\"version\": \"" + sVersionNbr + "\",";
                sPost += "\"targetIsFile\": \"true\"";
                sPost += "}";
                */

                String sPost = "id=" + Uri.EscapeDataString(sDocID) +
                "&digest=" + Uri.EscapeDataString(sLocation) +
                "&version=" + Uri.EscapeDataString(sVersionNbr);
                //"&targetIsFile=" + Uri.EscapeDataString(true);

                List<string> lstCookies = new List<string>();
                //lstCookies.Add(string.Format("\"{0}\"=\"{1}\"", "MH331", session[1]));      //App ID
                //lstCookies.Add(string.Format("\"{0}\"=\"{1}\"", "MH333", session[0]));      //Session ID
                lstCookies.Add("MH331=" + session[1]);
                lstCookies.Add("MH333=" + session[0]); 

                HttpWebResponse Response = doPOST(uri, sPost, lstCookies);

                //The following code is not run due to the error encountered within the doPost method...
                if (Response != null)
                {
                    StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                    JavaScriptSerializer js = new JavaScriptSerializer();

                    string sRetData = sr.ReadToEnd();

                    var oDictionary = js.Deserialize<Dictionary<string, dynamic>>(sRetData);

                    oDictionary = oDictionary;
                }
            }
            catch (Exception oEX)
            {
                //General.LogMsg(oEX);

                string sMSG = oEX.Message + Environment.NewLine + Environment.NewLine + oEX.StackTrace;
                sMSG = sMSG;
            }

            return sRetVal;
        }

        private HttpWebResponse doPOST( String uri, String postString, List<string> lstCookies )
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);

                if (lstCookies != null && lstCookies.Count > 0)
                {
                    String cookies = "";
                    foreach (string sCookie in lstCookies)
                    {
                        cookies += sCookie + "; ";
                    }
                    Request.Headers.Add(HttpRequestHeader.Cookie, cookies);
                }
 


                Request.ClientCertificates.Add(new X509Certificate2(cert, certpass));
                Request.UserAgent = "Client Cert Sample";
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = postString.Length;
                Request.Accept = "application/json";
                System.Net.ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                StreamWriter requestWriter = new StreamWriter(Request.GetRequestStream());
                requestWriter.Write(postString);
                requestWriter.Close();

                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                return Response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private HttpWebResponse doPOST(String uri, String postString)
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);
                Request.ClientCertificates.Add(new X509Certificate2(cert, certpass));
                Request.UserAgent = "Client Cert Sample";
                Request.Method = "POST";
                Request.ContentType = "application/json";
                Request.ContentLength = postString.Length;
                Request.Accept = "application/json";
                System.Net.ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                StreamWriter requestWriter = new StreamWriter(Request.GetRequestStream());
                requestWriter.Write(postString);
                requestWriter.Close();

                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                return Response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private HttpWebResponse doGET(String uri)
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);
                Request.ClientCertificates.Add(new X509Certificate2(cert, certpass));
                Request.UserAgent = "Client Cert Sample";
                Request.Method = "GET";
                Request.ContentType = "application/json";
                Request.Accept = "application/json,application/pdf";
                System.Net.ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
                //StreamWriter requestWriter = new StreamWriter(Request.GetRequestStream());
                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                return Response;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        // will return the sessid (Session id) and the first instance of a FileBox (type 8) app
        public String[] getSession( string user, string password)
        {
            String[] retVal;
            int retry = 3;
            while (retry > 0)
            {
                Console.WriteLine("Trying to establish session.");
                try
                {

                    string postString = "{" +
                            "\"deviceRegion\" : \"" + region + "\"," +
                            "\"client\" : \"" + client + "\"," +
                            "\"selfRegisterOnFailure\" : true ," +
                            "\"userID\" : \"" + user + "\"," +
                            "\"password\" : \"" + password + "\"}";


                    String uri = "https://" + host + ":" + ctrl_port + "/ws/restricted/session";
                    Console.WriteLine("sending session request: " + uri);
                    HttpWebResponse Response = doPOST(uri, postString);
                    
                    StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    String output = sr.ReadToEnd();
                    
                    Console.WriteLine("Output from session: " + output);
                    
                    var dict = js.Deserialize<Dictionary<string, dynamic>>(output);
                    if (dict == null)
                    {
                        Console.WriteLine("Session creation returned null. Session create failed.");
                        return null;
                    }
                    var status = dict["msg"];
                    if (String.Compare("success", status, true) == 0)
                    {
                        retVal = new String[2];
                        retVal[0] = dict["sessionID"];
                        Console.WriteLine("Session id: " + retVal[0]);
                        if ( dict["apps"] == null)
                        {
                            Console.WriteLine("There are no apps configured. Session create failed.");
                            return null;
                        }

                        try { 
                            // if there is only 1 app:
                            Dictionary<string, dynamic> item = dict["apps"];
                            Console.WriteLine(" Processing app of type: " + Convert.ToInt32(item["appType"]));
                            // find the first app that is of type 8 (file box type)
                            if (Convert.ToInt32(item["appType"]) == 8)
                            {
                                retVal[1] = item["uniqueID"]; //unique ID of the app we will use to access WorkSite
                                return retVal;
                            }
                        } catch ( Exception e){
                            foreach (Dictionary<string, dynamic> app in dict["apps"])
                            {
                                Console.WriteLine(" Processing app of type: " + Convert.ToInt32(app["appType"]));
                                // find the first app that is of type 8 (file box type)
                                if (Convert.ToInt32(app["appType"]) == 8)
                                {
                                    retVal[1] = app["uniqueID"]; //unique ID of the app we will use to access WorkSite
                                    break;
                                }
                            }
                        }
                        return retVal;
                    } else {
                        Console.WriteLine( "Session start failed: " + status);
                        retry--;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Sorry. There is a problem with the app configurations in Mobile Helix App Server.");
                    return null;
                }
            }
            return null; //3 retries failed!
        }

        //this returns 1 or more roots - each root is a String[2] with the human readable name in [0] and uniqueid in [1]
        public List<String[]> getRoots( string[] ses )
        {
            if (ses != null)
            {
                session = ses;
            }
            Console.WriteLine("Trying to get Roots for session " + session[0]);
            try
            {

                String uri = "https://" + apps_host + ":" + apps_port + "/clientws/files/getroots?appid=" + 
                    Uri.EscapeDataString(session[1]) + "&sessionid=" + Uri.EscapeDataString(session[0]);

                HttpWebResponse Response = doGET(uri);
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                JavaScriptSerializer js = new JavaScriptSerializer();

                var ret = sr.ReadToEnd();
                
                Console.WriteLine(ret);

                //this is the current directory listing contents
                var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
                if (dict == null)
                    return null; //error!!

                int counter = 0;
                List<String[]> theRoots = new List<String[]>();

                //ok - are we getting back multiple roots? If yes, there will be a "roots" key
                if (dict.ContainsKey("roots"))
                {
                    //we need to print all roots - as long as they have a prop.containsKey["26"] - that's the unique resource ID we need
                    //this code works for multiple resources (Array vs singleton)
                    foreach (Dictionary<string, dynamic> r in dict["roots"]){
                        String[] s = new String[2];
                        var props = js.Deserialize<Dictionary<string, dynamic>>(r["props"]);
                        if (props.ContainsKey("26")){
                            s[0] = r["digest"];
                            s[1] = props["26"];
                            theRoots.Add(s);
                        }

                    }
                }
                else
                {
                    Console.WriteLine("no roots elements");
                    return null;
                }

                return theRoots;
            }
            catch (Exception e)
            {
                //this code works when there is exactly 1 resource (so singleton instead of array)
                try
                {
                    String uri = "https://" + apps_host + ":" + apps_port + "/clientws/files/getroots?appid=" + 
                    Uri.EscapeDataString(session[1]) + "&sessionid=" + Uri.EscapeDataString(session[0]);

                    HttpWebResponse Response = doGET(uri);
                    StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                    JavaScriptSerializer js = new JavaScriptSerializer();

                    var ret = sr.ReadToEnd();
                
                    Console.WriteLine(ret);

                    //this is the current directory listing contents
                    var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
                    if (dict == null)
                        return null; //error!!

                    int counter = 0;
                    List<String[]> theRoots = new List<String[]>();

                    //ok - are we getting back multiple roots? If yes, there will be a "roots" key
                    if (dict.ContainsKey("roots"))
                    {
                        String[] s = new String[2];
                        s[0] = null;
                        s[1] = null;

                        foreach (KeyValuePair<string, object> r in dict["roots"])
                        {
                            if (r.Key == "digest")
                            {
                                s[0] = r.Value.ToString();
                            }
                            if (r.Key == "props")
                            {
                                String x = r.Value.ToString();

                                var props = js.Deserialize<Dictionary<string, dynamic>>(r.Value.ToString());
                                if (props.ContainsKey("26"))
                                {
                                    s[1] = props["26"];
                                    if (s[0] != null)
                                    {
                                        theRoots.Add(s);
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("no roots elements");
                        return null;
                    }

                    return theRoots;
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                    return null;
                }
            }

        }

        
        public Stream getSyncdir(string[] session, string digest, string target)
        {
            if (target == null)
                target = "ROOT";
            try
            {

                String uri =
                    "https://" + apps_host + ":" + apps_port + 
                    "/clientws/files/syncdir?appid=" + Uri.EscapeDataString(session[1]) + 
                    "&digest=" + Uri.EscapeDataString(digest) + 
                    "&sessionid=" + Uri.EscapeDataString(session[0]) + 
                    "&target=" + Uri.EscapeDataString(target);

                HttpWebResponse Response = doGET(uri);
                return Response.GetResponseStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public String getFilename(string docid, string location)
        {
            if (session != null && session.Length == 2)
            {
                if (
                    location == null ||
                    location.Length > 0
                   )
                {
                    location = "ROOT";
                }
                try
                {
                    String uri = "https://" + apps_host + ":" + apps_port +
                               "/clientws/files/getfileinfo?appid=" +
                               Uri.EscapeDataString(session[1]) +
                               "&digest=" + Uri.EscapeDataString(location) +
                               "&id=" + Uri.EscapeDataString(docid) +
                               "&sessionid=" + Uri.EscapeDataString(session[0]);

                    HttpWebResponse Response = doGET(uri);
                    StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var ret = sr.ReadToEnd();
                    var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
                    var status = dict["msg"];
                    if (String.Compare("success", status, true) == 0)
                    {
                        String name = dict["file"]["displayName"];
                        return name;
                    }
                    else
                        return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }
            return null;
        }

        private Stream getPDF(string docid, string filename, string location)
        {
            if (session != null && session.Length == 2)
            {
                try
                {
                    String uri;
                    

                    if  (
                            filename != null &&
                            filename.Length > 0
                        )
                    {
                        uri = "https://" + apps_host + ":" + apps_port +
                            "/clientws/files/getfileview?appid=" +
                            Uri.EscapeDataString(session[1]) +
                            "&digest=" + Uri.EscapeDataString(location) +
                            "&filename=" + Uri.EscapeDataString(filename) +
                            "&id=" + Uri.EscapeDataString(docid) +
                            "&sessionid=" + Uri.EscapeDataString(session[0]);
                    }
                    else
                    {
                        uri = "https://" + apps_host + ":" + apps_port +
                            "/clientws/files/getfileview?appid=" +
                            Uri.EscapeDataString(session[1]) +
                            "&digest=" + Uri.EscapeDataString(location) +
                            "&id=" + Uri.EscapeDataString(docid) +
                            "&sessionid=" + Uri.EscapeDataString(session[0]);
                    }
    

                    HttpWebResponse Response = doGET(uri);
                    // Print the repsonse headers.
                    //Console.WriteLine("{0}", Response.Headers);
                    //Console.WriteLine();
                    // Get the certificate data.
                    return Response.GetResponseStream();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public Stream GetDocId(string docid, string filename, string location)
        {
           
            /*
            digest - Digest of the parent directory containing the file you want to view. "ROOT" if this file is in the root directory of the mobilized file system. This is the "parentDigest" field returned in by the syncdir service.
            id - Unique ID of the file. This is the globalKey returned by the syncdir service.
            filename - Display name of the file. We use this for content type inference if we need it (i.e. by looking at the file extension). This is the displayName property returned from the syncdir service.
            sessionid - Base-64 encoded unique ID of the user session on the app server.
            appid - Numeric ID of the Controller app that mobilizes the file system you want to query.
            */
            if (session != null && session.Length == 2)
            {
                // if filename is blank, need to find it
                if (
                    location == null 
                   )
                {
                    location = "ROOT";
                }

                return getPDF(docid, filename, location);
            }
            return null;
        }

        // you must call this function to list the contents of a selected root before calling GetListing
        // when there is more than 1 Resource in 1 app
        public String GetRootListing(String[] root)
        {
            currentRoot = null; //start over!

            var sync = getSyncdir(session, root[0], root[1]);
            StreamReader sr = new StreamReader(sync, Encoding.Default);

            var ret = sr.ReadToEnd();

            Console.WriteLine(ret);
            JavaScriptSerializer js = new JavaScriptSerializer();
            var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
            var status = dict["msg"];
            if (String.Compare("success", status, true) == 0)
            {
                currentRoot = root;
                return ret;
            }
            return "error: " + dict["msg"];

        }

        //call this when exiting back to the root listing menu (when there are multiple resources in one app)
        public void clearCurrentRoot()
        {
            
        }

        /* This is the starting point for listing contents/directories. 
           Parameters: root (previously obtained by calling getRoots) and digest (location to list)
        */
        public String GetListing(string root, string digest)
        {
            if (root == null || root == "")
                root = "ROOT";
            if (digest == null || digest == "")
                digest = "ROOT";
            Console.WriteLine("root: " + root + " | digest: " + digest);
            JavaScriptSerializer js = new JavaScriptSerializer();
            
            if (session != null && session.Length == 2)
            {
                if (currentRoot != null)
                {
                    root = currentRoot[0]; //this will either be the global currentRoot set by GetRootListing or the one returned from the getroots call
                }
                Console.WriteLine("root = " + root);

                var sync = getSyncdir(session, root, digest);
                StreamReader sr = new StreamReader(sync, Encoding.Default);
                    
                var ret = sr.ReadToEnd();

                Console.WriteLine(ret);
                    
                var dict1 = js.Deserialize<Dictionary<string, dynamic>>(ret);
                var status1 = dict1["msg"];
                if (String.Compare("success", status1, true) == 0)
                {
                    return ret;
                }
                return "error: " + dict1["msg"];
            }
            else
            {
                Console.WriteLine("ERROR: Session creation failed.");
                return "error: Session creation failed.";
            }
        }
    }

    //Implement the ICertificatePolicy interface.
    class CertPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint,
    X509Certificate certificate, WebRequest request, int certificateProblem)
        {
            // You can do your own certificate checking.
            // You can obtain the error values from WinError.h.

            // Return true so that any certificate will work with this sample.
            return true;
        }
    }


    sealed class AcceptAllCertificatePolicy : ICertificatePolicy
    {
        private const uint CERT_E_UNTRUSTEDROOT = 0x800B0109;

        public bool CheckValidationResult(ServicePoint srvPoint, X509Certificate
        certificate, WebRequest request, int certificateProblem)
        {
            // Just accept.
            return true;
        }
    }
}
