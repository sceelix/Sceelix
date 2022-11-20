"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[8536],{8999:(A,e,t)=>{t.r(e),t.d(e,{Parameters:()=>P,assets:()=>v,contentTitle:()=>n,default:()=>u,frontMatter:()=>s,metadata:()=>h,toc:()=>d});var r=t(7462),c=t(7294),a=t(3905),C=t(9715);const s={title:"Path Decompose"},n=void 0,h={unversionedId:"Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf",id:"Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf",title:"Path Decompose",description:"",source:"@site/docs/07-Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf.mdx",sourceDirName:"07-Nodes/Nodes/Path",slug:"/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf.mdx",tags:[],version:"current",frontMatter:{title:"Path Decompose"},sidebar:"tutorialSidebar",previous:{title:"Path Create",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathCreateProcedure_fe4221f3-991c-4161-bbae-58f5a60f8d42"},next:{title:"Path Divide",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a"}},v={},d=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Outputs",id:"outputs",level:2}],P=A=>{let{children:e}=A;return(0,a.kt)(c.Fragment,null,(0,a.kt)(C.r,{name:"Entity Type",type:"Choice",open:!0,description:(0,a.kt)(c.Fragment,null,"The type of entities into which the path should be decomposed."),mdxType:"ParameterTree"}))},o={toc:d,Parameters:P};function u(A){let{components:e,...c}=A;return(0,a.kt)("wrapper",(0,r.Z)({},o,c,{components:e,mdxType:"MDXLayout"}),(0,a.kt)("p",null,"Decomposes a Path into vertices or edges, all without\ndestroying the links between the path parts."),(0,a.kt)("p",null,(0,a.kt)("img",{alt:"NodeImage_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf",src:t(6098).Z,width:"700",height:"300"})),(0,a.kt)("b",null,"Tags:")," Path, Entity",(0,a.kt)("h2",{id:"parameters"},"Parameters"),(0,a.kt)(P,{mdxType:"Parameters"}),(0,a.kt)("h2",{id:"inputs"},"Inputs"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("b",null,"Input")," [Single | ",(0,a.kt)("i",null,"Path"),"]: Path to be decomposed.")),(0,a.kt)("br",null),(0,a.kt)("h2",{id:"outputs"},"Outputs"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("b",null,"Output")," [",(0,a.kt)("i",null,"Entity"),']: Path subentities (according to the selected "Entity Type" parameter).'),(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("b",null,"Original")," [",(0,a.kt)("i",null,"Path"),"]: The original path.")))}u.isMDXComponent=!0},6098:(A,e,t)=>{t.d(e,{Z:()=>r});const r="data:image/png;base64,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"}}]);