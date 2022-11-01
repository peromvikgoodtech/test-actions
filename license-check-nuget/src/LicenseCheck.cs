﻿using System.Linq;
using System.Text.Json;

string workflowError = "::error::";
string local_error = "Error: ";
string error = String.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")) ? local_error : workflowError;

string packagesFile = "licenses.json";
string allowedLicensesFile = "allowed_licenses.json";


if (!File.Exists(allowedLicensesFile))
{
    Console.WriteLine(error + "No allowed_licenses.json file found!");
    Environment.Exit(1);
}

var options = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

string allowedLicensesString = File.ReadAllText(allowedLicensesFile);
AllowedLicenses allowedLicenses = JsonSerializer.Deserialize<AllowedLicenses>(allowedLicensesString, options)!;

string packagesString = File.ReadAllText(packagesFile);
List<Package> packages = JsonSerializer.Deserialize<List<Package>>(packagesString, options)!;

var copyleftError = error + @"The included package(s) listed below use a copyleft licence.
Please ensure that your code will not be subject to a copyleft license as a consequence.
When OK, add the package(s) to ""packages"" in the ""allowed_licenses.json"" file:

{0}";

var licenseError = error + @"The included package(s) listed below have licences that haven't been whitelisted in ""allowed_licences.json"".
You may whitelist the licence type or package, or remove the package entirely:

{0}";

List<Package> copyleftErrorPackages = new();
List<Package> licenseErrorPackages = new();

foreach (var package in packages)
{
    var packageLicenses = package.LicenseType.Split(" OR ").ToList();
    if (packageLicenses.Any(pl => allowedLicenses.Permissive.Contains(pl)))
    {
        continue;
    }
    else if (packageLicenses.Any(pl => allowedLicenses.Copyleft.Contains(pl)))
    {
        if (!allowedLicenses.Packages.Contains(package.PackageName))
        {
            copyleftErrorPackages.Add(package);
        }
    }
    else if (!allowedLicenses.Packages.Contains(package.PackageName))
    {
        licenseErrorPackages.Add(package);
    }
}

if (copyleftErrorPackages.Any())
{
    var packageList = copyleftErrorPackages.Select(package => $"{package.PackageName,-20}: {package.LicenseType}");
    Console.WriteLine(String.Format(copyleftError, String.Join(Environment.NewLine, packageList)));
}
if (licenseErrorPackages.Any())
{
    var packageList = licenseErrorPackages.Select(package => $"{package.PackageName,-20}: {package.LicenseType}");
    Console.WriteLine(String.Format(licenseError, String.Join(Environment.NewLine, packageList)));
}

if (copyleftErrorPackages.Any() || licenseErrorPackages.Any())
{
    Environment.Exit(1);
}











