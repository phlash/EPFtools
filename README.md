# EPFtools
Explore Microelectronics EPF011/021 8051 core MCU programming tools

Currently an early (but working) flash dump / serial console tool written in C#
(because the original EPConsole.exe used CLR, and thus serial port config is known working)
that can be compiled/run on Linux or Windows with mono (mcs) or CLR (csc).

Sorry, no Makefile yet, it's trivial to build with mono: mcs serial.cs
