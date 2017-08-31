// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public class NuGetPackageInfo
    {
        private bool _hashComputed;
        private int _hashCode;

        public string AssemblyInfo { get; private set; }
        public FrameworkName Target { get; private set; }
        public ImmutableList<NuGetPackageId> SupportedPackages { get; private set; }

        public NuGetPackageInfo(string assemblyInfo, FrameworkName target, IEnumerable<NuGetPackageId> supportedPackages)
        {
            AssemblyInfo = assemblyInfo;
            Target = target;
            var packages = supportedPackages != null ? supportedPackages : Enumerable.Empty<NuGetPackageId>();
            SupportedPackages = packages.ToImmutableList();
        }

        public override bool Equals(object obj)
        {
            NuGetPackageInfo other = obj as NuGetPackageInfo;
            if (other == null)
                return false;

            return string.Equals(other.AssemblyInfo, AssemblyInfo, System.StringComparison.Ordinal)
                && Target.Equals(other.Target);
        }

        public override int GetHashCode()
        {
            if (!_hashComputed)
            {
                _hashCode = (AssemblyInfo ?? string.Empty).GetHashCode() + Target.GetHashCode();
                _hashComputed = true;
            }
            return _hashCode;
        }
    }
}
