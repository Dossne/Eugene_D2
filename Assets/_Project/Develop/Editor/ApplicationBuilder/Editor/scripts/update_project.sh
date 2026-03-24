#check we have all need variables are setup
if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi

if [ -z ${GIT_COMMIT+x} ]; then
    echo "GIT_COMMIT not set or empty"; 
else 
    echo "GIT_COMMIT='$GIT_COMMIT'"; 
    echo "UNITY_PROJECT_IN_WS_PATH=$UNITY_PROJECT_IN_WS_PATH"
    cd $UNITY_PROJECT_IN_WS_PATH
    git checkout $GIT_COMMIT

    retVal=$?
    if [ $retVal -ne 0 ]; then
        echo "CI: error to checkout for $GIT_COMMIT commit"
        git status
        exit 1
    fi
fi



