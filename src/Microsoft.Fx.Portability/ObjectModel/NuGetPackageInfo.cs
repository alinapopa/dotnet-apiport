// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class NuGetPackageInfo
    {
        public string AssemblyInfo { get; private set; }
        public FrameworkName Target { get; private set; }
        public IEnumerable<NuGetPackageId> SupportedPackages { get; private set; }

        public NuGetPackageInfo( string assemblyInfo, FrameworkName target, IEnumerable<NuGetPackageId> supportedPackages)
        {
            AssemblyInfo = assemblyInfo;
            Target = target;
            SupportedPackages = supportedPackages != null? supportedPackages.ToList(): Enumerable.Empty<NuGetPackageId>();
        }
    }

    public class NuGetPackageId
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string Hyperlink { get; set; }
    }
}
