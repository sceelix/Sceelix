"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[7543],{9965:(e,t,r)=>{r.r(t),r.d(t,{Parameters:()=>p,assets:()=>u,contentTitle:()=>o,default:()=>c,frontMatter:()=>A,metadata:()=>s,toc:()=>m});var n=r(7462),a=r(7294),i=r(3905),l=r(9715);const A={title:"Attribute"},o=void 0,s={unversionedId:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a",id:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a",title:"Attribute",description:"",source:"@site/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a.mdx",sourceDirName:"07-Nodes/Nodes/Basic",slug:"/Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a.mdx",tags:[],version:"current",frontMatter:{title:"Attribute"},sidebar:"tutorialSidebar",previous:{title:"Actor Translate",permalink:"/docs/Nodes/Nodes/Actor/Sceelix-Actors-Procedures-ActorTranslateProcedure_930eb356-619d-4db7-b68a-016d9a6d1e97"},next:{title:"Collection",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b"}},u={},m=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Parameter Inputs",id:"parameter-inputs",level:3},{value:"Outputs",id:"outputs",level:2},{value:"Parameter Outputs",id:"parameter-outputs",level:3}],p=e=>{let{children:t}=e;return(0,i.kt)(a.Fragment,null,(0,i.kt)(l.r,{name:"Operations",type:"Select",open:!0,description:(0,i.kt)(a.Fragment,null,"Type of attribute operation to perform."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Copy All",type:"Compound",description:(0,i.kt)(a.Fragment,null,"Copies all attributes between a source entity/entities and a target entity/entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Origin",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities with the attributes to be copied."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Entity Collection",type:"Compound",ports:[1,2],description:(0,i.kt)(a.Fragment,null,"Entity collection from which the attributes will be copied."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Destination",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities where the attributes are to be copied to."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Other",type:"Compound",ports:[3,4],description:(0,i.kt)(a.Fragment,null,"The attributes will be copied to another entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Mode",type:"Choice",description:(0,i.kt)(a.Fragment,null,"Defines how the attributes should be copied. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Set")," means that the whole attribute list on the target will be erased and made equal to the source. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Replace")," means that items that exist on the target already will have their values replaced, while the others will keep existing. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Complement")," means that only the attributes that don't exist on the target will be copied.",(0,i.kt)("br",null)),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Others",type:"Compound",ports:[5,6],description:(0,i.kt)(a.Fragment,null,"The attributes will be copied other entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Mode",type:"Choice",description:(0,i.kt)(a.Fragment,null,"Defines how the attributes should be copied. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Set")," means that the whole attribute list on the target will be erased and made equal to the source. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Replace")," means that items that exist on the target already will have their values replaced, while the others will keep existing. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Complement")," means that only the attributes that don't exist on the target will be copied."),mdxType:"ParameterTree"})))),(0,i.kt)(l.r,{name:"Entity",type:"Compound",ports:[7,8],description:(0,i.kt)(a.Fragment,null,"Single entity from which the attributes will be copied."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Destination",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities where the attributes are to be copied to."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Other",type:"Compound",ports:[3,4],description:(0,i.kt)(a.Fragment,null,"The attributes will be copied to another entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Mode",type:"Choice",description:(0,i.kt)(a.Fragment,null,"Defines how the attributes should be copied. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Set")," means that the whole attribute list on the target will be erased and made equal to the source. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Replace")," means that items that exist on the target already will have their values replaced, while the others will keep existing. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Complement")," means that only the attributes that don't exist on the target will be copied.",(0,i.kt)("br",null)),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Others",type:"Compound",ports:[5,6],description:(0,i.kt)(a.Fragment,null,"The attributes will be copied other entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Mode",type:"Choice",description:(0,i.kt)(a.Fragment,null,"Defines how the attributes should be copied. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Set")," means that the whole attribute list on the target will be erased and made equal to the source. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Replace")," means that items that exist on the target already will have their values replaced, while the others will keep existing. ",(0,i.kt)("br",null),(0,i.kt)("b",null,"Complement")," means that only the attributes that don't exist on the target will be copied."),mdxType:"ParameterTree"})))))),(0,i.kt)(l.r,{name:"Delete All",type:"Compound",ports:[9,10],description:(0,i.kt)(a.Fragment,null,"Deletes all attributes from an entity."),mdxType:"ParameterTree"}),(0,i.kt)(l.r,{name:"Delete",type:"Compound",ports:[11,12],description:(0,i.kt)(a.Fragment,null,"Deletes a certain attribute from an entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"Attribute to delete."),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Set",type:"Compound",description:(0,i.kt)(a.Fragment,null,'Creates/changes attribute values on entities. Source values and destination attributes can be determined and many different ways. For instance, choosing Origin="Entity" and Destination="Self", an entity can set its own attributes from fixed values or expressions. Yet by choosing Origin="Entity Collection" and Destination="Others", it is possible to grab a list of values, aggregate them and store them in attributes of other entities.'),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Origin",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities that define the source values to be set."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Entity Collection",type:"Compound",ports:[13,14],description:(0,i.kt)(a.Fragment,null,"Entity collection from which the value will be copied."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Value",type:"Object",description:(0,i.kt)(a.Fragment,null,"A parameter that accepts any type of data. Is set as an expression by default."),mdxType:"ParameterTree"}),(0,i.kt)(l.r,{name:"Destination",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities where the attributes are to be set."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Selves",type:"Compound",description:(0,i.kt)(a.Fragment,null,"The attribute will be set on the source entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Other",type:"Compound",ports:[15,16],description:(0,i.kt)(a.Fragment,null,"The attribute will be set on a different entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Others",type:"Compound",ports:[17,18],description:(0,i.kt)(a.Fragment,null,"The attribute will be set on other entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"})))),(0,i.kt)(l.r,{name:"Entity",type:"Compound",ports:[19,20],description:(0,i.kt)(a.Fragment,null,"Entity from which the value will be copied."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Value",type:"Object",description:(0,i.kt)(a.Fragment,null,"A parameter that accepts any type of data. Is set as an expression by default."),mdxType:"ParameterTree"}),(0,i.kt)(l.r,{name:"Destination",type:"Select",description:(0,i.kt)(a.Fragment,null,"The entity/entities where the attributes are to be set."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Self",type:"Compound",description:(0,i.kt)(a.Fragment,null,"The attribute will be set on the source entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Other",type:"Compound",ports:[15,16],description:(0,i.kt)(a.Fragment,null,"The attribute will be set on a different entity."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"})),(0,i.kt)(l.r,{name:"Others",type:"Compound",ports:[17,18],description:(0,i.kt)(a.Fragment,null,"The attribute will be set on other entities."),mdxType:"ParameterTree"},(0,i.kt)(l.r,{name:"Attribute",type:"Attribute",description:(0,i.kt)(a.Fragment,null,"The attribute to be set."),mdxType:"ParameterTree"}))))))))},d={toc:m,Parameters:p};function c(e){let{components:t,...a}=e;return(0,i.kt)("wrapper",(0,n.Z)({},d,a,{components:t,mdxType:"MDXLayout"}),(0,i.kt)("p",null,'Performs attribute manipulation on entities, between entities and within subentities.\nThis procedure includes different operations that handle attributes. Some, such as the "Set" include subparameters\nwith further customization and inputs/outputs.'),(0,i.kt)("p",null,(0,i.kt)("img",{alt:"NodeImage_b3ee6334-f7cb-435c-ab3b-3802fc835e0a",src:r(4982).Z,width:"700",height:"300"})),(0,i.kt)("b",null,"Tags:")," Copy All, Delete All, Delete, Set",(0,i.kt)("h2",{id:"parameters"},"Parameters"),(0,i.kt)(p,{mdxType:"Parameters"}),(0,i.kt)("h2",{id:"inputs"},"Inputs"),(0,i.kt)("i",null,"This node has no native inputs."),(0,i.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,i.kt)("ul",null,(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port1"},"[1]")," ",(0,i.kt)("b",null,"Input")," [Collective | ",(0,i.kt)("i",null,"Entity"),"]: Entities from which the attributes will be copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port3"},"[3]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: Entity to which the attributes will be copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port5"},"[5]")," ",(0,i.kt)("b",null,"Input")," [Collective | ",(0,i.kt)("i",null,"Entity"),"]: Entities to which the attributes will be copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port7"},"[7]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: Entity from which the attributes will be copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port9"},"[9]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: The entity whose attributes are to be deleted."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port11"},"[11]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: The entity whose attribute is to be deleted."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port13"},"[13]")," ",(0,i.kt)("b",null,"Input")," [Collective | ",(0,i.kt)("i",null,"Entity"),"]: Entities that hold the attribute values to be read."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port15"},"[15]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: The entity whose attribute is to be set."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port17"},"[17]")," ",(0,i.kt)("b",null,"Input")," [Collective | ",(0,i.kt)("i",null,"Entity"),"]: The entities whose attribute is to be set."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port19"},"[19]")," ",(0,i.kt)("b",null,"Input")," [Single | ",(0,i.kt)("i",null,"Entity"),"]: Entity that holds attribute values to be read.",(0,i.kt)("br",null))),(0,i.kt)("h2",{id:"outputs"},"Outputs"),(0,i.kt)("i",null,"This node has no native outputs."),(0,i.kt)("h3",{id:"parameter-outputs"},"Parameter Outputs"),(0,i.kt)("ul",null,(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port2"},"[2]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entities from which the attributes were copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port4"},"[4]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entity to which the attributes were copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port6"},"[6]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entities to which the attributes were copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port8"},"[8]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entity from which the attributes were copied."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port10"},"[10]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: The entity whose attributes were deleted."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port12"},"[12]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: The entity whose attribute was deleted."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port14"},"[14]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entities that hold the attribute values that were read from or written to."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port16"},"[16]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: The entity whose attribute was set."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port18"},"[18]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: The entities whose attribute was set."),(0,i.kt)("li",{parentName:"ul"},(0,i.kt)("a",{name:"port20"},"[20]")," ",(0,i.kt)("b",null,"Output")," [",(0,i.kt)("i",null,"Entity"),"]: Entity that holds the attribute values that were read from or written to.")))}c.isMDXComponent=!0},4982:(e,t,r)=>{r.d(t,{Z:()=>n});const n="data:image/png;base64,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"}}]);