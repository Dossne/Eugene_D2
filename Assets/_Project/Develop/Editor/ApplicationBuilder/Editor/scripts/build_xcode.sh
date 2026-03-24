#check we have all need variables are setup
echo "BUILD_ID: $BUILD_ID"
echo "CONFIGURATION: $CONFIGURATION"
echo "SHORT_PROJECT_NAME: $SHORT_PROJECT_NAME"
echo "UNITY_PROJECT_IN_WS_PATH: $UNITY_PROJECT_IN_WS_PATH"

if [ -z ${BUILD_ID+x} ]; then
    echo "Error: BUILD_ID enviriment variable must be setup";
    exit 1
else 
    echo "BUILD_ID='$BUILD_ID'"; 
fi

if [ -z ${CONFIGURATION+x} ]; then
    echo "Error: CONFIGURATION enviriment variable must be setup";
    exit 1
else 
    echo "CONFIGURATION='$CONFIGURATION'"; 
fi

#check we have all need variables are setup
if [ -z ${SHORT_PROJECT_NAME+x} ]; then
    echo "Error: SHORT_PROJECT_NAME environment variable must be setup";
    exit 1
else 
    echo "SHORT_PROJECT_NAME='SHORT_PROJECT_NAME'"; 
fi

if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi


export PATH_TO_SAYBUILDER_IN_PROJECT=${UNITY_PROJECT_IN_WS_PATH}/Assets/SayBuilder
# export UNITY_PROJECT_IN_WS_PATH="."

#here we find last by creation time folder without ipa in ext
export LAST_PROJECT=$(find . -name "*build${BUILD_ID}" -type d)

echo "Found following project: $LAST_PROJECT"

if [ -z ${LAST_PROJECT+x} ]; then
    echo "Error: last project not found";
    exit 1
else 
    echo "Last project found: $LAST_PROJECT"
fi

export CONFIG_PATH=$UNITY_PROJECT_IN_WS_PATH/Assets/SayBuilder/Editor/projects/${SHORT_PROJECT_NAME}/configs

export PROVISIONING_PROFILE_FILE=$CONFIG_PATH/$CONFIGURATION.mobileprovision
export EXPORT_OPTIONS_PLIST=./_ios/$CONFIGURATION-export-options.plist

echo "PROVISIONING_PROFILE_FILE=$PROVISIONING_PROFILE_FILE"

open $PROVISIONING_PROFILE_FILE


#here some mix macos and bash magic for extract some date from mobile provision profile
export PROVISIONING_PROFILE_SPECIFIER=$(/usr/libexec/PlistBuddy -c 'Print :Name' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE))
echo "PROVISIONING_PROFILE_SPECIFIER=$PROVISIONING_PROFILE_SPECIFIER"
export TEAM=$(/usr/libexec/PlistBuddy -c 'Print :TeamIdentifier:0' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE))
echo "TEAM=$TEAM"
export BUNDLE=$(/usr/libexec/PlistBuddy -c 'Print :Entitlements:application-identifier' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE) | sed -e "s/$TEAM.//")
echo "BUNDLE=$BUNDLE"

export SIGN_METHOD="ad-hoc"
if [ "$CONFIGURATION" = "store" ] ; then
    export SIGN_METHOD="app-store"
fi

export PROJECT_PATH="$LAST_PROJECT"
echo "Last project path: $PROJECT_PATH"
export IPA_BASE_NAME=$SHORT_PROJECT_NAME-$CONFIGURATION-build$BUILD_ID
export IPA_PATH=$PROJECT_PATH

mkdir -p $PROJECT_PATH/$BUILD_ID

#here we prepare export option plist for xcodebuild with actuall data
cp $CONFIG_PATH/export-options.template.plist $EXPORT_OPTIONS_PLIST

