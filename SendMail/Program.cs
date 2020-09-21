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
using System.Threading;
using System.Threading.Tasks;
namespace SendMail
{
    class Program
    {
       
        static async Task Main(params string[] args)
        {
            await ConfirmationEmailSender.SendMailAsync(args);
          
        }
    }
}