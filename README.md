# NetworkVideoEncoder
Using c# and ffmpeg to encode video's over a network both in mono and windows

**early version but works!!**

## usage

* Linux requires that the ffmpeg binary is under /usr/bin/
* Windows requires that the ffmpeg.exe binary is supplied locally next to the slave.exe
* Running the c# binaries will output the correct input arguments
* ffmpeg command file example
  * -i "DATA" -c:v libx264 -preset fast -crf 50 -map 0:0 -map 0:1 -c:s copy -c:a copy "OUT.extension"  
  * the command file has to contain the DATA and OUT keywords. 

## limitations

* Video to pictures is not supported
