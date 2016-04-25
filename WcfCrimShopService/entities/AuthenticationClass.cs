using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using Newtonsoft.Json;
using System.IO;

namespace WcfCrimShopService.entities
{
    public class AuthenticationClass
    {
        Objects.ConfigObject config = JsonConvert.DeserializeObject<Objects.ConfigObject>(File.ReadAllText(System.AppDomain.CurrentDomain.BaseDirectory + @"Config.json"));
        public string IsAuthenticated(string username, string pwd)
        {
            string isValidMember = string.Empty;
            string emailAddress = string.Empty;

            

            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, config.ActiveDirectoryInformation.domain))
            {
                // validate credentials
                bool isValid = pc.ValidateCredentials(username, pwd);
                
                if (isValid)
                {
                    //get the groups of the user
                    var src = UserPrincipal.FindByIdentity(pc, username).GetGroups(pc);
                    var usesr = UserPrincipal.FindByIdentity(pc, username);
                    
                    var result = new List<string>();
                    src.ToList().ForEach(sr => result.Add(sr.SamAccountName));
                    //*************************

                    UserPrincipal user = UserPrincipal.FindByIdentity(pc, username);
                    string[] groups = config.ActiveDirectoryInformation.group;
                    foreach (string group in groups)
                    {
                        GroupPrincipal gp = GroupPrincipal.FindByIdentity(pc, group);
                        if (user.IsMemberOf(gp))
                        {
                            //isValidMember = "authorized";
                            emailAddress = usesr.EmailAddress;
                            return emailAddress;
                        }
                        else
                        {
                            emailAddress = "0";
                        }
                    }
                    
                }
                else { emailAddress = "00"; }

            }

            return emailAddress;
        }
    }
}














