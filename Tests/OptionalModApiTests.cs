using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using RandomStartMod.Compat;
using Xunit;

public class OptionalModApiTests
{
    [Fact]
    public void SuccessfulIntegrationCanRunAgain()
    {
        var warnings = new List<string>();
        var api = new OptionalModApi("Example", warnings.Add);
        int calls = 0;
        Assert.True(api.TryRun(() => calls++));
        Assert.True(api.TryRun(() => calls++));
        Assert.Equal(2, calls);
        Assert.Empty(warnings);
    }

    [Fact]
    public void MissingApiDisablesOnlyThatIntegrationAndWarnsOnce()
    {
        var warnings = new List<string>();
        var broken = new OptionalModApi("Broken", warnings.Add);
        Assert.False(broken.TryRun(() => OptionalModApi.RequireType("Missing.Mod.Api")));
        bool retried = false;
        Assert.False(broken.TryRun(() => retried = true));
        Assert.False(retried);
        Assert.Single(warnings);
        Assert.Contains("Broken", warnings[0]);
        Assert.Contains("Missing.Mod.Api", warnings[0]);
        Assert.True(new OptionalModApi("Working", warnings.Add).TryRun(() => { }));
        Assert.Single(warnings);
    }

    [Fact]
    public void InvocationFailureReportsTheUnderlyingError()
    {
        var warnings = new List<string>();
        var api = new OptionalModApi("Example", warnings.Add);
        Assert.False(api.TryRun(() => throw new TargetInvocationException(
            new InvalidOperationException("API changed"))));
        Assert.Contains("API changed", Assert.Single(warnings));
    }

    [Fact]
    public void ResolvesTypesFromAlreadyLoadedAssemblies()
    {
        Assert.Equal(typeof(ApiFixture), OptionalModApi.RequireType(typeof(ApiFixture).FullName));
    }

    [Fact]
    public void ResolvesAndWritesAnExpectedStaticField()
    {
        FieldInfo field = OptionalModApi.RequireField(typeof(ApiFixture), "Enabled", true, typeof(bool));
        field.SetValue(null, true);
        Assert.True(ApiFixture.Enabled);
        field.SetValue(null, false);
    }

    [Fact]
    public void RejectsChangedFieldTypesBeforeWriting()
    {
        Assert.Throws<MissingFieldException>(() =>
            OptionalModApi.RequireField(typeof(ApiFixture), "Limit", false, typeof(bool)));
    }

    [Fact]
    public void RejectsAnInstanceFieldWhereAStaticFieldIsExpected()
    {
        Assert.Throws<MissingFieldException>(() =>
            OptionalModApi.RequireField(typeof(ApiFixture), "Limit", true, typeof(int)));
    }

    [Fact]
    public void InvokesTheExpectedMethodOverload()
    {
        var instance = new ApiFixture();
        MethodInfo begin = OptionalModApi.RequireMethod(typeof(ApiFixture), "Begin", typeof(void));
        begin.Invoke(instance, null);
        Assert.Equal(20, instance.Limit);
        Assert.Throws<MissingMethodException>(() =>
            OptionalModApi.RequireMethod(typeof(ApiFixture), "Begin", typeof(bool)));
    }

    [Fact]
    public void ResolvesMembersInheritedByAnIntegrationType()
    {
        Assert.NotNull(OptionalModApi.RequireField(typeof(DerivedFixture), "Limit", false, typeof(int)));
        Assert.NotNull(OptionalModApi.RequireMethod(typeof(DerivedFixture), "Begin", typeof(void)));
    }

    [Fact]
    public void BuiltModHasNoOptionalModAssemblyDependencies()
    {
        // Read metadata without loading the net472 mod into this net8 test host.
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "RandomStartMod.dll"));
        using var pe = new PEReader(stream);
        MetadataReader metadata = pe.GetMetadataReader();
        string[] dependencies = metadata.AssemblyReferences.Select(handle =>
            metadata.GetString(metadata.GetAssemblyReference(handle).Name)).ToArray();
        Assert.Contains("Assembly-CSharp", dependencies);
        Assert.Contains("0Harmony", dependencies);
        foreach (string optional in new[] { "KCSG", "VEF", "ShipsHaveInsides", "RealRuins",
            "NoPauseChallenge", "MyLittlePlanet", "PrepareModerately" })
            Assert.DoesNotContain(optional, dependencies);
    }

    public class ApiFixture
    {
        public static bool Enabled = false;
        public int Limit = 10;
        public void Begin() => Limit = 20;
        public void Begin(int limit) => Limit = limit;
    }

    public class DerivedFixture : ApiFixture { }
}
