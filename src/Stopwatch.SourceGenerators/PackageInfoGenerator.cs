using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Stopwatch.SourceGenerators;

[Generator]
public class PackageInfoGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Combine both Directory.Packages.props and compilation metadata
		var packagesPropsFile = context.AdditionalTextsProvider
			.Where(file => file.Path.EndsWith("Directory.Packages.props"))
			.Collect();

		var compilationAndProps = context.CompilationProvider.Combine(packagesPropsFile);

		// Generate the source code
		context.RegisterSourceOutput(compilationAndProps, (spc, source) =>
		{
			var (compilation, propsFiles) = source;
			var packages = ExtractPackagesFromMetadata(compilation);

			if (packages.Length == 0)
			{
				packages = ParsePackagesProps(propsFiles[0]);
			}

			var sourceCode = GenerateSource(packages);
			spc.AddSource("GeneratedPackageInfo.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
		});
	}

	private static PackageInfo[] ParsePackagesProps(AdditionalText file)
	{
		var packages = new List<PackageInfo>();
		var content = file.GetText()?.ToString();
		if (string.IsNullOrEmpty(content))
		{
			return [];
		}

		try
		{
			var doc = XDocument.Parse(content);
			var packageVersions = doc.Descendants("PackageVersion");

			foreach (var packageVersion in packageVersions)
			{
				var name = packageVersion.Attribute("Include")?.Value;
				var version = packageVersion.Attribute("Version")?.Value;

				if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(version))
				{
					// Filter out internal/build-only packages
					if (!ShouldIncludePackage(name!))
					{
						continue;
					}

					packages.Add(new PackageInfo
					{
						Name = name!,
						Version = version!,
						Url = $"https://www.nuget.org/packages/{name}"
					});
				}
			}
		}
		catch
		{
			// If parsing fails, return empty list
		}

		return packages.ToArray();
	}

	private static PackageInfo[] ExtractPackagesFromMetadata(Compilation compilation)
	{
		var packages = new List<PackageInfo>();

		var portableExecutables = compilation.References.OfType<PortableExecutableReference>().ToArray();
		var defaultPathSubString = $"nuget{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}";
		var nugetPathPrefix = GetNuGetPathPrefix(portableExecutables, defaultPathSubString);
		if (string.IsNullOrEmpty(nugetPathPrefix))
		{
			var alternativePath = $"{Path.DirectorySeparatorChar}nuget{Path.DirectorySeparatorChar}";
			nugetPathPrefix = GetNuGetPathPrefix(portableExecutables, alternativePath);
		}

		if (string.IsNullOrEmpty(nugetPathPrefix))
		{
			return [];
		}

		foreach (var reference in portableExecutables)
		{
			// Extract package information from the file path
			// NuGet packages are typically stored in paths like:
			// .nuget/packages/PackageName/Version/lib/...
			// or D:\Packages\NuGet\PackageName\Version\lib\...
			var path = reference.FilePath;
			if (string.IsNullOrEmpty(path) ||
				path?.StartsWith(nugetPathPrefix, StringComparison.OrdinalIgnoreCase) == false)
			{
				continue;
			}

			// Get the package name and version from the path
			var relativePath = path!.Substring(nugetPathPrefix!.Length).TrimStart(Path.DirectorySeparatorChar);
			var segments = relativePath.Split(Path.DirectorySeparatorChar);
			if (segments.Length < 2)
			{
				continue;
			}

			var packageName = segments[0];
			var version = segments[1];

			// Skip if already processed or should be filtered
			if (!ShouldIncludePackage(packageName))
			{
				continue;
			}


			// The folder name is usually lowercase, try to find the last occurrence of the same
			// string in the full path - the .dll name is usually correctly cased.
			var lastIndex = path.LastIndexOf(packageName, StringComparison.OrdinalIgnoreCase);
			if (lastIndex >= 0)
			{
				packageName = path.Substring(lastIndex, packageName.Length);
			}

			// Check if we haven't already added this package
			if (!packages.Any(p => p.Name == packageName))
			{
				packages.Add(new PackageInfo
				{
					Name = packageName,
					Version = version,
					Url = $"https://www.nuget.org/packages/{packageName}"
				});
			}
		}

		return packages.ToArray();
	}

	private static string? GetNuGetPathPrefix(PortableExecutableReference[] executables, string expectedSubstring)
	{
		var nugetPathSource = executables.FirstOrDefault(p => p.FilePath?.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase) == true)?.FilePath;
		if (string.IsNullOrEmpty(nugetPathSource))
		{
			return null;
		}

		var nugetPathPrefix = nugetPathSource!.Substring(0, nugetPathSource.IndexOf(expectedSubstring, StringComparison.OrdinalIgnoreCase) + expectedSubstring.Length);
		return nugetPathPrefix;
	}

	private static bool ShouldIncludePackage(string packageName)
	{
		// Exclude internal/build-only packages and framework packages
		var excludedPrefixes = new[]
		{
			"Microsoft.SourceLink",
			"Microsoft.NETCore",
			"Microsoft.NET.Runtime",
			"Microsoft.NET.Sdk",
			"Microsoft.NET.Workload",
			"Microsoft.AspNetCore",
			"Microsoft.Win32",
			"Microsoft.Windows.SDK",
			"Microsoft.Bcl",
			"Microsoft.Extensions.DependencyModel",
			"Microsoft.Extensions.FileProviders",
			"Microsoft.Extensions.FileSystemGlobbing",
			"System.Private",
			"System.Text.Json",
			"System.Text.Encodings",
			"System.Resources",
			"System.Runtime",
			"System.Security",
			"System.Threading",
			"System.Collections",
			"System.Diagnostics",
			"System.Reflection",
			"runtime.",
			"Xamarin.AndroidX.Annotation",
		};

		var excludedNames = new[]
		{
			"netstandard.library",
			"NETStandard.Library",
		};

		// Check prefixes
		foreach (var prefix in excludedPrefixes)
		{
			if (packageName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}

		// Check exact names
		foreach (var name in excludedNames)
		{
			if (packageName.Equals(name, System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}

		return true;
	}

	private static string GenerateSource(PackageInfo[] packages)
	{
		var sb = new StringBuilder();
		sb.AppendLine("// <auto-generated/>");
		sb.AppendLine("using System.Collections.Generic;");
		sb.AppendLine();
		sb.AppendLine("namespace Stopwatch.Dialogs;");
		sb.AppendLine();
		sb.AppendLine("public static class GeneratedPackageInfo");
		sb.AppendLine("{");
		sb.AppendLine("    public static List<PackageInfo> GetPackages()");
		sb.AppendLine("    {");
		sb.AppendLine("        return new List<PackageInfo>");
		sb.AppendLine("        {");

		foreach (var package in packages)
		{
			var escapedName = EscapeString(package.Name);
			var escapedVersion = EscapeString(package.Version);
			var escapedUrl = EscapeString(package.Url);
			sb.AppendLine($"            new PackageInfo(\"{escapedName}\", \"{escapedVersion}\", \"{escapedUrl}\"),");
		}

		sb.AppendLine("        };");
		sb.AppendLine("    }");
		sb.AppendLine("}");

		return sb.ToString();
	}

	private static string EscapeString(string value)
	{
		return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
	}

	private class PackageInfo
	{
		public string Name { get; set; } = "";
		public string Version { get; set; } = "";
		public string Url { get; set; } = "";
	}
}
