#!/bin/bash
dotnet build --configuration release

rm -rf APP
mkdir APP

mv bin/Release/net10.0/* APP/
mv APP/DotProj.exe APP/dotproj.exe