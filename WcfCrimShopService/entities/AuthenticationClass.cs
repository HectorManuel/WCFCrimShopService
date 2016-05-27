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
                }
                catch (Exception e)
                {
                    con.LogTransaction(username, e.Message);
                }
                
                
                if (isValid)
                {
                    con.LogTransaction(username, "usuario es valido");
                    //get the groups of the user
                    try
                    {
                        
                                           
                        var src = UserPrincipal.FindByIdentity(pc, username).GetGroups(pc);
                       
                        
                        
                        var result = new List<string>();

                        try
                        {
                            src.ToList().ForEach(sr => result.Add(sr.SamAccountName));
                        }
                        catch (Exception e)
                        {
                            con.LogTransaction(username, "Groupt to list SamAccountName" +  e.Message);
                        }
                        
                        //*************************
                        con.LogTransaction(username, "verificando los grupo6s");
                        //UserPrincipal user = UserPrincipal.FindByIdentity(pc, username);
                        string[] groups = config.ActiveDirectoryInformation.group;
                        
                        var usesr = UserPrincipal.FindByIdentity(pc, username);
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
                                emailAddress = "0";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        con.LogTransaction(username, e.Message);
                    }
                    
                    
                }
                else { emailAddress = "00"; }

            }

            return emailAddress;
        }
    }
}










