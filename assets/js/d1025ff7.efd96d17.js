"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[4008],{2868:(e,t,r)=>{r.r(t),r.d(t,{Parameters:()=>o,assets:()=>i,contentTitle:()=>u,default:()=>m,frontMatter:()=>l,metadata:()=>c,toc:()=>f});var A=r(7462),a=r(7294),n=r(3905),s=r(9715);const l={title:"Surface Place"},u=void 0,c={unversionedId:"Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81",id:"Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81",title:"Surface Place",description:"",source:"@site/docs/07-Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81.mdx",sourceDirName:"07-Nodes/Nodes/Surface",slug:"/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePlaceProcedure_2dbcc442-f71a-43af-a11a-ece62ff48f81.mdx",tags:[],version:"current",frontMatter:{title:"Surface Place"},sidebar:"tutorialSidebar",previous:{title:"Surface Paint",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfacePaintProcedure_690a154f-5fa0-4f60-bf12-25f25d764db0"},next:{title:"Surface Sample",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSampleProcedure_b91f4cb3-e7e9-40db-8fce-6352b88683f5"}},i={},f=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Parameter Inputs",id:"parameter-inputs",level:3},{value:"Outputs",id:"outputs",level:2},{value:"Parameter Outputs",id:"parameter-outputs",level:3}],o=e=>{let{children:t}=e;return(0,n.kt)(a.Fragment,null,(0,n.kt)(s.r,{name:"Inputs",type:"Select",open:!0,description:(0,n.kt)(a.Fragment,null,"The type of input port. ",(0,n.kt)("br",null),"Setting a ",(0,n.kt)("b",null,"Single")," (circle) input means that the node will be executed once per surface. ",(0,n.kt)("br",null),"Setting a ",(0,n.kt)("b",null,"Collective")," (square) input means that the node will be executed once for all surfaces. Especially useful when actors are to be placed on the surfaces, but it would be complex to match with the right surface."),mdxType:"ParameterTree"},(0,n.kt)(s.r,{name:"Single",type:"Compound",ports:[1],description:(0,n.kt)(a.Fragment,null,"Processes one Surface entity at a time."),mdxType:"ParameterTree"}),(0,n.kt)(s.r,{name:"Collective",type:"Compound",ports:[2],description:(0,n.kt)(a.Fragment,null,"Processes all Surface entities at a time."),mdxType:"ParameterTree"})),(0,n.kt)(s.r,{name:"Entity",type:"Select",open:!0,description:(0,n.kt)(a.Fragment,null,"Entities to place on the surface."),mdxType:"ParameterTree"},(0,n.kt)(s.r,{name:"Actor",type:"Compound",ports:[3,4],description:(0,n.kt)(a.Fragment,null,"Places actors on the surface."),mdxType:"ParameterTree"},(0,n.kt)(s.r,{name:"Sample Location",type:"Compound",description:(0,n.kt)(a.Fragment,null,"Location to use as placement reference."),mdxType:"ParameterTree"},(0,n.kt)(s.r,{name:"Position",type:"Vector3D",description:(0,n.kt)(a.Fragment,null,"Position relative to the actor scope origin."),mdxType:"ParameterTree"}),(0,n.kt)(s.r,{name:"Offset",type:"Choice",description:(0,n.kt)(a.Fragment,null,"Indicates if the position is measured as absolute units or scope-size relative value (between 0 and 1)"),mdxType:"ParameterTree"}))),(0,n.kt)(s.r,{name:"Mesh",type:"Compound",ports:[5,6],description:(0,n.kt)(a.Fragment,null,"Places meshes on the surface."),mdxType:"ParameterTree"},(0,n.kt)(s.r,{name:"Keep Planar",type:"Boolean",description:(0,n.kt)(a.Fragment,null,"Indicates if the meshes adapt all their vertices on the terrain, of if they should be forced to remain planar Enforcing planarity could mean that not all vertices would be lying on the terrain."),mdxType:"ParameterTree"})),(0,n.kt)(s.r,{name:"Path",type:"Compound",ports:[7,8],description:(0,n.kt)(a.Fragment,null,"Places paths on the surface."),mdxType:"ParameterTree"})))},d={toc:f,Parameters:o};function m(e){let{components:t,...a}=e;return(0,n.kt)("wrapper",(0,A.Z)({},d,a,{components:t,mdxType:"MDXLayout"}),(0,n.kt)("p",null,"Allows the placement of entities on top of surfaces."),(0,n.kt)("p",null,(0,n.kt)("img",{alt:"NodeImage_2dbcc442-f71a-43af-a11a-ece62ff48f81",src:r(8678).Z,width:"700",height:"300"})),(0,n.kt)("b",null,"Tags:")," Surface",(0,n.kt)("h2",{id:"parameters"},"Parameters"),(0,n.kt)(o,{mdxType:"Parameters"}),(0,n.kt)("h2",{id:"inputs"},"Inputs"),(0,n.kt)("i",null,"This node has no native inputs."),(0,n.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port1"},"[1]")," ",(0,n.kt)("b",null,"Single")," [Single | ",(0,n.kt)("i",null,"Surface"),"]: Processes one Surface entity at a time."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port2"},"[2]")," ",(0,n.kt)("b",null,"Collective")," [Collective | ",(0,n.kt)("i",null,"Surface"),"]: Processes all Surface entities at a time."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port3"},"[3]")," ",(0,n.kt)("b",null,"Actor")," [Collective | ",(0,n.kt)("i",null,"Actor"),"]: Actors to be placed on the surface."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port5"},"[5]")," ",(0,n.kt)("b",null,"Mesh")," [Collective | ",(0,n.kt)("i",null,"Mesh"),"]: Meshes to be placed on the surface."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port7"},"[7]")," ",(0,n.kt)("b",null,"Path")," [Collective | ",(0,n.kt)("i",null,"Path"),"]: Paths to be placed on the surface.",(0,n.kt)("br",null))),(0,n.kt)("h2",{id:"outputs"},"Outputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("b",null,"Surface")," [",(0,n.kt)("i",null,"Surface"),"]: Surface on which the entities where placed.")),(0,n.kt)("h3",{id:"parameter-outputs"},"Parameter Outputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port4"},"[4]")," ",(0,n.kt)("b",null,"Actor")," [",(0,n.kt)("i",null,"Actor"),"]: Actors that were placed on the surface."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port6"},"[6]")," ",(0,n.kt)("b",null,"Mesh")," [",(0,n.kt)("i",null,"Mesh"),"]: Meshes that were placed on the surface."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port8"},"[8]")," ",(0,n.kt)("b",null,"Path")," [",(0,n.kt)("i",null,"Path"),"]: Paths that were placed on the surface.")))}m.isMDXComponent=!0},8678:(e,t,r)=>{r.d(t,{Z:()=>A});const A="data:image/png;base64,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"}}]);