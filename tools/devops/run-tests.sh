#!/bin/bash -ex

ls -l
pwd
ls -l xamarin-macios/
ls -l
if [[ "$SYSTEM_DEBUG" == "true" ]]; then
  DEBUG="-d"
else
  DEBUG=""
fi
#make $DEBUG -C xamarin-macios/builds download -j
#make $DEBUG -C xamarin-macios/builds .stamp-mono-ios-sdk-destdir -j
#make $DEBUG -C xamarin-macios/tests vsts-device-tests
echo "exit 0" > bar.sh
chmod a+x bar.sh
./bar.sh
# if [ $? -eq 0 ]; then

# publish to GH that there's a failure
#   echo "##vso[task.setvariable variable=TestState]success"
#   echo "hello"
# else
#   echo "##vso[task.setvariable variable=TestState]failure"
# fi
# echo "$(TestState)"