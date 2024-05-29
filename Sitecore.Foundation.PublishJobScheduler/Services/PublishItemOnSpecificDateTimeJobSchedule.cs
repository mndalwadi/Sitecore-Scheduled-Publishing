using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Tasks;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Configuration;
using Sitecore.Publishing;
using Sitecore.SecurityModel;
using Sitecore.Globalization;
using Sitecore.Data.Managers;
using System.Net.Mail;
using System.Text;

namespace Sitecore.Foundation.PublishJobScheduler.Services
{
    public class PublishItemOnSpecificDateTimeJobSchedule
    {
        int removeJobAfterSelectedDays = 10;
        DateTime currentDatetime = Sitecore.DateUtil.ToServerTime(DateTime.Now);
        Database masterDb = Factory.GetDatabase("master");
        public void Execute(Item[] items, Sitecore.Tasks.CommandItem commandItem, ScheduleItem scheduleItem)
        {
            Log.Info("Start Task Job Scheduler: Sitecore.Foundation.PublishJobScheduler.Services.PublishItemOnSpecificDateTimeJobSchedule", this);

            List<string> lstDbs = new List<string>(), lstLanguages = new List<string>(), lstItems = new List<string>();
            string mailbody = string.Empty, strLanguages = string.Empty, strDbs = string.Empty;
            StringBuilder sb = new StringBuilder();
            try
            {
                Log.Info("Current Date and time: " + currentDatetime.ToString(), this);
                //DateTime currentDatetime = Sitecore.DateUtil.ToServerTime(DateTime.Now);

                Log.Info("Fetching 'Publish Cron Job Folder' item from path: '/sitecore/system/Modules/Publish Cron Job'", this);
                Item publishCronJobFolderItem = masterDb.GetItem(Templates.SystemModuleItems.PublishCronJob);    // "/sitecore/system/Modules/'Publish Cron Job'"

                if (publishCronJobFolderItem != null)
                {
                    List<Item> PublishCronJobItems = publishCronJobFolderItem.Axes.GetDescendants().Where(x => x.TemplateID == Templates.PublishCronJob.ID).ToList();

                    if (PublishCronJobItems != null && PublishCronJobItems.Count() > 0)
                    {
                        foreach (Item item in PublishCronJobItems)
                        {
                            Log.Info("'Publish Cron Job' Item Path: " + item.Paths.FullPath, this);

                            CheckboxField chkAutoRemove = item.Fields[Templates.PublishCronJob.Fields.AutoRemove];
                            bool isAutoRemove = !string.IsNullOrEmpty(item[Templates.PublishCronJob.Fields.AutoRemove]) && chkAutoRemove.Checked;

                            CheckboxField chkInactive = item.Fields[Templates.PublishCronJob.Fields.Inactive];
                            bool isInactive = !string.IsNullOrEmpty(item[Templates.PublishCronJob.Fields.Inactive]) && chkInactive.Checked;

                            CheckboxField chkJobDone = item.Fields[Templates.PublishCronJob.Fields.JobDone];
                            bool isJobDone = !string.IsNullOrEmpty(item[Templates.PublishCronJob.Fields.JobDone]) && chkJobDone.Checked;

                            DateField dfExecuteExactlyAtDateTime = item.Fields[Templates.PublishCronJob.Fields.ExecuteExactlyAtDateTime];
                            DateTime dt = !string.IsNullOrEmpty(item[Templates.PublishCronJob.Fields.ExecuteExactlyAtDateTime]) ? Sitecore.DateUtil.ToServerTime(dfExecuteExactlyAtDateTime.DateTime) : DateTime.MinValue;

                            if (!isInactive && !isJobDone && dt != DateTime.MinValue)
                            {
                                Log.Info("Current Date: " + currentDatetime.ToString() + "  and  'ExecuteExactlyAtDateTime' datetime field: " + dt, this);
                                Log.Info("Is 'Current Date' greater than or equal to 'ExecuteExactlyAtDateTime'? : " + (currentDatetime >= dt), this);

                                if (currentDatetime >= dt)
                                {
                                    CheckboxField chkpublishWithSubItems = item.Fields[Templates.PublishCronJob.Fields.PublishWithSubItems];
                                    bool isPublishWithSubItems = chkpublishWithSubItems != null ? chkpublishWithSubItems.Checked : false;

                                    CheckboxField chkAutoremove = item.Fields[Templates.PublishCronJob.Fields.AutoRemove];

                                    List<Item> treelistItems = item.Fields[Templates.PublishCronJob.Fields.Items] != null && ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.Items]).GetItems().Count() > 0
                                                                ? ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.Items]).GetItems().ToList() : null;

                                    List<Item> mlfPublishingTarget = item.Fields[Templates.PublishCronJob.Fields.PublishingTarget] != null && ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.PublishingTarget]).GetItems().Count() > 0
                                                                ? ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.PublishingTarget]).GetItems().ToList() : null;

