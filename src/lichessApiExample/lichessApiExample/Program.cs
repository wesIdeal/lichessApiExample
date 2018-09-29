using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace lichessApiExample
{
    class Program
    {
        static Uri baseUri = new Uri("https://lichess.org");
        static void Main(string[] args)
        {
            var getGamesOpts = Parser.Default.ParseArguments<GetGamesOptions>(args)
                .WithParsed<GetGamesOptions>(opts => GetLichessGames(opts));

        }

        private static void GetLichessGames(GetGamesOptions opts)
        {
            try
            {
                var exportUri = baseUri + $"games/export/{opts.TargetUser}";
                //https://lichess.org/games/export/fptan?max=10
                StringBuilder queryString = new StringBuilder();

                var consoleDescription = $"Exporting games for {opts.TargetUser} with the following options:\r\n";
                consoleDescription += opts.GetDescription();
                Console.Write(consoleDescription);
                Console.WriteLine("Continue? y for Yes or any other character for no.");

                if (Char.ToLower(Console.ReadKey().KeyChar).Equals('y'))
                {
                    var fileText = string.Empty;
                    var url = $"{exportUri}{opts.CreateQueryString()}";
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
                    var request = (HttpWebRequest)HttpWebRequest.Create(url);
                    request.Method = "GET";
                    if (!string.IsNullOrWhiteSpace(opts.AccessToken))
                    {
                        request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {opts.AccessToken}");
                    }
                    request.ProtocolVersion = HttpVersion.Version10;

                    var response = (HttpWebResponse)request.GetResponse();
                    //var responseEncoding = Encoding.GetEncoding(response.CharacterSet);

                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default))
                    {
                        fileText = sr.ReadToEnd();
                    }
                    File.WriteAllText(opts.OutFile, fileText);

                }
                if (Debugger.IsAttached) { Console.ReadKey(); }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
                if (Debugger.IsAttached) { Console.ReadKey(); }
            }

        }


    }
    public static class DescHelper
    {
        public static object GetDefault(this Type t)
        {
            return Activator.CreateInstance(t); 
        }
        public static string CreateQueryString(this GetGamesOptions opts)
        {
            var qsOptions = new Dictionary<string, string>();

            foreach (var pInfo in opts.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(QueryStringDescriptionAttribute), false).Any()))
            {
                var value = pInfo.GetValue(opts);
                var type = pInfo.PropertyType;
                var defaultVal = type.GetDefault();
                if (!AreEqual(defaultVal, value, type))
                {
                    var attr = (QueryStringDescriptionAttribute[])pInfo.GetCustomAttributes(typeof(QueryStringDescriptionAttribute), false);
                    if (attr.Any())
                    { qsOptions.Add(attr.First().QueryStringKey, value.ToString()); }
                }
            }

            return qsOptions.CreateQueryString();

        }

        private static bool AreEqual(object defaultVal, object value, Type t)
        {
            if (value == null && defaultVal == null) return true;
            if (value == null ^ defaultVal == null) return false;
            if (defaultVal is IComparable && value is IComparable)
            {
                var v1 = (IComparable)defaultVal;
                var v2 = (IComparable)value;

                return v1 == v2;
            }
            else
            {
                var v1 = Convert.ChangeType(defaultVal, t);
                var v2 = Convert.ChangeType(value, t);
                return v1 == v2;
            }
        }

        public static string GetDescription(this GetGamesOptions opts)
        {
            StringBuilder sb = new StringBuilder();
            var descriptiveOptions = opts.GetType().GetProperties().Where(x => Attribute.IsDefined(x, typeof(DescriptionAttribute)) && Attribute.IsDefined(x, typeof(OptionAttribute)));
            foreach (var opt in descriptiveOptions)
            {
                var desc = opt.GetPropertyDescription(opts);
                var val = opt.GetPropertyValue(opts);
                sb.AppendLine($"{desc}: {val}");
            }
            return sb.ToString();
        }
        private static string CreateQueryString(this Dictionary<string, string> d)
        {
            StringBuilder sb = new StringBuilder();
            if (d.Any())
            {
                sb.Append("?" + string.Join("&", d.Select(x => $"{x.Key}={x.Value}")));
            }
            return sb.ToString();
        }
        public static string GetPropertyValue(this PropertyInfo opt, object o)
        {
            var val = opt.GetValue(o);
            if (opt.PropertyType.IsEnum)
            {
                return opt.GetEnumDescription(o);

            }
            if (opt.PropertyType.Equals(typeof(bool)))
            {
                return ((bool)val).Equals(true) ? "Yes" : "No";
            }
            return val.ToString();
        }

        public static string GetPropertyDescription(this PropertyInfo opt, object o)
        {
            var attrInfo = opt.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attrInfo.Any())
            {
                var attr = attrInfo[0] as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
            return opt.Name;
        }
        private static string GetEnumDescription(this PropertyInfo e, object o)
        {
            var value = e.GetValue(o);
            var attr = (DescriptionAttribute[])e.PropertyType.GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attr.Any())
            {
                return attr[0].Description;
            }
            return e.Name;
        }
        public static string GetDescription(this Enum e)
        {
            System.Reflection.FieldInfo oFieldInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])oFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return e.ToString();
            }
        }
    }
}
