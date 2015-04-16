using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http; // if you want text formatting helpers (recommended)
using System.Security.Cryptography.X509Certificates;
using System.IO;


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
                //You must change the path to point to your .cer file location. 
                X509Certificate Cert = X509Certificate2.CreateFromCertFile("d:\\mobile helix\\cert.cer");
                // Handle any certificate errors on the certificate from the server.
                ServicePointManager.CertificatePolicy = new CertPolicy();
                // You must change the URL to point to your Web server.
                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://192.168.1.113:8082/ws/restricted/registerserver");
                Request.ClientCertificates.Add(new X509Certificate2( @"D:\Mobile Helix\mycert.p12", "coverity"));

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
    }

    public class doWork
    {
        private String cert = null;

        public doWork( string c ){
            cert = c;
        }

        public doWork(){
            //cert will stay null and will throw an exception in session..
            // shouldn't get here as cert path is a required parameter in Options.
        }
        public void GetNrl( string host, string port ){
            session(host, port, "ws/restricted/registerserver" );
        }

        public void GetDocId(string host, string port, string username, string password, string docid)
        {
            session(host, port, " ");
        }

        public void GetListing(string host, string port, string username, string password)
        {
            session(host, port, " ");
        }

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