                                    List<Item> mlfLanguageList = item.Fields[Templates.PublishCronJob.Fields.Language] != null && ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.Language]).GetItems().Count() > 0
                                                                ? ((MultilistField)item.Fields[Templates.PublishCronJob.Fields.Language]).GetItems().ToList() : null;

                                    foreach (Item langItem in mlfLanguageList)
                                    {
                                        lstLanguages.Add(langItem.Name);
                                    }
                                    foreach (Item publishingTargetItem in mlfPublishingTarget)
                                    {
                                        lstDbs.Add(publishingTargetItem[Templates.PublishingTarget.Fields.TargetDatabase]);
                                    }

                                    if (treelistItems != null && treelistItems.Count() > 0)
                                    {
                                        int counter = 0;
                                        foreach (Item treeItem in treelistItems)
                                        {
                                            lstItems.Add(treeItem.Paths.FullPath);

                                            //PublishItem(treeItem, isPublishWithSubItems);
                                            if (mlfPublishingTarget != null && mlfPublishingTarget.Count() > 0)
                                            {
                                                foreach (Item publishingTargetItem in mlfPublishingTarget)
                                                {
                                                    Database targetDatabase = null;

                                                    string targetdb = publishingTargetItem[Templates.PublishingTarget.Fields.TargetDatabase];

                                                    if (!string.IsNullOrEmpty(targetdb))
                                                    {
                                                        Log.Info("Target Database: " + targetdb, this);

                                                        targetDatabase = Database.GetDatabase(targetdb);
                                                        //PublishItem(treeItem, targetDatabase, isPublishWithSubItems);
                                                        //counter++;

                                                        if (mlfLanguageList != null && mlfLanguageList.Count() > 0)
                                                        {
                                                            foreach (Item mlflangItem in mlfLanguageList)
                                                            {
                                                                //List<Language> languages = languages = LanguageManager.GetLanguages(masterDb).ToList();
                                                                using (new LanguageSwitcher(mlflangItem.Name))
                                                                {
                                                                    Item langItem = masterDb.GetItem(treeItem.ID);

                                                                    if (langItem != null)
                                                                    {
                                                                        var latestVersion = langItem.Versions.GetLatestVersion();
                                                                        if (latestVersion != null && latestVersion.Versions.Count > 0)
                                                                        {
                                                                            PublishItem(langItem, targetDatabase, isPublishWithSubItems);
                                                                            Log.Info("'Publish Cron Job' job has published item: " + langItem.Paths.FullPath + " in database: " + targetdb + " for language: " + langItem.Language.Name, this);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Log.Info("No language selected in 'Languages' field", this);
                                                        }
                                                        //counter++;
                                                    }
                                                    else
                                                    {
                                                        Log.Info("'Target Database' field value is null or empty. Publishing target Item path is: " + publishingTargetItem.Paths.FullPath, this);
                                                    }
                                                }
                                                counter++;
                                            }
                                            else
                                            {
                                                Log.Info("No database selected in 'Publishing Target' field", this);
                                            }
                                        }
                                        //if (treelistItems.Count() == counter)
                                        //{
                                        using (SecurityDisabler disabler = new SecurityDisabler())
                                        {
                                            item.Editing.BeginEdit();
                                            try
                                            {
                                                item[Templates.PublishCronJob.Fields.Inactive] = "1";
                                                item[Templates.PublishCronJob.Fields.JobDone] = "1";
                                                item.Editing.EndEdit();
                                                isJobDone = true;
                                            }
                                            catch (Exception ex)
                                            {
                                                item.Editing.CancelEdit();
                                                Log.Error("Error on item update for Item path: " + item.Paths.FullPath + " and error is: " + ex.Message, ex, this);
                                            }
                                        }
                                        //}
                                        //else
                                        //{
                                        //    Log.Info("'Items' field items count does not match the counter variable value", this);
                                        //}
                                    }
                                    else
                                    {
                                        Log.Info("No items selected in 'Items' field", this);
                                    }

                                    sb.AppendLine("<div style=\"color:black;font-size:11pt;font-family:Calibri,Helvetica,sans-serif;\">Hi,<br/><br/>");
                                    sb.AppendLine("<b>" + item.Name + "</b> job has just executed at " + dt.ToString("dd-MM-yyyy hh:mm tt") + "<br/>");
                                    sb.AppendLine("<b>Item path:</b> " + item.Paths.FullPath + "<br/><br/>");
                                    sb.AppendLine("<strong>Language:</strong> " + string.Join(", ", lstLanguages) + "<br/>");
                                    sb.AppendLine("<strong>Target Database:</strong> " + string.Join(", ", lstDbs) + "<br/>");
                                    sb.AppendLine("<strong>Publish with subitems:</strong> " + (isPublishWithSubItems ? "Yes" : "No") + "<br/>");

                                    sb.AppendLine("<strong>Items:</strong><ul style=\"margin-top:0;\">");
                                    foreach (string strItem in lstItems)
                                        sb.AppendLine("<li>" + strItem + "</li>");
                                    sb.AppendLine("</ul><br/>");
                                    //sb.AppendLine("Note that the item will not be published if the item is in Workflow state or if the 'Publish' date field contains a future date.<br/><br/><br/>-----<br/>");
                                    sb.AppendLine("<p>Note that Publishing a Sitecore item requires multiple requirements to be met. <br/>");
                                    sb.AppendLine(" - Verify that your item is in the final workflow state, if a workflow is specified. If no workflow is specified, the workflow status should be blank.<br/>");
                                    sb.AppendLine(" - Verify that your item is publishable, or if a particular version is not getting published, make sure the version is publishable.</p><br/><br/><br/>-----<br/>");
                                    sb.AppendLine("<i>This email is configured to be automatically sent to you. Please contact your project administrators if you think you receive this message by mistake!</i><br/><br/></div>");

                                    string subject = Constants.SMTP.MailServerSUbject.Replace("{{Datetime}}", dt.ToString("dd-MM-yyyy hh:mm tt"));
                                    mailbody = sb.ToString();

                                    SendMail(Constants.SMTP.MailServerReceiverMail, subject, mailbody);
                                }
                            }

                            // Check if the item should be deleted after 10 days
                            if (isAutoRemove && isJobDone && currentDatetime - dt > TimeSpan.FromDays(removeJobAfterSelectedDays))
                            {
                                item.Recycle(); // Item will be deleted from CMS and move to the recycle bin so you can restore it if needed.
                                //item.Delete(); // Item will be permanently deleted from the CMS and will not be moved to the Recycle Bin.
                            }
                        }
                    }
                    else
                    {
                        Log.Info("Not found any job items from the 'Publish Cron Job Folder'. Path: '/sitecore/system/Modules/Publish Cron Job'", this);
                    }
                }
                else
                {
                    Log.Info("Not found 'Publish Cron Job Folder' item from path: '/sitecore/system/Modules/Publish Cron Job'", this);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Task Job Scheduler Sitecore.Foundation.PublishJobScheduler.Services.PublishItemOnSpecificDateTimeJobSchedule Exception: " + ex.Message, ex, this);
            }

            Log.Info("End Task Job Scheduler: Sitecore.Foundation.PublishJobScheduler.Services.PublishItemOnSpecificDateTimeJobSchedule", this);
        }

        private void PublishItem(Item item, Database targetDatabase, bool isPublishWithSubItems)
        {
            try
            {
                // The publishOptions determine the source and target database,
                // the publish mode and language, and the publish date
                PublishOptions publishOptions = new PublishOptions(item.Database, targetDatabase, PublishMode.SingleItem, item.Language, currentDatetime);  // Create a publisher with the publishoptions
                Publisher publisher = new Publisher(publishOptions);

                // Choose where to publish from
                publisher.Options.RootItem = item;

                // Publish children as well?
                publisher.Options.Deep = isPublishWithSubItems;

                // Do the publish!
                publisher.PublishAsync();

                item.Publishing.ClearPublishingCache();
                Log.Info("Published item from 'Sitecore.Foundation.PublishJobScheduler.Services.PublishItemOnSpecificDateTimeJobSchedule' and item path is: " + item.Paths.FullPath, this);
            }
            catch (Exception ex)
            {
                Log.Error("Error in 'Sitecore.Foundation.PublishJobScheduler.Services.PublishItemOnSpecificDateTimeJobSchedule' while publishing item and the item path is :" + item.Paths.FullPath + " and the error message is: " + ex.Message, ex, this);
            }
        }

        public bool SendMail(string Reciepent = "", string Subject = "", string EmailBody = "", string EmailCC = "", string EmailBCC = "")
        {
            bool result = false;
            //var rootitem = Context.Database.GetItem(Sitecore.Context.Site.RootPath);

            string Server = Constants.SMTP.MailServer;
            int Port = int.Parse(Constants.SMTP.MailServerPort);

            string SenderName = Constants.SMTP.MailServerSenderName;
            string MailServerUserName = Constants.SMTP.MailServerUserName;
            string MailServerPassword = Constants.SMTP.MailServerPassword;

            string SenderEmail = Constants.SMTP.MailServerSenderMail;
            string MailServerSenderMailPassword = Constants.SMTP.MailServerSenderMailPassword;

            bool isSSL = bool.Parse(Constants.SMTP.MailServerUseSsl);

            using (MailMessage mail = new MailMessage())
            {
                if (!string.IsNullOrEmpty(Reciepent))
                {
                    string[] Reciepents = Reciepent.Split(';');
                    foreach (var email in Reciepents)
                    {
                        if (!string.IsNullOrWhiteSpace(email))
                            mail.To.Add(email)
;
                    }
                }

                if (!string.IsNullOrEmpty(EmailCC))
                {
                    string[] CCId = EmailCC.Split(';');
                    foreach (var ccemail in CCId)
                    {
                        if (!string.IsNullOrWhiteSpace(ccemail))
                            mail.CC.Add(ccemail);
                    }
                }
                if (!string.IsNullOrEmpty(EmailBCC))
                {
                    string[] BCCId = EmailBCC.Split(';');
                    foreach (var bccemail in BCCId)
                    {
                        if (!string.IsNullOrWhiteSpace(bccemail))
                            mail.Bcc.Add(bccemail);
                    }
                }

                mail.Subject = Subject;
                mail.Body = EmailBody;
                mail.IsBodyHtml = true;
                if (!string.IsNullOrWhiteSpace(SenderEmail))
                {
                    mail.From = new MailAddress(SenderEmail, SenderName);
                }

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = Server;

                    if (Port != 0)
                        smtp.Port = Port;

                    smtp.EnableSsl = isSSL;

                    if (!string.IsNullOrWhiteSpace(MailServerUserName) && !string.IsNullOrWhiteSpace(MailServerPassword))    //if (!string.IsNullOrWhiteSpace(SenderEmail) && !string.IsNullOrWhiteSpace(Password))
                    {
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new System.Net.NetworkCredential(MailServerUserName, MailServerPassword);     //smtp.Credentials = new System.Net.NetworkCredential(SenderEmail, Password);
                    }
                    try
                    {
                        smtp.Send(mail);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                        Log.Error("Error has been occurred while sending email. Error message is: " + ex.Message, ex, this);
                    }
                }
            }
            return result;
        }
    }
}