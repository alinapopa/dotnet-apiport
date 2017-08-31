// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Fx.Portability.Resources;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Microsoft.Fx.Portability.Analyzer
{
    public class ReflectionMetadataDependencyFinder : IDependencyFinder
    {
        private readonly IDependencyFilter _assemblyFilter;

        public ReflectionMetadataDependencyFinder(IDependencyFilter assemblyFilter)
        {
            _assemblyFilter = assemblyFilter;
        }

        public IDependencyInfo FindDependencies(ImmutableDictionary<IAssemblyFile, bool> files, IProgressReporter _progressReporter)
        {
            using (var task = _progressReporter.StartTask(LocalizedStrings.DetectingAssemblyReferences))
            {
                try
                {
                    return ReflectionMetadataDependencyInfo.ComputeDependencies(files, _assemblyFilter, _progressReporter);
                }
                catch (Exception)
                {
                    task.Abort();

                    throw;
                }
            }
        }
    }
}
