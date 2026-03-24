import sys
import subprocess

if len(sys.argv) < 3:
    sys.exit(1)

firebase_app_id = sys.argv[1]
symbols_path = sys.argv[2]

firebase_command = "/usr/local/bin/firebase crashlytics:symbols:upload --app={} {}".format(firebase_app_id, symbols_path)

try:
    subprocess.run(firebase_command, shell=True, check=True)
except subprocess.CalledProcessError as e:
    print("Error executing command:", e)
    sys.exit(1)