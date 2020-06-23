using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using CommandLine;
using System.Text.RegularExpressions;
using System.IO;

namespace WrtSettings {

    internal static class App {
        [Verb("decrypt", HelpText = "Decrypt a config file.")]
        internal class DecryptOptions {
            [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output file to be written.")]
            public string OutputFile { get; set; }
        }

        [Verb("encrypt", HelpText = "Encrypt a config file.")]
        internal class EncryptOptions {
            [Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
            public string InputFile { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output file to be written.")]
            public string OutputFile { get; set; }

            NvramFormat nvramFormat;
            [Option('f', "format", Required = true, HelpText = "NVRAM format. Valid values are  AsuswrtVersion1, AsuswrtVersion2, Tomato, DDWrt, Text")]
            public NvramFormat NvramFormat {
                get => nvramFormat;
                set {
                    if (value == NvramFormat.All) {
                        throw new ArgumentException("Format must not be All");
                    }
                    nvramFormat = value;
                }
            }
        }

        [STAThread]
        static int Main(string[] args) {
            if (args?.Length == 0) {
                bool createdNew;
                var mutexSecurity = new MutexSecurity();
                mutexSecurity.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
                using (var setupMutex = new Mutex(false, @"Global\JosipMedved_WrtSettings", out createdNew, mutexSecurity)) {
                    System.Windows.Forms.Application.EnableVisualStyles();
                    System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

                    Medo.Application.UnhandledCatch.ThreadException += new EventHandler<ThreadExceptionEventArgs>(UnhandledCatch_ThreadException);
                    Medo.Application.UnhandledCatch.Attach();

                    Application.Run(new MainForm());
                }
                return 0;
            } else {
                return CommandLine.Parser.Default.ParseArguments<DecryptOptions, EncryptOptions>(args)
                  .MapResult(
                    (DecryptOptions opts) => Decrypt(opts),
                    (EncryptOptions opts) => Encrypt(opts),
                    errs => 1);
            }
        }

        private static int Decrypt(DecryptOptions opts) {
            var nv = new Nvram(opts.InputFile, NvramFormat.All);
            var csv = String.Join(Environment.NewLine, nv.Variables.Select(d => $"{d.Key},\"{d.Value}\""));
            System.IO.File.WriteAllText(opts.OutputFile, csv);
            return 0;
        }

        private readonly static Regex splitter = new Regex(@"^([^,]+),""(.*)""\r?$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);
        private static int Encrypt(EncryptOptions opts) {
            var csv = System.IO.File.ReadAllText(opts.InputFile);
            var nv = new Nvram(null, opts.NvramFormat);
            var matchCollection = splitter.Matches(csv);
            foreach (var match in matchCollection.OfType<Match>()) {
                nv.Variables[match.Groups[1].Value] = match.Groups[2].Value;
            }
            nv.Save(opts.OutputFile);
            return 0;
        }

        private static void UnhandledCatch_ThreadException(object sender, ThreadExceptionEventArgs e) {
#if !DEBUG
            Medo.Diagnostics.ErrorReport.ShowDialog(null, e.Exception, new Uri("https://medo64.com/feedback/"));
#else
            throw e.Exception;
#endif
        }

    }
}
