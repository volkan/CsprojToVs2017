using System;
using System.Linq;
using Project2015To2017.Definition;

namespace Project2015To2017.Transforms
{
	internal sealed class PackageReferenceTransformation : ITransformation
	{
		public Project Transform(Project definition, IProgress<string> progress)
		{
			var existingPackageReferences = definition.PackageReferences;

				var testReferences = Array.Empty<PackageReference>();
				if (definition.Type == ApplicationType.TestProject && existingPackageReferences.All(x => x.Id != "Microsoft.NET.Test.Sdk"))
				{
					testReferences = new[]
					{
						new PackageReference(id: "Microsoft.NET.Test.Sdk", version: "15.0.0"),
						new PackageReference(id: "MSTest.TestAdapter", version: "1.1.11"),
						new PackageReference(id: "MSTest.TestFramework", version: "1.1.11")
					};

					var versions = definition.TargetFrameworks?
						.Select(f => int.TryParse(f.Replace("net", string.Empty), out int result) ? result : default(int?))
						.Where(x => x.HasValue)
						.Select(v => v < 100 ? v * 10 : v);

					if (versions != null)
					{
						if (versions.Any(v => v < 450))
						{
							progress.Report($"Warning - target framework net40 is not compatible with the MSTest NuGet packages. Please consider updating the target framework of your test project(s)");
						}
					}
				}
				
				var adjustedPackageReferences = existingPackageReferences
													.Concat(testReferences)
													.ToList()
													.AsReadOnly();

				foreach (var reference in testReferences)
				{
					progress.Report($"Adding nuget reference to {reference.Id}, version {reference.Version}.");
				}

			return definition.WithPackageReferences(adjustedPackageReferences);
		}
	}
}
