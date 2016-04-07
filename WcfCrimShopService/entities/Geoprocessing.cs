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
        string PhotoPdfUri = string.Empty;
        //Uri OficialCatUri = new Uri("");
        //Uri ListaColindanteUri = new Uri("");
        WebClient webClient = new WebClient();
        //string path = @"S:\14_CRIM_2014-2015\Operaciones\Datos\Trabajado\Productos Cartograficos\PrintTest";
        string path = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives";
        //string map, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string cNumber)
        public async Task<string> FotoAerea(string map, string cNumber, string format, string template, string geoInfo, string parcelTitle, string sub_Title, string bf, string pr, string bf_distance_unit)
        {
            //cNumber = "1234";

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

            var buffer = new GPString("Buffer", bf);
            var parcelList = new GPString("Parcelas", pr);

            var bufferDistance = new GPString("Buffer_Distance_Units", bf_distance_unit);
           
            //Esri.ArcGISRuntime.Geometry.LinearUnit esriMeters = Esri.ArcGISRuntime.Geometry.LinearUnits.Meters;
            
            //var bufferDistance = new GPLinearUnit("Buffer_Distance_Units", esriMeters, 10);


            //add GPParameters to the parameter collection  
            parameter.GPParameters.Add(jsonMap);
            parameter.GPParameters.Add(Format);
            parameter.GPParameters.Add(layoutTemplate);
            parameter.GPParameters.Add(georef);
            parameter.GPParameters.Add(parcel);
            parameter.GPParameters.Add(subtitle);
            parameter.GPParameters.Add(control);
            //buffer parameters
            parameter.GPParameters.Add(buffer);
            parameter.GPParameters.Add(parcelList);
            parameter.GPParameters.Add(bufferDistance);

            ////executeTask with the parameter collection defgined above, await the result.
            var result = await gp.SubmitJobAsync(parameter);

            while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut && result.JobStatus != GPJobStatus.Failed)
            {
                result = await gp.CheckJobStatusAsync(result.JobID);
                Debug.WriteLine(result.JobStatus);
                await Task.Delay(1000);
            }
            string zipPath = string.Empty;
            if (result.JobStatus == GPJobStatus.Succeeded)
            {
                var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                if (outParam != null && outParam.Uri != null)
                {

                    //PhotoPdfUri = outParam.Uri;
                    //create temp directoy for downloads
                    //path = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives";
                    //string zipPath = @"C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives.zip";

                    string fileName = "\\" + parcelTitle + ".pdf";
                    //DirectoryInfo dir;
                    try
                    {
                        //verify is the directory exists. return the order folder
                        zipPath = MakeStoreFolder(cNumber, fileName);

                        #region verify the directory old
                        //if (Directory.Exists(path))
                        //{
                        //    Debug.WriteLine("directory: " + path + "   exist");
                        //}
                        //else
                        //{
                        //    //try to create the directory
                        //    dir = Directory.CreateDirectory(path);
                        //}

                        //var stremData = await webClient.OpenReadTaskAsync(outParam.Uri);
                        #endregion

                        //return the file path if it was created
                        string saved = LoadUriPdf(outParam.Uri, zipPath, fileName);


                        //webClient.DownloadFile(outParam.Uri, zipPath + folderName);
                        #region zip and email
                        ////zip file creation
                        //if (!File.Exists(zipPath))
                        //{
                        //    ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                        //}
                        //else
                        //{
                        //    File.Delete(zipPath);
                        //    ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                        //}


                        //if (File.Exists(zipPath))
                        //{
                        //    //dir.Delete();
                        //    Directory.Delete(path, true);
                        //    if (!Directory.Exists(path))
                        //    {
                        //        Debug.WriteLine("directory: " + path + "   deleted");
                        //    }
                        //    else
                        //    {
                        //        Debug.WriteLine(" unable to dele directory: " + path);
                        //    }

                        //    try
                        //    {
                        //        MailMessage mail = new MailMessage();
                        //        SmtpClient smtpServer = new SmtpClient("mail.crimpr.net");
                        //        mail.From = new MailAddress("cdprcasosweb@crimpr.net");
                        //        mail.To.Add("hasencio@gmtgis.com");
                        //        mail.Subject = "Test mail 1";
                        //        mail.Body = "mail with attachment";

                        //        Attachment attachment = new Attachment(zipPath);
                        //        mail.Attachments.Add(attachment);

                        //        smtpServer.Port = 25;
                        //        smtpServer.Credentials = new System.Net.NetworkCredential("CDPRCASOSWEB", "Cc123456");
                        //        smtpServer.EnableSsl = false;

                        //        smtpServer.Send(mail);
                        //        Debug.WriteLine("MailSend");
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        Debug.WriteLine(e.ToString());
                        //    }
                        //    finally { }

                        //}
                        #endregion
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

            return zipPath;
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
            string storePath = string.Empty;
            if (result.JobStatus == GPJobStatus.Succeeded)
            {
                var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                if (outParam != null && outParam.Uri != null)
                {
                    //OficialCatUri = outParam.Uri;
                    string fileName = @"\MapasCatastralesOficiales.pdf";
                    try
                    {
                        storePath = MakeStoreFolder(ctrl, fileName);
                        string save = LoadUriPdf(outParam.Uri, storePath, fileName);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error: ", e.ToString());
                    }
                    
                }

            }

            return storePath;
        }

        public async Task<string> CallingMaps(string template, string array, string geo, string ctrl)
        {
            //List<Objects.OrderItemCatastral> allCat
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
            string storePath = string.Empty;
            if (result.JobStatus == GPJobStatus.Succeeded)
            {
                var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                if (outParam != null && outParam.Uri != null)
                {
                    //OficialCatUri = outParam.Uri;
                    string fileName = @"\"+ template +".pdf";
                    try
                    {
                        storePath = MakeStoreFolder(ctrl, fileName);
                        string save = LoadUriPdf(outParam.Uri, storePath, fileName);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error: ", e.ToString());
                    }

                }

            }
            return storePath;
        }
        //string template, string array, string geo, string ctrl
        public async Task<string> OficialMaps1(List<Objects.OrderItemCatastral> cadastre)
        {
            List<Objects.Scale> listScale10 = new List<Objects.Scale>();
            List<Objects.Scale> listScale1 = new List<Objects.Scale>();

            foreach (var cad in cadastre)
            {
                if (cad.template == "MapaCatastral_10k")
                {
                    listScale10.Add(new Objects.Scale 
                    { 
                        template = cad.template,
                        geo = cad.itemName,
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
                if (cad.template == "MapaCatastral_1k")
                {
                    listScale1.Add(new Objects.Scale
                    {
                        template = cad.template,
                        geo = cad.itemName,
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
            }
            string array2 = string.Empty;
            string array = string.Empty;
            string storePath = string.Empty;
            if (listScale10.Count != 0)
            {
                array = "(";
                bool firstItem = true;
                foreach (var cad in listScale10)
                {
                    if (!firstItem)
                    {
                        array += ",";
                    }
                    firstItem = false;
                    array += "'" + cad.cuad + "'";
                }
                array += ")";
                //array.Replace(",)", ")");

                //storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);

            }

            if (listScale1.Count != 0)
            {
                array2 = "(";
                bool firstItem = true;
                foreach (var cad in listScale1)
                {
                    if (!firstItem)
                    {
                        array2 += ",";
                    }
                    firstItem = false;
                    array2 += "'" + cad.cuad + "'";
                }
                array2 += ")";
                //array2.Replace(",)", ")");

                storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
            }
                        

            return storePath;
        }

        public async Task<string> FotoAerea1(List<Objects.OrderItemPhoto> allPics)
        {
            string zipPath = string.Empty;
            foreach (var pic in allPics)
            {
                string map = pic.Item;
                string cNumber = pic.ControlNumber;
                string format = pic.Format;
                string template = pic.LayoutTemplate;
                string geoInfo = pic.GeorefInfo;
                string parcelTitle = pic.Parcel;
                string sub_Title = pic.subtitle;
                string bf = pic.buffer;
                string pr = pic.parcelList;
                string bf_distance_unit = pic.distance;

                var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
                var taskName = "Mapas de Fotos Aerea";
                var gp = new Geoprocessor(new Uri(serviceURL + "/" + taskName));
                var parameter = new GPInputParameter();
                var jsonMap = new GPString("Web_Map_As_JSON", map);
                var Format = new GPString("Format", format);
                var layoutTemplate = new GPString("Layout_Template", template);
                var georef = new GPString("Georef_info", geoInfo);
                var parcel = new GPString("Parcel", parcelTitle);
                var subtitle = new GPString("Subtitle", sub_Title);
                var control = new GPString("Control", cNumber);
                var buffer = new GPString("Buffer", bf);
                var parcelList = new GPString("Parcelas", pr);
                var bufferDistance = new GPString("Buffer_Distance_Units", bf_distance_unit);
                parameter.GPParameters.Add(jsonMap);
                parameter.GPParameters.Add(Format);
                parameter.GPParameters.Add(layoutTemplate);
                parameter.GPParameters.Add(georef);
                parameter.GPParameters.Add(parcel);
                parameter.GPParameters.Add(subtitle);
                parameter.GPParameters.Add(control);
                parameter.GPParameters.Add(buffer);
                parameter.GPParameters.Add(parcelList);
                parameter.GPParameters.Add(bufferDistance);
                var result = await gp.SubmitJobAsync(parameter);
                while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut && result.JobStatus != GPJobStatus.Failed)
                {
                    result = await gp.CheckJobStatusAsync(result.JobID);
                    Debug.WriteLine(result.JobStatus);
                    await Task.Delay(1000);
                }
                
                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                    if (outParam != null && outParam.Uri != null)
                    {
                        string fileName = @"\" + parcelTitle + ".pdf";
                        try
                        {
                            zipPath = MakeStoreFolder(cNumber, fileName);
                            string saved = LoadUriPdf(outParam.Uri, zipPath, fileName);
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
            }
            return zipPath;
        }


        //Im gonna pass the path were all the folders are stored, the control number of the order
        //to name the folder for the order with it, and the file name  that will be use for the pdf that will be created
        //if the folder exist it simply uses it it does not recreate it.
        public string MakeStoreFolder(string cnNumber, string file)
        {

            var folderToSave = path + @"\" + cnNumber;
            var Pfile = folderToSave + file;
            DirectoryInfo dir;
            try
            {
                if (Directory.Exists(folderToSave))
                {
                    if (File.Exists(Pfile))
                    {
                        File.Delete(Pfile);
                    }
                    return folderToSave;
                }
                else
                {
                    dir = Directory.CreateDirectory(folderToSave);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Directory coudl not be created : ",e.ToString());
            }
            return folderToSave;
        }

        //receive the uri, the folder  to save the file and the file name, verify that the file doesn't exist and 
        //create or recreate it in the folder of the order. then return the file path.
        public string LoadUriPdf(Uri uri, string folder, string fileName)
        {
            string file = folder + fileName;
            try
            {
                if (!File.Exists(file))
                {
                    webClient.DownloadFile(uri, file);
                }
                else
                {
                    File.Delete(file);
                    webClient.DownloadFile(uri, file);
                }
            }
            catch (Exception e)
            {
                file = e.ToString();
            }
            
            return file;
        }

        public string ZipAndSendEmail(string orderFolderPath, string clientEmail)
        {
            string zipPath = orderFolderPath + ".zip";
            //zip file creation
            //verify that the zip file doesnt exist
            if (!File.Exists(zipPath))
            {
                ZipFile.CreateFromDirectory(orderFolderPath, zipPath, CompressionLevel.Optimal, true);
            }
            else
            {
                File.Delete(zipPath);
                ZipFile.CreateFromDirectory(orderFolderPath, zipPath, CompressionLevel.Optimal, true);
            }

            //*****************************************************************
            //this area goes the generator for the urls of the service to download the file
            //*****************************************************************
            if (File.Exists(zipPath))
            {
                //dir.Delete();
                #region delete unzip directory
                //Directory.Delete(path, true);
                //if (!Directory.Exists(path))
                //{
                //    Debug.WriteLine("directory: " + path + "   deleted");
                //}
                //else
                //{
                //    Debug.WriteLine(" unable to dele directory: " + path);
                //}
                #endregion
                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient smtpServer = new SmtpClient("mail.crimpr.net");
                    mail.From = new MailAddress("cdprcasosweb@crimpr.net");
                    mail.To.Add(clientEmail);
                    mail.Subject = "Test mail 1";
                    //como body voy a enviar un url para llamar el zipPath file;
                    mail.Body = zipPath;

                    //Attachment attachment = new Attachment(zipPath);
                    //mail.Attachments.Add(attachment);

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
            return zipPath;
        }
    }
}