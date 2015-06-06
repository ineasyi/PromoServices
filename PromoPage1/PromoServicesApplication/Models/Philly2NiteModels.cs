using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PromoServicesApplication.Models
{
    public class Philly2NiteModels
    {

        public static void Post(EventTableProd a)
        {
            CookieContainer container = new CookieContainer();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://philly.cities2night.com/");
            request.CookieContainer = container;
            request.Timeout = 30000;
            request.KeepAlive = true;

            HttpWebResponse loadResp = (HttpWebResponse)request.GetResponse();
            loadResp.Close();
            loadResp.Dispose();

            ASCIIEncoding encoding = new ASCIIEncoding();

            //for now im setting the entire string to make things easier
            string postData = string.Format("ci_token=&email=ineasyi%40aol.com&password={0}", "slogan15");

            byte[] data = encoding.GetBytes(postData);

            HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("http://philly.cities2night.com/ajax/actions/verify");
            request2.CookieContainer = container;

            request2.Method = "POST";
            request2.ContentType = "application/x-www-form-urlencoded";
            request2.ContentLength = postData.Length;
            request2.KeepAlive = true;

            Stream newStream = request2.GetRequestStream();
            newStream.Write(data, 0, data.Length);

            HttpWebResponse loadResp2 = (HttpWebResponse)request2.GetResponse();
            loadResp2.Close();

            HttpWebRequest Party = (HttpWebRequest)WebRequest.Create("http://philly.cities2night.com/events/create");
            Party.KeepAlive = true;
            Party.CookieContainer = container;

            HttpWebResponse PartyResp = (HttpWebResponse)Party.GetResponse();

            StreamReader reader = new StreamReader(PartyResp.GetResponseStream());
            string page1 = reader.ReadToEnd(); // here we check for the multi-part form to ensure we are proper page before sending form data

            string curBoundary = CreateFormDataBoundary();

            HttpWebRequest PostRequest = (HttpWebRequest)WebRequest.Create("http://philly.cities2night.com/post/events/create");
            PostRequest.Method = "POST";
            PostRequest.KeepAlive = true;
            PostRequest.Timeout = 30000;
            PostRequest.CookieContainer = container;
            PostRequest.ContentType = "multipart/form-data; boundary=" + curBoundary;

            Stream postReqStream = PostRequest.GetRequestStream();

            postReqStream = CreateForm.BuildForm(a, postReqStream, curBoundary);

            HttpWebResponse response = (HttpWebResponse)PostRequest.GetResponse();
            using (StreamReader r = new StreamReader(response.GetResponseStream()))
            {
                r.ReadToEnd();
            };
        }

        public class CreateForm
        {
            public static string FormDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";
            public static string MultiformDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";

            internal static Stream BuildForm(EventTableProd a, Stream strm, string boundary)
            {
                #region
                // CONVERT DATES HERE
                string startDate = a.StartDate.ToString("D").Replace(",", "");
                string endDate = a.EndDate.ToString("D").Replace(",", "");
                string startHour = a.StartDate.ToString("t");
                string endHour = a.EndDate.ToString("t");
                ///
                /// Age can either be blank, 18, or 21
                ///
                List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ci_token", ""),
                    new KeyValuePair<string, string>("event_place", ""), // Must be set dynamically from website
                    new KeyValuePair<string, string>("event_title", a.Name),
                    new KeyValuePair<string, string>("event_short_description", ""),
                    new KeyValuePair<string, string>("event_description", a.Description),
                    new KeyValuePair<string, string>("event_age_restriction", a.Age), // leave blank for any age
                    new KeyValuePair<string, string>("event_entryfee", a.Cost),
                    new KeyValuePair<string, string>("event_specials", ""),
                    new KeyValuePair<string, string>("event_url", ""), // pass yourpartyhub URL here if possible
                    new KeyValuePair<string, string>("event_start_date", startDate),
                    new KeyValuePair<string, string>("event_start_time", startHour),
                    new KeyValuePair<string, string>("event_end_date", endDate),
                    new KeyValuePair<string, string>("event_end_time", endHour), 
                    new KeyValuePair<string, string>("weekly_repeat", "")
                };

                #endregion

                string form = "";
                byte[] formbytes = null;

                foreach (KeyValuePair<string, string> kvp in formData)
                {
                    form += string.Format(FormDataTemplate, boundary, kvp.Key, kvp.Value);
                }
                foreach (string t in a.Type.Split(','))
                {
                    form += string.Format(FormDataTemplate, boundary, "categories[]", t);
                }

                form += string.Format(MultiformDataTemplate, boundary, "eventImage", a.ID.ToString()); // file header|boundary
                formbytes = System.Text.Encoding.UTF8.GetBytes(form);
                strm.Write(formbytes, 0, formbytes.Length);

                //MemoryStream target = new MemoryStream();
                //a.Flyer.InputStream.CopyTo(target, new byte[1024].Length);
                //byte[] data = target.ToArray();
                //strm.Write(data, 0, data.Length);

                byte[] lastpart = null;
                string lastpartOfForm = "\r\n";
                lastpartOfForm += string.Format(FormDataTemplate, boundary, "url", "");
                lastpart = Encoding.UTF8.GetBytes(lastpartOfForm);
                strm.Write(lastpart, 0, lastpart.Length);
                string url = "--" + boundary + "--";
                byte[] finish = Encoding.UTF8.GetBytes(url);
                strm.Write(finish, 0, finish.Length);

                return strm;
            }
        }

        // Creates dynamic form boundary
        public static string CreateFormDataBoundary()
        {
            return "---------------------------" + DateTime.Now.Ticks.ToString("x");
        }
    
    }
}