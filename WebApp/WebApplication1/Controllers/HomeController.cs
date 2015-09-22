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
        private doWork work = null;
        private String[] session = null;
       
        private bool initFunction(){
            try
            {
                session = new String[2];
                session[0] = (string)(Session["sessid"]);
                session[1] = (string)(Session["appid"]);
                byte[] cert = (byte[])(Session["certificate"]);
                String certpass = (string)(Session["certpass"]);
                string h = (string)(Session["host"]);
                string p = (string)(Session["port"]);
                string a = (string)(Session["apps_host"]);
                string s = (string)(Session["apps_port"]);
                string u = (string)(Session["userName"]);
                string pass = (string)(Session["password"]);

                if (h != null && a != null)
                {
                    work = new doWork(
                        session,
                        cert,
                        certpass,
                        h,
                        p,
                        a,
                        s,
                        u,
                        pass
                    );
                    return true;
                }
                else
                {
                    return false;
                }
            } catch(Exception e){
                return false;
            }

        }

        public FileContentResult getDocument(string docid, string fileName, string parentDigest )
        {
            if ( initFunction() == false ){
                 //init will fail if the proper values are not in the session
                return null;
            }
            try{

                // when pulling up doc by docid or NRL
                if (parentDigest == null)
                    parentDigest = (string)(Session["rootid"]);

                if ( fileName == null)
                    fileName = work.getFilename(docid, null);
                
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
            catch (Exception e)
            {
                ViewBag.Message = "Error fetching file." + e.Message;
                return null;
            }
        }

        public ActionResult nrl(HttpPostedFileBase file)
        {
            if (initFunction() == false)
            {
                return RedirectToAction("init", "home", null);
            }
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    string nrlContents;
                    using (StreamReader inputStreamReader = new StreamReader(file.InputStream))
                    {
                        // make sure bogus files don't get too far
                        int counter = 10;
                        while ((nrlContents = inputStreamReader.ReadLine()) != null && counter > 0)
                        {
                            counter--;
                            if ( nrlContents.StartsWith("!nrtdms") == true )
                                return RedirectToAction("getDocument", new { docid = nrlContents }); 
                        } 

                    }
                }
                catch (Exception e)
                {
                    ViewBag.message = e.Message;
                }
            }
            return View();
        }
        public ActionResult docid( string doc)
        {
            if (initFunction() == false)
            {
                return RedirectToAction("init", "home"); //no cookies means need to start at the beginning            
            }
            //getDocument(string docid, string fileName, string parentDigest)
            if (doc != null)
                return RedirectToAction("getDocument", new { docid = doc} ); //no cookies means need to start at the beginning            
            else
                return View();
        }

        public ActionResult getListings(string where)
        {
            if (initFunction() == false)
            {
                return RedirectToAction("init", "home"); //no cookies means need to start at the beginning            
            } 
            try
            {
                Stream sync = work.getSyncdir(session, (string) (Session["rootid"]), where);
                if ( sync == null)
                    return RedirectToAction("init", "home"); //no sync means we need to start at the beginning

                StreamReader sr = new StreamReader(sync, Encoding.Default);
                var ret = sr.ReadToEnd();
                var dict = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>( ret );
                mobileHelixItems myItem;
                List< mobileHelixItems > items = new List<mobileHelixItems>();

                if (dict["changes"] == null)
                {
                    ViewBag.message = "This folder is empty";
                    return View(items);
                }

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
            catch (Exception e)
            {
                return RedirectToAction("init", "home"); //no cookies means need to start at the beginning
            }
        }

        

        public ActionResult init(HttpPostedFileBase file, string certpassword, string h, string port, string apps_host, string apps_port, string user, string pass, string region, string client)
        {
            
            if (initFunction() == true)
            {
                return RedirectToAction("index", "home", null);
            }
            if (file != null && file.ContentLength > 0) 
            {

                try
                {
                    BinaryReader b = new BinaryReader(file.InputStream);
                    byte[] cert = b.ReadBytes( (int) file.InputStream.Length);
                    Session["certificate"] = cert;
                    doWork work = new doWork(region, client, cert, certpassword, h, port, apps_host, apps_port, user, pass);
                    String[] session = work.getSession( user, pass);

                    Session["userName"] = user;
                    Session["password"] = pass;
                    Session["sessid"] = session[0];
                    Session["appid"] = session[1];
                    Session["host"] = h ;
                    Session["port"] = port;
                    Session["apps_host"] = apps_host;
                    Session["apps_port"] = apps_port;
                    string theCert = Encoding.UTF8.GetString(cert);
                    Session["cert"] = theCert;
                    Session["certpass"] = certpassword;

                    
                    String[] roots = work.getRoots(session);
                    Session["rootid"] = roots[0];
                    //ViewBag.Message = "redirecting...";
                    //return View();
                    return RedirectToAction("index", "home", null);
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

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Please contact Ilya Dreytser at ilya@mobilehelix.com with any questions or comments.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "This is a demo web application developed by Mobile Helix Inc. It facilitates interactions with WorkSite.";
            return View();
        }


    }
}