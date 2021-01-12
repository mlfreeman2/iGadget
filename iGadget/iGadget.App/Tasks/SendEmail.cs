using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using iGadget.DB;
using iGadget.App.Models.Config;
using iGadget.App.Util;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Utils;

namespace iGadget.App.Tasks.Shared
{
    public class SendEMail
    {
        private IServiceProvider _serviceProvider;

        private IConfiguration _config;

        private IBackgroundJobClient _client;

        public SendEMail(IServiceProvider sp, IBackgroundJobClient client)
        {
            _serviceProvider = sp;
            _client = client;
            _config = _serviceProvider.GetService<IConfiguration>();
        }

        [ObjectFriendlyJobDisplayName("Send Queued EMail ID {0}")]
        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new int[] { 10, 90 }, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void SendAnEMail(int eMailID)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            using (var dbContext = scope.ServiceProvider.GetRequiredService<iGadgetDataCache>())
            {
                try
                {
                    var message = dbContext.EMails.Include(a => a.Attachments).Include(a => a.Bcc).First<DB.Models.EMail>(a => a.ID == eMailID);

                    var mailServerSettings = new MailAccount();
                    _config.GetSection($"MailAccounts:{message.From}").Bind(mailServerSettings);

                    var messageBuilder = new BodyBuilder();
                    messageBuilder.HtmlBody = message.HTMLBody;
                    messageBuilder.TextBody = message.TextBody;
                    foreach (var attachment in message.Attachments.ToList())
                    {
                        var msgAttachment = messageBuilder.LinkedResources.Add(attachment.FileName, attachment.Content);
                        msgAttachment.ContentId = MimeUtils.GenerateMessageId();
                        msgAttachment.ContentDisposition = new ContentDisposition(attachment.ContentDisposition);
                        messageBuilder.HtmlBody = messageBuilder.HtmlBody.Replace($"cid:{attachment.FileName}", $"cid:{msgAttachment.ContentId}");
                    }

                    var mimeMessage = new MimeMessage();
                    mimeMessage.Body = messageBuilder.ToMessageBody();

                    foreach (var recipient in message.Bcc)
                    {
                        mimeMessage.Bcc.Add(MailboxAddress.Parse(recipient.ToString()));
                    }

                    mimeMessage.From.Add(MailboxAddress.Parse(mailServerSettings.From));
                    mimeMessage.Subject = message.Subject;

                    using (var client = new SmtpClient())
                    {
                        client.Connect(mailServerSettings.Server, mailServerSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                        client.Authenticate(mailServerSettings.User, mailServerSettings.Password);
                        client.Send(mimeMessage);
                    }

                    message.Sent = true;
                    message.SentDate = DateTime.Now;
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException($"Unable to send email ID {eMailID}.", e);
                }
            }
        }
    }
}