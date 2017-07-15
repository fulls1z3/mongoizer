using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

[assembly:AssemblyTitle("Mongoizer.Tests")]
[assembly:AssemblyProduct("Mongoizer.Tests")]
[assembly:AssemblyCopyright("Copyright (c) 2017 Burak Tasci")]

[assembly:ComVisible(false)]

[assembly:Guid("940c56f4-b746-44fb-b089-a241d8ef63ba")]

[assembly:CollectionBehavior(DisableTestParallelization = true)]
[assembly:TestCollectionOrderer("XUnitOrderer.TestCollectionOrderer", "XUnitOrderer")]
