#!/bin/bash -e

set -o pipefail

if test -n "$V"; then set +x; fi
if test -z "$TOP"; then echo "TOP not set"; exit 1; fi
if test -z "$DOTNET_DESTDIR"; then echo "DOTNET_DESTDIR not set"; exit 1; fi
if test -z "$MAC_DESTDIR"; then echo "MAC_DESTDIR not set"; exit 1; fi
if test -z "$IOS_DESTDIR"; then echo "IOS_DESTDIR not set"; exit 1; fi
if test -z "$MAC_FRAMEWORK_DIR"; then echo "MAC_FRAMEWORK_DIR not set"; exit 1; fi
if test -z "$MONOTOUCH_PREFIX"; then echo "MONOTOUCH_PREFIX not set"; exit 1; fi

cp="cp -c"

make -C "$TOP/tools/mtouch" dotnet -j
make -C "$TOP/tools/mmp" dotnet -j
make -C "$TOP/src" dotnet -j
make -C "$TOP/msbuild" dotnet -j

# the Microsoft.*OS.Runtime.<RID> nugets
copy_ios_native_libs_to_runtime_pack ()
{
	local platform=$1
	local sdk=$2
	local fat=$3
	local rid_family=$4
	local architectures=$5
	#shellcheck disable=SC2155
	local platform_lower=$(echo "$platform" | tr '[:upper:]' '[:lower:]')
	local rid=$platform_lower-$rid_family
	local packageid=Microsoft.$platform.Runtime.$rid
	local destdir=$DOTNET_DESTDIR/$packageid/runtimes/$rid/native
	local sdk_dir="$TOP/_ios-build/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/SDKs/$sdk.sdk"
	local lib_dir="$sdk_dir/lib/"
	local include_dir="$sdk_dir/include/"

	mkdir -p "$destdir"

	local thin=()
	for arch in $architectures; do
		thin+=(-extract_family "$arch")
	done

	local inputs=("$lib_dir"/libapp.a)
	inputs+=("$lib_dir"/libextension.a)
	inputs+=("$lib_dir"/libtvextension.a)
	inputs+=("$lib_dir"/libwatchextension.a)
	inputs+=("$lib_dir"/libxamarin*)
	for element in "${inputs[@]}"; do
		if [[ x$fat == x1 ]]; then
			lipo "$element" "${thin[@]}" -output "$destdir/$(basename "$element")"
		else
			$cp "$element" "$destdir"
		fi
	done

	mkdir -p "$destdir/Frameworks"
	local frameworks=()
	frameworks+=("$sdk_dir"/Frameworks/Xamarin.framework "$sdk_dir"/Frameworks/Xamarin-debug.framework)
	for element in "${frameworks[@]}"; do
		local fw_name
		fw_name=$(basename "$element" .framework)
		$cp -r "$element" "$destdir/Frameworks/"
		if [[ x$fat == x1 ]]; then
			lipo "$element/$fw_name" "${thin[@]}" -output "$destdir/Frameworks/$fw_name.framework/$fw_name"
		fi
	done

	$cp "$TOP"/tools/mtouch/simlauncher.mm "$destdir"

	$cp -r "$include_dir/xamarin" "$destdir/"
}
copy_ios_native_libs_to_runtime_pack "iOS"     "MonoTouch.iphoneos"        1 "arm64" "arm64"
copy_ios_native_libs_to_runtime_pack "iOS"     "MonoTouch.iphoneos"        1 "arm"   "armv7 armv7s"
copy_ios_native_libs_to_runtime_pack "iOS"     "MonoTouch.iphonesimulator" 1 "x64"   "x86_64"
copy_ios_native_libs_to_runtime_pack "iOS"     "MonoTouch.iphonesimulator" 1 "x86"   "i386"
copy_ios_native_libs_to_runtime_pack "tvOS"    "Xamarin.AppleTVOS"         0 "arm64" "arm64"
copy_ios_native_libs_to_runtime_pack "tvOS"    "Xamarin.AppleTVSimulator"  0 "x64"   "x86_64"
copy_ios_native_libs_to_runtime_pack "watchOS" "Xamarin.WatchOS"           0 "arm"   "armv7k arm64_32"
copy_ios_native_libs_to_runtime_pack "watchOS" "Xamarin.WatchSimulator"    0 "x86"   "i386"

copy_macos_native_libs_to_runtime_pack ()
{
	local platform=$1
	local sdk=$2
	local fat=$3
	local rid_family=$4
	local architectures=$5
	local rid_family=osx
	local architectures=x86_64
	#shellcheck disable=SC2155
	local platform_lower=$(echo "$platform" | tr '[:upper:]' '[:lower:]')
	local rid=osx-x64
	local packageid=Microsoft.$platform.Runtime.$rid
	local destdir=$DOTNET_DESTDIR/$packageid/runtimes/$rid/native
	local sdk_dir="$TOP/_mac-build/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/SDKs/$sdk.sdk"
	local lib_dir="$sdk_dir/lib/"
	local include_dir="$sdk_dir/include/"

	mkdir -p "$destdir"

	local inputs=()
	inputs+=("$lib_dir"/libxammac.a)
	inputs+=("$lib_dir"/libxammac.dylib)
	inputs+=("$lib_dir"/libxammac-debug.a)
	inputs+=("$lib_dir"/libxammac-debug.dylib)

	for element in "${inputs[@]}"; do
		#shellcheck disable=SC2155
		local filename=$(basename "$element")
		$cp "$element" "$destdir/${filename/xammac/xamarin}"
	done

	$cp "$TOP"/tools/mtouch/simlauncher.mm "$destdir"

	$cp -r "$include_dir/xamarin" "$destdir/"
}
copy_macos_native_libs_to_runtime_pack "macOS"     "Xamarin.macOS"        0 "osx" "x86_64"

# the Xamarin.*OS.Sdk nugets
create_sdk_nugets ()
{
	local platform=$1
	local legacy_destdir=$2
	#shellcheck disable=SC2155
	local platform_lower=$(echo "$platform" | tr '[:upper:]' '[:lower:]')
	local packageid=Microsoft.$platform.Sdk
	local destdir=$DOTNET_DESTDIR/$packageid

	mkdir -p "$destdir/tools/bin"
	mkdir -p "$destdir/tools/lib"

	$cp "$legacy_destdir/Version" "$destdir/"
	$cp "$legacy_destdir/buildinfo" "$destdir/tools/"

	# btouch
	# FIXME: should this go into a separate package?
	$cp -r "$legacy_destdir/lib/bgen" "$destdir/tools/lib/"
	$cp "$legacy_destdir/bin/bgen" "$destdir/tools/bin/"

	# mlaunch
	# FIXME: should this go into a separate package?
	if [[ "$platform" != "macOS" ]]; then
		$cp -r "$legacy_destdir/lib/mlaunch" "$destdir/tools/lib/"
		$cp "$legacy_destdir/bin/mlaunch" "$destdir/tools/bin/"
	fi

	chmod -R +r "$destdir"
}
create_sdk_nugets  "iOS"     "$IOS_DESTDIR$MONOTOUCH_PREFIX"
create_sdk_nugets  "tvOS"    "$IOS_DESTDIR$MONOTOUCH_PREFIX"
create_sdk_nugets  "watchOS" "$IOS_DESTDIR$MONOTOUCH_PREFIX"
create_sdk_nugets  "macOS"   "$MAC_DESTDIR$MAC_FRAMEWORK_DIR/Versions/Current"
