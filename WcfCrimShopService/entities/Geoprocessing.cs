using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;


namespace WcfCrimShopService.entities
{
    public class Geoprocessing
    {
        public async Task<string> FotoAerea(string map)
        {
            string testing = map;

            var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
            var taskName = "Mapas de Fotos Aerea";
            //var taskCatastro = "Mapas de Catastro";
            //Create geoprocessor
            var gp = new Geoprocessor(new Uri(serviceURL + "/" + taskName));

            //Set up the parameters
            var parameter = new GPInputParameter();

            //GPParameter creation
            var jsonMap = new GPString("Web_Map_As_JSON", map);
            var Format = new GPString("Format", "PDF");
            var layoutTemplate = new GPString("Layout_Template", "Peticiones-11x17");
            var georef = new GPString("Georef_info", "False");
            var parcel = new GPString("Parcel", "987-654-321-00");
            var subtitle = new GPString("Subtitle", "Prueba de web service");
            var control = new GPString("Control", "Control:1234");

            //add GPParameters to the parameter collection
            parameter.GPParameters.Add(jsonMap);
            parameter.GPParameters.Add(Format);
            parameter.GPParameters.Add(layoutTemplate);
            parameter.GPParameters.Add(georef);
            parameter.GPParameters.Add(parcel);
            parameter.GPParameters.Add(subtitle);
            parameter.GPParameters.Add(control);

            ////executeTask with the parameter collection defgined above, await the result.
            var result = await gp.SubmitJobAsync(parameter);

            while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut)
            {
                result = await gp.CheckJobStatusAsync(result.JobID);
                Debug.WriteLine(result.JobStatus);
                await Task.Delay(2000);
            }

            if (result.JobStatus == GPJobStatus.Succeeded)
            {
                var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                if (outParam != null && outParam.Uri != null)
                {
                    var webClient = new WebClient();
                    //create temp directoy for downloads
                    string path = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives";
                    string zipPath = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives.zip";
                    DirectoryInfo dir;
                    try
                    {
                        //verify is the directory exists.
                        if (Directory.Exists(path))
                        {
                            Debug.WriteLine("directory: " + path + "   exist");
                        }
                        else
                        {
                            //try to create the directory
                            dir = Directory.CreateDirectory(path);
                        }

                        //var stremData = await webClient.OpenReadTaskAsync(outParam.Uri);

                        //get an output file location from the user
                        webClient.DownloadFile(outParam.Uri, path + "\\test.pdf");

                        if (!File.Exists(zipPath))
                        {
                            ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                        }
                        else
                        {
                            File.Delete(zipPath);
                            ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                        }
                        

                        if (File.Exists(zipPath))
                        {
                            //dir.Delete();
                            Directory.Delete(path, true);
                            if (!Directory.Exists(path))
                            {
                                Debug.WriteLine("directory: " + path + "   deleted");
                            }
                            else
                            {
                                Debug.WriteLine(" unable to dele directory: " + path);
                            }

                            try
                            {
                                MailMessage mail = new MailMessage();
                                SmtpClient smtpServer = new SmtpClient("mail.crimpr.net");
                                mail.From = new MailAddress("cdprcasosweb@crimpr.net");
                                mail.To.Add("hasencio@gmtgis.com");
                                mail.Subject = "Test mail 1";
                                mail.Body = "mail with attachment";

                                Attachment attachment = new Attachment(zipPath);
                                mail.Attachments.Add(attachment);

                                smtpServer.Port = 25;
                                smtpServer.Credentials = new System.Net.NetworkCredential("CDPRCASOSWEB", "Cc123456");
                                smtpServer.EnableSsl = false;

                                smtpServer.Send(mail);
                                Debug.WriteLine("MailSend");
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine(e.ToString());
                            }
                            finally { }

                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Process failed", e.ToString());
                    }
                    finally { }
                }
            }
            else
            {
                var message = string.Empty;

                foreach (var msg in result.Messages)
                {
                    message += msg.Description + "\n";
                    
                }

                Debug.WriteLine(message);
            }

            return "code completed";
        }
    }
}