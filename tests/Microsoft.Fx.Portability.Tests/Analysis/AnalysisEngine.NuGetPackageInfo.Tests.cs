using Microsoft.Fx.Portability.Analysis;
using Microsoft.Fx.Portability.ObjectModel;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using Xunit;
using static Microsoft.Fx.Portability.Tests.TestData.TestFrameworks;

namespace Microsoft.Fx.Portability.Tests.Analysis
{
    /// <summary>
    /// Tests GetNuGetPackageInfo for AnalysisEngine
    /// </summary>
    public class AnalysisEngineTestsNuGetPackageInfo
    {
        /// <summary>
        /// Tests that given a set of assemblies, the correct nuget package is returned.
        /// </summary>
        [Fact]
        public void TestGetNugetPackageInfo()
        {
            // Arrange
            var nugetPackageAssembly = GetAssemblyInfo("NugetPackageAssembly", "2.0.5.0", isExplicitlySpecified: false);
            var inputAssemblies = new[]
            {
                GetAssemblyInfo("TestUserAssembly", "2.0.5.0", isExplicitlySpecified: true),
                nugetPackageAssembly,
                GetAssemblyInfo("TestUserLibrary", "5.0.0", isExplicitlySpecified: true),
            };
            var targets = new[] { Windows80, Net11, NetStandard16 };
            var packageFinder = Substitute.For<IPackageFinder>();

            var nugetPackageWin80 = GetNuGetPackage("TestNuGetPackage", "1.3.4");
            var nugetPackageNetStandard = GetNuGetPackage("TestNuGetPackage", "10.0.8");

            packageFinder.TryFindPackage(nugetPackageAssembly.AssemblyIdentity, targets, out var packages)
                .Returns(x =>
                {
                    // return this value in `out var packages`
                    x[2] = new Dictionary<FrameworkName, IEnumerable<NuGetPackageId>>
                    {
                        { Windows80,  new[] { nugetPackageWin80 } },
                        { NetStandard16,  new[] { nugetPackageNetStandard } }
                    }
                    .ToImmutableDictionary();
                    return true;
                });

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            // Act
            var nugetPackageResult = engine.GetNuGetPackagesInfo(inputAssemblies.Select(x => x.AssemblyIdentity), targets).ToArray();

            // Assert

            // We expect that it was able to locate this particular package and
            // return a result for each input target framework.
            Assert.Equal(nugetPackageResult.Count(), targets.Length);

            var windows80Result = nugetPackageResult.Single(x => x.Target == Windows80);
            var netstandard16Result = nugetPackageResult.Single(x => x.Target == NetStandard16);
            var net11Result = nugetPackageResult.Single(x => x.Target == Net11);

            Assert.Equal(1, windows80Result.SupportedPackages.Count);
            Assert.Equal(1, netstandard16Result.SupportedPackages.Count);
            // We did not have any packages that supported .NET Standard 2.0
            Assert.Empty(net11Result.SupportedPackages);

            Assert.Equal(nugetPackageWin80, windows80Result.SupportedPackages.First());
            Assert.Equal(nugetPackageNetStandard, netstandard16Result.SupportedPackages.First());

            foreach (var result in nugetPackageResult)
            {
                Assert.Equal(result.AssemblyInfo, nugetPackageAssembly.AssemblyIdentity);
            }
        }

        [Fact]
        public void ComputeAssembliesToRemove_PackageFound()
        {
            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0", IsExplicitlySpecified = true };

            var packageFinder = Substitute.For<IPackageFinder>();
            var targets = new List<FrameworkName>()
            {
                new FrameworkName("Windows Phone, version=8.1"),
                new FrameworkName(".NET Standard,Version=v1.6")
            };

            var packageId = new List<NuGetPackageId>() { new NuGetPackageId("", "", "") };

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            var nugetPackageResult = new List<NuGetPackageInfo>() {
                new NuGetPackageInfo(userAsm1.AssemblyIdentity, targets[0], packageId),
                new NuGetPackageInfo(userAsm1.AssemblyIdentity, targets[1], packageId)
            };

            var assemblies = engine.ComputeAssembliesToRemove(new[] { userAsm1 }, targets, nugetPackageResult);

            Assert.True(assemblies.Any());
            Assert.Equal(assemblies.First(), userAsm1.AssemblyIdentity);
        }

        [Fact]
        public void ComputeAssembliesToRemove_PackageNotFound()
        {
            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0", IsExplicitlySpecified = true };

            var packageFinder = Substitute.For<IPackageFinder>();
            var targets = new List<FrameworkName>()
            {
                new FrameworkName("Windows Phone, version=8.1"),
                new FrameworkName(".NET Standard,Version=v1.6")
            };

            var packageId = new List<NuGetPackageId>() { new NuGetPackageId("", "", "") };

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            var nugetPackageResult = new List<NuGetPackageInfo>() {
                new NuGetPackageInfo(userAsm1.AssemblyIdentity, targets[0], packageId),
            };

            var assemblies = engine.ComputeAssembliesToRemove(new[] { userAsm1 }, targets, nugetPackageResult);

            Assert.False(assemblies.Any());
        }

        [Fact]
        public void ComputeAssembliesToRemove_FlagNotSet()
        {
            var userAsm1 = new AssemblyInfo() { AssemblyIdentity = "userAsm1, Version=1.0.0.0", FileVersion = "1.0.0.0" };

            var packageFinder = Substitute.For<IPackageFinder>();
            var targets = new List<FrameworkName>() { new FrameworkName("Windows Phone, version=8.1") };

            var packageIdAsm1 = new List<NuGetPackageId>() { new NuGetPackageId() };

            var engine = new AnalysisEngine(Substitute.For<IApiCatalogLookup>(), Substitute.For<IApiRecommendations>(), packageFinder);

            var nugetPackageResult = new List<NuGetPackageInfo>() { new NuGetPackageInfo(userAsm1.AssemblyIdentity, targets.First(), packageIdAsm1) };

            var assemblies = engine.ComputeAssembliesToRemove(new[] { userAsm1 }, targets, nugetPackageResult);

            Assert.False(assemblies.Any());
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version, bool isExplicitlySpecified)
        {
            return GetAssemblyInfo(assemblyName, version, string.Empty, isExplicitlySpecified);
        }

        private static AssemblyInfo GetAssemblyInfo(string assemblyName, string version, string location, bool isExplicitlySpecified)
        {
            var name = new FrameworkName(assemblyName, Version.Parse(version));
            return new AssemblyInfo { AssemblyIdentity = name.ToString(), IsExplicitlySpecified = isExplicitlySpecified };
        }

        private static NuGetPackageId GetNuGetPackage(string packageId, string version, string url = null)
        {
            return new NuGetPackageId(packageId, version, url);
        }
    }
}
