X-Ray Builder GUI

http://www.mobileread.com/forums/showthread.php?t=245754

A .Net application that processes e-books to create X-Ray files for the Amazon Kindle (Paperwhite, Voyage, Fire). * X-Rays are built complete with chapter locations and excerpts.

Original X-Ray Builder by Ephemerality <Nick Niemi - ephemeral.vilification@gmail.com>
Modified version by darrenmcg
Original X-Ray script by shinew
http://www.mobileread.com/forums/showthread.php?t=157770
http://www.xunwang.me/xray/

Requirements:
• .Net Framework 4.0+

Books should be DRM-free.
Chapter detection on DRM books is not supported.

If you create an X-Ray file for a book, but still use the DRM copy on your Kindle, you will run into issues where the excerpts do not line up do where they are in the DRM-free version. In this case, you have 2 options. Either use the DRM-free version on your Kindle, or as an alternative you can determine the offset required to fix the excerpt locs and specify it with the 'offset' setting.

As an example:

Excerpt: "The quick brown fox jumps over the lazy dog."
On my DRM copy, X-Ray is showing: "x jumps over the lazy dog."
I open the rawML (I use Notepad++), find that excerpt in the book and determine the difference between the original and the DRM version
In this case the DRM version is 18 characters ahead of the original, so I must specify an offset of 18 in Settings.
From all the books I have tested with this, the offset was always the same. If for some reason the offset is variable throughout the book, this will obviously fail.

Note that the offset will also be applied to chapter locations.

After downloading the terms from Shelfari, they will be exported to a .aliases file in ./ext, named after the book's ASIN. The alias file allows you to define aliases for characters/topics manually to maximize the number of excerpts found within the book.
Initially terms would automatically search by a character's first name as well (Catelyn Stark would also search for Catelyn) but issues arose for things like "General John Smith" so it is left as a manual feature for now.
Aliases follow the format:
Character Name|Alias1,Alias2,Etc

Similarly, chapters are automatically detected as well if the table of contents is labelled properly, then exported to a .chapters file in ./ext. This allows any random chapters you may not want included, like copyright pages, acknowledgements, etc. Also allows you to setup parts in case the book is divided into parts that include multiple chapters.
Chapter format:
Name|start|end

Any file that mobi_unpack can process should work (mobi, azw, azw3, etc).