# How to Update RestSharp

RestSharp is distributed via NuGet. However, it's unsigned, so we have to sign it ourselves.

1. Download the latest RestSharp package using NuGet
2. In NuGet Package Manager, extract the .NET-4.0 version to this directory
3. Follow [this guide](http://buffered.io/posts/net-fu-signing-an-unsigned-assembly-without-delay-signing/) to sign
	1. Use `tonka\src\MfgBom\AssemblySignature.snk`