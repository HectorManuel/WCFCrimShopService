using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Security;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net.Mime;
using System.Threading;
using Esri.ArcGISRuntime.Http;

namespace WcfCrimShopService.entities
{
    public class Geoprocessing
    {
        string PhotoPdfUri = string.Empty;
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
       
        WebClient webClient = new WebClient();
        
        
        //string path = System.AppDomain.CurrentDomain.BaseDirectory + @"OrderFolder\";
        /// <summary>
        /// Get the download storage path
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            
            return config.OrderDownloadStorage;
        }

        /// <summary>
        /// Generate the authentication token for the arcgis services
        /// </summary>
        /// <returns></returns>
        public async Task GenerateToken()
        {
            try
            {
                var opt = new GenerateTokenOptions();
                opt.TokenAuthenticationType = TokenAuthenticationType.ArcGISToken;

                var cred = await IdentityManager.Current.GenerateCredentialAsync(config.PortalAuthentication.ServiceUri, 
                                                                                config.PortalAuthentication.username, 
                                                                                config.PortalAuthentication.password, 
                                                                                opt);

                IdentityManager.Current.AddCredential(cred);

            }
            catch (ArcGISWebException webExp)
            {
                Debug.WriteLine("Unable to authenticate : " + webExp.Message);
            }
            
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
            DBConnection connection = new DBConnection();
            string storePath = string.Empty;
            try
            {
                //List<Objects.OrderItemCatastral> allCat
                //var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
                //string taskName = "Mapas de Catastro";
                var gp = new Geoprocessor(new Uri(config.MapasCatastral));

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

                while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut && result.JobStatus != GPJobStatus.Failed)
                {
                    try 
                    {
                        result = await gp.CheckJobStatusAsync(result.JobID);

                        Debug.WriteLine(result.JobStatus +" Catastral ");
                        await Task.Delay(5000);
                    }
                    catch (System.Threading.Tasks.TaskCanceledException)
                    {
                        //connection.LogTransaction(ctrl, "ThreadAbortEsception");
                        Debug.WriteLine("Cancel Exception");
                        connection.UpdateFailedCad(ctrl, array, "false");
                    }

                }
                
                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                    if (outParam != null && outParam.Uri != null)
                    {
                        //OficialCatUri = outParam.Uri;
                        string fileName = @"\" + template + ".pdf";
                        try
                        {
                            storePath = MakeStoreFolder(ctrl, fileName);
                            string save = LoadUriPdf(outParam.Uri, storePath, fileName);
                      
                            Objects.path = storePath;
                        }
                        catch (Exception e)
                        {
                            //Debug.WriteLine("Error: ", e.ToString());
                            connection.LogTransaction(ctrl, e.Message);
                            Objects.path = "Error";
                        }

                    }

                }
                else
                {

                    if (result.JobStatus ==GPJobStatus.Failed){
                        return "failed";
                    }

                    if (result.JobStatus == GPJobStatus.TimedOut)
                    {
                        return "time out";
                    }
                        

                }
            }
            catch (Exception e)
            {
                connection.LogTransaction(ctrl, e.Message);
            }
            
            return storePath;
        }
        
        /// <summary>
        /// OfficialMaps1 read the list of Cadastral maps that needs to create and separate them in 1:10000 and 1:1000
        /// calling the CallingMaps function for each scale.
        /// </summary>
        /// <param name="cadastre"></param>
        /// <returns></returns>
        public async Task<string> OficialMaps(List<Objects.OrderItemCatastral> cadastre)
        {
            List<Objects.Scale> listScale10 = new List<Objects.Scale>();
            List<Objects.Scale> listScale1 = new List<Objects.Scale>();
            DBConnection connection = new DBConnection();
            foreach (var cad in cadastre)
            {
                if (cad.template == "MapaCatastral_10k")
                {
                    listScale10.Add(new Objects.Scale 
                    { 
                        template = cad.template,
                        geo = "true",
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
                if (cad.template == "MapaCatastral_1k")
                {
                    listScale1.Add(new Objects.Scale
                    {
                        template = cad.template,
                        geo = "true",
                        cuad = cad.cuadricula,
                        controlNum = cad.ControlNumber
                    });
                }
            }
            string array2 = string.Empty;
            string array = string.Empty;
            string storePath = string.Empty;
            #region 1:10k menor o igual a 25
            if (listScale10.Count > 0 && listScale10.Count <=25)
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
                try
                {
                    storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);
                }
                catch (Exception e)
                {
                    storePath = "failed";
                    Debug.WriteLine(e.Message);
                }
                
                switch (storePath){
                    case "failed":
                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k " + storePath);
                        connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                        Thread.Sleep(5000);
                        Debug.WriteLine(storePath);
                        connection.LogTransaction(listScale10[0].controlNum, "Trying Mapa Catastral oficial 1:10k");
                        storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);
                        if (storePath == "failed" || storePath == "time out")
                        {
                            connection.LogTransaction(listScale10[0].controlNum, "1:10k Geoprocess failed again");
                            connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                        }
                        else
                        {
                            connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k creado");
                            connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                        }
                        break;
                    case "time out":
                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k " + storePath);
                        connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                        Thread.Sleep(5000);
                        connection.LogTransaction(listScale10[0].controlNum, "Trying Mapa Catastral oficial 1:10k");
                        storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);
                        if (storePath == "failed" || storePath == "time out")
                        {
                            connection.LogTransaction(listScale10[0].controlNum, "1:10k Geoprocess failed again");
                            connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                        }
                        else
                        {
                            connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k creado");
                            connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                        }
                        break;
                    default:
                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k creado");
                        connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                        break;
                }

            }
            #endregion
            #region 1:10k mayor de 25
            else if (listScale10.Count > 25)
            {
                int count =0;
                try
                {
                    int parts = 0;
                    for (int i = 0; i < listScale10.Count; i++)
                    {
                        
                        if (count == 0)
                        {
                            array += "(";
                        }
                        if (count != 0)
                        {
                            array += ",";
                        }
                        array += "'" + listScale10[i].cuad + "'";
                        count++;
                        if (count == 25 || i == listScale10.Count-1)
                        {
                            array += ")";
                            Thread.Sleep(5000);
                            try
                            {
                                storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);


                                switch (storePath)
                                {
                                    case "failed":
                                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k " + storePath);
                                        //connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                                        Thread.Sleep(5000);
                                        connection.LogTransaction(listScale10[0].controlNum, "Trying Mapa Catastral oficial 1:10k");
                                        storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);
                                        if (storePath == "failed" || storePath == "time out")
                                        {
                                            connection.LogTransaction(listScale10[0].controlNum, "1:10k Geoprocess failed again");
                                            //connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                                            connection.UpdateFailedCad(listScale10[0].controlNum, array, "false");
                                            /** 
                                             * function para denotar como falso solo las cuadriculas qe no se han creado.**/
                                        }
                                        else
                                        {
                                            connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k creado");
                                            connection.UpdateFailedCad(listScale10[0].controlNum, array, "true");
                                            //connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                                        }
                                        break;
                                    case "time out":
                                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k " + storePath);
                                        //connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                                        Thread.Sleep(5000);
                                        connection.LogTransaction(listScale10[0].controlNum, "Trying Mapa Catastral oficial 1:10k");
                                        storePath = await CallingMaps(listScale10[0].template, array, listScale10[0].geo, listScale10[0].controlNum);
                                        if (storePath == "failed" || storePath == "time out")
                                        {
                                            connection.LogTransaction(listScale10[0].controlNum, "1:10k Geoprocess failed again");
                                           // connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "false");
                                            connection.UpdateFailedCad(listScale10[0].controlNum, array, "false");
                                        }
                                        else
                                        {
                                            connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k creado");
                                            connection.UpdateFailedCad(listScale10[0].controlNum, array, "true");
                                           // connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                                        }
                                        break;
                                    default:
                                        connection.LogTransaction(listScale10[0].controlNum, "Mapa Catastral oficial 1:10k ("+ (parts+=1) +") creado");
                                        connection.UpdateFailedCad(listScale10[0].controlNum, array, "true");
                                        //connection.UpdateCadStatus(listScale10[0].controlNum, listScale10[0].template, "1:10000", "true");
                                        break;
                                }

                                
                            }
                            catch (Exception e)
                            {
                                connection.LogTransaction(listScale10[0].controlNum, e.Message + " 1:10000");
                            }
                            array = string.Empty;
                            count = 0;
                        }
                    }
                }
                catch
                {
                    storePath = "failed";
                    //Debug.WriteLine(e.Message);
                }

                


            }
            #endregion
            #region 1:1k menor o igual a 50
            if (listScale1.Count > 0 && listScale1.Count<=50)
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
                try
                {
                    storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
                }
                catch (Exception e)
                {
                    connection.LogTransaction(listScale1[0].controlNum, e.Message + " 1:1000");
                }
                
                switch (storePath)
                {
                    case "failed":
                        connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k " + storePath);
                        //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                        Thread.Sleep(5000);
                        connection.LogTransaction(listScale1[0].controlNum, "Trying Mapa Catastral oficial 1:1k");
                        storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
                        if (storePath == "failed" || storePath == "time out")
                        {
                            connection.LogTransaction(listScale1[0].controlNum, "1:1k Geoprocess failed again");
                            //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                        }
                        else
                        {
                            connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado");
                            //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                        }
                        break;
                    case "time out":
                        connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k " + storePath);
                        connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                        Thread.Sleep(5000);
                        connection.LogTransaction(listScale1[0].controlNum, "Trying Mapa Catastral oficial 1:1k");
                        storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
                        if (storePath == "failed" || storePath == "time out")
                        {
                            connection.LogTransaction(listScale1[0].controlNum, "1:1k Geoprocess failed again");
                            connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                        }
                        else
                        {
                            connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado");
                            connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                        }
                        break;
                    default:
                        connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado");
                        connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                        break;
                }
            }
#endregion
            #region 1:1K mayo de 50
            else if (listScale1.Count > 50)
            {
                int count = 0;
                try
                {
                    int parts = 0;
                    for (int i = 0; i < listScale1.Count; i++)
                    {
                        if (count == 0)
                        {
                            array2 += "(";
                        }
                        if (count != 0)
                        {
                            array2 += ",";
                        }
                        array2 += "'" + listScale1[i].cuad + "'";
                        count++;
                        if (count == 50 || i == listScale1.Count-1)
                        {
                            array2 += ")";
                            Thread.Sleep(5000);
                            storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);

                            switch (storePath)
                            {
                                case "failed":
                                    connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k " + storePath);
                                    //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                                    Thread.Sleep(5000);
                                    connection.LogTransaction(listScale1[0].controlNum, "Trying Mapa Catastral oficial 1:1k");
                                    storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
                                    if (storePath == "failed" || storePath == "time out")
                                    {
                                        connection.LogTransaction(listScale1[0].controlNum, "1:1k Geoprocess failed again");
                                        //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                                        connection.UpdateFailedCad(listScale1[0].controlNum, array2, "false");
                                    }
                                    else
                                    {
                                        connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado");
                                        connection.UpdateFailedCad(listScale1[0].controlNum, array2, "true");
                                        //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                                    }
                                    break;
                                case "time out":
                                    connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k " + storePath);
                                    //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                                    Thread.Sleep(5000);
                                    connection.LogTransaction(listScale1[0].controlNum, "Trying Mapa Catastral oficial 1:1k");
                                    storePath = await CallingMaps(listScale1[0].template, array2, listScale1[0].geo, listScale1[0].controlNum);
                                    if (storePath == "failed" || storePath == "time out")
                                    {
                                        connection.LogTransaction(listScale1[0].controlNum, "1:1k Geoprocess failed again");
                                        //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "false");
                                        connection.UpdateFailedCad(listScale1[0].controlNum, array2, "false");
                                    }
                                    else
                                    {
                                        connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado");
                                        connection.UpdateFailedCad(listScale1[0].controlNum, array2, "true");
                                        //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                                    }
                                    break;
                                default:
                                    connection.LogTransaction(listScale1[0].controlNum, "Mapa Catastral oficial 1:1k creado parte" + parts++);
                                    connection.UpdateFailedCad(listScale1[0].controlNum, array2, "true");
                                    //connection.UpdateCadStatus(listScale1[0].controlNum, listScale1[0].template, "1:1000", "true");
                                    break;
                            }
                            
                            count = 0;
                            array2 = string.Empty;
                        }
                    }
                }
                catch
                {
                    storePath = "failed";
                    //Debug.WriteLine(e.Message);
                }

                


            }
            #endregion
            return storePath;
        }

        /// <summary>
        /// Retrieve the list of Aerial Photos of the order and creates a pdf for each one.
        /// </summary>
        /// <param name="allPics"></param>
        /// <returns></returns>
        public async Task<string> FotoAerea(Objects.OrderItemPhoto pic)//allPics
        {
            
            string zipPath = string.Empty;
            DBConnection connection = new DBConnection();
            string number = string.Empty;
            try
            {
                //foreach (var pic in allPics)
               // {
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
                    string title = pic.title;

                    //var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
                    //var taskName = "Mapas de Fotos Aerea";
                    var gp = new Geoprocessor(new Uri(config.FotoAereaUrl));
   
                    
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
                    var _title = new GPString("Title", title);

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
                    parameter.GPParameters.Add(_title);
                    await Task.Delay(2000);
                    var result = await gp.SubmitJobAsync(parameter);
                    while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut && result.JobStatus != GPJobStatus.Failed)
                    {
                        try
                        {
                            result = await gp.CheckJobStatusAsync(result.JobID);
                            Debug.WriteLine(result.JobStatus + "foto aerea");
                            await Task.Delay(5000);
                        }
                        catch (TaskCanceledException)
                        {
                            Debug.WriteLine("Abort Exception");
                            System.Threading.Thread.ResetAbort();
                        }

                    }

                    if (result.JobStatus == GPJobStatus.Succeeded)
                    {
                        var outParam = await gp.GetResultDataAsync(result.JobID, "Output_File") as GPDataFile;

                        if (outParam != null && outParam.Uri != null)
                        {
                            parcelTitle = FileNameValidation(pic.Parcel);
                            string fileName = @"\" + parcelTitle + ".pdf";
                            try
                            {
                                zipPath = MakeStoreFolder(cNumber, fileName);
                                string saved = LoadUriPdf(outParam.Uri, zipPath, fileName);
                                connection.LogTransaction(cNumber, "Foto Aerea Creada");
                                connection.UpdatephotoStatus(cNumber, template, parcelTitle, sub_Title, title, "true");
                                Objects.path = zipPath;

                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("Process failed", e.ToString());
                                connection.LogTransaction(cNumber, "Foto Aerea No Creada");
                                connection.UpdatephotoStatus(cNumber, template, parcelTitle, sub_Title, title, "false");
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
                        connection.LogTransaction(cNumber, "Foto Aerea No Creada");
                        connection.UpdatephotoStatus(cNumber, template, parcelTitle, sub_Title, title, "false");
                        Debug.WriteLine(message);
                    }
                //}
            }
            catch (Exception ex)
            {
                connection.LogTransaction(number, ex.Message + " error con arcgis llamados");
            }
            

            return zipPath;
        }

        /// <summary>
        /// Task to run the ExtractData geoprocess 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public async Task<string> ExtractData(Objects.ElementoDeExtraccion element) // Elemento de extraccino
        {
            string zipPath = string.Empty;
            DBConnection connection = new DBConnection();
            string number = string.Empty;
            try
            {
                //foreach (var pic in allPics)
                // {
                string areaOfInterest = element.Area_of_Interest;
                string cNumber = element.ControlNumber;
                string format = element.Feature_Format;
                string rasterFormat = element.Raster_Format;
                string layersToClip = element.Layers_to_Clip;
                List<GPString> test = new List<GPString>();
                


                //var serviceURL = "https://www.satasgis.crimpr.net/crimgis/rest/services/Mapas/ExtractData/GPServer/";
                //var taskName = "Extrae y comprime data";
                
                var gp = new Geoprocessor(new Uri(config.ExtractDataUrl));
                
                var parameter = new GPInputParameter();
                var layers = new GPString("Layers_to_Clip", layersToClip);
                test.Add(layers);
                var multi = new GPMultiValue<GPString>("Layers_to_Clip", test);
                var area = new GPString("Area_of_Interest", areaOfInterest);
                var featureFormat = new GPString("Feature_Format", format);
                var raster = new GPString("Raster_Format", rasterFormat);
                //var test = new GPMultiValue<GPString>(List<GPString>Test);

                parameter.GPParameters.Add(multi);
                parameter.GPParameters.Add(area);
                parameter.GPParameters.Add(featureFormat);
                parameter.GPParameters.Add(raster);
               
                await Task.Delay(2000);
                var result = await gp.SubmitJobAsync(parameter);
                while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted && result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut && result.JobStatus != GPJobStatus.Failed)
                {
                    try
                    {
                        result = await gp.CheckJobStatusAsync(result.JobID);
                        Debug.WriteLine(result.JobStatus + "Extraccion");
                        await Task.Delay(5000);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("Task Cancelled Exception");
                        connection.UpdateExtractDataStatus(cNumber, layersToClip, areaOfInterest, format, rasterFormat, "false");
                    }

                }

                if (result.JobStatus == GPJobStatus.Succeeded)
                {
                    var outParam = await gp.GetResultDataAsync(result.JobID, "Output_Zip_File") as GPDataFile;

                    if (outParam != null && outParam.Uri != null)
                    {
                        //parcelTitle = FileNameValidation(pic.Parcel);
                        string fileName = @"\ExtraccionParcelasOutput.zip";
                        try
                        {
                            zipPath = MakeStoreFolder(cNumber, fileName);
                            string saved = LoadUriPdf(outParam.Uri, zipPath, fileName);
                            connection.LogTransaction(cNumber, "Elemento Extraccion Creado");
                            connection.UpdateExtractDataStatus(cNumber, layersToClip, areaOfInterest, format, rasterFormat, "true");
                            Objects.path = zipPath;

                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Process failed", e.Message.ToString());
                            connection.LogTransaction(cNumber, "Elemento Extraccion No Creada");
                            connection.UpdateExtractDataStatus(cNumber, layersToClip, areaOfInterest, format, rasterFormat, "false");
                        }
                    }
                }
                else
                {
                    var message = result.JobStatus ;
                    connection.LogTransaction(cNumber, "Extraccion No Creada"+ message);
                    connection.UpdateExtractDataStatus(cNumber, layersToClip, areaOfInterest, format, rasterFormat, "false");
                    Debug.WriteLine(message);
                }
                //}
            }
            catch (Exception ex)
            {
                connection.LogTransaction(number, ex.Message + " error con arcgis llamados");
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
            string path = GetPath();
            var folderToSave = path + cnNumber;
            var Pfile = folderToSave + file;
            DirectoryInfo dir;
            try
            {
                if (Directory.Exists(folderToSave))
                {
                    
                    return folderToSave;
                }
                else
                {
                    dir = Directory.CreateDirectory(folderToSave);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Directory could not be created : ", e.Message.ToString());
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
            DBConnection conForLog = new DBConnection();
            string file = folder + fileName;
            int duplicate = 0;
            try
            {
                if (!File.Exists(file))
                {
                    webClient.DownloadFile(uri, file);
                }
                else
                {
                    //File.Delete(file);
                    string tempFilename = string.Empty;
                    while (File.Exists(file))
                    {
                        duplicate++;
                        if (fileName.Contains(".zip"))
                        {
                            tempFilename = fileName.Replace(".zip", "(" + duplicate + ").zip");
                        }
                        else
                        {
                            tempFilename = fileName.Replace(".pdf", "(" + duplicate + ").pdf");
                        }
                        
                        file = folder + tempFilename;
                    }

                    try
                    {
                        webClient.DownloadFile(uri, file);
                    }
                    catch
                    {
                        try
                        {
                            webClient.DownloadFile(uri, file);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        
                    }
                    
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
        public string ZipAndSendEmail(string orderFolderPath, string clientEmail, string control)
        {
            DBConnection conForLog = new DBConnection();
            string zipPath = orderFolderPath + ".zip";
            //zip file creation
            //verify that the zip file doesnt exist
            if (!File.Exists(zipPath))
            {
                try
                {
                    ZipFile.CreateFromDirectory(orderFolderPath, zipPath, CompressionLevel.Optimal, true);
                }
                catch
                {
                    ZipFile.CreateFromDirectory(orderFolderPath, zipPath, CompressionLevel.Optimal, true);
                }

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

                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient smtpServer = new SmtpClient(config.EmailConfiguration.SMTPClient);
                    mail.From = new MailAddress(config.EmailConfiguration.MailAddress);
                    mail.To.Add(clientEmail);
                    mail.Subject = "Productos Cartografícos Núm. Control: " + control;
                    mail.BodyEncoding = System.Text.Encoding.UTF8;
                    //como body voy a enviar un url para llamar el zipPath file;

                    DateTime date = new DateTime();
                    date = DateTime.Now;
                    DateTime expDate = date.AddDays(5);
                    string expirationDate = expDate.ToString("d MMMM yyyy", CultureInfo.CreateSpecificCulture("es-PR"));

                    mail.Body = "Su orden esta lista, la mista estada diponible hasta el  "+ expirationDate +" y puede ser descargada del siguiente enlace: "+ config.MailDownloadPath + control + ".zip";


                    string htmlBody = "<div>Su Orden de productos cartografícos esta lista</div>";
                    htmlBody += "<div>La misma estara disponible hasta <b>" + expirationDate + "</b></div>"; 
                    htmlBody += "<div>Puede descargar el archivo del siguiente enlace: ";
                    htmlBody += "<a href=\"" + config.MailDownloadPath + control + ".zip" + "\">Presione para descargar</a></div>";
                    htmlBody += "<p>Contenido de la orden numero "+ control +":</p>";
                    htmlBody += "<ul>";


                    string photos = conForLog.GetPhotoProducts(control);
                    string cadastre = conForLog.GetCadastralProducts(control);
                    string listAdyacent = conForLog.GetListProducts(control);
                    string extraccion = conForLog.GetExtractDataItem(control);

                    if (!string.IsNullOrEmpty(photos))
                    {
                        htmlBody += photos;
                    }
                    if (!string.IsNullOrEmpty(listAdyacent))
                    {
                        htmlBody += listAdyacent;
                    }
                    if (!string.IsNullOrEmpty(extraccion))
                    {
                        htmlBody += extraccion;
                    }
                    if (!string.IsNullOrEmpty(cadastre))
                    {
                        htmlBody += cadastre;
                    }

                    htmlBody += "</ul>";

                    ContentType mimeType = new ContentType("text/html");

                    AlternateView alternate = AlternateView.CreateAlternateViewFromString(htmlBody, mimeType);
                    mail.AlternateViews.Add(alternate);

                    string updatingPath = conForLog.UpdateFolderPath(control, config.MailDownloadPath + control + ".zip");

                    smtpServer.Port = config.EmailConfiguration.port;
                    smtpServer.Credentials = new System.Net.NetworkCredential(config.EmailConfiguration.Username, config.EmailConfiguration.Password);
                    smtpServer.EnableSsl = false;

                    try
                    {
                        smtpServer.Send(mail);
                        conForLog.LogTransaction(control, "Email send");
                        Debug.WriteLine("MailSend");
                        Directory.Delete(orderFolderPath, true);
                    }
                    catch (Exception e)
                    {
                        conForLog.LogTransaction(control, e.Message);
                    }

                }
                catch (Exception e)
                {
                    conForLog.LogTransaction(control, e.Message);
                    Debug.WriteLine(e.ToString());
                }

            }
            return zipPath;
        }

        /// <summary>
        /// this function is in charge of generating the list of adjacent parcels in both pdf and csv.
        /// </summary>
        /// <param name="itemsFromDb"></param>
        /// <param name="customerName"></param>
        /// <returns>the location where it is stored</returns>
        public string AdyacentListGenerator(List<Objects.OrderItemList> itemsFromDb, string customerName)
        {
            //Objects.ListaCol listaCombinada = new Objects.ListaCol();
            string zipPath = string.Empty;

            foreach (var lista in itemsFromDb)
            {
                string name = FileNameValidation(lista.itemName);
                
                DBConnection conect = new DBConnection();
                zipPath = MakeStoreFolder(itemsFromDb[0].ControlNumber, @"\" + name + ".pdf");
                string pdfName = Path.Combine(zipPath, name + ".pdf");
                Objects.ListaCol lisCol = JsonConvert.DeserializeObject<Objects.ListaCol>(lista.item);
                //create csv file
                string csvPath = Path.Combine(zipPath, name + ".csv");
                try
                {
                    int dup = 0;
                    if (File.Exists(pdfName))
                    {
                        while (File.Exists(pdfName))
                        {
                            dup++;
                            pdfName = Path.Combine(zipPath, name + "(" + dup + ").pdf");
                            csvPath = Path.Combine(zipPath, name +"("+ dup + ").csv");
                        }

                    }

                    if (!File.Exists(csvPath))
                    {
                        File.Create(csvPath).Dispose();
                    }
                    else
                    {
                        File.Delete(csvPath);
                        File.Create(csvPath).Dispose();
                    }
                    StringBuilder csvContent = new StringBuilder();

                    using (Document doc = new Document(new RectangleReadOnly(1191, 842), 25, 25, 45, 35))//A3 (842,1191) nearest to 11x17, A4 (595,842) nearest to 8.5x11
                    {
                        
                        PdfWriter wr = PdfWriter.GetInstance(doc, new FileStream(pdfName, FileMode.Create));

                        ColindantePdfEventHandler e = new ColindantePdfEventHandler()
                        {
                            cantidad = lisCol.ListaColindante.Count.ToString(),
                            controlNumber = lista.ControlNumber,
                            contribuyente = customerName,
                            Parcela = name
                        };

                        wr.PageEvent = e;

                        doc.Open();


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

                        //csv headers
                        string headers = "Parcela de Procedencia" + "," + "Parcela" + "," + "Catastro" + "," + "Municipio" + "," + "Dueño" + "," + "Dirección Física" + "," + "Dirección Postal";
                        csvContent.AppendLine(headers);


                        foreach (var item in lisCol.ListaColindante)
                        {
                            table.AddCell(item.ParcelaProcedencia);
                            table.AddCell(item.Parcela);
                            table.AddCell(item.Catastro);
                            table.AddCell(item.Municipio);
                            table.AddCell(item.Dueno);
                            table.AddCell(item.DireccionFisica);
                            table.AddCell(item.DireccionPostal);

                            //string for the csv rows
                            string row = item.ParcelaProcedencia + "," + item.Parcela + "," + item.Catastro + "," + item.Municipio + "," + item.Dueno + "," + item.DireccionFisica + "," + item.DireccionPostal;
                            csvContent.AppendLine(row);

                        }


                        doc.Add(table);

                        //enter data in csv 
                        using (StreamWriter file = new StreamWriter(new FileStream(csvPath, FileMode.Create), Encoding.UTF8))
                        {
                            file.Write(csvContent.ToString());
                        }

                        doc.Close();
                        conect.LogTransaction(lista.ControlNumber, "Lista Colindante Creada");
                        conect.UpdateListStatus(lista.ControlNumber, lista.itemName, lista.item, "true");
                        Objects.path = zipPath;
                    }
                }
                catch (Exception e)
                {
                    conect.LogTransaction(lista.ControlNumber, e.Message);
                    conect.UpdateListStatus(lista.ControlNumber, lista.itemName, lista.item, "true");
                    Objects.path = "Error";
                }
            }
            return zipPath;
        }

        /// <summary>
        /// Calculate the price for the product item requested
        /// </summary>
        /// <param name="item"></param>
        /// <param name="qty"></param>
        /// <returns>subtotal</returns>
        public string CalculatePrice(string item,int qty)
        {
            string total = string.Empty;
            DBConnection responseHandler = new DBConnection();
            //Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText( System.AppDomain.CurrentDomain.BaseDirectory+ @"Config.json"));


            Objects.ProductPrice price = responseHandler.PriceProduct(item.ToUpper(), qty);

            total =Convert.ToString(qty * price.price);
            
            
            return total;
        }

        /// <summary>
        /// Validate the name for the file to avoid illegal characters
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static string FileNameValidation(string filename)
        {
            try
            {
                return Regex.Replace(filename, @"[^\w\d()_-]", "", RegexOptions.None, TimeSpan.FromSeconds(1));
            }
            catch (RegexMatchTimeoutException)
            {
                return filename;
            }
        }

        /// <summary>
        /// Make a zip file for any purpose, use when files need to be created again due to a previous failure
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string MakeZipAgain(string path, string controlNumber)
        {
            string zipPath = path + ".zip";
            string zipName = controlNumber + ".zip";
            //zip file creation
            //verify that the zip file doesnt exist
            if (!File.Exists(zipPath))
            {
                try
                {
                    ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                }
                catch
                {
                    ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
                }


            }
            else
            {
                string temp = zipPath;
                
                int dup = 0;
                while (File.Exists(temp))
                {
                    dup++;
                    temp = temp.Replace(".zip", "(" + dup + ").zip");
                    zipName = zipName.Replace(".zip", "(" + dup + ").zip");
                }
                zipPath = temp;

                //File.Delete(zipPath);
                ZipFile.CreateFromDirectory(path, zipPath, CompressionLevel.Optimal, true);
            }

            

            return zipName;
        }
    }
}