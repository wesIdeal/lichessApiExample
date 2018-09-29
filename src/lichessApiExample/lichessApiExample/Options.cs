using CommandLine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lichessApiExample
{

    public class QueryStringDescriptionAttribute : DescriptionAttribute
    {
        public QueryStringDescriptionAttribute(string key, string txtDescription)
            :base (txtDescription)
        {
            QueryStringKey = key;
        }
        public string QueryStringKey { get; set; }
    }

    public enum SideColor {
        [Description("Both")]
        both,
        [Description("White")]
        w,
        [Description("Black")]
        b }
    //public class DescriptionAttribute : BaseAttribute{
    //    string _desc = "";
    //    public override string ToString()
    //    {
    //        return _desc;
    //    }
    //    public DescriptionAttribute(string desc)
    //    {
    //        _desc = desc;
    //    }
    //}
    public class Options : IOptions
    {
        private string _patPath = "";

        public string AccessToken { get; set; }


        public string AccessTokenPath
        {
            get
            {
                return _patPath;
            }
            set
            {
                _patPath = value;
                if (!File.Exists(_patPath))
                {
                    throw new ArgumentException("Access token path given does not exist.");
                }
                else
                {
                    AccessToken = File.ReadAllText(_patPath);
                }
            }
        }

    }
    [Verb("get-games", HelpText = "Get games for a user")]
    public class GetGamesOptions : Options
    {
        private string _outFile = "";
        [Option("targetUser", HelpText = "The user whose games you would like.")]
        [Description("User")]
        public string TargetUser { get; set; }
        [Option("max", HelpText = "Max games to retrieve. Blank for all.")]
        [QueryStringDescription("max","Max games")]
        public int? Max { get; set; }
        [Option("evals", HelpText = "Get evaluations, when available.", Default = false)]
        [QueryStringDescription("evals","Include Evaluations")]
        public bool Evals { get; set; }
        [Option('c', "color", HelpText = "Color of games to download. Either 'w' or 'b'")]
        [QueryStringDescription("color", "Target color")]
        public SideColor Color { get; set; }
        [Option('o', "out", HelpText = "Output file to store games. Will overwrite existing.", Required = true)]
        [Description("Output Path")]
        public string OutFile
        {
            get { return _outFile; }
            set
            {
                _outFile = value;
                var dir = Path.GetDirectoryName(_outFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }
    }
}
