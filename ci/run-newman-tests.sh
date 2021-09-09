#!/bin/bash
set -ex;

: "${PWS_APP_NAME:?PWS_APP_NAME needs to be set to the Pivotal Web Service app name.}"
: "${PWS_PATH_TO_TESTS:?PWS_PATH_TO_TESTS needs to be set to the path to the test json files.}"

npm install -g newman-reporter-html

echo "Newman version:" 
newman --version

COLLECTION_FILE_NAME="$PWS_APP_NAME.postman_collection.json"

ENVIRONMENT_FILE_NAME=$PWS_APP_NAME
if [ ! -z "$PWS_APP_SUFFIX" ]; then
    ENVIRONMENT_FILE_NAME="$PWS_APP_NAME-$PWS_APP_SUFFIX"
fi
ENVIRONMENT_FILE_NAME="$ENVIRONMENT_FILE_NAME.postman_environment.json"

onExit () {
    if [ "$?" != "0" ]; then
        echo "Tests failed"; # Don't deploy
        exit 1;
    else
        echo "Tests passed";
    fi
}

trap onExit EXIT;

utcTimestamp=$(date +"%Y-%m-%d_%H%M%S")

newman run $PWS_PATH_TO_TESTS/$COLLECTION_FILE_NAME \
    --environment $PWS_PATH_TO_TESTS/$ENVIRONMENT_FILE_NAME \
    --reporters="html,cli" \
    --reporter-html-export="reports/$utcTimestamp-newman-results.html" \
    ${PWS_DELAY_REQUEST:+ --delay-request=${PWS_DELAY_REQUEST}} \
    ${PWS_TEST_FOLDER:+ --folder=${PWS_TEST_FOLDER}} \
    ${PWS_TEST_URL:+ --global-var ${PWS_TEST_URL}} \
    ${PWS_TEST_CLIENT:+ --global-var ${PWS_TEST_CLIENT}} \
    ${PWS_TEST_SECRET:+ --global-var ${PWS_TEST_SECRET}};
