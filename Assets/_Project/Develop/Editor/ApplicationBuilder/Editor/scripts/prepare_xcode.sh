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
export CONFIG_PATH=$UNITY_PROJECT_IN_WS_PATH/Assets/SayBuilder/Editor/projects/${SHORT_PROJECT_NAME}/configs
export PROVISIONING_PROFILE_FILE=$CONFIG_PATH/$CONFIGURATION.mobileprovision
export EXPORT_OPTIONS_PLIST=./_ios/$CONFIGURATION-export-options.plist

open $PROVISIONING_PROFILE_FILE

#here some mix macos and bash magic for extract some date from mobile provision profile
export PROVISIONING_PROFILE_SPECIFIER=$(/usr/libexec/PlistBuddy -c 'Print :Name' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE))
echo $PROVISIONING_PROFILE_SPECIFIER
export TEAM=$(/usr/libexec/PlistBuddy -c 'Print :TeamIdentifier:0' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE))
echo $TEAM
export BUNDLE=$(/usr/libexec/PlistBuddy -c 'Print :Entitlements:application-identifier' /dev/stdin <<< $(security cms -D -i $PROVISIONING_PROFILE_FILE) | sed -e "s/$TEAM.//")
echo $BUNDLE

export SIGN_METHOD="ad-hoc"
if [ "$CONFIGURATION" = "store" ] ; then
    export SIGN_METHOD="app-store"
fi

#here we prepare export option plist for xcodebuild with actuall data
cp $CONFIG_PATH/export-options.template.plist $EXPORT_OPTIONS_PLIST

sed -i -e "s/REPLACE_ME_PROVISION_PROFILE_NAME/$PROVISIONING_PROFILE_SPECIFIER/g" $EXPORT_OPTIONS_PLIST
sed -i -e "s/REPLACE_ME_METHOD/$SIGN_METHOD/g" $EXPORT_OPTIONS_PLIST
sed -i -e "s/REPLACE_ME_BUNDLE/$BUNDLE/g" $EXPORT_OPTIONS_PLIST


export LAST_PROJECT=$(find . -name "*build${BUILD_ID}" -type d)

echo "Found following project: $LAST_PROJECT"

if [ -z ${LAST_PROJECT+x} ]; then
    echo "Error: last project not found";
    exit 1
else 
    echo "Last project found: $LAST_PROJECT"
    cp -r $LAST_PROJECT ./_ios/xcode_last
fi


echo "PROVISIONING_PROFILE_SPECIFIER=$PROVISIONING_PROFILE_SPECIFIER\nBUNDLE_ID=$BUNDLE\nTEAM=$TEAM\nLAST_PROJECT=$LAST_PROJECT" > $WORKSPACE/provisions.env

chmod +x $LAST_PROJECT/firebase-run
chmod +x $LAST_PROJECT/firebase-upload-symbols
chmod +x $LAST_PROJECT/firebase_symbols.sh
chmod +x $LAST_PROJECT/process_symbols.sh