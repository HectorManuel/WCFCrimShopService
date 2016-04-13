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
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;


namespace WcfCrimShopService.entities
{
    public class Geoprocessing
    {
        string PhotoPdfUri = string.Empty;
       
        WebClient webClient = new WebClient();
        
        //string path = @"S:\14_CRIM_2014-2015\Operaciones\Datos\Trabajado\Productos Cartograficos\PrintTest"; C:\Users\hasencio\Documents\MyProjects\Store\WebApp\pdfArchives
        //string path = @"C:\Users\hasencio\Documents\visual studio 2013\Projects\WcfCrimShopService\WcfCrimShopService\OrderFolder";
        string path = System.AppDomain.CurrentDomain.BaseDirectory + @"OrderFolder\";
        
        //*****Generate the pdf of Aerial Photo calling the geoprocess and passing the required arguments
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

                        #region verify the directory old commented
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
                        #region zip and email comented
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

        //*****Generate the pdf of Oficial cadastral templates calling the geoprocess and passing the required arguments
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

        /// <summary>
        /// Function Specifically designto call and generate the cadastral template pdf
        /// </summary>
        /// <param name="template"></param>
        /// <param name="array"></param>
        /// <param name="geo"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// OfficialMaps1 read the list of Cadastral maps that needs to create and separate them in 1:10000 and 1:1000
        /// calling the CallingMaps function for each scale.
        /// </summary>
        /// <param name="cadastre"></param>
        /// <returns></returns>
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

                storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);

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

        /// <summary>
        /// Retrieve the list of Aerial Photos of the order and creates a pdf for each one.
        /// </summary>
        /// <param name="allPics"></param>
        /// <returns></returns>
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


        /// <summary>
        /// The function will create a folder with name the control number to store all the pdf of the same order
        /// under the same folder. if check if the folder does not exist it create it.
        /// </summary>
        /// <param name="cnNumber"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public string MakeStoreFolder(string cnNumber, string file)
        {

            var folderToSave = path + cnNumber;
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

        /// <summary>
        /// the function will take the uri created when the geoprocess are run and download the file to the folder
        /// created in the MakeStoreFolder function. Storing all the pdf related to the same order under the folder with the
        /// control number as name.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Once all files are created and downloaded, this fucntion is in charge of compressing the folder 
        /// with the archive inside and sending the url to get the zip file via email
        /// </summary>
        /// <param name="orderFolderPath"></param>
        /// <param name="clientEmail"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemsFromDb"></param>
        /// <param name="customerName"></param>
        /// <returns></returns>
        public string AdyacentListGenerator(List<Objects.OrderItemList> itemsFromDb, string customerName)
        {
            string zipPath = string.Empty;
            foreach (var lista in itemsFromDb)
            {
                zipPath = MakeStoreFolder(itemsFromDb[0].ControlNumber, @"\" + lista.itemName + "_colindante.pdf");
                Objects.ListaCol lisCol = JsonConvert.DeserializeObject<Objects.ListaCol>(lista.item);
                using (Document doc = new Document(new RectangleReadOnly(1191, 842), 25, 25, 45, 35))//A3 (842,1191) nearest to 11x17, A4 (595,842) nearest to 8.5x11
                {
                    PdfWriter wr = PdfWriter.GetInstance(doc, new FileStream(zipPath + @"\"+lista.itemName+"_colindante.pdf", FileMode.Create));

                    ColindantePdfEventHandler e = new ColindantePdfEventHandler()
                    {
                        cantidad = lisCol.ListaColindante.Count.ToString(),
                        controlNumber = lista.ControlNumber,
                        contribuyente = customerName,
                        Parcela = lista.itemName
                    };

                    wr.PageEvent = e;

                    doc.Open();



                    //completar eso, el json y el prionting del pdf
                    PdfPTable table = new PdfPTable(7);
                    table.WidthPercentage = 100;
                    float[] widths = { 12.00F, 8.00F, 10.00F, 10.00F, 12.00F, 20.00F, 20.00F };
                    //table.SetWidthPercentage(widths,new RectangleReadOnly(1191, 842));
                    table.SetTotalWidth(widths);
                    PdfPCell cell = new PdfPCell(new Phrase("Parcela de procedencia", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell.Colspan = 1;
                    cell.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell1 = new PdfPCell(new Phrase("Parcela", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell1.Colspan = 1;
                    cell1.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell2 = new PdfPCell(new Phrase("Catastro", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell2.Colspan = 1;
                    cell2.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell3 = new PdfPCell(new Phrase("Municipio", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell3.Colspan = 1;
                    cell3.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell4 = new PdfPCell(new Phrase("Dueño", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell4.Colspan = 1;
                    cell4.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell5 = new PdfPCell(new Phrase("Dirección Física", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell5.Colspan = 1;
                    cell5.HorizontalAlignment = 0;//0=left 1=center 2=right
                    PdfPCell cell6 = new PdfPCell(new Phrase("Dirección Postal", FontFactory.GetFont(FontFactory.HELVETICA_BOLD)));
                    cell6.Colspan = 1;
                    cell6.HorizontalAlignment = 0;//0=left 1=center 2=right

                    table.AddCell(cell);
                    table.AddCell(cell1);
                    table.AddCell(cell2);
                    table.AddCell(cell3);
                    table.AddCell(cell4);
                    table.AddCell(cell5);
                    table.AddCell(cell6);

                    foreach (var item in lisCol.ListaColindante)
                    {
                        table.AddCell(item.ParcelaProcedencia);
                        table.AddCell(item.Parcela);
                        table.AddCell(item.Catastro);
                        table.AddCell(item.Municipio);
                        table.AddCell(item.Dueno);
                        table.AddCell(item.DireccionFisica);
                        table.AddCell(item.DireccionPostal);

                    }


                    doc.Add(table);
                    doc.NewPage();

                    Paragraph par = new Paragraph("this is my first pdf new line");
                    doc.Add(par);
                    //JObject obj = JObject.Parse(json);

                    doc.Close();
                }
            }
            
            
            
            return zipPath;
        }
    }
}