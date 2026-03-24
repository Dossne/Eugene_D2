export LC_ALL=en_US.UTF-8
export LANG=en_US.UTF-8

function to-abs-path {
    local target="$1"

    if [ "$target" == "." ]; then
        echo "$(pwd)"
    elif [ "$target" == ".." ]; then
        echo "$(dirname "$(pwd)")"
    else
        echo "$(cd "$(dirname "$1")"; pwd)/$(basename "$1")"
    fi
}

if [ -z ${BUILD_ID+x} ]; then
    echo "Error: BUILD_ID enviriment variable must be setup";
    exit 1
else 
    echo "BUILD_ID='$BUILD_ID'"; 
fi

if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi


#here we find last by creation time folder without ipa in ext
if [ -d ./_$PLATFORM ] 
then

    if [ "$PLATFORM" = "ios" ] ; then
        MASK="*build${BUILD_ID}.ipa"
    else
        MASK="*build${BUILD_ID}.*"
    fi
    
    echo "Trying find last project by mask $MASK"
    export LAST_PROJECT=$(find ./_$PLATFORM -name $MASK)
    echo "Found following project: $LAST_PROJECT"
    LAST_1=./_$PLATFORM/last.${LAST_PROJECT##*.}
    echo $LAST_1
    LAST_2=./_$PLATFORM/$CONFIGURATION-last.${LAST_PROJECT##*.}
    echo $LAST_2
    LAST_3=./_$PLATFORM/$CONFIGURATION-с$BUILD_ID.${LAST_PROJECT##*.}
    echo $LAST_3
    if [[ ! -z "$LAST_PROJECT" ]]; then
        cp "$LAST_PROJECT" "$LAST_1"
        cp "$LAST_PROJECT" "$LAST_2"
        cp "$LAST_PROJECT" "$LAST_3"
    else
        echo "Can not found any build by mask ${MASK}"
        exit 1
    fi

    echo "UPLOAD_TO_STORE = $UPLOAD_TO_STORE"
    echo "FORCE_UPLOAD_TO_STORE = $FORCE_UPLOAD_TO_STORE"

    if [ "$UPLOAD_TO_STORE" = true ] || [ "$FORCE_UPLOAD_TO_STORE" = true ]; then
        if [ "$FORCE_UPLOAD_TO_STORE" = true ] || [ "$CONFIGURATION" = 'store' ]; then
            PATH_TO_FASTLANE_FOLDER=$UNITY_PROJECT_IN_WS_PATH/Assets/SayBuilder/Editor
            echo "Check: $PATH_TO_FASTLANE_FOLDER/fastlane"
            if [ -d $PATH_TO_FASTLANE_FOLDER/fastlane ]; then
                LAST_PROJECT_ABS=$(to-abs-path $LAST_PROJECT)
                echo $LAST_PROJECT
                echo $LAST_PROJECT_ABS

                echo "PATH_TO_FASTLANE=$PATH_TO_FASTLANE"

                if [ "$PLATFORM" = "android" ]; then
                    echo "Generate Appfile"
                    # Update package/bundle id 
                    cp $PATH_TO_FASTLANE_FOLDER/fastlane/Appfile.template $PATH_TO_FASTLANE_FOLDER/fastlane/Appfile
                    sed -i -e "s/REPLACE_ME_BUNDLE/$BUNDLE_ID/g" $PATH_TO_FASTLANE_FOLDER/fastlane/Appfile
                    echo "Upload to GP"
                    cd $UNITY_PROJECT_IN_WS_PATH/Assets/SayBuilder/Editor
                    ${PATH_TO_FASTLANE}fastlane supply  --aab $LAST_PROJECT_ABS --track internal
                else
                    echo "Upload to TF"
                    cd $UNITY_PROJECT_IN_WS_PATH/Assets/SayBuilder/Editor
                    ${PATH_TO_FASTLANE}fastlane ios test_flight ipa:$LAST_PROJECT_ABS
                fi
            else
                echo "Cannot find fastlane folder, but UPLOAD_TO_STORE=$UPLOAD_TO_STORE or FORCE_UPLOAD_TO_STORE=$FORCE_UPLOAD_TO_STORE"
                exit 1
            fi  
        else
            echo "Only 'store' configuration supports upload to store unless FORCE_UPLOAD_TO_STORE is true"
            exit 1
        fi
    fi


else
    echo "Error: ./_$PLATFORM folder do not exist, this means unity not create any build yet."
fi

    