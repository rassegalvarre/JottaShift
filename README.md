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

#### Shared albums
Jottacloud does not have an official API, but photo-albums in Jottacloud that are marked as "Shared" can be accessed via a public URL.
The program uses this URL to lookup the photos in the album, and search for the photos in the "Sync" folder.

#### Google Photos

- [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth/1.73)
- [Google.Apis.PhotosLibrary.v1](https://www.nuget.org/packages/Google.Apis.PhotosLibrary.v1/1.34.0.1223) [is deprecated and will be replaced with a new implementation]


#### Steam


### Jobs


## Setup