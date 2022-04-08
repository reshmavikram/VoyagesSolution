using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using VoyagesAPIService.Infrastructure.Helper;

namespace VoyagesAPIService.Utility
{
    public class Utility
    {
        public static int GetTimeZone(string UTCDateTime)
        {
            string[] timePart = null;
            int timeZone = 0;
            if (UTCDateTime.Contains("+"))
            {
                timePart = UTCDateTime.Split("+");
                timeZone = Convert.ToInt32("+" + timePart[1].Split(":")[0]);
            }
            else if (UTCDateTime.Contains("-"))
            {
                timePart = UTCDateTime.Split("-");
                timeZone = Convert.ToInt32("-" + timePart[3].Split(":")[0]);
            }

            return timeZone;
        }

        public static void SendEmail(string LogString,bool isDistance)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient(AzureVaultKey.GetVaultValue("SmtpHostName"));
                mail.From = new MailAddress(AzureVaultKey.GetVaultValue("SmtpFromEmail"));
                var mailTo = AzureVaultKey.GetVaultValue("SmtpToEmail");
                var lstofToData = mailTo.Split(';').ToList();
                foreach (var item in lstofToData)
                {
                    mail.To.Add(item);
                }
                string env = "";
                #if Dev
                            env = "Dev";
                #elif Uat
                            env = "Uat";
                #endif

                if (env.Equals("Dev"))
                {
                    if (isDistance)
                    {
                        mail.Subject = "DEV:Distance API Called in Position warning or Analyzed weather calculation";
                    }
                    else
                    {
                        mail.Subject = "DEV:Problem while processing Excel";
                    }
                }
                else if (env.Equals("Uat"))
                {
                    if (isDistance)
                    {
                        mail.Subject = "UAT:Distance API Called in Position warning or Analyzed weather calculation";
                    }
                    else
                    {
                        mail.Subject = "UAT:Problem while processing Excel";
                    }
                }
                else
                {
                    if (isDistance)
                    {
                        mail.Subject = "Distance API Called in Position warning or Analyzed weather calculation";
                    }
                    else
                    {
                        mail.Subject = "Problem while processing Excel";
                    }
                }
               
                mail.Body = LogString;
                mail.IsBodyHtml = true;
                SmtpServer.EnableSsl = true;
                SmtpServer.Port = Convert.ToInt32(AzureVaultKey.GetVaultValue("SmtpPort"));
                SmtpServer.Credentials = new System.Net.NetworkCredential(mail.From.Address, AzureVaultKey.GetVaultValue("SmtpPassword"));
                SmtpServer.Send(mail);

            }
            catch (Exception ex)
            {
                // Log.Writelog("Method:SendEmail", Log.LogType.Error, ex);
            }
        }


    }
}
