(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[1427],{4923:function(e,A,t){"use strict";t.r(A),t.d(A,{frontMatter:function(){return i},contentTitle:function(){return C},metadata:function(){return d},toc:function(){return a},Parameters:function(){return l},default:function(){return c}});var r=t(2122),n=t(9756),o=t(7294),s=t(3905),u=t(3910),i={title:"Log"},C=void 0,d={unversionedId:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414",id:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414",isDocsHomePage:!1,title:"Log",description:"Writes logging messages to the designated log panel.",source:"@site/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414.mdx",sourceDirName:"07-Nodes/Nodes/Basic",slug:"/Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414",editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-LogProcedure_3ec9e7df-c668-4b94-920c-143e01083414.mdx",version:"current",frontMatter:{title:"Log"},sidebar:"tutorialSidebar",previous:{title:"Group",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-GroupProcedure_44d56290-9f2d-4add-b0a6-dbfaaaed1e18"},next:{title:"Property",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-PropertyProcedure_c29f89c9-5215-49cb-8270-663b2cbd4cd3"}},a=[{value:"Parameters",id:"parameters",children:[]},{value:"Inputs",id:"inputs",children:[{value:"Parameter Inputs",id:"parameter-inputs",children:[]}]},{value:"Outputs",id:"outputs",children:[]}],l=function(e){e.children;return(0,s.kt)(o.Fragment,null,(0,s.kt)(u.r,{name:"Inputs",type:"Select",open:!0,description:(0,s.kt)(o.Fragment,null,"The type of input port. ",(0,s.kt)("br",null),"Setting a ",(0,s.kt)("b",null,"Single")," (circle) input means that the node will be executed once per entity. Useful to log a different message for each entity. ",(0,s.kt)("br",null),"Setting a ",(0,s.kt)("b",null,"Collective")," (square) input means that the node will be executed once for all entities. Useful to log a message for a whole set of entities."),mdxType:"ParameterTree"},(0,s.kt)(u.r,{name:"Single",type:"Compound",ports:[1],description:(0,s.kt)(o.Fragment,null,"Processes one Entity entity at a time."),mdxType:"ParameterTree"}),(0,s.kt)(u.r,{name:"Collective",type:"Compound",ports:[2],description:(0,s.kt)(o.Fragment,null,"Processes all Entity entities at a time."),mdxType:"ParameterTree"})),(0,s.kt)(u.r,{name:"Type",type:"Choice",open:!0,description:(0,s.kt)(o.Fragment,null,"Type of message to printed."),mdxType:"ParameterTree"}),(0,s.kt)(u.r,{name:"Messages",type:"List",open:!0,description:(0,s.kt)(o.Fragment,null,"The list of messages to be printed."),mdxType:"ParameterTree"},(0,s.kt)(u.r,{name:"Text",type:"String",description:(0,s.kt)(o.Fragment,null,"A message to log."),mdxType:"ParameterTree"})))},g={toc:a,Parameters:l};function c(e){var A=e.components,o=(0,n.Z)(e,["components"]);return(0,s.kt)("wrapper",(0,r.Z)({},g,o,{components:A,mdxType:"MDXLayout"}),(0,s.kt)("p",null,"Writes logging messages to the designated log panel."),(0,s.kt)("p",null,(0,s.kt)("img",{alt:"NodeImage_3ec9e7df-c668-4b94-920c-143e01083414",src:t(1115).Z})),(0,s.kt)("b",null,"Tags:")," Entity",(0,s.kt)("h2",{id:"parameters"},"Parameters"),(0,s.kt)(l,{mdxType:"Parameters"}),(0,s.kt)("h2",{id:"inputs"},"Inputs"),(0,s.kt)("i",null,"This node has no native inputs."),(0,s.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,s.kt)("ul",null,(0,s.kt)("li",{parentName:"ul"},(0,s.kt)("a",{name:"port1"},"[1]")," ",(0,s.kt)("b",null,"Single")," [Single | ",(0,s.kt)("i",null,"Entity"),"]: Processes one Entity entity at a time."),(0,s.kt)("li",{parentName:"ul"},(0,s.kt)("a",{name:"port2"},"[2]")," ",(0,s.kt)("b",null,"Collective")," [Collective | ",(0,s.kt)("i",null,"Entity"),"]: Processes all Entity entities at a time.",(0,s.kt)("br",null))),(0,s.kt)("h2",{id:"outputs"},"Outputs"),(0,s.kt)("ul",null,(0,s.kt)("li",{parentName:"ul"},(0,s.kt)("b",null,"Output")," [",(0,s.kt)("i",null,"Entity"),"]: Entities that were sent to the input.")))}c.isMDXComponent=!0},1115:function(e,A){"use strict";A.Z="data:image/png;base64,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"}}]);