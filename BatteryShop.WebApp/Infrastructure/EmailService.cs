using System;
using System.Net.Mail;
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
                    msg.Subject = "Your OTP Code - Battery Store";
                    msg.IsBodyHtml = true;

                    msg.Body = $@"<html>
<body style='font-family:Arial,sans-serif;padding:20px;'>
    <h2 style='color:#333;'>Email Verification</h2>
    <p style='color:#555;font-size:14px;'>Use the code below to complete your login:</p>
    <div style='background:#f8f9fa;padding:15px;border-radius:8px;text-align:center;margin:20px 0;'>
        <span style='font-size:36px;letter-spacing:8px;font-weight:bold;color:#0d6efd;'>{otpCode}</span>
    </div>
    <p style='color:#888;font-size:12px;'>This code expires in 5 minutes. If you did not request this, please ignore this email.</p>
</body>
</html>";

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
