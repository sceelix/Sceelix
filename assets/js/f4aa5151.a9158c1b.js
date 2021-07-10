(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[1855],{3905:function(e,t,r){"use strict";r.d(t,{Zo:function(){return u},kt:function(){return d}});var n=r(7294);function o(e,t,r){return t in e?Object.defineProperty(e,t,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[t]=r,e}function i(e,t){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),r.push.apply(r,n)}return r}function a(e){for(var t=1;t<arguments.length;t++){var r=null!=arguments[t]?arguments[t]:{};t%2?i(Object(r),!0).forEach((function(t){o(e,t,r[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):i(Object(r)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))}))}return e}function c(e,t){if(null==e)return{};var r,n,o=function(e,t){if(null==e)return{};var r,n,o={},i=Object.keys(e);for(n=0;n<i.length;n++)r=i[n],t.indexOf(r)>=0||(o[r]=e[r]);return o}(e,t);if(Object.getOwnPropertySymbols){var i=Object.getOwnPropertySymbols(e);for(n=0;n<i.length;n++)r=i[n],t.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(o[r]=e[r])}return o}var s=n.createContext({}),l=function(e){var t=n.useContext(s),r=t;return e&&(r="function"==typeof e?e(t):a(a({},t),e)),r},u=function(e){var t=l(e.components);return n.createElement(s.Provider,{value:t},e.children)},p={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},g=n.forwardRef((function(e,t){var r=e.components,o=e.mdxType,i=e.originalType,s=e.parentName,u=c(e,["components","mdxType","originalType","parentName"]),g=l(r),d=o,h=g["".concat(s,".").concat(d)]||g[d]||p[d]||i;return r?n.createElement(h,a(a({ref:t},u),{},{components:r})):n.createElement(h,a({ref:t},u))}));function d(e,t){var r=arguments,o=t&&t.mdxType;if("string"==typeof e||o){var i=r.length,a=new Array(i);a[0]=g;var c={};for(var s in t)hasOwnProperty.call(t,s)&&(c[s]=t[s]);c.originalType=e,c.mdxType="string"==typeof e?e:o,a[1]=c;for(var l=2;l<i;l++)a[l]=r[l];return n.createElement.apply(null,a)}return n.createElement.apply(null,r)}g.displayName="MDXCreateElement"},6410:function(e,t,r){"use strict";r.r(t),r.d(t,{frontMatter:function(){return a},contentTitle:function(){return c},metadata:function(){return s},toc:function(){return l},default:function(){return p}});var n=r(2122),o=r(9756),i=(r(7294),r(3905)),a={},c="Log Viewer",s={unversionedId:"Working With Sceelix/LogViewer",id:"Working With Sceelix/LogViewer",isDocsHomePage:!1,title:"Log Viewer",description:"The log window displays all kinds of debug, warning, error or otherwise important information.",source:"@site/docs/03-Working With Sceelix/08-LogViewer.md",sourceDirName:"03-Working With Sceelix",slug:"/Working With Sceelix/LogViewer",permalink:"/docs/Working With Sceelix/LogViewer",editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/03-Working With Sceelix/08-LogViewer.md",version:"current",sidebarPosition:8,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Data Explorer",permalink:"/docs/Working With Sceelix/DataExplorer"},next:{title:"Image Viewer",permalink:"/docs/Working With Sceelix/ImageViewer"}},l=[],u={toc:l};function p(e){var t=e.components,a=(0,o.Z)(e,["components"]);return(0,i.kt)("wrapper",(0,n.Z)({},u,a,{components:t,mdxType:"MDXLayout"}),(0,i.kt)("h1",{id:"log-viewer"},"Log Viewer"),(0,i.kt)("p",null,"The log window displays all kinds of debug, warning, error or otherwise important information."),(0,i.kt)("p",null,(0,i.kt)("img",{src:r(2078).Z})),(0,i.kt)("p",null,"Logs messages can be sent by any other Sceelix window. In addition to their textual message content, they are identified by their type: information, debug, warning and error. Each type features its own indicative icon, as shown in the image above. Messages are displayed in the order in which they are sent."),(0,i.kt)("p",null,"Error messages usually encompass more details about the source and reason for the event. In such cases, hovering the log message will show more details in a tooltip. Such details are also written to the ",(0,i.kt)("a",{parentName:"p",href:"../Setting%20Up/Logging"},"application logs"),"."),(0,i.kt)("p",null,"In some cases, double-clicking a message will trigger an action that will allow its exact origin to be traced back. Such is the case for graph nodes: double-clicking the message will zoom on the node that issued it, which can be of great help when troubleshooting problems."),(0,i.kt)("p",null,"The contents of the log window can be cleared at any moment using the button \u201cClear\u201d at the window top menu. Other windows can also issue commands to clear the windows. Such is the case of the ",(0,i.kt)("a",{parentName:"p",href:"GraphEditor"},"graph editor"),", if the option \u201cClear Logs on Execution\u201d is checked in the Sceelix Settings."))}p.isMDXComponent=!0},2078:function(e,t,r){"use strict";t.Z=r.p+"assets/images/LogWindow-7b3efcda8458d332a1b7a1c34474d11c.png"}}]);