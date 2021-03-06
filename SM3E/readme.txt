﻿﻿﻿==========================================================================================
Super Metroid Memory Managed Editor.
Created: ~2018-01-01
Version: -- [2018-01-24]
License: TBA
==========================================================================================

-- DESCRIPTION --

SM3E is a A ROM hacking tool for editing Super Metroid. It will feature automatic
management of the data in the ROM, so there is no need to repoint anything manually.


-- AUTHOR'S NOTES --

This is a project I started in the summer of 2017, and was originally written in C++,
using SDL for the user interface. However, since building a UI that way is a pain, I
eventually stopped development. Now that I have started learning C#, and because UI
development with XAML is much easier, I've decided to give it another go. Parts of the 
code have been directly ported, other parts are new.


-- FEATURES --

At the moment, not even all functionality of the original C++ version has been
implemented. For now the program mostly functions as a level viewer for Super Metroid. Any
available editing functions are not permanent, since saving is not implemented either.


-- USAGE --

Should you decide to build the project, make sure to place the contents of Resources.zip 
in the same folder as the executable. Furthermore, a valid Super Metroid ROM (Unheadered)
must be placed in the same folder with the file name 'SuperMetroid.sfc' (or change the
file name on the first line of SuperMetroid.txt to reflect the name of the ROM).

==========================================================================================