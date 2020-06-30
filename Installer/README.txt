Maid Fiddler for COM3D2
=======================

Project page: https://github.com/denikson/COM3D2.MaidFiddler

Maid Fiddler is a real-time game manipulation tool for COM3D2.

Changelog:

* 1.0.8.1
  - Potentially fixed crashs when on game exit
  - Added initial Japanese localizations (thanks, @NaokiSato102!)
* 1.0.8.0
  - Updated internals for potential better stability
  - Fixed crashes when scout maids are selected in some cases
  - Added better logging: now GUI logs are generated to `<MG GUI folder>/mf_log.txt`
* 1.0.7.3
  - Add support for 1.48
  - IMPORTANT: You need 1.48 for this version!
* 1.0.7.2
  - Fix game crash when using certain scripts
* 1.0.7.0
  - Add support for 1.44
  - Add support for new Feeling property
* 1.0.6.0
  - Add support for age value
  - Add ability to watch some values better
  - Add support for previously unchangeable values
  - Add game version check
* 1.0.5.1
  - Unlocking free mode items now unlocks Empire Life Mode events as well
  - Add default config.ini generating
* 1.0.5.0
  - Add "Additional relation" option for 1.31
  - IMPORTANT: YOU MUST HAVE 1.31 OR NEWER INSTALLED TO USE THIS VERSION
* 1.0.4.4
  - Fix overflow error in some properties
* 1.0.4.3
  - Fix certain maid properties not being handled correctly by the GUI
  - Update English UI for 1.28
* 1.0.4.2
  - Fix some UI elements for non-negative integers permitting negative values
  - Fix crashing when personality is changed to Ladylike (for real this time)
* 1.0.4.1
  - Fixed crash when personality is changed to Ladylike
  - Potential fix for crash on save reload
  - Update TW translations
* 1.0.4.0
  - Added auto-updater functionality
  - Added "unlock everything in free mode" option
  - Allow main window to be resized freely
* 1.0.3.3
  - Maid Fiddler will now automatically remove invalid skills when all skills are unlocked
  - Update codebase to work with COM 1.23 (WILL NOT WORK ON OLDER VERSIONS)
* 1.0.3.1
  - Fix game crashing when enabling "all skills visible"
* 1.0.3.0
  - Fix GUI not working if the computer didn't have UCRT installed
  - Fix "all skills visible" trying to enable even personality-restricted skills
  - Add "remove invalid skills" option that removes personality-restricted skills from maid
  - Add warning to encourage more careful use of Maid Fiddler
* 1.0.2.0
  - Remove lower limit on stats
  - Add Russian translation to the bundle
  - Add "Always display NTR skills" option
  - Address potential crashes when using "Unlock all yotogi skills" option
  - Address GUI not launching in some rare cases
* 1.0.1.0
  - Fix remote error causing GUI to freeze
  - Fix typo in GUI script
  - Bundle Cecil.Inject alongsite plugin
  - Add "all dances selectable" cheat
* 1.0.0.0
  - Initial release!


---------------------------------
The MIT License (MIT)

Copyright (c) 2018 Geoffrey Horsington

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.