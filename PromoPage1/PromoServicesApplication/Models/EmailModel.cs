using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Net.Mail;

namespace PromoServicesApplication.Models
{
    public static class EmailModel
    {
        public static bool SendEventPostedMail(EventTableProd a, string eventUrl)
        {
            //-----------------------working email code--------------------------//
            try
            {
                MailMessage email = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                email.From = new MailAddress("natea@yourpartyhub.com");
                email.To.Add(new MailAddress(a.Email));
                email.Bcc.Add(new MailAddress("natearzu@gmail.com"));
                email.Subject = "Your Event Was Posted!";
                email.IsBodyHtml = true;
                email.Body = 
                    "This is your confirmation email that your event was posted." + Environment.NewLine + 
                    "View & share your event: <a href='" + eventUrl + "'>" + eventUrl + "</a>" +
                    Environment.NewLine + "Need additional promotion? Or would like your event listed on" +
                    " YourPartyHub.com's font page, please send an email to natea@yourpartyhub.com.";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("natearzu", "slogan55");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(email);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public static bool SendMail2(InquiryTable i)
        {
            try
            {
                MailMessage email = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                email.From = new MailAddress("natea@yourpartyhub.com");
                email.To.Add(new MailAddress(i.Email));
                email.Bcc.Add(new MailAddress("natearzu@gmail.com"));
                email.Subject = "Promo Services Inquiry";
                email.Body = "This is your confirmation email that your request was received successfully." +
                    Environment.NewLine +
                    "Inquiry summary: [" + i.Message + "]" +
                    Environment.NewLine +
                    "*You will be contacted within 24 hours.";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("natearzu", "slogan55");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(email);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}