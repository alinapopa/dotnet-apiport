﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ApiPort.Resources;
using Microsoft.Fx.Portability;
using Microsoft.Fx.Portability.ObjectModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ApiPort.CommandLine
{
    internal class AnalyzeOptions : CommandLineOptions
    {
        public override string Name { get; } = "analyze";

        public override string HelpMessage { get; } = LocalizedStrings.CmdAnalyzeHelp;

        public override ICommandLineOptions Parse(IEnumerable<string> args)
        {
            var mappings = new Dictionary<string, string>
            {
                { "-f", "file" },
                { "-h", "help" },
                { "-?", "help" },
                { "-e", "endpoint" },
                { "-o", "out" },
                { "-d", "description" },
                { "-t", "target" },
                { "-r", "resultFormat" },
                { "-p", "showNonPortableApis" },
                { "-b", "showBreakingChanges" },
                { "-u", "showRetargettingIssues" },
                { "-i", "ignoreAssemblyFile" },
                { "-s", "suppressBreakingChange" },
            };

            var options = ApiPortConfiguration.Parse<Options>(args, mappings);

            return options.Help ? CommonCommands.Help : new AnalyzeCommandLineOption(options);
        }

        private class Options
        {
            public string Endpoint { get; set; }
            public List<string> File { get; set; } = new List<string>();
            public string Out { get; set; }
            public string Description { get; set; }
            public List<string> Target { get; set; }
            public List<string> ResultFormat { get; set; }
            public bool ShowNonPortableApis { get; set; }
            public bool ShowBreakingChanges { get; set; }
            public bool ShowRetargettingIssues { get; set; }
            public bool NoDefaultIgnoreFile { get; set; }
            public string IgnoreAssemblyFile { get; set; }
            public List<string> SuppressBreakingChange { get; set; }
            public bool Help { get; set; }
            public string TargetMap { get; set; }
        }

        private class AnalyzeCommandLineOption : ConsoleDefaultApiPortOptions, ICommandLineOptions
        {
            private readonly static string[] s_ValidExtensions = new string[]
            {
                ".dll",
                ".exe",
                ".winmd",
                ".ilexe",
                ".ildll"
            };

            private readonly ICollection<IAssemblyFile> _inputAssemblies = new SortedSet<IAssemblyFile>(new AssemblyFileComparer());

            // Case insensitive so that if this is run on a case-sensitive file system, we don't override anything 
            private readonly ICollection<string> _invalidInputFiles = new SortedSet<string>(StringComparer.Ordinal);

            public AppCommands Command { get; } = AppCommands.AnalyzeAssemblies;

            public AnalyzeCommandLineOption(Options options)
            {
                Description = options.Description;
                ServiceEndpoint = options.Endpoint;
                Targets = options.Target;
                OutputFormats = options.ResultFormat;
                TargetMapFile = options.TargetMap;
                BreakingChangeSuppressions = options.SuppressBreakingChange;

                //Set OverwriteOutputFile to true if the output file name is explicitly specified 
                if(!string.IsNullOrWhiteSpace(options.Out))
                {
                    OverwriteOutputFile = true;
                    OutputFileName = options.Out;
                }

                UpdateRequestFlags(options);
                UpdateInputAssemblies(options);
            }

            public override IEnumerable<IAssemblyFile> InputAssemblies
            {
                get
                {
                    return _inputAssemblies;
                }

                set
                {
                    throw new InvalidOperationException();
                }
            }

            public override IEnumerable<string> InvalidInputFiles
            {
                get
                {
                    return _invalidInputFiles;
                }
                set
                {
                    throw new InvalidOperationException();
                }
            }

            private void UpdateRequestFlags(Options options)
            {
                if (options.ShowBreakingChanges)
                {
                    RequestFlags |= AnalyzeRequestFlags.ShowBreakingChanges;
                }

                if (options.ShowRetargettingIssues)
                {
                    RequestFlags |= AnalyzeRequestFlags.ShowRetargettingIssues;
                    RequestFlags |= AnalyzeRequestFlags.ShowBreakingChanges;
                }

                if (options.ShowNonPortableApis)
                {
                    RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
                }

                // If nothing is set, default to ShowNonPortableApis 
                if ((RequestFlags & (AnalyzeRequestFlags.ShowBreakingChanges | AnalyzeRequestFlags.ShowNonPortableApis)) == AnalyzeRequestFlags.None)
                {
                    RequestFlags |= AnalyzeRequestFlags.ShowNonPortableApis;
                }
            }

            private void UpdateInputAssemblies(Options options)
            {
                foreach (var file in options.File)
                {
                    UpdateInputAssemblies(file);
                }
            }

            /// <summary> 
            /// This will search the input given and find all paths 
            /// </summary> 
            /// <param name="path">A file and directory path</param> 
            private void UpdateInputAssemblies(string path, bool skipBinaryIfPackageExists = false)
            {
                if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        //If the user passes in a whole directory, set a flag to skip the analysis of that binary if a NuGet package already exists
                        UpdateInputAssemblies(file, true);
                    }
                }
                else if (File.Exists(path))
                {
                    // Only add files with valid PE extensions to the list of 
                    // assemblies to analyze since others are not valid assemblies 
                    if (HasValidPEExtension(path))
                    {
                        _inputAssemblies.Add(new FilePathAssemblyFile(path, skipBinaryIfPackageExists));
                    }
                }
                else
                {
                    _invalidInputFiles.Add(path);
                }
            }

            private bool HasValidPEExtension(string assemblyLocation)
            {
                return s_ValidExtensions.Contains(Path.GetExtension(assemblyLocation), StringComparer.OrdinalIgnoreCase);
            }

            private class AssemblyFileComparer : IComparer<IAssemblyFile>
            {
                public int Compare(IAssemblyFile x, IAssemblyFile y)
                {
                    if (x == null)
                    {
                        return y == null ? 0 : -1;
                    }


                    return x.Name.CompareTo(y?.Name);
                }
            }

            private class FilePathAssemblyFile : IAssemblyFile
            {
                private readonly string _path;
                private readonly bool _skipBinaryIfPackageExists;

                public FilePathAssemblyFile(string path, bool skipBinaryIfPackageExists = false)
                {
                    _path = path;
                    _skipBinaryIfPackageExists = skipBinaryIfPackageExists;
                }

                public string Name => _path;

                public bool Exists => File.Exists(_path);

                public bool SkipBinaryIfPackageExists => _skipBinaryIfPackageExists;

                public string Version
                {
                    get
                    {
                        try
                        {
                            return FileVersionInfo.GetVersionInfo(_path).FileVersion;
                        }
                        catch (ArgumentException)
                        {
                            // Temporary workaround for CoreCLR-on-Linux bug (dotnet/corefx#4727) that prevents get_FileVersion from working on that platform
                            // This bug is now fixed and the correct behavior should be present in .NET Core RC2
                            return new Version(0, 0).ToString();
                        }
                    }
                }

                public Stream OpenRead() => File.OpenRead(_path);
            }
        }
    }
}