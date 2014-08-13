*** X-Ray Builder GUI ****
* An application that takes a mobi/azw book file and creates an X-Ray file for that book with
* excerpt locations and chapter definitions.
*
* Created by Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>
* Original X-Ray script by shinew
* (http://www.mobileread.com/forums/showthread.php?t=157770)
* (http://www.xunwang.me/xray/)
**********************

Requirements:
-.Net Framework
-HtmlAgilityPack (http://htmlagilitypack.codeplex.com/)
 I have included the .dll along with the source and binaries. As far as I can tell, this is  allowed. If not, let me know!

Using the GUI vs the console application should be much more straight forward.
Take a look under the settings page to ensure everything looks ok there, the paths should auto-fill but you can customize them if you wish.
Pick a book, enter the shelfari URL and hit begin.
After the Shelfari info is downloaded, it will prompt if you want to run notepad to edit the aliases.
Same for chapters.

After downloading the terms from Shelfari, they will be exported to a .aliases file in ./ext, named after the book's ASIN. The alias file allows you to define aliases for characters/topics manually to maximize the number of excerpts found within the book.

Aliases follow the format:
Character Name|Alias1,Alias2,Etc
John Smith|Mr. Smith,Johnny,John

Ensure that any aliases that are very basic, eg John, are at the end. Otherwise, if you have a setup like "John Smith|John,Johnny", "John" will always match before "Johnny" does and it will look weird when you're viewing the X-Ray.

Similarly, chapters are automatically detected as well if the table of contents is labelled properly, then exported to a .chapters file in ./ext. This allows any random chapters you may not want included, like copyright pages, acknowledgements, etc. Also allows you to setup parts in case the book is divided into parts that include multiple chapters.
Chapter format:
Name|start|end

Any file that kindleunpack can process should work (mobi, azw, azw3, etc).

Books should be DRM-free.