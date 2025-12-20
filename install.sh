#!/bin/bash
dotnet publish --configuration release

rm -rf APP
mkdir APP

mv bin/Release/*/*/native/* APP/
mv APP/DotProj.exe APP/dotproj.exe
mv APP/DotProj APP/dotproj