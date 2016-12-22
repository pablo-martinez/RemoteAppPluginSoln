# Introduction
I worked hard to make Myrtille as straightforward as possible, with commented code, but some points may needs additional information.

If you have any issue, question or suggestion that isn't addressed into this synthetic documentation, FAQ or Wiki, please don't hesitate to contact me (cedrozor@gmail.com) or ask the community (https://groups.google.com/forum/#!forum/myrtille_rdp).

## History
Myrtille started back in 2007 as a PoC, with coworkers on our spare time, under the name AppliKr. I (Cedric Coste) was in charge of the websites (admin and frontal), business and database layers. The objective was to demonstrate it was possible to virtualize desktops and applications into a web browser only using native web technologies (no plugin). HTML5 was in early stage and it was a real challenge to do it only with HTML4. We were pioneers on that matter, along with Guacamole (HTML5/VNC), and wanted to take part on the emerging SaaS market.

It was quite a success because, at that time, the zero plugin concept was innovative and unlike solutions using Java, activeX or Flex; the ease of use and security aspects were also improved. But it was also slow and buggy and was left aside.

In 2011, I had the opportunity to create a company, named Steemind, with Catalin Trifanescu (a former coworker). I rewrote everything and used FreeRDP as rdp client (AppliKr was using RDesktop) and speed and stability were way better. The company failed in 2012 because we weren't able to achieve a decent fund raising and our customers were moving to Citrix and other major corporate companies in the VDI business.

I had recently some spare time and, while I was taking my breakfast with some blueberry jam, I decided to extract and improve the Steemind core technology and open source it.
	
I hope you will enjoy Myrtille! :)

Special thanks to Catalin Trifanescu for its support.

## Installation

You need at least IIS 7.0+ before installing myrtille (the HTTP(S) to RDP gateway). It installs as a role on Windows Servers and as a feature on others Windows versions.

