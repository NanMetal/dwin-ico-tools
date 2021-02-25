# DWIN ICO Tool
Tool to process DWIN LCD display .ICO files. This is the C# .NET version of the [original](https://github.com/b-pub/dwin-ico-tools) by Brent Burton [[@b-pub](https://github.com/b-pub)].

## What

DWIN LCD displays use a number of image and container files to
skin the UI elements on the display. These displays are used on
Creality's Ender 3 v2 and other 3d printers, and the
configuration files to support these displays is included in the
[Marlin firmware](https://github.com/MarlinFirmware/Marlin).

One file they use is "9.ICO", which is a structured file
containing the icons.

## How-To
##### Extracting the images
There are two ways at the moment:
* Drag and drop the "9.ICO" to the executable file
* Or pass the file ("9.ICO") as an argument in the command line

A "out" folder will be created with all the images in the form of ```INDEX_NAME.jpg```
See the [dwin.h file](https://github.com/MarlinFirmware/Marlin/blob/2.0.x/Marlin/src/lcd/dwin/e3v2/dwin.h#L103) from Marlin for their indices and names.

##### Making a .ICO
Same as extracting the images:
* Drag and drop the "out" folder to the executable file
* Or pass the folder ("out") as an argument in the command line

An "out.ICO" file will be created.

## Examples
Using powershell:
1. Assuming 9.ico is the file:
 ```.\DwinIcoTool.exe 9.ico```

2. Assuming "out" is the folder containing all the images:
 ```.\DwinIcoTool.exe out```

## Dependencies

* .NET Framework 4

This should work on Windows 10 out of the box (Tested on Windows Sandbox).

## Test
I tested this with the [9.ICO](https://github.com/MarlinFirmware/Configurations/tree/release-2.0.6/config/examples/Creality/Ender-3%20V2/DWIN_SET) file from Marlin configuration examples for the Creality Ender 3 V2.
Extracting the images and redoing the .ico file without modifying produces the same file as the original.

I DID NOT TEST FLASHING A MODIFIED ONE.

## Credits

The original tools were created by:
* Brent Burton [[@b-pub](https://github.com/b-pub)]

## License

dwin-ico-tools is published under the GPL 3 license. See
the LICENSE file for details.
DwinIcoTool uses the same as this is a fork.
