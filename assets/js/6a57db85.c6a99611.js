"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[242],{251:(e,t,a)=>{a.r(t),a.d(t,{Parameters:()=>o,assets:()=>d,contentTitle:()=>u,default:()=>m,frontMatter:()=>n,metadata:()=>c,toc:()=>i});var A=a(7462),r=a(7294),l=a(3905),s=a(9715);const n={title:"Surface Sample"},u=void 0,c={unversionedId:"Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5",id:"Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5",title:"Surface Sample",description:"",source:"@site/docs/07-Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5.mdx",sourceDirName:"07-Nodes/Nodes/Surface",slug:"/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5.mdx",tags:[],version:"current",frontMatter:{title:"Surface Sample"},sidebar:"tutorialSidebar",previous:{title:"Surface Place",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81"},next:{title:"Surface Save",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSaveProcedure_88daf46e-8bdb-408e-b445-3d0f633edf8c"}},d={},i=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Parameter Inputs",id:"parameter-inputs",level:3},{value:"Outputs",id:"outputs",level:2}],o=e=>{let{children:t}=e;return(0,l.kt)(r.Fragment,null,(0,l.kt)(s.r,{name:"Inputs",type:"Select",open:!0,description:(0,l.kt)(r.Fragment,null,"The surface(s) to be sampled. ",(0,l.kt)("br",null),"Setting a ",(0,l.kt)("b",null,"Single")," (circle) input means that the node will be executed once per surface. ",(0,l.kt)("br",null),"Setting a ",(0,l.kt)("b",null,"Collective")," (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be placed on the surfaces, but it would be complex to match with the right surface."),mdxType:"ParameterTree"},(0,l.kt)(s.r,{name:"Single",type:"Compound",ports:[1],description:(0,l.kt)(r.Fragment,null,"Processes one Surface entity at a time."),mdxType:"ParameterTree"}),(0,l.kt)(s.r,{name:"Collective",type:"Compound",ports:[2],description:(0,l.kt)(r.Fragment,null,"Processes all Surface entities at a time."),mdxType:"ParameterTree"})),(0,l.kt)(s.r,{name:"Data",type:"Multi-Type List",open:!0,description:(0,l.kt)(r.Fragment,null,"The surface layer from which the data should be sampled."),mdxType:"ParameterTree"},(0,l.kt)(s.r,{name:"Colors",type:"Compound",description:(0,l.kt)(r.Fragment,null,"Samples color information from the surface."),mdxType:"ParameterTree"},(0,l.kt)(s.r,{name:"Sampling",type:"Choice",description:(0,l.kt)(r.Fragment,null,"The sampling method. Since there may be several values corresponding to the area of the actor, there are several ways to keep the sampled data:",(0,l.kt)("br",null),(0,l.kt)("b",null,"First")," means that the first value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Average")," means that the values will be averaged.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Last")," means that the last value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"List")," means that all the values will be returned as a list."),mdxType:"ParameterTree"}),(0,l.kt)(s.r,{name:"Data",type:"Attribute",description:(0,l.kt)(r.Fragment,null,"Attribute where the sampled data should be stored to."),mdxType:"ParameterTree"})),(0,l.kt)(s.r,{name:"Heights",type:"Compound",description:(0,l.kt)(r.Fragment,null,"Samples height information from the surface."),mdxType:"ParameterTree"},(0,l.kt)(s.r,{name:"Sampling",type:"Choice",description:(0,l.kt)(r.Fragment,null,"The sampling method. Since there may be several values corresponding to the area of the actor, there are several ways to keep the sampled data:",(0,l.kt)("br",null),(0,l.kt)("b",null,"First")," means that the first value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Average")," means that the values will be averaged.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Last")," means that the last value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"List")," means that all the values will be returned as a list."),mdxType:"ParameterTree"}),(0,l.kt)(s.r,{name:"Data",type:"Attribute",description:(0,l.kt)(r.Fragment,null,"Attribute where the sampled data should be stored to."),mdxType:"ParameterTree"})),(0,l.kt)(s.r,{name:"Normals",type:"Compound",description:(0,l.kt)(r.Fragment,null,"Samples normal information from the surface."),mdxType:"ParameterTree"},(0,l.kt)(s.r,{name:"Sampling",type:"Choice",description:(0,l.kt)(r.Fragment,null,"The sampling method. Since there may be several values corresponding to the area of the actor, there are several ways to keep the sampled data:",(0,l.kt)("br",null),(0,l.kt)("b",null,"First")," means that the first value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Average")," means that the values will be averaged.",(0,l.kt)("br",null),(0,l.kt)("b",null,"Last")," means that the last value will be chosen.",(0,l.kt)("br",null),(0,l.kt)("b",null,"List")," means that all the values will be returned as a list."),mdxType:"ParameterTree"}),(0,l.kt)(s.r,{name:"Data",type:"Attribute",description:(0,l.kt)(r.Fragment,null,"Attribute where the sampled data should be stored to."),mdxType:"ParameterTree"}))))},f={toc:i,Parameters:o};function m(e){let{components:t,...r}=e;return(0,l.kt)("wrapper",(0,A.Z)({},f,r,{components:t,mdxType:"MDXLayout"}),(0,l.kt)("p",null,"Performs sampling/extraction of surface layer data, such as height, color\nor normals, storing this information as attributes the actors placed in the same area."),(0,l.kt)("p",null,(0,l.kt)("img",{alt:"NodeImage_b91f4cb3-e7e9-40db-8fce-6352b88683f5",src:a(9325).Z,width:"700",height:"300"})),(0,l.kt)("b",null,"Tags:")," Actor, Surface",(0,l.kt)("h2",{id:"parameters"},"Parameters"),(0,l.kt)(o,{mdxType:"Parameters"}),(0,l.kt)("h2",{id:"inputs"},"Inputs"),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("b",null,"Actor")," [Collective | ",(0,l.kt)("i",null,"Actor"),"]: The actor whose position is to be used for sampling.")),(0,l.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("a",{name:"port1"},"[1]")," ",(0,l.kt)("b",null,"Single")," [Single | ",(0,l.kt)("i",null,"Surface"),"]: Processes one Surface entity at a time."),(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("a",{name:"port2"},"[2]")," ",(0,l.kt)("b",null,"Collective")," [Collective | ",(0,l.kt)("i",null,"Surface"),"]: Processes all Surface entities at a time.",(0,l.kt)("br",null))),(0,l.kt)("h2",{id:"outputs"},"Outputs"),(0,l.kt)("ul",null,(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("b",null,"Surface")," [",(0,l.kt)("i",null,"Surface"),"]: Surface that was sampled."),(0,l.kt)("li",{parentName:"ul"},(0,l.kt)("b",null,"Actor")," [",(0,l.kt)("i",null,"Actor"),"]: The actor whose position was used for sampling.")))}m.isMDXComponent=!0},9325:(e,t,a)=>{a.d(t,{Z:()=>A});const A="data:image/png;base64,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"}}]);