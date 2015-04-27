using System;
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
        private String version = "0.15";
        public String getVersion(){
            return version;
        }

    }


    public class doTests
    {




        public void test1()
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://192.168.1.113:8082/ws/restricted/registerserver");
                Request.ClientCertificates.Add(new X509Certificate2( @"e:\mobile helix\mycert.p12", "coverity"));

                Request.UserAgent = "Client Cert Sample";
                Request.Method = "GET";
                System.Net.ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();

                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                // Print the repsonse headers.
                Console.WriteLine("{0}", Response.Headers);
                Console.WriteLine();
                // Get the certificate data.
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                int count;
                char[] ReadBuf = new char[1024];
                do
                {
                    count = sr.Read(ReadBuf, 0, 1024);
                    if (0 != count)
                    {
                        Console.WriteLine(new string(ReadBuf));
                    }

                } while (count > 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public char[] JSONtest()
        {
            try
            {
                //You must change the path to point to your .cer file location. 
                X509Certificate Cert = X509Certificate2.CreateFromCertFile("c:\\mobile helix\\cert.cer");
                // Handle any certificate errors on the certificate from the server.
                ServicePointManager.CertificatePolicy = new CertPolicy();
                // You must change the URL to point to your Web server.
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://192.168.1.113:8082/ws/restricted/registerserver");
                Request.ClientCertificates.Add(new X509Certificate2(@"C:\Mobile Helix\mycert.p12", "coverity"));

                Request.UserAgent = "Client Cert Sample";
                Request.Method = "GET";
                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
                // Print the repsonse headers.
                Console.WriteLine("{0}", Response.Headers);
                Console.WriteLine();
                // Get the certificate data.
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                int count;
                char[] ReadBuf = new char[1024];
                sr.Read(ReadBuf, 0, 1024);
                return ReadBuf;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

    }

    public class doWork
    {
        private String cert = null;
        private String certpass = null;
        private String host = null;
        private String ctrl_port = null;
        private String apps_port = null;

        public doWork(string c, string cp, string h, string p)
        {
            cert = c;
            certpass = cp;
            host = h;
            ctrl_port = p;
            apps_port = (Int32.Parse(p) + 200).ToString();

        }

        public doWork()
        {
            //cert will stay null and will throw an exception in session..
            // shouldn't get here as cert path is a required parameter in Options.
        }

        private HttpWebResponse doPOST( String uri, String postString)
        {
            try
            {
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(uri);
                Request.ClientCertificates.Add(new X509Certificate2(@cert, certpass));
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
                Request.ClientCertificates.Add(new X509Certificate2(@cert, certpass));
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

        private String[] getSession( string user, string password)
        {
            String[] retVal;
            int retry = 3;
            while (retry > 0)
            {
                Console.WriteLine("Trying to establish session.");
                try
                {
                    string postString = "{" +
                            "\"deviceRegion\" : \"Default\"," +
                            "\"client\" : \"mobilehelixpoc\"," +
                            "\"userID\" : \"" + user + "\"," +
                            "\"password\" : \"" + password + "\"}";

                    String uri = "https://" + host + ":" + ctrl_port + "/ws/restricted/session";
                    HttpWebResponse Response = doPOST(uri, postString);
                    // Print the repsonse headers.
                    //Console.WriteLine("{0}", Response.Headers);
                    //Console.WriteLine();
                    // Get the certificate data.
                    StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    //var obj = js.Deserialize<dynamic>(sr.ReadToEnd());
                    var dict = js.Deserialize<Dictionary<string, dynamic>>(sr.ReadToEnd());
                    var status = dict["msg"];
                    if (String.Compare("success", status, true) == 0)
                    {
                        retVal = new String[2];
                        retVal[0] = dict["sessionID"];
                        Console.WriteLine("Session id: " + retVal[0]);
                        foreach (Dictionary<string, dynamic> app in dict["apps"])
                        {
                            if (Convert.ToInt32(app["appType"]) == 8)
                            {
                                retVal[1] = app["uniqueID"];
                                break;
                            }
                        }
                        return retVal;
                    } else {
                        retry--;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
            }
            return null; //3 retries failed!
        }

        private String[] getRoots( string[] session )
        {
            String[] retVal;
            Console.WriteLine("Trying to get Roots for session " + session[0]);
            try
            {

                String uri = "https://" + host + ":" + apps_port + "/clientws/files/getroots?appid=" + 
                    Uri.EscapeDataString(session[1]) + "&sessionid=" + Uri.EscapeDataString(session[0]);

                HttpWebResponse Response = doGET(uri);
                // Print the repsonse headers.
                //Console.WriteLine("{0}", Response.Headers);
                //Console.WriteLine();
                // Get the certificate data.
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                JavaScriptSerializer js = new JavaScriptSerializer();
                //var obj = js.Deserialize<dynamic>(sr.ReadToEnd());
                var ret = sr.ReadToEnd();
                var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
                var status = dict["msg"];
                if (String.Compare("success", status, true) == 0)
                {
                    //for now - assume only 1 file resource
                    retVal = new String[1];
                    retVal[0] = dict["roots"]["digest"];
                    return retVal;
                } else 
                    return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        private System.Collections.ArrayList getSyncdir(string[] session, string digest, string target)
        {
            String[] retVal;
            try
            {

                String uri =
                    "https://" + host + ":" + apps_port + 
                    "/clientws/files/syncdir?appid=" + Uri.EscapeDataString(session[1]) + 
                    "&digest=" + Uri.EscapeDataString(digest) + 
                    "&sessionid=" + Uri.EscapeDataString(session[0]) + 
                    "&target=" + Uri.EscapeDataString(target);

                HttpWebResponse Response = doGET(uri);
                // Print the repsonse headers.
                //Console.WriteLine("{0}", Response.Headers);
                //Console.WriteLine();
                // Get the certificate data.
                StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
                JavaScriptSerializer js = new JavaScriptSerializer();
                //var obj = js.Deserialize<dynamic>(sr.ReadToEnd());
                var ret = sr.ReadToEnd();
                var dict = js.Deserialize<Dictionary<string, dynamic>>(ret);
                var status = dict["msg"];
                if (String.Compare("success", status, true) == 0)
                {
                    /*
                    Dictionary<string, dynamic> t = dict["roots"];
                    int count = t.Count();
                    retVal = new String[count];
                    */

                    System.Collections.ArrayList t = dict["changes"]["adds"];
                    return t;
                    
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        private Stream getPDF(string[] session, string docid, string filename, string location)
        {
            if (session.Length == 2)
            {
                try
                {

                    string uri = "https://" + host + ":" + apps_port +
                        "/clientws/files/getfileview?appid=" + 
                        Uri.EscapeDataString(session[1]) +
                        "&digest=" + Uri.EscapeDataString(location) +
                        //"&filename=" + WebUtility.UrlEncode(filename) +
                        "&filename=" + Uri.EscapeDataString(filename) +
                        "&id=" + Uri.EscapeDataString(docid) +
                        "&sessionid=" + Uri.EscapeDataString(session[0]);
                        

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

        public void GetNrl( string host, string port ){
            //session(host, port, "ws/restricted/registerserver" );
        }

        public void GetDocId(string username, string password, string docid, string filename, string location)
        {
           
            /*
            digest - Digest of the parent directory containing the file you want to view. "ROOT" if this file is in the root directory of the mobilized file system. This is the "parentDigest" field returned in by the syncdir service.
            id - Unique ID of the file. This is the globalKey returned by the syncdir service.
            filename - Display name of the file. We use this for content type inference if we need it (i.e. by looking at the file extension). This is the displayName property returned from the syncdir service.
            sessionid - Base-64 encoded unique ID of the user session on the app server.
            appid - Numeric ID of the Controller app that mobilizes the file system you want to query.
            */
            String[] session = getSession(username, password);
            if (session != null && session.Length == 2)
            {
               // var o = 
            }
        }

        public void GetListing(string username, string password)
        {
            String[] session = getSession( username, password );
            String[] globalKeys = null;
            String[] whereIsWaldo = new String[99];
            int waldoCounter = 0;
            String[] root = null;
            if (session != null && session.Length == 2)
            {
                root = getRoots( session );
                if (root != null)
                {
                    var list = getSyncdir(session, root[0], root[0]);
                    if (list != null)
                    {
                        try
                        {
                            int counter = 1;
                            globalKeys = new String[list.Count];
                            whereIsWaldo[ waldoCounter ] = root[0]; //starting point!
                            foreach (Dictionary<string, dynamic> item in list)
                            {
                                if (String.Compare("true", item["isFile"], true) == 0)
                                {
                                    Console.WriteLine("name: " + item["displayName"]);
                                    Console.WriteLine("id" + item["globalKey"]);
                                    Console.WriteLine("location" + item["parentDigest"]);
                                }
                                else
                                {
                                    Console.WriteLine("item: " + counter + " folder: " + item["globalKey"]);
                                    Console.WriteLine("name: " + item["displayName"]);
                                    globalKeys[counter-1] = item["globalKey"];
                                    counter++;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("Unable to display items in this folder.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to list contents.");
                    }
                } else 
                {
                    Console.WriteLine("Unable to get filesystem roots.");
                }

            }
            else
            {
                Console.WriteLine("Session creation failed.");
            }
            string wait = "";
            while ( String.Compare( "x", wait, true ) != 0 )
            {
                wait = Console.ReadLine();
                if (String.Compare("x", wait, true) == 0)
                    return;

                int res;
                if (Int32.TryParse( wait, out res) == true)
                {
                    //record changes
                    if (res > 0)
                    {
                        Console.WriteLine("item: 0 folder: " + whereIsWaldo[waldoCounter]);
                        waldoCounter++;
                        whereIsWaldo[waldoCounter] = globalKeys[res-1];
                    }
                    //record where we are before going 1 level down
                    //globalKeys[0] = g
                    if (root == null || globalKeys == null)
                    {
                        Console.WriteLine("either root or globalKeys are null");
                    }

                    String gohere = null;
                    if (res == 0)
                    {
                        waldoCounter--;
                        gohere = whereIsWaldo[waldoCounter-1];
                        if (waldoCounter > 0)
                            Console.WriteLine("item: 0 folder: " + whereIsWaldo[waldoCounter-1]);
                    }
                    else
                        gohere = globalKeys[res-1];

                    var list = getSyncdir(session, root[0], gohere);
                    if (list != null)
                    {
                        try
                        {
                            int counter = 1;
                            
                            globalKeys = new String[list.Count];
                            
                            foreach (Dictionary<string, dynamic> item in list)
                            {
                                if (String.Compare("true", item["isFile"], true) == 0)
                                {
                                    Console.WriteLine("name: " + item["displayName"]);
                                    Console.WriteLine("id: " + item["globalKey"]);
                                    Console.WriteLine("location: " + item["parentDigest"]);
                                    string FILE_PATH = "E:\\Mobile Helix\\Visual Studio Projects\\" + item["displayName"];
                                    using (var writeStream = File.OpenWrite(FILE_PATH))
                                    {
                                        getPDF(session, item["globalKey"], item["displayName"], item["parentDigest"]).CopyTo(writeStream);
                                        writeStream.Close();
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("item: " + counter + " folder: " + item["globalKey"]);
                                    Console.WriteLine("name: " + item["displayName"]);
                                    globalKeys[counter-1] = item["globalKey"];
                                    counter++;
                                }
                            }
                            Console.WriteLine("");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine("Unable to display items in this folder.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to list contents.");
                    }
                }
            }
            return;
        }


        /*
        private void session( string host, string port, string command)
        {
            // Obtain the certificate. 
            try
            {
	            //You must change the path to point to your .cer file location. 
                //"d:\\mobile helix\\cert1.cer"
                X509Certificate Cert = X509Certificate.CreateFromCertFile( cert );
	            // Handle any certificate errors on the certificate from the server.
	            ServicePointManager.CertificatePolicy = new CertPolicy();
	            
	            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(host+ ":" + port + "/" + command);
	            Request.ClientCertificates.Add(Cert);
	            Request.UserAgent = "Client Cert Sample";
	            Request.Method = "GET";
	            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
	            // Print the repsonse headers.
	            Console.WriteLine("{0}",Response.Headers);
	            Console.WriteLine();
	            // Get the certificate data.
	            StreamReader sr = new StreamReader(Response.GetResponseStream(), Encoding.Default);
	            int count;
	            char [] ReadBuf = new char[1024];
	            do
	            {
		            count = sr.Read(ReadBuf, 0, 1024);
		            if (0 != count)
		            {
			            Console.WriteLine(new string(ReadBuf));
		            }
						
	            }while(count > 0);
            }
            catch(Exception e)
            {
	            Console.WriteLine(e.Message);
            }
        }
        
        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://192.168.1.113:1247/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("__mh__901__/ping");
                if (response.IsSuccessStatusCode)
                {
                    string product = await response.Content.ReadAsAsync<string>();
                    Console.WriteLine(product);
                }
                /*
                // HTTP POST
                var gizmo = new Product() { Name = "Gizmo", Price = 100, Category = "Widget" };
                response = await client.PostAsJsonAsync("api/products", gizmo);
                if (response.IsSuccessStatusCode)
                {
                    Uri gizmoUrl = response.Headers.Location;

                    // HTTP PUT
                    gizmo.Price = 80;   // Update price
                    response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

                    // HTTP DELETE
                    response = await client.DeleteAsync(gizmoUrl);
                }
                 */
        /*
            }
        }
        */

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
        /*public bool CheckValidationResult(ServicePoint sp,
        X509Certificate cert, WebRequest req, int problem)
        {
            return true;  
        }*/
    }
}
/*
HttpWebRequest requestC = (HttpWebRequest)WebRequest.Create("http://192.168.1.113:8082/ws/restricted/registerserver");
                X509Certificate cert1 = X509Certificate.CreateFromCertFile("cert.cer");
                requestC.ClientCertificates.Add(cert1);

                HttpWebResponse myHttpWebResponse = (HttpWebResponse)requestC.GetResponse();
                // Gets the stream associated with the response.
                Stream receiveStream = myHttpWebResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, encode);
                Console.WriteLine("\r\nResponse stream received.");
                Char[] read = new Char[256];
                // Reads 256 characters at a time.     
                int count = readStream.Read(read, 0, 256);
                Console.WriteLine("HTML...\r\n");
                while (count > 0)
                {
                    // Dumps the 256 characters on a string and displays the string to the console.
                    String str = new String(read, 0, count);
                    Console.Write(str);
                    count = readStream.Read(read, 0, 256);
                }
                Console.WriteLine("");
                // Releases the resources of the response.
                myHttpWebResponse.Close();
                // Releases the resources of the Stream.
                readStream.Close();

*/