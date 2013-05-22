NemID Java Reverser
----
As a bachelors project at the IT University of Copenhagen, I was supposed to reverse-engineer the Danish governments Single-Signon system - [NemID](https://www.nemid.nu). The NemID client is written in Java, and runs as a browser applet. Part of the project was to reverse engineer the obfuscations used by DanID (creator of NemID).

The project was unsuccessfull, but in the process I managed to piece together a Java Class Renamer in C# which operates at the byte-code level of a Java application. 

Java Renamer
--
I made the renamer because one of the obfuscations used by NemID was renaming classes, methods and fields to keywords in Java - such as "abstract", "for", "do" etc.

My application works by using [Krakatu](https://github.com/Storyyeller/Krakatau), an excellent Java disassembler and reassembler (also available on github -yay!). This project disassembles a Jar file (or individual .class file) into a series of .j files, one for each Java class. Each .j file is a byte-code representation of the corresponding class, written in the [Jasmin-style](http://jasmin.sourceforge.net/) syntax.

My C# application will then parse these files, find all class-, method- and field-names that are illegal in Java source code, and replace them with a sequential name in the form "OrigClassXX", "OrigMethodXX" and "OrigFieldXX" (each unique across the entire application). This allows one to more easily work with the obfuscated bytecode. Also, this will work on other Java projects as well, and not just NemID.

Finally, the (possibly renamed) .j files are reassembled into .class files which can be inspected in other decompilers like JD-Gui (or whatever you want to do from then on).

NemID String obfuscation
--
For all the geeks working on NemID, I included this because I'd already made it and it seems to work well. 

NemID employs a form of string encryption, where all strings (and I do mean all) are encrypted, and first run through some method ("wuddlecakes") in order to get the original form. I (and others before me) reverse engineered the method to decrypt strings, and included it in this project. While processing the .j files from the previous step, I also decrypt all strings I find and remove the calls to the original decrypt method. This should make it much easier to work with any NemID code.

This part is only relevant for the NemID client and will probably crash on anything else. So if you're using my code to rename classes in some arbitrary project, comment out the line that decrypts strings ;).

Improvements
--
A short list of improvements possible:

+ **Configurable list of permanent-renames**
Say you find out that class "abstract" should be "SimpleList", it would be awesome to say that "java/org/corp/abstract" should be renamed to "java/org/corp/SimpleList". The same goes for methods and fields. This way, the project could be used as an iterative tool, gradually improving the deobfuscation-ness of the bytecode your working on - in terms of naming.

+ **More readability**
The code I wrote was put together in a few hours, so it's not overly awesome. But it's a great start (and I've had to work out how to parse strings and work with constant-pool's that include type-shorthands in type-shorthands). 

Howto: Steps to make it work on NemID
--
**1. Fetch NemID**
Open up any browser, and visit a NemID webpage (login pages for practially all Danish banks, such as Nordea or Danske Bank use NemID). After you've run the Java applet (no need to login), you'll have all the Jar files you need.

**2. Copy the Jar files**

* **Bootloader**
The main applet is a bootloader, and can be downloaded from here the page you just visited. It will have some embed / object tag, which links to the applet. My last visit spawned this url: https://applet.danid.dk/bootapplet/1369257520270 
Download that applet, and put it into the "NemID" folder in the repository.

* **Plugins**
Navigate to the ".oces2" folder in your home directory. For linux this will be "~/.oces2", in windows it will be "C:\Users\\[name]\\.oces2". In that folder, theres some subfolders, "danid" and under that: "plugins". In "plugins", copy all the .jar files to the "NemID" folder in the repository.

**3. Disassemble**
I've prepared a batch script which will run Krakatau on all the Jar files and output the results to the same "disassembled" folder (You will need a python setup for this to work). In linux, you'll do pretty much the same, just in bash or similar.

Basically, the command for each jar file is:
python Krakatau\disassemble.py -out disassembled [file.jar]

**4. Run renamer & string deobfuscator**
The source code provided can now be run on the disassebled .j files.

**5. Reassemble**
Similar to step 3, I've prepared an assemble batch script which will reassemble the .j files using Krakatau.

Authors note
--
This is a really fast project I did very quickly. So don't expect much. I've run it quite a few times on NemID with some great results. Although I haven't done much in forms of testing that the output was identical to the input in forms of what the methods are called all over the project - other than a few quick tests on some simple cases (again, from NemID) which included inheritance and method/field calls across classes.

Also, the code quality is medium or below, as there are close to no comments. But the code should be somewhat easy to follow, as it is structurally set up and follows (my) logical form of thinking.