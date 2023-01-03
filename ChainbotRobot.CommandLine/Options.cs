using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChainbotRobot.CommandLine
{
    public class Options
    {
        [Option('f',"file",Required =true,HelpText ="The xaml file or project.json file.")]
        public string File { get; set; }

        [Option('i', "input",HelpText ="Input arguments in json string format.")]
        public string Input { get; set; }

        [Option('l', "log",HelpText ="Log write to file.")]
        public string Log { get; set; }
    }
}
