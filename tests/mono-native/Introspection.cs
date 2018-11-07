using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;
using NUnit.Framework;
using ObjCRuntime;

namespace Mono.Native.Tests
{
	[TestFixture]
	public class Introspection
	{
		[TestFixtureSetUp]
		public void Setup ()
		{
#if !JENKINS
			Assert.Ignore ("Only run on Jenkins.");
#endif
		}

		void AssertShouldExist (string name)
		{
			var pathName = Path.Combine (NativePlatformConfig.RootDirectory, name);
			Assert.That (File.Exists (pathName), $"Found {name}.");
		}

		void AssertShouldNotExist (string name)
		{
			var pathName = Path.Combine (NativePlatformConfig.RootDirectory, name);
			Assert.That (File.Exists (pathName), Is.False, $"Should not have {name}.");
		}

		void CheckDynamicLibrary ()
		{
			AssertShouldExist (NativePlatformConfig.DynamicLibraryName);
			AssertShouldNotExist (NativePlatformConfig.GetDynamicLibraryName (!NativePlatformConfig.UsingCompat));
			AssertShouldNotExist ("libmono-native.dylib");

			var count = Directory.GetFiles (NativePlatformConfig.RootDirectory).Count (file => file.Contains ("mono-native"));
			Assert.That (count, Is.EqualTo (1), "exactly one mono-native library.");
		}

		void CheckStaticLibrary ()
		{
			AssertShouldNotExist ("libmono-native.dylib");
			AssertShouldNotExist ("libmono-native-compat.dylib");
			AssertShouldNotExist ("libmono-native-unified.dylib");

			var count = Directory.GetFiles (NativePlatformConfig.RootDirectory).Count (file => file.Contains ("mono-native"));
			Assert.That (count, Is.EqualTo (0), "zero mono-native libraries.");
		}

		[Test]
		public void CheckLibrary ()
		{
			switch (NativePlatformConfig.LinkMode) {
			case MonoNativeLinkMode.Dynamic:
				CheckDynamicLibrary ();
				break;
			case MonoNativeLinkMode.Static:
				CheckStaticLibrary ();
				break;
			default:
				Assert.Fail ($"Unknown link mode: {NativePlatformConfig.LinkMode}");
				break;
			}
		}

		[Test]
		public void CheckSymbols ()
		{
			string libname;
			switch (NativePlatformConfig.LinkMode) {
			case MonoNativeLinkMode.Dynamic:
				libname = NativePlatformConfig.DynamicLibraryName;
				break;
			case MonoNativeLinkMode.Static:
				libname = null;
				break;
			default:
				Assert.Fail ($"Unknown link mode: {NativePlatformConfig.LinkMode}");
				return;
			}

			Console.Error.WriteLine ($"TEST!");
			mono_native_initialize ();
			Console.Error.WriteLine ($"TEST #1!");

			var dylib = Dlfcn.dlopen (libname, 0);
			Console.Error.WriteLine ($"DYLIB: {libname} - {dylib}");
			Assert.That (dylib, Is.Not.EqualTo (IntPtr.Zero), "dlopen()ed mono-native");

			var symbol = Dlfcn.dlsym (dylib, "mono_native_initialize");
			Console.Error.WriteLine ($"SYMBOL: {symbol}");
			Assert.That (symbol, Is.Not.EqualTo (IntPtr.Zero), "dlsym() found mono_native_initialize()");
		}

		[DllImport ("__Internal")]
		extern static void mono_native_initialize ();

		void DumpDirectory (string dir)
		{
			Console.Error.WriteLine ($"DUMP DIRECTORY: {dir}");
			DumpDirectory (dir, string.Empty, Path.GetFileName (dir));
		}

		void DumpDirectory (string dir, string indent, string prefix)
		{
			Console.Error.WriteLine ($"{indent}- {prefix}");
			foreach (var subdir in Directory.GetDirectories (dir)) {
				var name = Path.Combine (prefix, Path.GetFileName (subdir));
				DumpDirectory (subdir, indent + "  ", name);
			}

			foreach (var file in Directory.GetFiles (dir)) {
				var name = Path.Combine (prefix, Path.GetFileName (file));
				Console.Error.WriteLine ($"{indent} * {name}");
			}
		}

		[Test]
		public void MartinTest ()
		{
			var linkMode = NativePlatformConfig.LinkMode;
			Console.Error.WriteLine ($"LINK MODE: {linkMode}");

			Console.Error.WriteLine ($"ROOT DIR: {NativePlatformConfig.RootDirectory}");
			DumpDirectory (NativePlatformConfig.RootDirectory);
		}

		[Test]
		public void TestInvoke ()
		{
			mono_native_initialize ();
		}
	}
}
