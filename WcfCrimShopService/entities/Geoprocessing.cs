using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;


namespace WcfCrimShopService.entities
{
    public class Geoprocessing
    {
        public async Task<string> FotoAerea(string map)
        {
            string testing = map;

            var serviceURL = "http://mapas.gmtgis.net/arcgis/rest/services/Geoprocesos/ProductosCartograficos/GPServer";
            var taskName = "AdvancedHighQualityPrinting";
            var taskCatastro = "Mapas de Catastro";
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