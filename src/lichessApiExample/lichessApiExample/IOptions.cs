using CommandLine;
using CommandLine.Text;

namespace lichessApiExample
{
    public interface IOptions
    {
        [Option('t', "token", HelpText = "Personal access token. ")]
        string AccessToken { get; set; }
        [Option('p', "tokenPath", HelpText = "Path to stored personal access token.\r\n\t\t\t\tsee: https://lichess.org/account/oauth/token")]
        string AccessTokenPath { get; set; }
    }
}