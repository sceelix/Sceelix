(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[5797],{3905:function(e,t,n){"use strict";n.d(t,{Zo:function(){return d},kt:function(){return h}});var a=n(7294);function i(e,t,n){return t in e?Object.defineProperty(e,t,{value:n,enumerable:!0,configurable:!0,writable:!0}):e[t]=n,e}function r(e,t){var n=Object.keys(e);if(Object.getOwnPropertySymbols){var a=Object.getOwnPropertySymbols(e);t&&(a=a.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),n.push.apply(n,a)}return n}function l(e){for(var t=1;t<arguments.length;t++){var n=null!=arguments[t]?arguments[t]:{};t%2?r(Object(n),!0).forEach((function(t){i(e,t,n[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(n)):r(Object(n)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(n,t))}))}return e}function o(e,t){if(null==e)return{};var n,a,i=function(e,t){if(null==e)return{};var n,a,i={},r=Object.keys(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||(i[n]=e[n]);return i}(e,t);if(Object.getOwnPropertySymbols){var r=Object.getOwnPropertySymbols(e);for(a=0;a<r.length;a++)n=r[a],t.indexOf(n)>=0||Object.prototype.propertyIsEnumerable.call(e,n)&&(i[n]=e[n])}return i}var s=a.createContext({}),c=function(e){var t=a.useContext(s),n=t;return e&&(n="function"==typeof e?e(t):l(l({},t),e)),n},d=function(e){var t=c(e.components);return a.createElement(s.Provider,{value:t},e.children)},p={inlineCode:"code",wrapper:function(e){var t=e.children;return a.createElement(a.Fragment,{},t)}},u=a.forwardRef((function(e,t){var n=e.components,i=e.mdxType,r=e.originalType,s=e.parentName,d=o(e,["components","mdxType","originalType","parentName"]),u=c(n),h=i,m=u["".concat(s,".").concat(h)]||u[h]||p[h]||r;return n?a.createElement(m,l(l({ref:t},d),{},{components:n})):a.createElement(m,l({ref:t},d))}));function h(e,t){var n=arguments,i=t&&t.mdxType;if("string"==typeof e||i){var r=n.length,l=new Array(r);l[0]=u;var o={};for(var s in t)hasOwnProperty.call(t,s)&&(o[s]=t[s]);o.originalType=e,o.mdxType="string"==typeof e?e:i,l[1]=o;for(var c=2;c<r;c++)l[c]=n[c];return a.createElement.apply(null,l)}return a.createElement.apply(null,n)}u.displayName="MDXCreateElement"},1228:function(e,t,n){"use strict";n.r(t),n.d(t,{frontMatter:function(){return l},contentTitle:function(){return o},metadata:function(){return s},toc:function(){return c},default:function(){return p}});var a=n(2122),i=n(9756),r=(n(7294),n(3905)),l={},o="Installation and Dependencies",s={unversionedId:"Setting Up/Installation",id:"Setting Up/Installation",isDocsHomePage:!1,title:"Installation and Dependencies",description:"Sceelix is available for Windows, Mac and Linux platforms and the installation process for each platform differs.",source:"@site/docs/02-Setting Up/01-Installation.md",sourceDirName:"02-Setting Up",slug:"/Setting Up/Installation",permalink:"/docs/Setting Up/Installation",editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/02-Setting Up/01-Installation.md",version:"current",sidebarPosition:1,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"FAQ",permalink:"/docs/Introduction/FAQ"},next:{title:"Executing Sceelix",permalink:"/docs/Setting Up/Executing"}},c=[{value:"Windows",id:"windows",children:[{value:"Installer",id:"installer",children:[]},{value:"Portable",id:"portable",children:[]}]},{value:"Mac",id:"mac",children:[]},{value:"Linux",id:"linux",children:[]}],d={toc:c};function p(e){var t=e.components,n=(0,i.Z)(e,["components"]);return(0,r.kt)("wrapper",(0,a.Z)({},d,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("h1",{id:"installation-and-dependencies"},"Installation and Dependencies"),(0,r.kt)("p",null,"Sceelix is available for Windows, Mac and Linux platforms and the installation process for each platform differs."),(0,r.kt)("h2",{id:"windows"},"Windows"),(0,r.kt)("h3",{id:"installer"},"Installer"),(0,r.kt)("p",null,"Sceelix provides an installer that already contains all the necessary dependencies. Once you\u2019ve downloaded the installer, simply double-click the file to launch the installation process.  Like any other installation wizard, you\u2019ll be faced with the possibility to change the installation directory and shortcut creation options. The installer will then proceed to install Sceelix and all its dependencies for you. Configuration data and other extras are then unpacked to your User ",(0,r.kt)("inlineCode",{parentName:"p"},"AppData")," and ",(0,r.kt)("inlineCode",{parentName:"p"},"Documents")," folder when you first run Sceelix."),(0,r.kt)("h3",{id:"portable"},"Portable"),(0,r.kt)("p",null,"In the case you can\u2019t run installers or simply prefer to carry Sceelix in a USB flash drive, portable versions are always useful. The zip file that you\u2019ll download should first be extracted. It is not recommended to choose a location that may require special permissions (such as ",(0,r.kt)("inlineCode",{parentName:"p"},"Program Files")," or the ",(0,r.kt)("inlineCode",{parentName:"p"},"Windows")," folder). The extracted folder contains all the Sceelix files and a folder with the dependencies, which you\u2019ll need to have installed first. They are:"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"https://www.microsoft.com/en-us/download/details.aspx?id=49982"},".NET 4.6.1 Framework"),", although this is usually already present in most Windows setups."),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{parentName:"li",href:"https://www.microsoft.com/en-us/download/details.aspx?id=8109"},"DirectX End-User Runtimes (June 2010)"),", even though you might already have DirectX in your system, this may be required.")),(0,r.kt)("p",null,"In this portable version, all the configuration data (and extras) is stored in the original folder itself."),(0,r.kt)("h2",{id:"mac"},"Mac"),(0,r.kt)("p",null,"Double-click the downloaded dmg file. This will show a window with the installation instructions. In case you haven\u2019t installed Mono before, you should double-click the ",(0,r.kt)("inlineCode",{parentName:"p"},"MonoInstaller.pkg")," and follow the steps in the installation wizard. Afterwards, just drag and drop the Sceelix folder to the ",(0,r.kt)("inlineCode",{parentName:"p"},"Applications")," folder and Sceelix will be installed!"),(0,r.kt)("p",null,"When you\u2019ll first execute Sceelix on a new machine, a font caching process will need to take place, during which Sceelix will apparently hang and remain unresponsive. This is a standard and necessary process, lead by Mono (making it beyond Sceelix's control) and usually takes a few minutes. The subsequent executions (even after updates) will not need to undergo this process."),(0,r.kt)("h2",{id:"linux"},"Linux"),(0,r.kt)("p",null,"Since it uses Mono, Sceelix can run on pretty much every Linux distribution that supports it as well. At this point, ",(0,r.kt)("inlineCode",{parentName:"p"},".deb")," packages (for Ubuntu, Debian..) and ",(0,r.kt)("inlineCode",{parentName:"p"},".rpm")," packages (for Red Hat, CentOS, Fedora, SUSE\u2026) are provided, which in some systems can be easily installed using package managers (for example, Ubuntu already provides the ",(0,r.kt)("inlineCode",{parentName:"p"},"Ubuntu Software Center"),"). For most of these managers, a simple double-click on the package will launch the package manager, after which instructions are shown. Alternatively, you can install the packages using the terminal, as shown below."),(0,r.kt)("h4",{id:"to-install-the-deb-package-from-the-terminal"},"To install the .deb package from the terminal:"),(0,r.kt)("p",null,"Run"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre"},"sudo apt-get install mono-complete\n")),(0,r.kt)("p",null,"to install Mono. Then run:"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre"},"sudo dpkg -i Sceelix-X.X.X.X-LinuxInstaller.deb\n")),(0,r.kt)("p",null,"to install Sceelix (where X.X.X.X is the version number you are installing)."),(0,r.kt)("h4",{id:"to-install-the-rpm-package-from-the-terminal"},"To install the .rpm package from the terminal:"),(0,r.kt)("p",null,"Run"),(0,r.kt)("pre",null,(0,r.kt)("code",{parentName:"pre"},"rpm -ivh  -i Sceelix-X.X.X.X-LinuxInstaller.rpm\n")),(0,r.kt)("p",null,"to install Sceelix (where X.X.X.X is the version number you are installing) and the Mono dependencies."))}p.isMDXComponent=!0}}]);