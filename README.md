Build Management Script
============

Basic Unity script to help automate and log the game build process.

When the user wants to build the project they click the platform type (in my case OSX), and the script automatically builds it in a folder appropriately named "Build", appending the version number of the game. Once thats done it appends the information to a log, to keep track of failed builds and embedded scenes in the builds. Simple, and effective. While its a rough and hacked together project done in a few hours, its easy enough to understand and extend.

In order to compress files I'm using a .dll from [SharpZipLib](http://icsharpcode.github.io/SharpZipLib/), its attached to the github repository but its better still to have the latest version.
