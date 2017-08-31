// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.Versioning;

namespace Microsoft.Fx.Portability.ObjectModel
{
    public interface IPackageFinder
    {
        /// <summary>
        /// Retrieves the list of possible NuGet packages that contain a given assembly.
        /// Returns true if packages exist for that assembly (for any target), false if there are no packages
        /// </summary>
        bool TryFindPackage(string assemblyInfo, IEnumerable<FrameworkName> targets, out ImmutableDictionary<FrameworkName, IEnumerable<NuGetPackageId>> packages);
    }
}
