# JottaShift

## Description
JottaShift is a program that automates tasks for syncing Jottacloud-files with local storage and other third-party services.
It is built around my personal needs, and therefore may not be applicable for everyone. It is open source, so feel free to fork and modify it to your needs.

### Services
The purpose of the program is to automate tasks releated to moving and copying files, either within the Jottacloud "Sync" folder, or with other services.
The following services are currently supported:

#### Jottacloud
##### Sync folder
The program uses the "Sync" folder to access the files.
This requires the host-machine to have either the Jottacloud desktop client or CLI installed and a "Sync" folder set up.

##### Shared albums
Jottacloud does not have an official API, but photo-albums in Jottacloud that are marked as "Shared" can be accessed via a public URL.
The program uses this URL to lookup the photos in the album, search for the photos in the "Sync" folder and manage them as needed.

#### Google Photos
JottaShift has the ability to upload photos to Google Photos, create and manage albums, and add photos to albums.
It uses the offical NuGet packages for Google Photos API.
- [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth)
- [Google.Apis.PhotosLibrary.v1](https://www.nuget.org/packages/Google.Apis.PhotosLibrary.v1) [is deprecated and will be replaced with REST]

Using the Google Photos API requires authentication via OAuth 2.0, so new credentials must be created in the Google Cloud Console and added to the program.
I will not supply my own personal credentials.

Google documentation
- [Authentication via OAuth 2.0](https://developers.google.com/identity/protocols/oauth2)
- [Photos Library API REST reference](https://developers.google.com/photos/library/reference/rest)

#### Steam
Steam provides a Web API where some endpoints are public and some require an API key.
All are accessible via the an open-source NuGet package.
- [SteamWebAPI2](https://www.nuget.org/packages/SteamWebAPI2/5.0.0) 

The current implementation of the Web API only used the public endpoints and does not require and API key.

### Jobs
These are the currently implemented jobs in the program, which are built around my own needs. The are run on-demand by running the `JottaShift.exe` executable.

#### JottacloudTimelineExport
Jottacloud can be setup to sync photos and videos from a smartphone to Jottacloud storage.
However, Jottacloud will store these files in a folder named `Backup/Photo Timeline Uploads`, which is only accessable through the Jottacloud web-page, and will not be included in the "Sync" folder.

This job will export the photos and videos from the "Photo Timeline Uploads" folder, and store them in "Sync". The files will also be strucuted based on year and month.
This step requires the user to manually move the contents of the "Photo Timeline Uploads" folder to a staging-folder in "Sync", but then the job will detect those files and place 
them in correctly structured folders in "Sync" based on the photo/video captured date.
The staged content will then be deleted after the export is completed.

#### ChromecastUpload

#### SteamScreenshotsExport

#### ScreenshotsExport
TODO: Rename to "WallpaperExport" or similar, as it may not be limited to screenshots.



## Setup