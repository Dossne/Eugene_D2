#!/bin/bash

# Get Xcode version
XCODE_VERSION=$(xcodebuild -version | grep "Xcode" | awk '{print $2}')
# Minimum required Xcode version
MIN_XCODE_VERSION="15.4"

# Function to compare two version numbers
function version_compare() {
    local v1=$1
    local v2=$2

    # Splitting versions into components
    IFS='.' read -r -a v1_components <<< "$v1"
    IFS='.' read -r -a v2_components <<< "$v2"

    # Add trailing zeroes to make components of equal length
    while [ ${#v1_components[@]} -lt ${#v2_components[@]} ]; do
        v1_components+=('0')
    done
    while [ ${#v2_components[@]} -lt ${#v1_components[@]} ]; do
        v2_components+=('0')
    done

    # Compare each component
    for ((i=0; i<${#v1_components[@]}; i++)); do
        if [ "${v1_components[$i]}" -lt "${v2_components[$i]}" ]; then
            echo "-1"
            return
        elif [ "${v1_components[$i]}" -gt "${v2_components[$i]}" ]; then
            echo "1"
            return
        fi
    done

    # Versions are equal
    echo "0"
}

# Check if Xcode version is less than the minimum required version
comparison_result=$(version_compare "$XCODE_VERSION" "$MIN_XCODE_VERSION")

if [ "$comparison_result" -eq "-1" ]; then
    echo "[SayKit] Error: Xcode version $XCODE_VERSION is less than the required version $MIN_XCODE_VERSION."
    exit 1
else
    echo "[SayKit] Xcode version $XCODE_VERSION is compatible. Proceeding with the build..."
fi