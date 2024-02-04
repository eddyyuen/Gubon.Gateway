using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gubon.Gateway.Store.FreeSql.Models.Constant
{
    public class BuiltIn
    {
        public static string[] LoadBalancingPolicyType
        {
            get
            {
                return new string[] { "PowerOfTwoChoices", "RoundRobin", "Random", "LeastRequests", "FirstAlphabetical" };
            }
        }

        public static string[] AvailableDestinationsPolicy
        {
            get
            {
                return new string[] { "HealthyAndUnknown", "HealthyOrPanic" };
            }
        }

        public static string[] VersionPolicyType
        {
            get
            {
                return new string[] { "RequestVersionOrLower", "RequestVersionOrHigher", "RequestVersionExact" };
            }
        }
        public static string[] RequestVersion
        {
            get
            {
                return new string[] { "1.0", "1.1", "2", "3" };
            }
        }

        public static string[] SslProtocols
        {
            get
            {
                return new string[] { "None", "Ssl2", "Ssl3", "Tls", "Tls11", "Tls12", "Tls13" };
            }
        }
        public static string[] FailurePolicyList
        {
            get
            {
                return new string[] { "Redistribute", "Return503Error" };
            }
        }
        public static string[] PolicyList
        {
            get
            {
                return new string[] { "HashCookie", "ArrCookie", "Cookie", "CustomHeader" };
            }
        }
        public static string[] SecurePolicyList
        {
            get
            {
                return new string[] { "SameAsRequest", "Always", "None" };
            }
        }
        public static string[] SameSiteModeList
        {
            get
            {
                return new string[] { "Unspecified", "None", "Lax" , "Strict" };
            }
        }
    }
  
}
