using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MobileHelixUtility;
using System.IO;
using Newtonsoft.Json;
using WebApplication1.Models;
using System.Text;
using Newtonsoft.Json.Linq;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public FileContentResult getDocument(string docid, string fileName, string parentDigest)
        {
            if (Request.Cookies["mobilehelix1"] != null)
            {
                String[] session = new String[2];
                session[0] = Request.Cookies["mobilehelix1"]["sessid"];
                session[1] = Request.Cookies["mobilehelix1"]["appid"];
                byte[] cert = (byte[])(Session["certificate"]);
                String certpass = Request.Cookies["mobilehelix1"]["certpass"];
                string h = Request.Cookies["mobilehelix1"]["host"];
                string p = Request.Cookies["mobilehelix1"]["port"];
                string u = Request.Cookies["mobilehelix1"]["userName"];
                string pass = Request.Cookies["mobilehelix1"]["password"];

                doWork work = new doWork(
                    session,
                    cert,
                    certpass,
                    h,
                    p,
                    u,
                    pass
                );
                Stream docStream = work.GetDocId(docid, fileName, parentDigest);
                byte[] thePDF;
                using (var streamReader = new MemoryStream())
                {
                    docStream.CopyTo(streamReader);
                    thePDF = streamReader.ToArray();
                }

                string mimeType = "application/pdf";
                Response.AppendHeader("Content-Disposition", "inline; filename=" + fileName);
                return File(thePDF, mimeType);
            }
            return null;
        } 

        public ActionResult getListings(string where)
        {
            if (Request.Cookies["mobilehelix1"] == null)
            {
                return RedirectToAction("list", "home"); //no cookies means need to start at the beginning
            }
            else
            {
                String[] session = new String[2];
                session[0] = Request.Cookies["mobilehelix1"]["sessid"];
                session[1] = Request.Cookies["mobilehelix1"]["appid"];
                byte[] cert = (byte[])(Session["certificate"]);
                String certpass = Request.Cookies["mobilehelix1"]["certpass"];
                string h = Request.Cookies["mobilehelix1"]["host"];
                string p = Request.Cookies["mobilehelix1"]["port"];
                string u = Request.Cookies["mobilehelix1"]["userName"];
                string pass = Request.Cookies["mobilehelix1"]["password"];

                if (where == null)
                    where = "ROOT";

                doWork work = new doWork(
                    session, 
                    cert,
                    certpass,
                    h,
                    p,
                    u,
                    pass
                );
                Stream sync = work.getSyncdir(session, Request.Cookies["mobilehelix1"]["rootid"], where);
                if ( sync == null)
                    return RedirectToAction("list", "home"); //no sync means we need to start at the beginning

                StreamReader sr = new StreamReader(sync, Encoding.Default);
                var ret = sr.ReadToEnd();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>( ret );
                mobileHelixItems myItem;
                List< mobileHelixItems > items = new List<mobileHelixItems>();
                foreach (dynamic item in dict["changes"]["adds"])
                {
                    myItem = new mobileHelixItems();
                    myItem.displayName = item.displayName;
                    myItem.globalKey = item.globalKey;
                    myItem.parentDigest = item.parentDigest;
                    myItem.isFile = item.isFile;
                    myItem.current = where;
                    items.Add(myItem);
                }
                
                return View(items);
                
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.Hello = "Aaron";
            return View();
        }

        public ActionResult showListings()
        {

            return View();
        }

        public ActionResult list(HttpPostedFileBase file, string certpassword, string h, string port, string user, string pass)
        {
            Console.WriteLine( "certpassword: " + certpassword + " h: " + h + " port: " + port + " user: " + user + " pass: " + pass);
            if (file != null && file.ContentLength > 0) 
            {
                try
                {
                    BinaryReader b = new BinaryReader(file.InputStream);
                    byte[] cert = b.ReadBytes( (int) file.InputStream.Length);
                    Session["certificate"] = cert;
                    doWork work = new doWork(cert, certpassword, h, port, user, pass);
                    String[] session = work.getSession( user, pass);

                    HttpCookie cookie = new HttpCookie("mobilehelix1");
                    cookie.Values.Add("userName", user);
                    cookie.Values.Add("password", pass);
                    cookie.Values.Add("sessid", session[0]);
                    cookie.Values.Add("appid", session[1]);
                    cookie.Values.Add("host", h );
                    cookie.Values.Add("port", port);

                    string theCert = Encoding.UTF8.GetString(cert);
                    cookie.Values.Add("cert", theCert);
                    cookie.Values.Add("certpass", certpassword);

                    
                    String[] roots = work.getRoots(session);
                    cookie.Values.Add("rootid", roots[0]);
                    Response.Cookies.Add(cookie);
                    ViewBag.Message = "redirecting...";
                    return View();
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();  
        }
        public JsonResult lookAtMe()
        {
            //doWork(string certificate, string certPassword, string h, string port, string user, string pass )
            var work = new doTests();
            char[] test = work.JSONtest();
            return Json(new { Count = test.Length, RetVal = test }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}