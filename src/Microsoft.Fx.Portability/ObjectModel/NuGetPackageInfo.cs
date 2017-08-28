// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class NuGetPackageInfo
    {
        public string AssemblyInfo { get; set; }
        public FrameworkName Target { get; set; }
        public IList<NuGetPackageId> SupportedPackages { get; set; }
    }

    public class NuGetPackageId
    {
        public string PackageId { get; set; }
        public string Version { get; set; }
        public string Hyperlink { get; set; }
    }
}
