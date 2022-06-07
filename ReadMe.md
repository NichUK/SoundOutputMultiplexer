# Sound Output Multiplexer

This project allows audio to be played to multiple output devices on Windows (tested on 11, should work on XP upwards).

Audio can also be panned, and the volume adjusted for each output device.

We use it to split audio across two monitors which are side by side, with the left channel
playing through the left speaker of the left monitors, and the right channel playing through 
the right speakers of the right monitor, with balanced volume.

If you don't have a suitable virtual input device, then install 

[https://vb-audio.com/Cable/index.htm](VB-CABLE Virtual Audio Device)

It's donationware (suggested donation June '22 was $4.17), and provides a perfect
virtual audio output device for the OS, and virtual audio input source for this project.



#### Acknoweldgements

Audio manipulation supplied by CSCore. Awesome audio library!

https://github.com/filoe/cscore


Getting the tray app and service apps talking in .net 6 was greatly sped up
by following the techniques in this blog post:

https://erikengberg.com/named-pipes-in-net-6-with-tray-icon-and-service/