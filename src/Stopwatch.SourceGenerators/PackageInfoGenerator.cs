using System.Collections.Generic;
using System.IO;
using System.Linq;
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
			var packages = new List<PackageInfo>();

			// First, add packages from Directory.Packages.props
			if (propsFiles.Length > 0)
			{
				packages.AddRange(ParsePackagesProps(propsFiles[0]));
			}

			// Then, add packages from metadata references (includes SDK-added packages)
			packages.AddRange(ExtractPackagesFromMetadata(compilation));

			// Remove duplicates (keep first occurrence)
			var uniquePackages = packages
				.GroupBy(p => p.Name)
				.Select(g => g.First())
				.OrderBy(p => p.Name)
				.ToList();

			var sourceCode = GenerateSource(uniquePackages);
			spc.AddSource("GeneratedPackageInfo.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
		});
	}

	private static List<PackageInfo> ParsePackagesProps(AdditionalText file)
	{
		var packages = new List<PackageInfo>();
		var content = file.GetText()?.ToString();
		if (string.IsNullOrEmpty(content))
		{
			return packages;
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

		return packages;
	}

	private static List<PackageInfo> ExtractPackagesFromMetadata(Compilation compilation)
	{
		var packages = new List<PackageInfo>();

		foreach (var reference in compilation.References)
		{
			if (reference is not PortableExecutableReference peReference)
			{
				continue;
			}

			// Extract package information from the file path
			// NuGet packages are typically stored in paths like:
			// .nuget/packages/PackageName/Version/lib/...
			var path = peReference.FilePath;
			if (string.IsNullOrEmpty(path))
			{
				continue;
			}

			var parts = path!.Replace('\\', '/').Split('/');
			var packagesIndex = -1;

			// Find the "packages" directory in the path
			for (int i = 0; i < parts.Length; i++)
			{
				if (parts[i].Equals("packages", System.StringComparison.OrdinalIgnoreCase))
				{
					packagesIndex = i;
					break;
				}
			}

			// If we found packages directory and have enough parts after it
			if (packagesIndex >= 0 && packagesIndex + 2 < parts.Length)
			{
				var packageName = parts[packagesIndex + 1];
				var version = parts[packagesIndex + 2];

				// Skip if already processed or should be filtered
				if (!ShouldIncludePackage(packageName))
				{
					continue;
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
		}

		return packages;
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

	private static string GenerateSource(List<PackageInfo> packages)
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
