using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using MimeKit;

namespace RestfulRegistry.Services
{
    public class ConfirmationEmailSender
    {
        static string[] Scopes = { GmailService.Scope.GmailAddonsCurrentActionCompose, GmailService.Scope.GmailAddonsCurrentMessageAction, GmailService.Scope.GmailSend };
        static string ApplicationName = "Restful Resting Place";
        private static GoogleClientSecrets GetSecretsFromEnvironment()
        {
            var environmentConfiguration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
            var secretsEnv = environmentConfiguration["GoogleSecrets"];
            var secrets = JsonConvert.DeserializeObject<GoogleClientSecrets>(secretsEnv);
            return secrets;
        }
        /// <summary>
        /// Send Email
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task SendMailAsync(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                            .AddJsonFile("SendMailSettings.json")
                            .Build();
                Dictionary<string, string> MailSettings;
                MailSettings = configuration.GetSection("MailSettings").GetChildren().ToDictionary(x => x.Key, x => x.Value);
                MailSettings.Add("to", args[0]);
                MailSettings.Add("link", args[1]);
                GoogleClientSecrets gSecrets = GetSecretsFromEnvironment();
                string credPath = "token.json";
                UserCredential gcredential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                       gSecrets.Secrets,
                       Scopes,
                       MailSettings["account"],
                       CancellationToken.None,
                       new FileDataStore(credPath, true));
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = gcredential,
                    ApplicationName = ApplicationName,
                });

                SendConfirmationEmail(service, MailSettings);
                Console.WriteLine($"Succesfully sent registration email to {args[0]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void SendConfirmationEmail(GmailService gmail, Dictionary<string, string> dict)
        {
            MailMessage mailmsg = new MailMessage();
            {
                mailmsg.Subject = dict["subject"];
                mailmsg.Body = string.Format(dict["HTML"], dict["link"]);
                mailmsg.From = new MailAddress(dict["from"]);
                mailmsg.To.Add(new MailAddress(dict["to"]));
                mailmsg.IsBodyHtml = true;
            }
            ////add attachment if specified
            if (dict.ContainsKey("attachement"))
            {
                if (File.Exists(dict["attachment"]))
                {
                    Attachment data = new Attachment(dict["attachment"]);
                    mailmsg.Attachments.Add(data);
                }
                else
                {
                    Console.WriteLine("Error: Invalid Attachemnt");
                }
            }
            //Make mail message a Mime message
            MimeKit.MimeMessage mimemessage = MimeKit.MimeMessage.CreateFromMailMessage(mailmsg);
            Google.Apis.Gmail.v1.Data.Message finalmessage = new Google.Apis.Gmail.v1.Data.Message();
            finalmessage.Raw = Base64UrlEncode(mimemessage.ToString());
            var result = gmail.Users.Messages.Send(finalmessage, "me").Execute();
        }
        private static string Base64UrlEncode(string input)
        {
            var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
        }
    }
}


