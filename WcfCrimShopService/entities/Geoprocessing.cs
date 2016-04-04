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
        Uri PhotoPdfUri = new Uri("");
        Uri OficialCatUri = new Uri("");
        Uri ListaColindanteUri = new Uri("");
        string path = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives";

        //string map, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string cNumber)
        public async Task<string> FotoAerea(string map, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title)
        {
            cNumber = "1234";

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
            var Format = new GPString("Format", format);
            var layoutTemplate = new GPString("Layout_Template", template);
            var georef = new GPString("Georef_info", geoInfo);
            var parcel = new GPString("Parcel", parcelTitle);
            var subtitle = new GPString("Subtitle", sub_Title);
            var control = new GPString("Control", cNumber);

            var buffer = new GPString("Buffer", "");
            var parcelList = new GPString("Parcelas", "");

            
 
            var bufferDistance = new GPLinearUnit("Buffer_Distance_Units", Esri.ArcGISRuntime.Geometry.LinearUnits.Meters, 15.0);

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

                    PhotoPdfUri = outParam.Uri;
                    var webClient = new WebClient();
                    //create temp directoy for downloads
                    path = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives";
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
                        webClient.DownloadFile(outParam.Uri, path + @"\test.pdf");

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

        public async Task<string> OficialMaps(string template, string array, string geo, string ctrl)
        {
            var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
            string taskName = "Mapas de Catastro";
            var gp = new Geoprocessor(new Uri(serviceURL + "/" + taskName));

            //Set up the parameters
            var parameter = new GPInputParameter();

            var layoutTemplate = new GPString("Layout_Template", template);
            var pageRange = new GPString("Page_Range", array);
            var georef = new GPString("Georef_info", geo);
            var control = new GPString("Control", ctrl);

            parameter.GPParameters.Add(layoutTemplate);
            parameter.GPParameters.Add(pageRange);
            parameter.GPParameters.Add(georef);
            parameter.GPParameters.Add(control);

            //Execute task with the parameters collection defined above
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
                    OficialCatUri = outParam.Uri;
                    
                }

            }

            return "OficialCatastralMaps";
        }

        //Im gonna pass the path were all the folders are stored, the control number of the order
        //to name the folder for the order with it, and the file name  that will be use for the pdf that will be created
        public string MakeStoreFolder(string pathToSave,string cnNumber, string file)
        {

            var path = pathToSave + cnNumber;
            var Pfile = path+file;
            DirectoryInfo dir;
            try
            {
                if (Directory.Exists(path))
                {
                    if (File.Exists(Pfile))
                    {

                    }
                }
            }
            catch (Exception e)
            {

            }
            return "saved";
        }
    }
}