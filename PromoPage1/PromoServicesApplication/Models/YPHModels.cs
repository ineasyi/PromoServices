using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PromoServicesApplication.Models
{
    public class YPHModels
    {
        public static bool Post(EventTableProd a, out string eventUrl)
        {
            try
            {
                CookieContainer container = new CookieContainer();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.yourpartyhub.com/");
                request.CookieContainer = container;
                request.Timeout = 30000;
                request.KeepAlive = true;

                HttpWebResponse loadResp = (HttpWebResponse)request.GetResponse();
                loadResp.Close();
                loadResp.Dispose();

                ASCIIEncoding encoding = new ASCIIEncoding();

                //for now im setting the entire string to make things easier
                string postData = string.Format("mode=login&userAction=user_login&username1={0}&password1={1}", "phillyniteent", "slogan15");

                byte[] data = encoding.GetBytes(postData);

                HttpWebRequest request2 = (HttpWebRequest)WebRequest.Create("Http://www.yourpartyhub.com/users");
                request2.CookieContainer = container;

                request2.Method = "POST";
                request2.ContentType = "application/x-www-form-urlencoded";
                request2.ContentLength = postData.Length;
                request2.KeepAlive = true;

                Stream newStream = request2.GetRequestStream();
                newStream.Write(data, 0, data.Length);

                HttpWebResponse loadResp2 = (HttpWebResponse)request2.GetResponse();
                loadResp2.Close();

                HttpWebRequest Party = (HttpWebRequest)WebRequest.Create("http://www.yourpartyhub.com/events/createEvent");
                Party.KeepAlive = true;
                Party.CookieContainer = container;

                HttpWebResponse PartyResp = (HttpWebResponse)Party.GetResponse();

                StreamReader reader = new StreamReader(PartyResp.GetResponseStream());
                string page1 = reader.ReadToEnd(); // here we check for the multi-part form to ensure we are proper page before sending form data

                string curBoundary = CreateFormDataBoundary();

                HttpWebRequest PostRequest = (HttpWebRequest)WebRequest.Create("http://www.yourpartyhub.com/events/createEvent");
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
                eventUrl = response.ResponseUri.ToString();
                return true;
            }
            catch(WebException ex)
            {
                eventUrl = ex.Message;
                return false;
            }
            
        }

        public class CreateForm
        {
            public static string FormDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";
            public static string MultiformDataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}.jpg\"\r\nContent-Type: image/jpeg\r\n\r\n";

            internal static Stream BuildForm(EventTableProd a, Stream strm, string boundary)
            {
                string startHour = a.StartDate.Hour.ToString();
                string endHour = a.EndDate.Hour.ToString();
                #region
                if (a.StartDate.Hour == 0)
                    startHour = "12";
                if (a.StartDate.Hour > 12)
                    startHour = (a.StartDate.Hour - 12).ToString();
                if (a.EndDate.Hour == 0)
                    endHour = "12";
                if (a.EndDate.Hour > 12)
                    endHour = (a.EndDate.Hour - 12).ToString();

                List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("event", a.Name),
                    //new KeyValuePair<string, string>("type[]", a.Type),
                    new KeyValuePair<string, string>("description", a.Description),
                    new KeyValuePair<string, string>("start_date", a.StartDate.ToString("MM/dd/yyyy")),
                    new KeyValuePair<string, string>("end_date", a.EndDate.ToString("MM/dd/yyyy")),
                    new KeyValuePair<string, string>("eventStartHours", startHour),
                    new KeyValuePair<string, string>("eventStartMinute", String.Format("{0:00}", a.StartDate.Minute)),
                    new KeyValuePair<string, string>("eventStartMeridian", a.StartDate.ToString().Split(' ').Last().ToString()),
                    new KeyValuePair<string, string>("eventEndHours", endHour),
                    new KeyValuePair<string, string>("eventEndMinutes", String.Format("{0:00}", a.EndDate.Minute)),
                    new KeyValuePair<string, string>("eventEndMeridian", a.EndDate.ToString().Split(' ').Last().ToString()),
                    new KeyValuePair<string, string>("ticket", "on"),
                    new KeyValuePair<string, string>("recuring_start_date", ""), // can be null
                    new KeyValuePair<string, string>("recuring_end_date", ""),  // can be null
                    new KeyValuePair<string, string>("address", a.Venue),// actually this is the venue name
                    new KeyValuePair<string, string>("street", a.StreetAdress),
                    new KeyValuePair<string, string>("state", a.State),
                    new KeyValuePair<string, string>("check", "ok"),
                    new KeyValuePair<string, string>("city", a.City),
                    new KeyValuePair<string, string>("zip", a.Zip), // can be null
                    new KeyValuePair<string, string>("email", a.Email),
                    new KeyValuePair<string, string>("contact", a.Phone),
                    //new KeyValuePair<string, string>("music[]", a.Music),
                    new KeyValuePair<string, string>("dress", a.DressCode),
                    new KeyValuePair<string, string>("forum", "1"), // 0 or 1
                    new KeyValuePair<string, string>("featured", "0"), // 0 or 1
                    new KeyValuePair<string, string>("featured_name", ""), // can be null
                    new KeyValuePair<string, string>("featured_email", ""), // can be null
                    new KeyValuePair<string, string>("featured_date", ""), // can be null
                    new KeyValuePair<string, string>("agelimitLower", a.Age),
                    new KeyValuePair<string, string>("stock", "0"),
                    new KeyValuePair<string, string>("cost", a.Cost)
                };

                #endregion

                string form = "";
                byte[] formbytes = null;

                foreach (KeyValuePair<string, string> kvp in formData)
                {
                    form += string.Format(FormDataTemplate, boundary, kvp.Key, kvp.Value);
                }
                foreach(string t in a.Type.Split(','))
                {
                    form += string.Format(FormDataTemplate, boundary, "type[]", t);
                }
                foreach (string m in a.Music.Split(','))
                {
                    form += string.Format(FormDataTemplate, boundary, "music[]", m);
                }

                form += string.Format(MultiformDataTemplate, boundary, "eventImage",a.ID.ToString()); // file header|boundary
                formbytes = System.Text.Encoding.UTF8.GetBytes(form);
                strm.Write(formbytes, 0, formbytes.Length);
                strm.Write(a.Flyer, 0, a.Flyer.Length);

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