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
It uses the offical NuGet package for auth and the Library API via its REST endpoints.
- [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth)

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
Camera-sync from the Jottacloud-app will store the files in a folder named `Backup/Photo Timeline Uploads`, which is only accessable through the Jottacloud web-page, and will not be included in the "Sync" folder.

##### Prerequisites
Periodically, the user must move the files from the "Photo Timeline Uploads" folder to a staging-folder in "Sync".

##### Job description
The job will export the photos and videos from the "Photo Timeline Uploads" folder to "Sync".
The files will be strucuted in a "Yeae/Month" folder hierarchy based on the photo capture date. 
The staged content will be deleted after the export is completed.

#### ChromecastUpload

##### Prerequisites
Place images in a shared album in Jottacloud. Removed the images from the album after the job is executed.
The images must also be synced as to locate them on disk.

#### Job description
Job will find then local path for the images in the shared folder, and upload them to the specified album in Google Photos which is configured for Chromecast Ambient Mode.

#### SteamScreenshotsExport
This job is built for exporting screenshots taken in Steam. Steam saves screenshots to folder for each app Id.
The job will export the screenshots to a folder with the app name instead.
The folders will be categorized into alphabetic parent folders.

#### DesktopWallpaperExportJob
This job exports images from the staging-folder to a categorized folder based on the image resolution.

## Setup
### Environment variables
Some environment variables must be created in order to utilize some third-party APIs like Google Photos.
The keys can be found in [EnvironmentManager](src\JottaShift.Core\Configuration\EnvironmentManager.cs)

### AppSettings
Application settings area stored in [appsettings.json](src\JottaShift.Core\Configuration\appsettings.json). 
This file must be configured in accordance to the jobs that are intended to be executed.
If a jobs is disabled, the other values can be left blank.
