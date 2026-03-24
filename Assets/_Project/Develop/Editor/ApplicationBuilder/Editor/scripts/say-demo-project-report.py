import sys
import glob
import os
from datetime import datetime

from slack import WebClient
from slack import RTMClient
from parse import parse
import ssl

if __name__ == "__main__":
    if not os.getenv('WORKSPACE'):
        print("ERROR: WORKSPACE variable not exist")
        exit(1)

    if not os.getenv('BUILD_ID'):
        print("ERROR: BUILD_ID variable not exist")
        exit(1)

    if not os.getenv('JOB_URL'):
        print("ERROR: JOB_URL variable not exist")
        exit(1)

    build_id = os.getenv('BUILD_ID')
    path_to_artifact_folder = os.getenv('WORKSPACE') + "/" + build_id
    root_link = os.environ['JOB_URL'] if 'JOB_URL' in os.environ.keys() else ""

    ssl_context = ssl.create_default_context()
    ssl_context.check_hostname = False
    ssl_context.verify_mode = ssl.CERT_NONE

    slack_client = WebClient(token="xoxb-763326671504-1446183345717-ipbZpfwmdjfYfBYm2DmhoU6u", ssl=ssl_context)


    logs = []
    for file in glob.glob(path_to_artifact_folder + "/" + "*_c*.log"):
        logs.append(file)

    print("we have", len(logs), "log files")

    now = datetime.now()
    dt_string = now.strftime("%d/%m/%Y %H:%M:%S")
    slack_client.chat_postMessage(channel="C022PUKJ9FV", text="*" + dt_string + "*")

    links = []
    for log in sorted(logs):
        # editor_build_v2020.3.8f1_c79.log
        log_split = log.split('_')
        unity_version = log_split[2].replace('v', '')
        build_number = log_split[3].split('.')[0].replace('c', '')
        apks_by_build_number = glob.glob(path_to_artifact_folder + "/" + "*c" + build_number + ".apk")
        apk_exist = len(apks_by_build_number) == 1

        link = root_link + "lastSuccessfulBuild/artifact/" + build_id + "/" + os.path.basename(log)

        link_text = "<" + link + "|log>"
        result_text = "SUCCESS" if apk_exist else "FAIL"
        icon = ":green_heart:" if apk_exist else ":broken_heart:"
        message = icon + " " + unity_version + " " + result_text + " " + link_text
        print(message)
        slack_client.chat_postMessage(channel="C022PUKJ9FV", text=message)


