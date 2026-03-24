import subprocess
import requests
import glob
import json
import os


def find_last_artifact_by_configuration(configuration, platform, build_id):
    base_mask = "./_" + platform.lower() + "/*" + configuration + "*build" + str(build_id)
    
    print(platform, configuration)

    if platform.lower() == "ios":
        base_mask += ".ipa"
    else:
        if configuration == "store":
            base_mask += ".aab"
        else:
            base_mask += ".apk"

    print(base_mask)
    list_of_files = glob.glob(base_mask)
    if len(list_of_files) == 0:
        return []
    # TODO: finding last using sort by creating time, so-so
    latest_file = max(list_of_files, key=os.path.getctime)
    return [latest_file]


def extract_commit(path):
    try:
        commit = subprocess.check_output(['git', '-C', path, 'rev-parse', 'HEAD']).strip().decode('utf-8')
    except subprocess.CalledProcessError as e:
        commit="unknown"        
    return commit

def find_file_in_project_dir(filename, project_dir='./project'):
    for root, dirs, files in os.walk(project_dir, topdown=True):
        dirs.sort(reverse=True)  # Сортировка директорий в обратном алфавитном порядке
        if filename in files:
            return os.path.join(root, filename)
        for dir in dirs:
            result = find_file_in_project_dir(filename, os.path.join(root, dir))
            if result:
                return result
    return None

def get_unity_version(file_path):
    with open(file_path, 'r') as file:
        for line in file:
            if line.startswith('m_EditorVersion:'):
                return line.split(': ')[1].strip()
    return 'unknown'

def get_saykit_versions(file_path):
    with open(file_path, 'r') as file:
        data = json.load(file)
        android_version = data.get('android', 'unknown')
        ios_version = data.get('ios', 'unknown')
        unity_version = data.get('unity', 'unknown')
        return android_version, ios_version, unity_version

def upload_apk_to_diawi(apk_file):
    print("uploading "+ apk_file)


    description = os.environ.get('BUILD_DESCRIPTION')
    if not description:
        description = ""

        saykit_versions_file = find_file_in_project_dir('saykit_versions.json')
        if saykit_versions_file:
            android_version, ios_version, unity_version = get_saykit_versions(saykit_versions_file)
            description += f"SAYKIT_ANDROID_VERSION={android_version}\n"
            description += f"SAYKIT_IOS_VERSION={ios_version}\n"
            description += f"SAYKIT_UNITY_VERSION={unity_version}\n"
        else:
            description += "SAYKIT_ANDROID_VERSION=unknown\n"
            description += "SAYKIT_IOS_VERSION=unknown\n"
            description += "SAYKIT_UNITY_VERSION=unknown\n"


        description += f"CONFIGURATION={os.environ.get('CONFIGURATION', '')}\n"
        description += f"GIT_COMMIT={extract_commit('./project')}\n"

        description += f"SAYKIT_OVERWRITE={os.environ.get('SAYKIT_OVERWRITE', '')}\n"
        description += f"SAYKIT_GLOBAL_BRANCH={os.environ.get('SAYKIT_GLOBAL_BRANCH', '')}\n"
        description += f"SAYKIT_GLOBAL_COMMIT={extract_commit('./project/Assets/SayKit')}\n"
    
        description += f"UPLOAD_TO_STORE={os.environ.get('UPLOAD_TO_STORE', '')}\n"

        description += f"DEFINES_ADD={os.environ.get('DEFINES_ADD', '')}\n"
        description += f"BUNDLE_ID={os.environ.get('BUNDLE_ID', '')}\n"

        description += f"BUILD_URL={os.environ.get('BUILD_URL', '')}\n"
        description += f"DEVELOPER_DIR={os.environ.get('DEVELOPER_DIR', '')}\n"
        description += f"MIN_SDK_VERSION={os.environ.get('MIN_SDK_VERSION', '')}\n"
        description += f"TARGET_SDK_VERSION={os.environ.get('DEVELOPER_DIR', '')}\n"
        description += f"PATH_TO_GRADLE={os.environ.get('PATH_TO_GRADLE', '')}\n" 
        description += f"IPHONEOS_DEPLOYMENT_TARGET={os.environ.get('IPHONEOS_DEPLOYMENT_TARGET', '')}\n"
        description += f"UNITY_ENABLE_UNITY_ACCELERATOR={os.environ.get('UNITY_ENABLE_UNITY_ACCELERATOR', '')}\n"

        project_version_file = find_file_in_project_dir('ProjectVersion.txt')
        if project_version_file:
            unity_version = get_unity_version(project_version_file)
            description += f"UNITY_VERSION={unity_version}\n"
        else:
            description += "UNITY_VERSION=unknown\n"
        
        
            
    print("uploading comment:"+ description)

    files = {
        'token': (None, 'RfJ7mVv2wrYLVWFUR5yhh2Z7cqlluBS'),
        'file': (apk_file, open(apk_file, 'rb')),
        'comment': (None, description)
    }

    response = requests.post('https://builds.saygames.io/api/upload', files=files)

    if response.status_code == 200:
        link = response.json().get("url")
        if link is None:
            link = response.json().get("error")
    else:
        link = "unknown error, status code == " + str(response.status_code)

    return link


if __name__ == "__main__":


    cfg = os.environ['CONFIGURATION']
    platform = os.environ['PLATFORM']
    build_id = os.environ['BUILD_ID']
    root_link = os.environ['JOB_URL'] if 'JOB_URL' in os.environ.keys() else ""

    # use this file as storage, after this script build system inject env veriable from this file to env
    env_file = "./env_b" + build_id + ".txt"
    env_file_old = "./env.txt"

    if os.path.exists(env_file_old):
        os.remove(env_file_old)

    print("finding artifact for " + cfg)
    artifact_files = []
    if cfg != "all":
        artifact_files = find_last_artifact_by_configuration(cfg, platform, build_id)

    if len(artifact_files) == 0:
        print("ERROR: not find any build artifacts")
        exit(1)

    print("found:", artifact_files)

    links = []
    print(links)

    for file_name in artifact_files:
        print(file_name)
        if (".apk" in file_name) or (".ipa" in file_name) or (".aab" in file_name):
            links.append(upload_apk_to_diawi(file_name))
        else:
            links.append(root_link + "/ws/" + file_name)

    print(links)

    with open(env_file, "a+") as f:
        for link in links:
            f.write("LINK_TO_DIAWI=" + link + "\n")

    with open(env_file_old, "w") as f:
        for link in links:
            f.write("LINK_TO_DIAWI=" + link + "\n")            



