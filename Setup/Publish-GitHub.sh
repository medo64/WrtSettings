#!/bin/bash

OWNER=medo64
REPOSITORY=wrtsettings
TOKEN=`cat ~/GitHub.token 2> /dev/null`

if [[ "$TOKEN" == "" ]]; then
    echo "No GitHub token found." >&2
    exit 1
fi

VERSION_HASH=`git log -n 1 --format=%h`
VERSION_NUMBER=`git rev-list --count HEAD`
FILE_PREFIX=$REPOSITORY-rev$VERSION_NUMBER-$VERSION_HASH


BRANCH=`git rev-parse --abbrev-ref HEAD`

if [[ "$BRANCH" == "master" ]]; then
    DIFF=`git rev-list master..origin/master`
    if [[ "$DIFF" == "" ]]; then
        ./Publish.cmd

        RELEASE_URL=`curl -s -H "Authorization: token $TOKEN" https://api.github.com/repos/$OWNER/$REPOSITORY/releases/tags/latest | grep "\"url\"" | head -1 | cut -d\" -f4`
        if [[ "$RELEASE_URL" != "" ]]; then
            curl -s -H "Authorization: token $TOKEN" -X DELETE $RELEASE_URL
        fi
    
        git push origin :refs/tags/latest 2> /dev/null
        
        ASSET_UPLOAD_URL=`curl -s -H "Authorization: token $TOKEN" --data "{\"tag_name\": \"latest\", \"target_commitish\": \"master\", \"name\": \"Most recent build\", \"body\": \"This is the most recent automated build.\n\nFor the latest stable release go to http://jmedved.com/$REPOSITORY/.\", \"draft\": false, \"prerelease\": true}" -X POST https://api.github.com/repos/$OWNER/$REPOSITORY/releases | grep "\"upload_url\"" | cut -d\" -f4 | cut -d{ -f1`
        for FILE_EXTENSION in "exe" "zip"; do
            UPLOAD_RESULT=`curl -s -H "Authorization: token $TOKEN" -H "Content-Type: application/octet-stream" --data-binary @../Releases/$FILE_PREFIX.$FILE_EXTENSION -X POST $ASSET_UPLOAD_URL?name=$FILE_PREFIX.$FILE_EXTENSION`
            echo $UPLOAD_RESULT | grep --quiet "browser_download_url"
            if [[ $? == 0 ]]; then
                echo "$FILE_PREFIX.$FILE_EXTENSION"
            else
                echo "Failed upload for $FILE_PREFIX.$FILE_EXTENSION"

                RELEASE_URL=`curl -s -H "Authorization: token $TOKEN" https://api.github.com/repos/$OWNER/$REPOSITORY/releases/tags/latest | grep "\"url\"" | head -1 | cut -d\" -f4`
                curl -s -H "Authorization: token $TOKEN" -X DELETE $RELEASE_URL
                
                break;
            fi
        done
    else
        echo "Not all changes have been pushed to origin." >&2
        exit 1
    fi
else
    echo "Not in master branch." >&2
    exit 1
fi
