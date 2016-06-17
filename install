#!/bin/bash

tempDir="/tmp/bass24-temp"
cpuArchitecture=`uname -m`

zipUrls=(
  "http://www.un4seen.com/download.php?bass24-linux"
  "http://www.un4seen.com/download.php?bassmix24-linux"
)
zipDirectory="."

if [[ $cpuArchitecture == arm* ]]
then
  zipUrls=(
    "http://www.un4seen.com/stuff/bass24-linux-arm.zip"
    "http://www.un4seen.com/stuff/bass_aac-linux-arm.zip"
  )
  zipDirectory="hardfp"
fi

# Create a temp dir
mkdir $tempDir

# Go to temp
cd $tempDir

# Download zip files and extract
for zipUrl in "${zipUrls[@]}"
do
  echo "Downloading $zipUrl file ..."

  wget -qO- -O tmp.zip "$zipUrl" && unzip tmp.zip && rm tmp.zip
done

# Go to zip directory
cd $zipDirectory

# Install .so files
for file in $(find . -maxdepth 1 -name "*.so"); do
  echo "Installing $file file ..."

  sudo cp $file /usr/local/lib

  echo "/usr/local/lib/$file"

  sudo chmod a+rx /usr/local/lib/$file

  sudo ldconfig
done

# Remove tempDir
rm -r $tempDir
