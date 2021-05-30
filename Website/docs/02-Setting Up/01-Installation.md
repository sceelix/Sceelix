Installation and Dependencies
=====================

Sceelix is available for Windows, Mac and Linux platforms and the installation process for each platform differs.

## Windows

### Installer

Sceelix provides an installer that already contains all the necessary dependencies. Once you’ve downloaded the installer, simply double-click the file to launch the installation process.  Like any other installation wizard, you’ll be faced with the possibility to change the installation directory and shortcut creation options. The installer will then proceed to install Sceelix and all its dependencies for you. Configuration data and other extras are then unpacked to your User `AppData` and `Documents` folder when you first run Sceelix.

### Portable

In the case you can’t run installers or simply prefer to carry Sceelix in a USB flash drive, portable versions are always useful. The zip file that you’ll download should first be extracted. It is not recommended to choose a location that may require special permissions (such as `Program Files` or the `Windows` folder). The extracted folder contains all the Sceelix files and a folder with the dependencies, which you’ll need to have installed first. They are:

* [.NET 4.6.1 Framework](https://www.microsoft.com/en-us/download/details.aspx?id=49982), although this is usually already present in most Windows setups.
* [DirectX End-User Runtimes (June 2010)](https://www.microsoft.com/en-us/download/details.aspx?id=8109), even though you might already have DirectX in your system, this may be required.

In this portable version, all the configuration data (and extras) is stored in the original folder itself.

## Mac

Double-click the downloaded dmg file. This will show a window with the installation instructions. In case you haven’t installed Mono before, you should double-click the `MonoInstaller.pkg` and follow the steps in the installation wizard. Afterwards, just drag and drop the Sceelix folder to the `Applications` folder and Sceelix will be installed!

When you’ll first execute Sceelix on a new machine, a font caching process will need to take place, during which Sceelix will apparently hang and remain unresponsive. This is a standard and necessary process, lead by Mono (making it beyond Sceelix's control) and usually takes a few minutes. The subsequent executions (even after updates) will not need to undergo this process.

## Linux

Since it uses Mono, Sceelix can run on pretty much every Linux distribution that supports it as well. At this point, `.deb` packages (for Ubuntu, Debian..) and `.rpm` packages (for Red Hat, CentOS, Fedora, SUSE…) are provided, which in some systems can be easily installed using package managers (for example, Ubuntu already provides the `Ubuntu Software Center`). For most of these managers, a simple double-click on the package will launch the package manager, after which instructions are shown. Alternatively, you can install the packages using the terminal, as shown below.

#### To install the .deb package from the terminal:

Run

```
sudo apt-get install mono-complete
```
to install Mono. Then run:
```
sudo dpkg -i Sceelix-X.X.X.X-LinuxInstaller.deb
```
to install Sceelix (where X.X.X.X is the version number you are installing).


#### To install the .rpm package from the terminal:

Run
```
rpm -ivh  -i Sceelix-X.X.X.X-LinuxInstaller.rpm
```
to install Sceelix (where X.X.X.X is the version number you are installing) and the Mono dependencies.