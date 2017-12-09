# NetworkVideoEncoder
Using c# and ffmpeg to encode a batch of video's or music over a network both in mono and windows.

**early version but works!!**

## Usage

* Linux requires that the ffmpeg binary is under /usr/bin/
* Windows requires that the ffmpeg.exe binary is supplied locally next to the client.exe
* Running the **Server** or the **Client** binaries will output the correct input arguments
* ffmpeg command file example
    * File location given to the **Server** binary. The Client then uses this as input arguments for ffmpeg.
```bash
-i "DATA" -c:v libx264 -preset fast -crf 50 -map 0:0 -map 0:1 -c:s copy -c:a copy "OUT.extension"
```
  * Both "DATA" and "OUT" have to be in the command. The program will replace them when needed. The extension has be changed to mp4, mkv,... determines the extension in the result.

### Server

Server c:\ffmpeg.txt D:\videoFolder D:\outputFolder 8081

Server ffmpeg_command videoFolder outputFolder tcpPort


### Client

Client 8081

Client tcpPort

## Limitations

* Video to pictures is not supported
* The Server has to be started **before** the Clients.
  * UDP discovery can be improved

## Future

* detect and handle an ffmpeg crash or y/n statement
