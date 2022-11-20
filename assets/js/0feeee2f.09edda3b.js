"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[5173],{3007:(e,t,A)=>{A.r(t),A.d(t,{Parameters:()=>l,assets:()=>C,contentTitle:()=>s,default:()=>h,frontMatter:()=>i,metadata:()=>o,toc:()=>c});var a=A(7462),r=A(7294),d=A(3905),n=A(9715);const i={title:"Path Divide"},s=void 0,o={unversionedId:"Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a",id:"Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a",title:"Path Divide",description:"",source:"@site/docs/07-Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a.mdx",sourceDirName:"07-Nodes/Nodes/Path",slug:"/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDivideProcedure_5d95c0d1-140a-4735-90f2-97e8a34d7a7a.mdx",tags:[],version:"current",frontMatter:{title:"Path Divide"},sidebar:"tutorialSidebar",previous:{title:"Path Decompose",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathDecomposeProcedure_0f139dea-e38c-4b77-ab59-43f6e2c2bbcf"},next:{title:"Path Merge",permalink:"/docs/Nodes/Nodes/Path/Sceelix-Paths-Procedures-PathMergeProcedure_807f4655-bd7f-48e1-b809-1a9dee94c57a"}},C={},c=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Outputs",id:"outputs",level:2}],l=e=>{let{children:t}=e;return(0,d.kt)(r.Fragment,null,(0,d.kt)(n.r,{name:"Groups",type:"Multi-Type List",open:!0,description:(0,d.kt)(r.Fragment,null,"Criteria by which the path should be divided. If none is indicated, the whole set of edges will be considered."),mdxType:"ParameterTree"},(0,d.kt)(n.r,{name:"Adjacency",type:"Compound",description:(0,d.kt)(r.Fragment,null,"Divides the paths by their vertex connections."),mdxType:"ParameterTree"}),(0,d.kt)(n.r,{name:"Attribute",type:"Compound",description:(0,d.kt)(r.Fragment,null,"Divides the paths by attribute value, i.e. building sets of edges that share the same value."),mdxType:"ParameterTree"},(0,d.kt)(n.r,{name:"Value",type:"Object",description:(0,d.kt)(r.Fragment,null,"A parameter that accepts any type of data. Is set as an expression by default."),mdxType:"ParameterTree"})),(0,d.kt)(n.r,{name:"Direction",type:"Compound",description:(0,d.kt)(r.Fragment,null,"Divides the paths into sets of edges that share the same (approximate) normal direction."),mdxType:"ParameterTree"}),(0,d.kt)(n.r,{name:"Size",type:"Compound",description:(0,d.kt)(r.Fragment,null,"Divides the paths by number of edges or vertices, so that they don't exceed the requested size."),mdxType:"ParameterTree"},(0,d.kt)(n.r,{name:"Type",type:"Choice",description:(0,d.kt)(r.Fragment,null,"Type of element to divide by:",(0,d.kt)("br",null),(0,d.kt)("b",null,"Edges")," means that the paths will not exceed the indicated edge count.",(0,d.kt)("br",null),(0,d.kt)("b",null,"Vertices")," means that the paths will not exceed the indicated vertex count.",(0,d.kt)("br",null)),mdxType:"ParameterTree"}),(0,d.kt)(n.r,{name:"Count",type:"Int",description:(0,d.kt)(r.Fragment,null,"The maximum allowed count of edges or vertices to keep in each resulting path."),mdxType:"ParameterTree"}))),(0,d.kt)(n.r,{name:"Separate",type:"Optional",open:!0,description:(0,d.kt)(r.Fragment,null,"If true, each one of the edges of the path will be placed into a separate path entity."),mdxType:"ParameterTree"},(0,d.kt)(n.r,{name:"Separate",type:"Compound",description:(0,d.kt)(r.Fragment,null,"Separation options."),mdxType:"ParameterTree"},(0,d.kt)(n.r,{name:"Attributes",type:"Choice",description:(0,d.kt)(r.Fragment,null,"Defines what the attributes of the individual path entities should be: if they should take the attributes of the parent, use the ones of the edge or mix both."),mdxType:"ParameterTree"}),(0,d.kt)(n.r,{name:"Scope",type:"Choice",description:(0,d.kt)(r.Fragment,null,"Defines how the 3D Scope of the individual path entities should be: if they should inherit from the parent path or assume new ones, adjusted to the orientation of the edge."),mdxType:"ParameterTree"}))))},u={toc:c,Parameters:l};function h(e){let{components:t,...r}=e;return(0,d.kt)("wrapper",(0,a.Z)({},u,r,{components:t,mdxType:"MDXLayout"}),(0,d.kt)("p",null,"Divides paths into subpaths according to specific criteria."),(0,d.kt)("p",null,(0,d.kt)("img",{alt:"NodeImage_5d95c0d1-140a-4735-90f2-97e8a34d7a7a",src:A(7704).Z,width:"700",height:"300"})),(0,d.kt)("b",null,"Tags:")," Path",(0,d.kt)("h2",{id:"parameters"},"Parameters"),(0,d.kt)(l,{mdxType:"Parameters"}),(0,d.kt)("h2",{id:"inputs"},"Inputs"),(0,d.kt)("ul",null,(0,d.kt)("li",{parentName:"ul"},(0,d.kt)("b",null,"Input")," [Single | ",(0,d.kt)("i",null,"Path"),"]: The path to be divided.")),(0,d.kt)("br",null),(0,d.kt)("h2",{id:"outputs"},"Outputs"),(0,d.kt)("ul",null,(0,d.kt)("li",{parentName:"ul"},(0,d.kt)("b",null,"Output")," [",(0,d.kt)("i",null,"Path"),"]: The divided paths, according to the defined groups.")))}h.isMDXComponent=!0},7704:(e,t,A)=>{A.d(t,{Z:()=>a});const a="data:image/png;base64,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"}}]);