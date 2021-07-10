(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[5176],{3905:function(e,t,r){"use strict";r.d(t,{Zo:function(){return p},kt:function(){return f}});var n=r(7294);function i(e,t,r){return t in e?Object.defineProperty(e,t,{value:r,enumerable:!0,configurable:!0,writable:!0}):e[t]=r,e}function o(e,t){var r=Object.keys(e);if(Object.getOwnPropertySymbols){var n=Object.getOwnPropertySymbols(e);t&&(n=n.filter((function(t){return Object.getOwnPropertyDescriptor(e,t).enumerable}))),r.push.apply(r,n)}return r}function a(e){for(var t=1;t<arguments.length;t++){var r=null!=arguments[t]?arguments[t]:{};t%2?o(Object(r),!0).forEach((function(t){i(e,t,r[t])})):Object.getOwnPropertyDescriptors?Object.defineProperties(e,Object.getOwnPropertyDescriptors(r)):o(Object(r)).forEach((function(t){Object.defineProperty(e,t,Object.getOwnPropertyDescriptor(r,t))}))}return e}function c(e,t){if(null==e)return{};var r,n,i=function(e,t){if(null==e)return{};var r,n,i={},o=Object.keys(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||(i[r]=e[r]);return i}(e,t);if(Object.getOwnPropertySymbols){var o=Object.getOwnPropertySymbols(e);for(n=0;n<o.length;n++)r=o[n],t.indexOf(r)>=0||Object.prototype.propertyIsEnumerable.call(e,r)&&(i[r]=e[r])}return i}var l=n.createContext({}),s=function(e){var t=n.useContext(l),r=t;return e&&(r="function"==typeof e?e(t):a(a({},t),e)),r},p=function(e){var t=s(e.components);return n.createElement(l.Provider,{value:t},e.children)},u={inlineCode:"code",wrapper:function(e){var t=e.children;return n.createElement(n.Fragment,{},t)}},d=n.forwardRef((function(e,t){var r=e.components,i=e.mdxType,o=e.originalType,l=e.parentName,p=c(e,["components","mdxType","originalType","parentName"]),d=s(r),f=i,h=d["".concat(l,".").concat(f)]||d[f]||u[f]||o;return r?n.createElement(h,a(a({ref:t},p),{},{components:r})):n.createElement(h,a({ref:t},p))}));function f(e,t){var r=arguments,i=t&&t.mdxType;if("string"==typeof e||i){var o=r.length,a=new Array(o);a[0]=d;var c={};for(var l in t)hasOwnProperty.call(t,l)&&(c[l]=t[l]);c.originalType=e,c.mdxType="string"==typeof e?e:i,a[1]=c;for(var s=2;s<o;s++)a[s]=r[s];return n.createElement.apply(null,a)}return n.createElement.apply(null,r)}d.displayName="MDXCreateElement"},2426:function(e,t,r){"use strict";r.r(t),r.d(t,{frontMatter:function(){return a},contentTitle:function(){return c},metadata:function(){return l},toc:function(){return s},default:function(){return u}});var n=r(2122),i=r(9756),o=(r(7294),r(3905)),a={},c="Data Explorer",l={unversionedId:"Working With Sceelix/DataExplorer",id:"Working With Sceelix/DataExplorer",isDocsHomePage:!1,title:"Data Explorer",description:"The data explorer is the most generic viewer for information generated through graphs. Its purpose is to display all entities that have been produced in the last execution.",source:"@site/docs/03-Working With Sceelix/07-DataExplorer.md",sourceDirName:"03-Working With Sceelix",slug:"/Working With Sceelix/DataExplorer",permalink:"/docs/Working With Sceelix/DataExplorer",editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/03-Working With Sceelix/07-DataExplorer.md",version:"current",sidebarPosition:7,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Graph Editor",permalink:"/docs/Working With Sceelix/GraphEditor"},next:{title:"Log Viewer",permalink:"/docs/Working With Sceelix/LogViewer"}},s=[],p={toc:s};function u(e){var t=e.components,a=(0,i.Z)(e,["components"]);return(0,o.kt)("wrapper",(0,n.Z)({},p,a,{components:t,mdxType:"MDXLayout"}),(0,o.kt)("h1",{id:"data-explorer"},"Data Explorer"),(0,o.kt)("p",null,"The data explorer is the most generic viewer for information generated through graphs. Its purpose is to display all entities that have been produced in the last execution."),(0,o.kt)("p",null,"The entities are displayed in a hierarchy, with the main entities at the base level and their subentities as subitems. The attributes of each entity are also displayed, but in an orange color."),(0,o.kt)("p",null,(0,o.kt)("img",{src:r(1419).Z})),(0,o.kt)("p",null,"The base entity items can be selected, so as to view more details in the ",(0,o.kt)("a",{parentName:"p",href:"Inspector"},"inspector window"),". If the 3D Viewer is open and the selected entity has a visual representation, it will be highlighted. In addition, double-clicking the item will trigger a camera zoom to the entity."))}u.isMDXComponent=!0},1419:function(e,t,r){"use strict";t.Z=r.p+"assets/images/DataExplorerWindow-0c9186f82582799af89e4f6c2b253a90.png"}}]);