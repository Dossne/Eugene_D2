echo "params: $@"
#check we have all need variables are setup
if [ -z ${UNITY_PROJECT_IN_WS_PATH+x} ]; then
    export UNITY_PROJECT_IN_WS_PATH="./project"
fi
#check have unity version already defined
if [ -z ${UNITY_VERSION+x} ]; then
    echo "Unity version not define, so take from project";
    # TODO: remove this for small pyhton script?
    export UNITY_VERSION=$(cat $UNITY_PROJECT_IN_WS_PATH/ProjectSettings/ProjectVersion.txt | cut -d ':' -f2 | head -n 1 | xargs -L1 echo | sed 's/\/r//' | tr -d '[:space:]')
    echo $UNITY_VERSION
fi

echo "UNITY_VERSION=$UNITY_VERSION" >> ./env_b${BUILD_ID}.txt

export LANG=en_US.UTF-8

if [ -z ${UNITY_EDITORS_PATH+x} ]; then
    export UNITY_EDITORS_PATH="/Applications/Unity/Hub/Editor/"
fi

if [ ${UNITY_ENABLE_UNITY_ACCELERATOR} = true ]; then
    echo "unity accelerator enabled"
    export UNITY_UA_PARAMS="-EnableCacheServer -cacheServerEndpoint ${UNITY_ACCELERATOR_END_POINT} -cacheServerEnableDownload true -cacheServerEnableUpload true -cacheServerNamespacePrefix ${JOB_BASE_NAME}"
else
    echo "unity accelerator not enabled"
    export UNITY_UA_PARAMS="-EnableCacheServer -cacheServerEndpoint ${UNITY_ACCELERATOR_END_POINT} -cacheServerEnableDownload false -cacheServerEnableUpload false -cacheServerNamespacePrefix ${JOB_BASE_NAME}"
fi


echo "${UNITY_EDITORS_PATH}/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
ls -l "${UNITY_EDITORS_PATH}/${UNITY_VERSION}"

echo "${UNITY_EDITORS_PATH}/${UNITY_VERSION}"/Unity.app/Contents/MacOS/Unity ${UNITY_UA_PARAMS} -disable-assembly-updater $@
"${UNITY_EDITORS_PATH}/${UNITY_VERSION}"/Unity.app/Contents/MacOS/Unity ${UNITY_UA_PARAMS} -disable-assembly-updater $@ 
