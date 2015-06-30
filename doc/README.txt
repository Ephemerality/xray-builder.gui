*** X-Ray Builder ****
* A console application that takes a mobi book file and creates an X-Ray file for that book with
* excerpt locations and chapter definitions.
*
* Original X-Ray Builder by Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>
* Modified version by darrenmcg
* Original X-Ray script by shinew
* (http://www.mobileread.com/forums/showthread.php?t=157770)
* (http://www.xunwang.me/xray/)
**********************

Requirements:
-mobi2mobi (https://dev.mobileread.com/dist/tompe/mobiperl/)
 As of version 1.13, mobi2mobi requirement has been removed. Still in testing.
-mobi_unpack (http://www.mobileread.com/forums/attachment.php?attachmentid=84428&d=1332545649)
 A version of mobi_unpack, compiled with py2exe (http://www.py2exe.org/) has been included.
-HtmlAgilityPack (http://htmlagilitypack.codeplex.com/)
 I have included the .dll along with the source and binaries. As far as I can tell, this is  allowed. If not, let me know!

Program usage:
xraybuilder [-o path] [--offset N] [-p] [-r] [-s shelfariURL] [--spoilers] [-u path] mobiPath
-o path (--outdir)	Path defines the output directory
			If not specified, uses ./out
--offset N		Specifies an offset to be applied to every book location.
			N must be a number (usually negative)
			See more info below
-p path (--python)	Path must point to python.exe
			If not specified, uses the command "python", which requires the Python
			directory to be defined in the PATH environment variable.
-r (--saveraw)		Save raw book markup to the output directory
-s (--shelfari)		Shelfari URL
			If not specified, there will be a prompt asking for it
--spoilers		Use descriptions that contain spoilers
			Default behaviour is to use spoiler-free descriptions
-u path (--unpack)	Path must point to mobi_unpack.py
			If not specified, searches in the current directory

After used once, mobi2mobi and mobi_unpack paths will be saved as default and are not necessary to include every time.
You can also drag and drop a number of mobi files onto the exe after the mobi2mobi and mobi_unpack paths have been saved.

Books should be DRM-free. If you create an X-Ray file for a book, but still use the DRM copy on your Kindle, you will run into issues where the excerpts do not line up do where they are in the DRM-free version. In this case, you have 2 options. Either use the DRM-free version on your Kindle, or as an alternative you can determine the offset required to fix the excerpt locs and specify it with the --offset token.
As an example:
My excerpt: "The quick brown fox jumps over the lazy dog."
On my DRM copy, X-Ray is showing: "x jumps over the lazy dog."
I open the rawML (I use Notepad++), find that excerpt in the book and determine the difference between the original and the DRM version
In this case the DRM version is 18 characters ahead of the original, so I must specify "--offset -18" on the command line.
From all the books I have tested with this, the offset was always the same. If for some reason the offset is variable throughout the book, this will obviously fail.
Note that the offset will also be applied to chapter locs.
Chapter detection on DRM books is not supported.

After downloading the terms from Shelfari, they will be exported to a .aliases file in ./ext, named after the book's ASIN. The alias file allows you to define aliases for characters/topics manually to maximize the number of excerpts found within the book.
Initially I had it so that terms would automatically search by a character's first name as well (Catelyn Stark would also search for Catelyn) but issues arose for things like "General John Smith" so I have left it as a manual feature for now.
Aliases follow the format:
Character Name|Alias1,Alias2,Etc

Similarly, chapters are automatically detected as well if the table of contents is labelled properly, then exported to a .chapters file in ./ext. This allows any random chapters you may not want included, like copyright pages, acknowledgements, etc. Also allows you to setup parts in case the book is divided into parts that include multiple chapters.
Chapter format:
Name|start|end

Any file that mobi_unpack can process should work (mobi, azw, azw3, etc).