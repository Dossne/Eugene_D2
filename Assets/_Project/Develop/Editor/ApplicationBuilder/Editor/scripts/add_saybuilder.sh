#check we have all need variables are setup
if [ -z ${SAY_BUILDER_GIT_BRANCH+x} ]; then
    export SAY_BUILDER_GIT_BRANCH=master
else 
    echo "SAY_BUILDER_GIT_BRANCH=$SAY_BUILDER_GIT_BRANCH"; 
fi

if [ -z ${SHORT_PROJECT_NAME+x} ]; then
    echo "Error: SHORT_PROJECT_NAME environment variable must be setup";
    exit 1
else 
    echo "SHORT_PROJECT_NAME=$SHORT_PROJECT_NAME"; 
fi

if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi

echo "UNITY_PROJECT_IN_WS_PATH=$UNITY_PROJECT_IN_WS_PATH"; 

export PATH_TO_SAYBUILDER_IN_PROJECT=${UNITY_PROJECT_IN_WS_PATH}/Assets/SayBuilder

echo "Removing current repo copy SayBuilder code..."
rm -rf say-builder-unity
echo "...done"    
  

echo "Cloning global version SayKit from ${SAY_BUILDER_GIT_BRANCH} to SayKit workspace..."

git clone -b $SAY_BUILDER_GIT_BRANCH git@gitlab.saygames.io:saybuilder/say-builder-unity.git say-builder-unity
echo "...done"

echo "Keep only ${SHORT_PROJECT_NAME} project files"
mv "say-builder-unity/SayBuilder/Editor/projects/.${SHORT_PROJECT_NAME}" "say-builder-unity/SayBuilder/Editor/projects/${SHORT_PROJECT_NAME}"
echo "...done"

echo "Removing current _project_ SayBuilder code..."
rm -rf ${PATH_TO_SAYBUILDER_IN_PROJECT}
echo "...done"


echo "Copy cloning SayBuilder to project"
cp -r say-builder-unity/SayBuilder ${PATH_TO_SAYBUILDER_IN_PROJECT}
echo "...done"


# This function installs a .mobileprovision file into the system
# by extracting its UUID and copying it to the provisioning profiles directory.
# $1 - Path to the .mobileprovision file
install_mobileprovision() {
    local provision_file="$1"

    # Directory where iOS stores provisioning profiles
    local destination=~/Library/MobileDevice/Provisioning\ Profiles/

    # Extract UUID from the .mobileprovision file
    local uuid=$(/usr/libexec/PlistBuddy -c 'Print :UUID' /dev/stdin <<< $(security cms -D -i "$provision_file"))

    # Check if UUID extraction was successful
    if [ -z "$uuid" ]; then
        echo "Failed to extract UUID from the .mobileprovision file"
        cat $provision_file
        return 1
    fi

    # Copy and rename the file according to its UUID
    cp "$provision_file" "${destination}${uuid}.mobileprovision"

    echo "The .mobileprovision file ($uuid) has been installed."
}

echo "Update provision profiles"
if [ -z ${APP_KEY+x} ]; then
    echo "WARNING:: APP_KEY is not set. Skip downloading provision profiles"
else
    if [ "$PLATFORM" = "ios" ] ; then
        PATH_TO_CONFIGS=${PATH_TO_SAYBUILDER_IN_PROJECT}/Editor/projects/${SHORT_PROJECT_NAME}/configs
        curl https://api.launcher.saygames.io/api/appleProfile\?apiKey\=7TkwhhXAcap9DGfK8BlNwtzJv\&appKey\=${APP_KEY}\&kind\=appstore --output $PATH_TO_CONFIGS/store.mobileprovision
        md5 $PATH_TO_CONFIGS/store.mobileprovision
        curl https://api.launcher.saygames.io/api/appleProfile\?apiKey\=7TkwhhXAcap9DGfK8BlNwtzJv\&appKey\=${APP_KEY}\&kind\=adhoc --output $PATH_TO_CONFIGS/dev.mobileprovision
        md5 $PATH_TO_CONFIGS/dev.mobileprovision
        cp $PATH_TO_CONFIGS/dev.mobileprovision $PATH_TO_CONFIGS/release.mobileprovision
        #curl https://api.launcher.saygames.io/api/appleProfile\?apiKey\=7TkwhhXAcap9DGfK8BlNwtzJv\&appKey\=${APP_KEY}\&kind\=adhoc --output $PATH_TO_CONFIGS/release.mobileprovision
        md5 $PATH_TO_CONFIGS/release.mobileprovision


        install_mobileprovision $PATH_TO_CONFIGS/store.mobileprovision
        install_mobileprovision $PATH_TO_CONFIGS/release.mobileprovision
        install_mobileprovision $PATH_TO_CONFIGS/dev.mobileprovision

        # content=$(security cms -D -i "$PATH_TO_CONFIGS/store.mobileprovision")
        # echo $content
        # keyName="aps-environment"
        # echo "$content" | grep -q "<key>$keyName</key>"
        # if [ $? -eq 0 ]; then
        #     echo "CI: Key '$keyName' found."
        # else
        #     echo "CI: Key '$keyName' NOT found."
        # fi

        # keyName="in-app-purchase"
        # echo "$content" | grep -q "<key>$keyName</key>"
        # if [ $? -eq 0 ]; then
        #     echo "CI: Key '$keyName' found."
        # else
        #     echo "CI: Key '$keyName' NOT found."
        # fi

    fi    

fi