The .NET 4.0+ framework can be installed automatically by the myrtille installer (Setup.exe), enabled as a feature of IIS (Web Server role > Applications Development > ASP.NET 4.5 on Windows Server 2012) or installed standalone (https://www.microsoft.com/en-us/download/details.aspx?id=17718).

All releases here: https://github.com/cedrozor/myrtille/releases
- Setup.exe (preferred installation method): setup bootstrapper
- Myrtille.msi: MSI package (x86)

## Start remote application from URL
Starting from version 1.3.x, it's possible to run a program automatically, on session start, from an URL. It's a feature comparable to remoteApp, which makes it easy to launch remote applications directly from internet shortcuts (.url) placed on the desktop.

Currently not working with Windows 2008 servers. See notes and limitations.

### Syntax
https://yourserver/Myrtille/?__EVENTTARGET=&__EVENTARGUMENT=&server=*server*&domain=*domain* [optional]&user=*username*&password=*password*&stat=*Stat+enabled|disabled* [optional]&debug=*Debug+enabled|disabled* [optional]&browser=*HTML4|HTML5* [optional]&program=*executable path, name and parameters (double quotes must be escaped)* [optional]&width=*width (px)* [optional]&height=*height (px)* [optional]&connect=*Connect%21*

The parameters values **must be URL encoded**. You can use a tool like http://www.url-encode-decode.com/ for that purpose (just copy&paste the encoded parameters into the URL).

**CAUTION!** please be aware that the user credentials are passed within the URL! it's unsafe to use this feature from an untrusted device or browser, because it will save the URL (and credentials) into its history.

Fortunaly, it's possible to have browsers into incognito mode (hence not have the URL saved). See http://www.howtogeek.com/137466/how-to-always-start-any-browser-in-private-browsing-mode/

Additionnaly, usage of **https://** (instead of **http://**) is strongly advised to secure the network communication.

## File transfer
Myrtille supports both local and network file storage. If you want your domain users to have access to their documents whatever the connected server, follow these steps:
- Ensure the machine on which Myrtille is installed is part of the domain
- Create a network share, read/write accessible to the domain users (i.e: \\\MYNETWORKSHARE\Users)
- Create a Group Policy (GPO), or edit the default one, on your domain server with a folder redirection rule (for the "Documents" folder, see https://mizitechinfo.wordpress.com/2014/11/18/simple-step-configure-folder-redirection-in-window-server-2012-r2/)
- In the target tab, select basic configuration to redirect everyone's folder to the same location, with create a folder for each user under the root path (the network share)
- In the settings tab, ensure the user doesn't have exclusive rights to the documents folder (otherwise Myrtille won't be able to access it)

## Network
The installer adds the following rules to the machine firewall:
- "Myrtille Websockets": allow both directions TCP port 8181 (default)
- "Myrtille Websockets Secured": allow both directions TCP port 8431 (default)

## Security
The installer creates a self-signed certificate for myrtille (so you can use it at https://yourserver/myrtille), but you can set your own certificate (if you wish) as follow:
- export your SSL certificate in .PFX format, with the private key
- save it into the myrtille "ssl" folder with the name "PKCS12Cert.pfx"

If not using Google Chrome (client side), see detailed comments regarding the security configuration into the Myrtille "Web.Config" file. You may have to add an exception for the secured websockets port (default 8431) into your browser.

In case of issues, ensure the secured websockets port (default 8431) is not blocked by your firewall (or proxy, reverse proxy, VPN, etc.).

## Configuration
Both the gateway and services have their own .NET config files; the gateway also uses XDT transform files to adapt the settings depending on the current solution configuration.

You may also play with the gateway "js/config.js" file settings to fine tune the configuration depending on your needs.

## Code organization
- Myrtille.RDP: link to the myrtille FreeRDP fork. C++ code. RDP client, modified to forward the user input(s) and encode the session display into the configured image format(s). The modified code in FreeRDP is identified by region tags "#pragma region Myrtille" and "#pragma endregion".
- Myrtille.Common: C# code. Fleck Websockets library and common helpers.
- Myrtille.Services: C# code. WCF services, hosted by a Windows Service (or a console application in debug build). start/stop the rdp client and upload/download file(s) to/from the connected user documents folder.
- Myrtille.Services.Contracts: C# code. WCF contracts (interfaces).
- Myrtille.Web: C# code. Link between the browser and the rdp client; maintain correlation between http and rdp sessions.
- Myrtille.Setup: MSI installer.

## Build
Myrtille uses C#, C++ and pure Javascript code (no additional libraries). Microsoft Visual Studio Community 2015 was used as primary development environment, using the .NET 4.0 framework.
If you want Visual Studio to load the Myrtille setup project, you have to install the (official and free) Microsoft Visual Studio 2015 Installer Projects extension (https://visualstudiogallery.msdn.microsoft.com/f1cc3f3e-c300-40a7-8797-c509fb8933b9).

The Myrtille build have the two classic solution configurations: "Debug" and "Release", on "Any CPU" platform.

Starting from myrtille version 1.1.0, the FreeRDP code (modified for myrtille needs) is no longer part of the myrtille repository.

The (new) myrtille FreeRDP code can be found at https://github.com/cedrozor/FreeRDP

The objectives are:
- Have a loose coupled dependency between myrtille and FreeRDP (so that FreeRDP could be replaced by another RDP client implementation, if needed)
- Benefits from the latest FreeRDP changes (bugfixes, new features, latest RDP protocol support, etc.), by synchronizing the fork with the FreeRDP repository (periodically, with a stable branch or after ensuring the master branch is stable)
- Extends myrtille to other remote access protocols. The gateway is (always was) protocol agnostic. For example, myrtille could be linked to an SSH client (the same way it's linked to an RDP client), modified to handle the user inputs and display updates

Steps to build the FreeRDP fork (and have it working with myrtille):
- Git clone https://github.com/cedrozor/FreeRDP.git into "<myrtille root folder>\Myrtille.RDP\FreeRDP\" (**NOTE** if using TortoiseGit, the contextual menu won't show the "Git clone" option from the "Myrtille.RDP" folder; you will have to do it from elsewhere, outside of the myrtille tree; also, don't create the "FreeRDP" folder manually, just write it into the clone target path)
- Use cmake on it as detailed here: https://github.com/FreeRDP/FreeRDP/wiki/Build-on-Windows-Visual-C---2012-(32-and-64-bit) to generate the Visual Studio solution and projects accordingly to your dev environment (don't forget to install OpenSSL first; precompiled installers here: http://slproweb.com/products/Win32OpenSSL.html)
- Open and build the generated solution. **CAUTION** if OpenSSL is configured standalone (dlls not present into the Windows "System32" folder), you will have to copy *"libeay32.dll"* and *"ssleay32.dll"* files (located into "OpenSSL\bin") into the FreeRDP build output folder

If you plan to build the myrtille installer, you have first to build the FreeRDP fork (or you can add the FreeRDP fork solution to the myrtille solution and use the FreeRDP projects outputs instead of files).

### Startup projects
If you want to run Myrtille with Visual Studio, you should set startup projects on the solution in the following order (multiple startup projects):
- Myrtille.Services (Start)
- Myrtille.Web (Start)

You can choose the browser you want to use by right-clicking an ASPX page into the "Myrtille.Web" project and click "Browse With...".

The FreeRDP executable is intended to be run from the "Myrtille.RDP" folder.

You can debug FreeRDP, while an rdp session is active, by attaching the debugger to the process "wfreerdp.exe" (native code).

Before first run, retrieve (git clone) the myrtille FreeRDP fork (https://github.com/cedrozor/FreeRDP) into the "Myrtille.RDP" folder and build it there in order to generate "wfreerdp.exe".

Hit F5 to start debugging.

## Communication
### Overview
web browser <-> web gateway <-> wcf services <-> rdp client <-> rdp server

### Protocols
web browser <-HTTP(S),XMLHTTP,WS(S)-> web gateway <-WCF-> wcf services <-SYSTEM-> rdp client <-RDP-> rdp server

"SYSTEM" is simply starting FreeRDP as a local process, and capture the event of it being stopped.

In order to speed up the communication between the web gateway and the rdp client, I decided to bypass the wcf services layer and set a direct communication between the two by using named pipes.

It has 2 advantages: first, it's a bit faster than using TCP and, second, it guarantees a FIFO unstacking to preserve the images order.	It has also a drawback: both the web gateway and the wcf services must be on the same machine.

After weighing pros and cons, I decided to kept it as is because it also has 2 additional benefits: setting up a named pipe in C++ is a bit simpler than a TCP socket (named pipes are handled as simple files) and having a single setup is easier for the end user.
		
web gateway <-IPC-> rdp client
	
However, I'm fully aware that it breaks the distributed architecture pattern. Thus, if you want, feel free to move the named pipes management up to the wcf services layer and proxy the data from/to the web gateway.

This is a thing to consider if you want to isolate the web gateway from your intranet (into a DMZ for instance) and still be able to connect a machine on it.

## Notes and limitations
- Starting from myrtille version 1.2.0, the packaged FreeRDP and OpenSSL binaries use a statically-linked runtime; that means there is no longer need for the Microsoft Visual C++ redistributables (x86). It's still a good idea to install them however as they will be required if the build options are changed.

- On Windows Server 2008, the FreeRDP remoteApp and shell features don't work. It's not possible to start a remote application from URL. https://github.com/FreeRDP/FreeRDP/issues/1669

- On Windows Server 2012, you may have issues installing the Microsoft Visual C++ 2015 redistributables (x86) (http://stackoverflow.com/questions/31536606/while-installing-vc-redist-x64-exe-getting-error-failed-to-configure-per-machi). To circumvent that, ensure your system is fully updated (Windows updates) first or try to install the package "Windows8.1-KB2999226-x64.msu" manually.

- In order to keep the installation simple, both the myrtille gateway and services are installed on the same machine. They do however conform to a distributed architecture; if needed, given some additionnal code, myrtille services could acts as a proxy, so the gateway could be installed and operate separately (this could be handy if the gateway should go into a DMZ).

## Troubleshoot
First at all, ensure the Myrtille prerequisites are met (see "Prerequisites"). Note that IIS must be installed separately, before running the installer (see "Installation").

- The installation fails
	- Prerequisites are not downloaded: the installer was run directly using the MSI file; this exclude the bootstrapper (Setup.exe), whose purpose is to check and download/install the prerequisites if necessary
	- Prerequisites download fails: MSI installers use Internet Explorer for downloads; ensure the Internet Explorer enhanced security is disabled (server administration tools)
	- Error 1001: an MSI custom action failed (https://blogs.msdn.microsoft.com/vsnetsetup/2012/03/14/msi-installation-fails-while-installing-a-custom-action-with-error-1001-exception-occurred-while-initializing-the-installation/). Ensure IIS and the .NET framework are installed (http://stackoverflow.com/a/8487214) and the IIS Management Console is enabled (http://stackoverflow.com/a/23263836)
	- Check the Windows events logs ("System", "Application", etc.)

- I can't access http://yourserver/myrtille
	- Ensure IIS is started and "Myrtille.Web" application is running on the "MyrtilleAppPool" application pool.
	- Ensure .NET 4.0 is installed and the "MyrtilleAppPool" is running on it.
	- If using HTTPS, ensure a valid SSL certificate is installed on IIS and exported as .PFX into Myrtille "ssl" folder (see "Security").

- Nothing happens when I click "Connect!"
	- Ensure you entered valid connection information (server address, user credentials, etc.).
	- Ensure the network traffic (websockets and xmlhttp in particular) is not blocked by a firewall, proxy, reverse proxy, VPN or whatever.
	- Ensure IIS is started and "Myrtille.Web" application is running on the "MyrtilleAppPool" application pool.
	- Ensure .NET 4.0 is installed and the "MyrtilleAppPool" is running on it.
	- If using HTTPS with HTML5 rendering (hence secure websockets, WSS), ensure the secured websockets port (default 8431) is opened (see "Security").
	- Ensure the "Myrtille.Services" Windows service (or console application if running under Visual Studio) is started.
	- Ensure the RDP client ("wfreerdp.exe") does exists (into the "Myrtille.RDP" output folder, if running under Visual Studio, or into the "bin" folder otherwise); if not, you need to retrieve (git clone) the myrtille FreeRDP fork into the "Myrtille.RDP" folder and build it there.
	- Ensure the Microsoft Visual C++ 2015 redistributables (x86) are installed (control panel > programs > programs and features); they are required by the RDP client (myrtille\bin\wfreerdp.exe).
	- Try to run wfreerdp.exe (just double-click it, no params), into the myrtille binaries folder. if there is a missing dll, or another issue, Windows should popup an error message (i.e.: MSVCR120.dll = MSVC 12 = Microsoft Visual C++ 2013 redistributables, MSVCP140.dll = MSVC 14 = Microsoft Visual C++ 2015 redistributables, etc.). Depending on your build environment and options, you will need the appropriate redistributables.
	- Check the RDP server configuration (does the user exists, is it a member of the "Remote Desktop Users" group, are Remote Desktop CALs valid?, etc.). You can setup it automatically by importing the "Myrtille.RDP\RDPSetup.reg" file into registry.
	- Check the RDP server windows event logs.
	- Check the gateway windows event logs, particulary regarding .NET.
	- Retry with debug enabled and check logs (into the "log" folder). You can change their verbosity level in config (but be warned it will affect peformance and flood the disk if set too verbose).

- The RDP session continues to run after clicking "Disconnect"
	- Check the RDP server configuration (session disconnect timeout in particular). You can setup it automatically by importing the Myrtille "RDPSetup.reg" file into registry.

- Myrtille is slow or buggy
	- Enable the stats bar to have detailed information about the current connection. Check latency and bandwidth, among other things.
	- Ensure debug is disabled or otherwise logs are not set to "Information" level (Myrtille "Web.Config" file, "system.diagnostics" section, default is "Warning"). Check logs, if debug is enabled.
	- If debug is enabled and you are running Myrtille in debug mode under Visual Studio, you will have the FreeRDP window (session display) and console (rdp events) shown to you. It may help to debug.
	- Switch from HTML4 to HTML5 rendering, or inversely (should be faster with HTML5).
	- Check your network configuration (is something filtering the traffic?) and capabilities (high latency or small bandwidth?).
	- Maybe the default settings are not adapted to your configuration. You can tweak the "js/config.js" file as you wish (see extensive comments there).
	- Despite my best efforts to produce quality and efficient code, I may have missed/messed something... Please don't hesitate to tell me or add your contribution! Thanks! :)