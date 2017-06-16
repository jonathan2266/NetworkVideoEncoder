# NetworkVideoEncoder
Using c# and ffmpeg to encode a batch of video's or music over a network both in mono and windows.

**early version but works!!**

## usage

* Linux requires that the ffmpeg binary is under /usr/bin/
* Windows requires that the ffmpeg.exe binary is supplied locally next to the slave.exe
* Running the **NetworkVideoEncoder (master)** or the **Slave** binaries will output the correct input arguments
* ffmpeg command file example
    * File location given to the NetworkVideoEncoder. Then distributed with every new job to a slave. The slave then uses this to start ffmpeg.
```bash
-i "DATA" -c:v libx264 -preset fast -crf 50 -map 0:0 -map 0:1 -c:s copy -c:a copy "OUT.extension"
```
  * Both "DATA" and "OUT" have to be in the command. The program will replace them when needed. The extension has be changed to mp4, mkv,... determines the extension in the result.

### NetworkVideoEncoder

NetworkVideoEncoder c:\ffmpeg.txt D:\videoFolder D:\outputFolder 8081

NetworkVideoEncoder ffmpeg_command videoFolder outputFolder tcpPort


### Slave

Slave 8081

Slave tcpPort

## limitations

* Video to pictures is not supported
* The NetworkVideoEncoder has to be started before the slaves.
  * UDP discovery can be improved
