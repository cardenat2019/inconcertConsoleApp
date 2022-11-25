using System;
using System.Collections.Generic;
using System.Net.Mail;
using ConsoleApp.Models;
using System.Data.SqlClient;
using ConsoleApp.BP;
using ConsoleApp.Utils;
using System.Data;
using MimeKit;
using MailKit.Security;
using MailKit.Net.Smtp;

namespace ConsoleApp.Mail
{
    static class MailProcess
    {
        // Método que convierte en arreglos los TO, CC, BCC y sustituye las variables en Subject del mensaje con los datos de Campaña y Fecha
        public static void sendMailMessage(MailConfig mailConfig, string[] filename, string campaign, string date)
        {
            string[] TO  = mailConfig.To.Split(',');
            string[] CC  = mailConfig.Cc.Split(',');
            string[] BCC = mailConfig.Bcc.Split(',');

            mailConfig.Subject = BusinessProcess.makeTextFileName(mailConfig.Subject, campaign, date);

            sendMailMessage(TO, CC, BCC, mailConfig.Subject, mailConfig.Body, filename);

        }
        // Método para traer la configuración de Correos
        public static List<MailConfig> getMailConfigList()
        {
            List<MailConfig> mailConfigList = new List<MailConfig>();
            SqlDataReader reader = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spMailConfig", connection);
                    cmd.CommandType = CommandType.StoredProcedure;

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        MailConfig mailConfig = new MailConfig();

                        mailConfig.Id = Convert.ToInt32(reader[0]);
                        mailConfig.Subject = reader[1].ToString();
                        mailConfig.Body = reader[2].ToString();
                        mailConfig.To = reader[3].ToString();
                        mailConfig.Cc = reader[4].ToString() != null ? reader[4].ToString() : "";
                        mailConfig.Bcc = reader[5].ToString() != null ? reader[5].ToString() : "";
                        mailConfig.LogFileName = reader[6].ToString();

                        mailConfigList.Add(mailConfig);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error trayendo MailConfig: ", ex);
                BusinessProcess.addLogErrors("getMailConfigList", ex.Message);
            }

            return mailConfigList;
        }
        // Método para traer la configuración del Servidor SMTP del Cliente
        public static SmtpServerParameters getSmtpServerParameters()
        {
            SmtpServerParameters smtpServerParameters = new SmtpServerParameters();
            SqlDataReader reader = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(Config.ConnectionString()))
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("dbo.spSmtpServerParameter", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EnvironmetId", Constants.EnvironmentId);

                    reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        smtpServerParameters.Host = reader[2].ToString();
                        smtpServerParameters.Port = reader[3].ToString();
                        smtpServerParameters.EnabledSsl = Boolean.Parse(reader[4].ToString());
                        smtpServerParameters.Account = reader[5].ToString();
                        smtpServerParameters.Password = reader[6].ToString();
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error trayendo MailConfig: ", ex);
                BusinessProcess.addLogErrors("getSmtpServerParameters", ex.Message);
            }

