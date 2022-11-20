"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[4941],{9615:(e,t,A)=>{A.r(t),A.d(t,{Parameters:()=>p,assets:()=>c,contentTitle:()=>s,default:()=>m,frontMatter:()=>l,metadata:()=>a,toc:()=>d});var i=A(7462),n=A(7294),r=A(3905),o=A(9715);const l={title:"Unity Component Modify"},s=void 0,a={unversionedId:"Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82",id:"Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82",title:"Unity Component Modify",description:"",source:"@site/docs/07-Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82.mdx",sourceDirName:"07-Nodes/Nodes/Unity",slug:"/Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82",permalink:"/docs/Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityComponentModifyProcedure_b5874617-2bc3-4da8-b9f5-d051ac46ee82.mdx",tags:[],version:"current",frontMatter:{title:"Unity Component Modify"},sidebar:"tutorialSidebar",previous:{title:"Surface Subselect",permalink:"/docs/Nodes/Nodes/Surface/Sceelix-Surfaces-Procedures-SurfaceSubselectProcedure_e21e7aed-61ae-435f-a11b-0028251ea898"},next:{title:"Unity Entity Component",permalink:"/docs/Nodes/Nodes/Unity/Sceelix-Unity-Procedures-UnityEntityComponentProcedure_f4ddf1f0-64b2-4846-84fd-b919b480c492"}},c={},d=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Parameter Inputs",id:"parameter-inputs",level:3},{value:"Outputs",id:"outputs",level:2}],p=e=>{let{children:t}=e;return(0,r.kt)(n.Fragment,null,(0,r.kt)(o.r,{name:"Component",type:"Select",open:!0,description:(0,r.kt)(n.Fragment,null,"Component to be modified."),mdxType:"ParameterTree"},(0,r.kt)(o.r,{name:"Add Surface Tree",type:"Compound",description:(0,r.kt)(n.Fragment,null,"Adds a tree instance (or of any other prefab) to the Unity Surface component."),mdxType:"ParameterTree"},(0,r.kt)(o.r,{name:"Actor",type:"Select",description:(0,r.kt)(n.Fragment,null,"Actors whose position will represent tree positions on the terrain component"),mdxType:"ParameterTree"},(0,r.kt)(o.r,{name:"Single",type:"Compound",ports:[1],description:(0,r.kt)(n.Fragment,null,"Processes one Actor entity at a time."),mdxType:"ParameterTree"}),(0,r.kt)(o.r,{name:"Collective",type:"Compound",ports:[2],description:(0,r.kt)(n.Fragment,null,"Processes all Actor entities at a time."),mdxType:"ParameterTree"})),(0,r.kt)(o.r,{name:"Prefab",type:"String",description:(0,r.kt)(n.Fragment,null,"Tree prefab to be used."),mdxType:"ParameterTree"}),(0,r.kt)(o.r,{name:"Bend Factor",type:"Float",description:(0,r.kt)(n.Fragment,null,"Bend factor of the tree prefab, as understood by Unity."),mdxType:"ParameterTree"}),(0,r.kt)(o.r,{name:"Rotation",type:"Float",description:(0,r.kt)(n.Fragment,null,"Rotation of the tree prefab."),mdxType:"ParameterTree"}),(0,r.kt)(o.r,{name:"Scale",type:"Vector2D",description:(0,r.kt)(n.Fragment,null,"Scale of the tree prefab."),mdxType:"ParameterTree"}))))},u={toc:d,Parameters:p};function m(e){let{components:t,...n}=e;return(0,r.kt)("wrapper",(0,i.Z)({},u,n,{components:t,mdxType:"MDXLayout"}),(0,r.kt)("p",null,"Modifies pre-existing Unity Components on Unity Entities."),(0,r.kt)("p",null,(0,r.kt)("img",{alt:"NodeImage_b5874617-2bc3-4da8-b9f5-d051ac46ee82",src:A(2533).Z,width:"700",height:"300"})),(0,r.kt)("b",null,"Tags:")," Unity Entity",(0,r.kt)("h2",{id:"parameters"},"Parameters"),(0,r.kt)(p,{mdxType:"Parameters"}),(0,r.kt)("h2",{id:"inputs"},"Inputs"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("b",null,"Input")," [Collective | ",(0,r.kt)("i",null,"Unity Entity"),"]: Unity Entity whose component is to be modified.")),(0,r.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{name:"port1"},"[1]")," ",(0,r.kt)("b",null,"Single")," [Single | ",(0,r.kt)("i",null,"Actor"),"]: Processes one Actor entity at a time."),(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("a",{name:"port2"},"[2]")," ",(0,r.kt)("b",null,"Collective")," [Collective | ",(0,r.kt)("i",null,"Actor"),"]: Processes all Actor entities at a time.",(0,r.kt)("br",null))),(0,r.kt)("h2",{id:"outputs"},"Outputs"),(0,r.kt)("ul",null,(0,r.kt)("li",{parentName:"ul"},(0,r.kt)("b",null,"Output")," [",(0,r.kt)("i",null,"Unity Entity"),"]: Unity Entity whose component was modified.")))}m.isMDXComponent=!0},2533:(e,t,A)=>{A.d(t,{Z:()=>i});const i="data:image/png;base64,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zZFdgK9ReAGOkH/73/9cfORvb3RLbrr2sQ/7RgaAhsILcARF8R3+xr/4WdEFeCmFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBAChN4QUAoDSFFwCA0hReAABKU3gBACjsK4v/BxgG9yRsxHEBAAAAAElFTkSuQmCC"}}]);