sed -i -e "s/REPLACE_ME_PROVISION_PROFILE_NAME/$PROVISIONING_PROFILE_SPECIFIER/g" $EXPORT_OPTIONS_PLIST
sed -i -e "s/REPLACE_ME_METHOD/$SIGN_METHOD/g" $EXPORT_OPTIONS_PLIST
sed -i -e "s/REPLACE_ME_BUNDLE/$BUNDLE/g" $EXPORT_OPTIONS_PLIST

echo "--------------------------"
cat $EXPORT_OPTIONS_PLIST
echo "--------------------------"


chmod +x $LAST_PROJECT/firebase-run
chmod +x $LAST_PROJECT/firebase-upload-symbols
chmod +x $LAST_PROJECT/firebase_symbols.sh
chmod +x $LAST_PROJECT/process_symbols.sh


# if [ -z ${XCODE_PATH+x} ]; then
#     xcode-select --switch /Applications/Xcode.app
# else 
#     echo "switch xcode to $XCODE_PATH"
#     xcode-select --switch $XCODE_PATH
# fi

export LC_ALL=en_US.UTF-8
export LANG=en_US.UTF-8


if [ "$SAY_BUILDER_SKIP_POD_INSTALL" != "true" ]; then

    echo "pods update"
    pod $COCOA_POD_VERSION update
    echo "pods repo update"
    pod $COCOA_POD_VERSION  repo update
    echo "pods version"
    pod $COCOA_POD_VERSION --version
    echo "install pods"
    pod $COCOA_POD_VERSION install --project-directory=$PROJECT_PATH
fi

xcodebuild -version
XCODE_VERSION=$(xcodebuild -version)

echo "XCODE_VERSION=$XCODE_VERSION" >> ./env_b${BUILD_ID}.txt

# we have some difference between unity 2018 and 2019+ versions
# first one generate only xcode project file
# 2019 generate workspace
export USYM_UPLOAD_AUTH_TOKEN="fake"
if test -d "$PROJECT_PATH/Unity-iPhone.xcworkspace"; then
    echo "CI: we have xcworkspace so we build workspace"
    /usr/bin/xcodebuild -workspace $PROJECT_PATH/Unity-iPhone.xcworkspace \
        -scheme Unity-iPhone \
        -configuration Release \
        -destination "generic/platform=iOS" \
        -archivePath $PROJECT_PATH \
        -derivedDataPath $PROJECT_PATH/DerivedData \
        clean archive \
        GCC_ENABLE_OBJC_EXCEPTIONS=YES \
        CODE_SIGN_IDENTITY="iPhone Distribution" \
        SYNCHRONOUS_SYMBOL_PROCESSING=FALSE \
        USYM_UPLOAD_AUTH_TOKEN="NONE" 
else
echo "CI: we have only project, so we build project "
    /usr/bin/xcodebuild -project $PROJECT_PATH/Unity-iPhone.xcodeproj \
        -scheme Unity-iPhone \
        -configuration Release \
        -destination "generic/platform=iOS" \
        clean archive \
        -archivePath $PROJECT_PATH \
        GCC_ENABLE_OBJC_EXCEPTIONS=YES \
        PROVISIONING_PROFILE_SPECIFIER="$PROVISIONING_PROFILE_SPECIFIER" \
        DEVELOPMENT_TEAM=$TEAM \
        CODE_SIGN_IDENTITY="iPhone Distribution" \
        SYNCHRONOUS_SYMBOL_PROCESSING=FALSE \
        USYM_UPLOAD_AUTH_TOKEN="NONE" 
fi

# need exit if fail
echo "IPA_PATH=$IPA_PATH"
/usr/bin/xcodebuild -exportArchive -archivePath $PROJECT_PATH.xcarchive -exportPath $IPA_PATH -exportOptionsPlist $EXPORT_OPTIONS_PLIST || exit 1

#if not fail, lest find last ipa
IPA=$(find $IPA_PATH -name "*.ipa" | sort -n | tail -1)
echo "IPA=$IPA"
cp "$IPA" "./_ios/$IPA_BASE_NAME.ipa"