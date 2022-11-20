"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[5977],{4990:(e,A,t)=>{t.r(A),t.d(A,{Parameters:()=>i,assets:()=>v,contentTitle:()=>a,default:()=>f,frontMatter:()=>C,metadata:()=>u,toc:()=>d});var s=t(7462),r=t(7294),n=t(3905),c=t(9715);const C={title:"Mesh Unify"},a=void 0,u={unversionedId:"Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7",id:"Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7",title:"Mesh Unify",description:"",source:"@site/docs/07-Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7.mdx",sourceDirName:"07-Nodes/Nodes/Mesh",slug:"/Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7",permalink:"/docs/Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUnifyProcedure_a66e6c1f-3448-4813-a959-6c4fb3d01ea7.mdx",tags:[],version:"current",frontMatter:{title:"Mesh Unify"},sidebar:"tutorialSidebar",previous:{title:"Mesh UV Map",permalink:"/docs/Nodes/Nodes/Mesh/Sceelix-Meshes-Procedures-MeshUVMapProcedure_9470e48f-5ee8-4d84-a371-949f27786f0f"},next:{title:"Mesh Instance Create",permalink:"/docs/Nodes/Nodes/Mesh Instance/Sceelix-Meshes-Procedures-MeshInstanceCreateProcedure_9924b6f2-761d-4cfd-b06e-6ba5b7cf2072"}},v={},d=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Outputs",id:"outputs",level:2}],i=e=>{let{children:A}=e;return(0,n.kt)(r.Fragment,null,(0,n.kt)(c.r,{name:"Operations",type:"Select",open:!0,description:(0,n.kt)(r.Fragment,null,"Type of unification operation to perform."),mdxType:"ParameterTree"},(0,n.kt)(c.r,{name:"Remove Coincident Faces",type:"Compound",description:(0,n.kt)(r.Fragment,null,"Removes faces that may be connecting the same vertices."),mdxType:"ParameterTree"}),(0,n.kt)(c.r,{name:"Unify Planar Faces",type:"Compound",description:(0,n.kt)(r.Fragment,null,"Unifies faces that lie on the same plane. Vertices should be shared, so applying a vertex unification first may be necessary."),mdxType:"ParameterTree"}),(0,n.kt)(c.r,{name:"Unify Vertices",type:"Compound",description:(0,n.kt)(r.Fragment,null,"Merges mesh vertices that are overlapping (or positioned close together) into the same vertex reference.",(0,n.kt)("br",null)),mdxType:"ParameterTree"},(0,n.kt)(c.r,{name:"Tolerance",type:"Float",description:(0,n.kt)(r.Fragment,null,"The distance tolerance used for determining if two vertices should be joined."),mdxType:"ParameterTree"}))))},o={toc:d,Parameters:i};function f(e){let{components:A,...r}=e;return(0,n.kt)("wrapper",(0,s.Z)({},o,r,{components:A,mdxType:"MDXLayout"}),(0,n.kt)("p",null,"Unifies faces and vertices of meshes."),(0,n.kt)("p",null,(0,n.kt)("img",{alt:"NodeImage_a66e6c1f-3448-4813-a959-6c4fb3d01ea7",src:t(8408).Z,width:"700",height:"300"})),(0,n.kt)("b",null,"Tags:")," Mesh, Remove Coincident Faces, Unify Planar Faces, Unify Vertices",(0,n.kt)("h2",{id:"parameters"},"Parameters"),(0,n.kt)(i,{mdxType:"Parameters"}),(0,n.kt)("h2",{id:"inputs"},"Inputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("b",null,"Input")," [Single | ",(0,n.kt)("i",null,"Mesh"),"]: The Mesh to process.")),(0,n.kt)("br",null),(0,n.kt)("h2",{id:"outputs"},"Outputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("b",null,"Output")," [",(0,n.kt)("i",null,"Mesh"),"]: The Mesh that was processed.")))}f.isMDXComponent=!0},8408:(e,A,t)=>{t.d(A,{Z:()=>s});const s="data:image/png;base64,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"}}]);