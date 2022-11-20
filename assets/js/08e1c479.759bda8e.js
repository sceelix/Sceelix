"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[4513],{9496:(e,t,A)=>{A.r(t),A.d(t,{Parameters:()=>d,assets:()=>c,contentTitle:()=>l,default:()=>m,frontMatter:()=>i,metadata:()=>s,toc:()=>u});var r=A(7462),n=A(7294),a=A(3905),o=A(9715);const i={title:"Collection"},l=void 0,s={unversionedId:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b",id:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b",title:"Collection",description:"",source:"@site/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b.mdx",sourceDirName:"07-Nodes/Nodes/Basic",slug:"/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-CollectionProcedure_3f477acc-ff16-4ceb-9f54-0b1404dc149b.mdx",tags:[],version:"current",frontMatter:{title:"Collection"},sidebar:"tutorialSidebar",previous:{title:"Attribute",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-AttributeProcedure_b3ee6334-f7cb-435c-ab3b-3802fc835e0a"},next:{title:"Combinatorial",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CombinatorialProcedure_19fbd59a-ed19-483c-bd0d-e0c7773de563"}},c={},u=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Outputs",id:"outputs",level:2},{value:"Parameter Outputs",id:"parameter-outputs",level:3}],d=e=>{let{children:t}=e;return(0,a.kt)(n.Fragment,null,(0,a.kt)(o.r,{name:"Operation",type:"Select",open:!0,description:(0,a.kt)(n.Fragment,null,"Type of collection operation to perform."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Aggregation",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Aggregates attribute values and stores the result into other attributes."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Value",type:"Object",description:(0,a.kt)(n.Fragment,null,"A parameter that accepts any type of data. Is set as an expression by default."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Operation",type:"Choice",description:(0,a.kt)(n.Fragment,null,"Type of aggregative operation to perform."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Aggregated Value",type:"Attribute",description:(0,a.kt)(n.Fragment,null,"Attribute where to store the aggregated value."),mdxType:"ParameterTree"})),(0,a.kt)(o.r,{name:"Count",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Performs counting or indexing on the input collection."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Index",type:"Attribute",description:(0,a.kt)(n.Fragment,null,"Writes a sequence number corresponding to the number of each entity in the sequence."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Count",type:"Attribute",description:(0,a.kt)(n.Fragment,null,"Writes the total number of entities on the collection."),mdxType:"ParameterTree"})),(0,a.kt)(o.r,{name:"Distinct",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Filters repeated entities in the collection. Especially useful to remove repeated references to the same entity."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Order By",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Orders elements of the input collection according to values of their attributes."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Criteria",type:"List",description:(0,a.kt)(n.Fragment,null,"List of criteria on which the ordering should be based. Several criteria can be defined."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Criterium",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Attribute-based criteria by which to order."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Value",type:"Object",description:(0,a.kt)(n.Fragment,null,"A parameter that accepts any type of data. Is set as an expression by default."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Descending",type:"Boolean",description:(0,a.kt)(n.Fragment,null,"Indicates if the ordering on this criteria should be from the greatest to the smallest."),mdxType:"ParameterTree"})))),(0,a.kt)(o.r,{name:"Reverse",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Reverses the order of the input collection."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Shuffle",type:"Compound",description:(0,a.kt)(n.Fragment,null,"Randomly orders the elements in the input collection."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Seed",type:"Int",description:(0,a.kt)(n.Fragment,null,"Seed of the random shuffler. Controls the randomness look the shuffle."),mdxType:"ParameterTree"})),(0,a.kt)(o.r,{name:"Take",type:"Compound",ports:[1],description:(0,a.kt)(n.Fragment,null,"Takes a specified number of contiguous elements from a certain position of the input collection."),mdxType:"ParameterTree"},(0,a.kt)(o.r,{name:"Starting Index",type:"Int",description:(0,a.kt)(n.Fragment,null,"Index where to start the extraction."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Amount",type:"Int",description:(0,a.kt)(n.Fragment,null,"Number of items (starting at 'Starting Index') to extract to the first port."),mdxType:"ParameterTree"}),(0,a.kt)(o.r,{name:"Loop",type:"Boolean",description:(0,a.kt)(n.Fragment,null,"If checked, a repetitive, alternating pattern will be applied. Example: 10 elements (A,B,C,D,E,F,G,H,I,J), Starting and Amount = 2 will return A B,E,F,I,J to the first output and C,D,G,H to the second one."),mdxType:"ParameterTree"}))))},C={toc:u,Parameters:d};function m(e){let{components:t,...n}=e;return(0,a.kt)("wrapper",(0,r.Z)({},C,n,{components:t,mdxType:"MDXLayout"}),(0,a.kt)("p",null,"Performs data analysis, filtering and organization\non collections of entities."),(0,a.kt)("p",null,(0,a.kt)("img",{alt:"NodeImage_3f477acc-ff16-4ceb-9f54-0b1404dc149b",src:A(2770).Z,width:"700",height:"300"})),(0,a.kt)("b",null,"Tags:")," Entity, Aggregation, Count, Distinct, Order By, Reverse, Shuffle, Take",(0,a.kt)("h2",{id:"parameters"},"Parameters"),(0,a.kt)(d,{mdxType:"Parameters"}),(0,a.kt)("h2",{id:"inputs"},"Inputs"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("b",null,"Input")," [Collective | ",(0,a.kt)("i",null,"Entity"),"]: The entities that will be operated on.")),(0,a.kt)("br",null),(0,a.kt)("h2",{id:"outputs"},"Outputs"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("b",null,"Output")," [",(0,a.kt)("i",null,"Entity"),"]: The entities that were operated on.")),(0,a.kt)("h3",{id:"parameter-outputs"},"Parameter Outputs"),(0,a.kt)("ul",null,(0,a.kt)("li",{parentName:"ul"},(0,a.kt)("a",{name:"port1"},"[1]")," ",(0,a.kt)("b",null,"Else")," [",(0,a.kt)("i",null,"Entity"),"]: Entities that are not delivered through the other port.")))}m.isMDXComponent=!0},2770:(e,t,A)=>{A.d(t,{Z:()=>r});const r="data:image/png;base64,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"}}]);