using System;
using System.Configuration;
using System.IO;
using System.Net.Mail;
using System.Web.Hosting;
using Serilog;

namespace BatteryShop.WebApp.Infrastructure
{
    public static class EmailService
    {
        public static void SendOtp(string toEmail, string otpCode)
        {
            try
            {
                using (SmtpClient client = new SmtpClient())
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add(toEmail);
                    msg.Subject = ConfigurationManager.AppSettings["OtpEmailSubject"] ?? "Your OTP Code - Battery Store";
                    msg.IsBodyHtml = true;

                    string templatePath = HostingEnvironment.MapPath("~/EmailTemplates/OtpEmail.html");
                    msg.Body = File.ReadAllText(templatePath).Replace("{otpCode}", otpCode);

                    client.Send(msg);
                    Log.Information("OTP email sent to {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send OTP email to {Email}", toEmail);
            }

            Log.Information("OTP for {Email}: {OtpCode}", toEmail, otpCode);
        }
    }
}
