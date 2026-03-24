#check we have all need variables are setup
if [ -z ${SAYKIT_OVERWRITE+x} ]; then
    echo "Error: SAYKIT_OVERWRITE enviriment variable must be setup";
    exit 1
else 
    echo "SAYKIT_OVERWRITE='$SAYKIT_OVERWRITE'"; 
fi

if [ -z ${SAYKIT_GLOBAL_BRANCH+x} ]; then
    echo "Error: SAYKIT_GLOBAL_BRANCH enviriment variable must be setup";
    exit 1
else 
    echo "SAYKIT_GLOBAL_BRANCH='$SAYKIT_GLOBAL_BRANCH'"; 
fi

echo "Remove 'origin/' from $SAYKIT_GLOBAL_BRANCH"
export SAYKIT_GLOBAL_BRANCH=${SAYKIT_GLOBAL_BRANCH#origin/}
echo "New SAYKIT_GLOBAL_BRANCH=$SAYKIT_GLOBAL_BRANCH"

if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi

echo "UNITY_PROJECT_IN_WS_PATH=$UNITY_PROJECT_IN_WS_PATH";


# if [ -z ${SAYKIT_GLOBAL_COMMIT+x} ]; then
#     echo "Error: SAYKIT_GLOBAL_COMMIT enviriment variable must be setup";
#     exit 1
# else 
#     echo "SAYKIT_GLOBAL_COMMIT='$SAYKIT_GLOBAL_COMMIT'"; 
# fi



if [ "$SAYKIT_OVERWRITE" = true ] ; then


    echo "gitlabSourceRepoURL=$gitlabSourceRepoURL"
    echo "gitlabSourceBranch=$gitlabSourceBranch"

    if [ "$gitlabSourceRepoURL" == "git@gitlab.saygames.io:saykit/saykit-unity.git" ]; then
        echo "changes in $gitlabSourceRepoURL so we replace saykit branch to $gitlabSourceBranch"
        if [ -z ${gitlabSourceBranch+x} ]; then
            echo "gitlabSourceBranch is unset";
        else 
            echo "SAYKIT_GLOBAL_BRANCH is set to '$gitlabSourceBranch'";
            SAYKIT_GLOBAL_BRANCH=$gitlabSourceBranch
            echo "SAYKIT_GLOBAL_BRANCH=$SAYKIT_GLOBAL_BRANCH" >> ./env_b${BUILD_ID}.txt
        fi
    fi


    echo "Overwrite SayKit from global repo from $SAYKIT_GLOBAL_BRANCH branch"


    if [ ! -d "SayKit" ] ; then
        echo "SayKit folder does not exist, we need to clone"
        echo "Cloning global version SayKit from ${SAYKIT_GLOBAL_BRANCH} to SayKit workspace..."
        
        if [ -z ${SAYKIT_GLOBAL_COMMIT+x} ]; then
            echo "SAYKIT_GLOBAL_COMMIT not set, performing shallow clone"
            git clone --branch=${SAYKIT_GLOBAL_BRANCH} --depth 1 git@gitlab.saygames.io:saykit/saykit-unity.git SayKit
        else
            echo "SAYKIT_GLOBAL_COMMIT is set, performing full clone"
            git clone --branch=${SAYKIT_GLOBAL_BRANCH} git@gitlab.saygames.io:saykit/saykit-unity.git SayKit
        fi
        
        cd SayKit
        git status
        git rev-parse HEAD
        cd ..
        echo "...done"
    else
        echo "SayKit folder exist, we just clean and checkout and pull"
        cd SayKit
        git clean -f -d
        git checkout .
        git checkout ${SAYKIT_GLOBAL_BRANCH}
        git pull
        git status
        git rev-parse HEAD
        cd ..
    fi

    if [ ! -z ${SAYKIT_KEEP_FILES+x} ]; then
        echo "Found SAYKIT_KEEP_FILES='$SAYKIT_KEEP_FILES'"
        echo "Creating temporary directory for keeping files..."
        mkdir -p temp_keep_files
        
        # Читаем список файлов и копируем каждый в временную директорию
        IFS=',' read -ra FILES <<< "$SAYKIT_KEEP_FILES"
        for file in "${FILES[@]}"; do
            file=$(echo $file | xargs)  # Убираем лишние пробелы
            if [ -e "$UNITY_PROJECT_IN_WS_PATH/Assets/SayKit/$file" ]; then
                echo "Keeping file: $file"
                cp -r "$UNITY_PROJECT_IN_WS_PATH/Assets/SayKit/$file" "temp_keep_files/"
            else
                echo "Warning: File $file not found in SayKit directory"
            fi
        done
    fi

    echo "Removing current _project_ SayKit code..."
    rm -rf $UNITY_PROJECT_IN_WS_PATH/Assets/SayKit
    echo "...done"


    # echo "Copy cloning SayKit to project"
    # cp -r SayKit $UNITY_PROJECT_IN_WS_PATH/Assets/SayKit
    # echo "...done"

    echo "Move cloning SayKit to project"
    if [ ! -d "SayKit" ] ; then
        echo "ERROR: can not find SayKit folder (possible branch ${SAYKIT_GLOBAL_BRANCH} not exist)"
        exit 1
    else
        mv SayKit $UNITY_PROJECT_IN_WS_PATH/Assets/
        
        # Восстанавливаем сохранённые файлы
        if [ ! -z ${SAYKIT_KEEP_FILES+x} ] && [ -d "temp_keep_files" ]; then
            echo "Restoring kept files..."
            for file in temp_keep_files/*; do
                filename=$(basename "$file")
                cp -r "$file" "$UNITY_PROJECT_IN_WS_PATH/Assets/SayKit/$filename"
                echo "Restored: $filename"
            done
            rm -rf temp_keep_files
        fi
        
        echo "...done"
    fi

    if [ -z "$SAYKIT_GLOBAL_COMMIT" ]
    then
        echo "SAYKIT_GLOBAL_COMMIT is not setup or empty"
    else
        echo "SAYKIT_GLOBAL_COMMIT=$SAYKIT_GLOBAL_COMMIT"
        echo "Checkout SayKit repo to $SAYKIT_GLOBAL_COMMIT commit"
        cd $UNITY_PROJECT_IN_WS_PATH/Assets/SayKit
        git checkout $SAYKIT_GLOBAL_COMMIT

        retVal=$?
        if [ $retVal -ne 0 ]; then
            echo "CI: error to checkout for $GIT_COMMIT commit"
            git status
            exit 1
        fi        
        echo "...done"
    fi    

else
    echo "Keep SayKit from project"
fi