            return smtpServerParameters;
        }
        // Método para enviar los mensajes 
        public static bool sendMailMessage(string[] to, string[] cc, string[] bcc, string subject, string body, string[] fileName)
        {
            MailMessage message = new MailMessage();
            try
            {
                for (int i = 0; i < to.Length; i++)
                {
                    if (!to[i].Equals("")) { 
                        MailAddress ma = new MailAddress(to[i]);
                        message.To.Add(ma);
                    }
                }
                for (int i = 0; i < cc.Length; i++)
                {
                    if (!cc[i].Equals(""))
                    {
                        MailAddress ma = new MailAddress(cc[i]);
                        message.CC.Add(ma);
                    }
                }
                for (int i = 0; i < bcc.Length; i++)
                {
                    if (!bcc[i].Equals(""))
                    {
                        MailAddress ma = new MailAddress(bcc[i]);
                        message.Bcc.Add(ma);
                    }
                }

                SmtpServerParameters smtpServerParameters = getSmtpServerParameters();

                message.From = new MailAddress(smtpServerParameters.Account);

                message.Subject = subject;
                message.SubjectEncoding = System.Text.Encoding.UTF8;
                message.Body = body;
                message.BodyEncoding = System.Text.Encoding.UTF8;
                message.IsBodyHtml = true;
                message.Priority = MailPriority.High;
                for (int i = 0; i < fileName.Length; i++)
                {
                    if (fileName[i] != null)
                    {
                        Attachment attachedFile = new Attachment(fileName[i]);
                        message.Attachments.Add(attachedFile);
                    }
                }

                try
                {
                    using (var client = new System.Net.Mail.SmtpClient())
                    {
                        client.Credentials = new System.Net.NetworkCredential(smtpServerParameters.Account, smtpServerParameters.Password);
                        client.Host = smtpServerParameters.Host;
                        client.Port = Int32.Parse(smtpServerParameters.Port);
                        client.EnableSsl = smtpServerParameters.EnabledSsl;
                        client.Send(message);
                    }
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("Error al enviar correo: " + e.Message);
                    BusinessProcess.addLogErrors("sendMailMessage", ex.Message);
                    return false;
                }
            }
            catch (SmtpException ex)
            {
                // Console.WriteLine("SmtpException: " + e.Message);
                BusinessProcess.addLogErrors("sendMailMessage", ex.Message);
                return false;
            }

            return true;

        }

        // Metodo en prueba aún
        public static void mailkitSend()
        {
            var message = new MimeMessage();
            //message.From.Add(new MailboxAddress("Tonny Cardenas", "tonny.cardenas@hotmail.com"));
            message.From.Add(new MailboxAddress("Tonny Cardenas", "cardenat@gmail.com"));
            message.To.Add(new MailboxAddress("Tonny", "tonny.cardenas@hotmail.com"));
            message.Subject = "Prueba de Correo";

            message.Body = new TextPart("plain")
            {
                Text = @"Hola mundo, prueba de correo"
            };

            var builder = new BodyBuilder();

            string filename = "D:\\Aliat\\LOGS\\log_bads_duplicados_03-10-2022.txt";

            builder.Attachments.Add(filename);

            message.Body = builder.ToMessageBody();

            //MimeMessage mm = new MimeMessage();

            string Mensaje = "Mensaje Enviado";

            mailKitSendMessage(message, ref Mensaje);
        }

        // Metodo en Prueba aún
        public static bool mailKitSendMessage(MimeMessage message, ref string mensaje)
        {
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.Connect("mail.office365.com", 587, SecureSocketOptions.StartTls);
                    //client.Connect("smtp.gmail.com", 465, SecureSocketOptions.StartTls);
                }
                catch (System.Security.Authentication.AuthenticationException ex)
                {
                    //Console.WriteLine("Error trying to connect: {0}", ex.Message);
                    //Console.WriteLine("\tStatusCode: {0}", ex.StackTrace);
                    mensaje = "Error trying to connect: " + ex.Message + "\tStatusCode: " + ex.StackTrace;
                    return false;
                }
                catch (SmtpCommandException ex)
                {
                    //Console.WriteLine("Error trying to connect: {0}", ex.Message);
                    //Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);
                    mensaje = "Error trying to connect: " + ex.Message + "\tStatusCode: " + ex.StackTrace;
                    return false;
                }
                catch (SmtpProtocolException ex)
                {
                    //Console.WriteLine("Protocol error while trying to connect: {0}", ex.Message);
                    mensaje = "Protocol error while trying to connect: " + ex.Message;
                    return false;
                }

                // Note: Not all SMTP servers support authentication, but GMail does.
                if (client.Capabilities.HasFlag(SmtpCapabilities.Authentication))
                {
                    try
                    {
                        //client.Authenticate("tonny.cardenas@hotmail.com", "JEn0Epd32RD--");
                        client.Authenticate("cardenat@gmail.com", "JEn0Epd32RD*");
                    }
                    catch (SmtpCommandException ex)
                    {
                        //Console.WriteLine("Error trying to authenticate: {0}", ex.Message);
                        //Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);
                        mensaje = "Error trying to authenticate: " + ex.Message + "\tStatusCode: " + ex.StatusCode;
                        return false;
                    }
                    catch (SmtpProtocolException ex)
                    {
                        //Console.WriteLine("Protocol error while trying to authenticate: {0}", ex.Message);
                        mensaje = "Protocol error while trying to authenticate: " + ex.Message;
                        return false;
                    }
                }

                try
                {
                    client.Send(message);
                }
                catch (SmtpCommandException ex)
                {
                    //Console.WriteLine("Error sending message: {0}", ex.Message);
                    //Console.WriteLine("\tStatusCode: {0}", ex.StatusCode);

                    mensaje = "Error sending message: " + ex.Message + "\tStatusCode: " + ex.StatusCode;

                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            //Console.WriteLine("\tRecipient not accepted: {0}", ex.Mailbox);
                            mensaje += "\tRecipient not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.SenderNotAccepted:
                            //Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                            mensaje += "\tSender not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.MessageNotAccepted:
                            //Console.WriteLine("\tMessage not accepted.");
                            mensaje += "\tMessage not accepted.";
                            break;
                    }
                }
                catch (SmtpProtocolException ex)
                {
                    //Console.WriteLine("Protocol error while sending message: {0}", ex.Message);
                    mensaje += "Protocol error while sending message: " + ex.Message;
                }

                client.Disconnect(true);
            }
            return true;
        }
        

    }
}
