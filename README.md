# X-Ray Builder GUI
A .Net application that processes e-books to create an X-Ray file for the Amazon Kindle (Paperwhite, Voyage, Fire).  
X-Ray files are built complete with chapter locations and excerpts.  
Thanks to darrenmcg, Author Profile and End Actions files can also be built.

## Acknowledgements
Created by Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>  
New GUI, Shelfari search, X-Ray preview, End Actions, Author Profile, and various other new features by Darrenmcg  
Original X-Ray script by shinew (http://www.mobileread.com/forums/showthread.php?t=157770)

## Requirements
* Windows (untested on Mac/Linux with Mono)  
* .Net Framework 4.0+  
* Python 2.X if you plan on using mobi_unpack.py instead of the .exe (Optional)  
* HtmlAgilityPack (http://htmlagilitypack.codeplex.com/) (Included)  
  
## Limitations  
* Some (all?) X-Rays will not work properly on firmware version 5.6+ (Kindle Paperwhite 2 & 3, Voyage).  
Cause is still unknown.
* Notable Clips are not displayed. Research is necessary on how these are stored.
