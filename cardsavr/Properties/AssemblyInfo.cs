using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes. 
// Change them to the values specific to your project.

[assembly: AssemblyTitle("SwitchCardSaverAPI")]
[assembly: AssemblyDescription("Switch CardSavr .NET API")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Switch")]
[assembly: AssemblyProduct("CardSavr")]
[assembly: AssemblyCopyright("${AuthorCopyright}")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion("0.1.0.0")]

// name of log configuration file.
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

// allows the unit-test assembly access to internal classes/methods.
[assembly: InternalsVisibleTo("cardsavr_tests, PublicKey=002400000480000094000000060200000024000052534131000400000100010023d3e55a7e1642a59218e72eaaafa1b9245407c09aeef7de70f5f1e3a1b77cd54a62226b4297438e5109b729d7fa815dcdb853f508f7dc3547da119bf3504a6e56d45061ec6ed246ece45ecd1b1f454c9a454bff7059d1580f89c29ef0d06fedb66af2de7d860e701b800edc937b1fd030522b029c16f04f4bd0017072ec6d8e")]
[assembly: InternalsVisibleTo("cardsavr_e2e, PublicKey=0024000004800000940000000602000000240000525341310004000001000100955203bd8adf4d5eb1e7a8aa1142bf5254b9bb7203b961797798cd19079f50e4504b38c1fe97264a605ff00e63c39da58aad761d899eef18175a86028f95d0d1fc44194c797d79964cb1f6757b4cfd54e9cc57b63b1bd3627d42ec09b0bd1e3bdf10460ca6bbbb4833d0ba8456692123572730b5ebbc0e34bbe5ad4fc80badbd")]
