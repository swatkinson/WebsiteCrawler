using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Pinaka.Core;

namespace Pinaka.Core
{
    public class PinakaParameters
    {
        public bool Verbose { get; set; }
        public required string URL { get; set; }
        public bool Phone { get; set; }
        public bool Email { get; set; }
        public bool Address { get; set; }
        public bool Location { get; set; }
        public string? Output { get; set; }

        public bool Help { get; set; }

        private static Dictionary<string, string> _rawValues = new Dictionary<string, string>();
        public static Dictionary<string, string> RawValues
        {
            get
            {
                if (_rawValues.Count == 0)
                    throw new Exception("CLI parameters not yet parsed");
                return _rawValues;
            }
            set
            {
                _rawValues = value;
            }
        }

        public static PinakaParameters Parse(Dictionary<string, string> keyValuePairs)
        {
            var objParameters = new DyanamicInstance<PinakaParameters>();

            foreach (var key in keyValuePairs.Keys)
            {
                objParameters.SetParameter(key, keyValuePairs[key]);
            }

            if (string.IsNullOrWhiteSpace(objParameters.Instance.Output))
            {
                objParameters.Instance.Output = "Output.json";
            }

            return objParameters.Instance;
        }

        public static PinakaParameters Parse(string[] args)
        {
            var arguments = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                // Split the argument by '=' to handle key/value pairs
                string[] parts = arg.Split('=');

                // Check if the argument is in the format "key=value"
                if (parts.Length == 2)
                {
                    arguments[parts[0]] = parts[1];
                }
                // If not, assume it's just a named argument without a value
                else
                {
                    arguments[arg] = null;
                }
            }
            RawValues = arguments;
            return Parse(arguments);
        }

        public static void PrintHelp()
        {
            //Console.WriteLine("Help:");
            //Console.WriteLine("------");
            Console.WriteLine("Usage: Yank [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --help                     Display this help message");
            Console.WriteLine("  --verbose                  Enable verbose mode");
            Console.WriteLine("  --url=<url>                Specify the input URL");
            //Console.WriteLine("  --name=<personName>        Specify the input Person Name");
            //Console.WriteLine("  --company=<companyName>    Specify the input Company Name");
            Console.WriteLine("  --phone                    Whether you want phone number(s)");
            Console.WriteLine("  --email                    Whether you want email address(es)");
            Console.WriteLine("  --location                 Whether you want address(es)");
            Console.WriteLine("  --output=<filePath>        Specify output file path, if not provided it will be 'Output.json'");
        }
    }
}