#!/bin/bash
set -e

cp ./initdb.sql ./Dockerize/mssql/initdb.sql
cp ./appsettings.json.example ./Dockerize/web/appsettings.json

cd Dockerize
zip Judge1.zip -r ./*
