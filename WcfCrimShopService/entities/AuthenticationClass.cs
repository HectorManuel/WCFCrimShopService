using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using Newtonsoft.Json;
using System.IO;
using WcfCrimShopService.entities;
using System.Web.Hosting;

namespace WcfCrimShopService.entities
{
    public class AuthenticationClass
    {
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
        public string IsAuthenticated(string username, string pwd)
        {
            string isValidMember = string.Empty;
            string emailAddress = string.Empty;
            DBConnection con = new DBConnection();

            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, config.ActiveDirectoryInformation.domain))
            {
                bool isValid = false;
                // validate credentials
                try
                {
                    isValid = pc.ValidateCredentials(username, pwd);
                    var usesr = UserPrincipal.FindByIdentity(pc, username);
                    if (isValid)
                    {
                        con.LogTransaction(username, "usuario es valido");
                        //get the groups of the user
                        try
                        {

                            var result = new List<string>();

                            //*************************
                            con.LogTransaction(username, "verificando los grupos");
                            //UserPrincipal user = UserPrincipal.FindByIdentity(pc, username);
                            string[] groups = config.ActiveDirectoryInformation.group;

                            var src = UserPrincipal.FindByIdentity(pc, username).GetGroups(pc);

                            src.ToList().ForEach(sr => result.Add(sr.SamAccountName));
                            foreach (string group in groups)
                            {
                                GroupPrincipal gp = GroupPrincipal.FindByIdentity(pc, group);
                                if (usesr.IsMemberOf(gp))
                                {
                                    //isValidMember = "authorized";
                                    emailAddress = usesr.EmailAddress;
                                    con.LogTransaction(username, "usuario pertenece al grupo " + emailAddress);
                                    return emailAddress;

                                }
                                else
                                {
                                    emailAddress = "0"; // no pertenece al grupo
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            con.LogTransaction(username, e.Message);
                            return e.Message;
                        }


                    }
                    else { emailAddress = "00"; } // ususario o contraseña incorrecto
                }
                catch (Exception e)
                {
                    con.LogTransaction(username, e.Message);
                }
                
            }

            return emailAddress;
        }
    }
}










