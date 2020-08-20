### Linux Setup

##### .NET Runtime
You'll need to install the .NET runtimes according to Microsoft's instructions for your distribution:
https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu#2004-

For Ubuntu 20.04 this looks something like this:
```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update; \
  sudo apt-get install -y apt-transport-https && \
  sudo apt-get update && \
  sudo apt-get install -y dotnet-runtime-3.1
```

##### GDI
For some reason, image handling requires a library that isn't included with the .NET runtime package so if you see the error `The type initializer for 'Gdip' threw an exception.` you'll need to install `libgdiplus`:
```
sudo apt-get install libgdiplus
```

### Usage
The `--help` command will give all the different options.  

Basic usage would be something like this:
```
./XRayBuilder.Console xray --dataurl https://www.librarything.com/work/3206242 ~/Documents/The\ Hobbit\ -\ J.\ R.\ R.\ Tolkien.mobi
```