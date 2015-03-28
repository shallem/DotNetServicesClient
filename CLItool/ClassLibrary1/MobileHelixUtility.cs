using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http; // if you want text formatting helpers (recommended)


namespace MobileHelixUtility
{
    // just for getting started
    public class Class1
    {
        private int foo = 5;
        public int getFoo(){
            return foo;
        }

    }

    public class doNRL
    {
        public void go()
        {
            RunAsync().Wait();
        }
        static async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://timely-feedback.com/portal/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("users/logout");
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

    public class doDocId
    {


    }

    public class doFetch
    {


    }


}
