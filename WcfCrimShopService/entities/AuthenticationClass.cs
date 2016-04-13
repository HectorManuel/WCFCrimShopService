using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Collections;
using System.DirectoryServices.AccountManagement;

namespace WcfCrimShopService.entities
{
    public class AuthenticationClass
    {

        public string IsAuthenticated(string username, string pwd)
        {
            string isValidMember = string.Empty;
            using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "gmt.domaingis.com"))
            {
                // validate credentials
                bool isValid = pc.ValidateCredentials(username, pwd);

                if (isValid)
                {
                    var src = UserPrincipal.FindByIdentity(pc, username).GetGroups(pc);
                    var result = new List<string>();
                    src.ToList().ForEach(sr => result.Add(sr.SamAccountName));

                    UserPrincipal user = UserPrincipal.FindByIdentity(pc, username);
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(pc, "Developers");

                    if (user.IsMemberOf(group))
                    {
                        isValidMember = "authorized";
                    }
                    else
                    {
                        isValidMember = "notAuthorized";
                    }
                }
                else { isValidMember = "Username or password incorrect"; }

            }

            return isValidMember;
        }
    }
}














