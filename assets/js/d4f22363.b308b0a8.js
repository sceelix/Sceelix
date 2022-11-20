"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[4206],{8797:(e,t,A)=>{A.r(t),A.d(t,{Parameters:()=>d,assets:()=>C,contentTitle:()=>s,default:()=>u,frontMatter:()=>i,metadata:()=>c,toc:()=>l});var r=A(7462),o=A(7294),n=A(3905),a=A(9715);const i={title:"Copy"},s=void 0,c={unversionedId:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8",id:"Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8",title:"Copy",description:"",source:"@site/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8.mdx",sourceDirName:"07-Nodes/Nodes/Basic",slug:"/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/07-Nodes/Nodes/Basic/Sceelix-Core-Procedures-CopyProcedure_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8.mdx",tags:[],version:"current",frontMatter:{title:"Copy"},sidebar:"tutorialSidebar",previous:{title:"Conditional",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-ConditionalProcedure_8c5f1758-7567-41ef-9089-2e033169567d"},next:{title:"Entity Create",permalink:"/docs/Nodes/Nodes/Basic/Sceelix-Core-Procedures-EntityCreateProcedure_2ef7c2e5-c59f-464b-8557-c1e4e38216a9"}},C={},l=[{value:"Parameters",id:"parameters",level:2},{value:"Inputs",id:"inputs",level:2},{value:"Parameter Inputs",id:"parameter-inputs",level:3},{value:"Outputs",id:"outputs",level:2},{value:"Parameter Outputs",id:"parameter-outputs",level:3}],d=e=>{let{children:t}=e;return(0,n.kt)(o.Fragment,null,(0,n.kt)(a.r,{name:"Operation",type:"Select",open:!0,description:(0,n.kt)(o.Fragment,null,"Type of copy operation to perform."),mdxType:"ParameterTree"},(0,n.kt)(a.r,{name:"Relation",type:"Compound",description:(0,n.kt)(o.Fragment,null,"Performs entity copy operations on entity collections so as to match the number of entities in each defined channel (input-output set)."),mdxType:"ParameterTree"},(0,n.kt)(a.r,{name:"Channels",type:"List",description:(0,n.kt)(o.Fragment,null,"Channels (sets of input/output) for performing the relation copy."),mdxType:"ParameterTree"},(0,n.kt)(a.r,{name:"Channel",type:"Compound",ports:[1,2],description:(0,n.kt)(o.Fragment,null,"Defines a stream of entities (a input-output pair) to be considered in the relation copy operation."),mdxType:"ParameterTree"})),(0,n.kt)(a.r,{name:"Method",type:"Choice",description:(0,n.kt)(o.Fragment,null,"Specifies the data copy method. 'Clone' refer to producing independent copies of each entity, while 'Reference' means that only the entity references will be copied."),mdxType:"ParameterTree"}),(0,n.kt)(a.r,{name:"Type",type:"Select",description:(0,n.kt)(o.Fragment,null,"Type of relation copy. See each option for details."),mdxType:"ParameterTree"},(0,n.kt)(a.r,{name:"Clamp",type:"Compound",description:(0,n.kt)(o.Fragment,null,"The last item of the smallest collection will be copied until the collection sizes match."),mdxType:"ParameterTree"}),(0,n.kt)(a.r,{name:"Cross",type:"Compound",description:(0,n.kt)(o.Fragment,null,"Cross product operation between the collections. In other words, each item will be copied for every item of the other collections. For instance, if one list has the items A, B, C and the other D,E then the result will be AD, AE, BD, BE, CD, CE."),mdxType:"ParameterTree"}),(0,n.kt)(a.r,{name:"Repeat",type:"Compound",description:(0,n.kt)(o.Fragment,null,"The sequence of items from the smallest collection will be copied until the collection sizes match."),mdxType:"ParameterTree"}))),(0,n.kt)(a.r,{name:"Standard",type:"Compound",ports:[3,4],description:(0,n.kt)(o.Fragment,null,"Standard operation for creating copies of a single stream of entities."),mdxType:"ParameterTree"},(0,n.kt)(a.r,{name:"Number of Copies",type:"Int",description:(0,n.kt)(o.Fragment,null,"The number of copies to output. If 0, the original will be discarded."),mdxType:"ParameterTree"}),(0,n.kt)(a.r,{name:"Method",type:"Choice",description:(0,n.kt)(o.Fragment,null,"Specifies the data copy method. ",(0,n.kt)("br",null),(0,n.kt)("b",null,"Clone")," means that independent copies of each entity will be produced. ",(0,n.kt)("br",null),(0,n.kt)("b",null,"Reference")," means that only the entity ",(0,n.kt)("i",null,"references")," will be copied."),mdxType:"ParameterTree"}),(0,n.kt)(a.r,{name:"Index",type:"Attribute",description:(0,n.kt)(o.Fragment,null,"Attribute that will contain the index of the copy (starting at 0)."),mdxType:"ParameterTree"}))))},p={toc:l,Parameters:d};function u(e){let{components:t,...o}=e;return(0,n.kt)("wrapper",(0,r.Z)({},p,o,{components:t,mdxType:"MDXLayout"}),(0,n.kt)("p",null,"Creates copies of the input entities."),(0,n.kt)("p",null,(0,n.kt)("img",{alt:"NodeImage_50c9a7c0-6f52-470d-8bb7-2c8b663c94b8",src:A(256).Z,width:"700",height:"300"})),(0,n.kt)("h2",{id:"parameters"},"Parameters"),(0,n.kt)(d,{mdxType:"Parameters"}),(0,n.kt)("h2",{id:"inputs"},"Inputs"),(0,n.kt)("i",null,"This node has no native inputs."),(0,n.kt)("h3",{id:"parameter-inputs"},"Parameter Inputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port1"},"[1]")," ",(0,n.kt)("b",null,"Input")," [Collective | ",(0,n.kt)("i",null,"Entity"),"]: The entities to be copied."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port3"},"[3]")," ",(0,n.kt)("b",null,"Input")," [Single | ",(0,n.kt)("i",null,"Entity"),"]: The entity to be copied.",(0,n.kt)("br",null))),(0,n.kt)("h2",{id:"outputs"},"Outputs"),(0,n.kt)("i",null,"This node has no native outputs."),(0,n.kt)("h3",{id:"parameter-outputs"},"Parameter Outputs"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port2"},"[2]")," ",(0,n.kt)("b",null,"Output")," [",(0,n.kt)("i",null,"Entity"),"]: The entities and their copies."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("a",{name:"port4"},"[4]")," ",(0,n.kt)("b",null,"Output")," [",(0,n.kt)("i",null,"Entity"),"]: The entity and its copies.")))}u.isMDXComponent=!0},256:(e,t,A)=>{A.d(t,{Z:()=>r});const r="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAArwAAAEsCAYAAAAhNGCdAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAABCWSURBVHhe7d1viJ3ZXcDxfbOQ3TcOrrABtY7C6oiiq1iNEmpe+CelVLIKsqwgkRWJQmEqq4wo5EVdgqAE23VTurijrhIVZFxaGtoFg6USKVtCQR2CxW2pJlUqU11sKgiP/U3vmT5z58xk5k4y59zffH7wITP3z/Pcm82LL2fPfe5DXxy+PAAAQFaCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV6AI/Ch6x8dVi7+1vAzT//scPrM24Z3nPupzd//bO0vq48H4P4RvAAP0IurLw2Pn3x8eOihh3b1dQsLw7tXfm34/N0vVo8BwOEIXoAH4Nadzw4/dvYnqoG7m29f+o7htRsfqx4PgNkJXoD7LFZ1Y9V2HLOLi4vD8vLycPXq1eH69evD2trasLKyMiwtLW17XIitDrXjAjAbwQtwH31i/VPDiRMntgfsV8L27t27w26zuro6LEwF8h9e/ZPq8QE4OMELcB+99dQPbkVrrN7euHFjkrV7z507d4YzZ85sPTf2/ca2iNo5ADgYwQtwn1y89J6tYI1V3vX19UnO7m9iFTi2PpRjxBUdaucB4GAEL8B98NmNz2/bynDp0qVJxh5sYn9vOUaIy5nVzgfA/glegPsgPqhWIvXJJ5+c5Otsc+HCha1j/fLyu6rnA2D/BC/QXERdfBnDPHvsGx7bitQrV65M0nW2Ga/yPvLoo9XzzRNbM4DWBC/QXERRCbwMbt68OUnX2ad23Hn1lsVvqf53BzgqghdoLlvwvvHGG5NsnX1OnjxZPfY8ErxAa4IXaE7w7pzpa/nOM8ELtCZ4geayBe+1a9cm2TrbxDV5a8edV4IXaE3wAs3Vgvfy5cubH96aF88888zWa4+rLBxm4kNv5VhxxYfa+XoVX51cXnsheIHWBC/QXC14I57maeIb1cprj+0Is25riC+fiMgtx4rwn6eJ911eeyF4gdYEL9BchuCNGYdqfE3wLLOysrJ1jIWFhc3tDfM0ghfokeAFmssSvHE5ssN829p4lTisrq5O7pmfEbxAjwQv0FyW4I25ePHitvdx9uzZfa3SRhyPYzmeN48jeIEeCV6guUzBO70HN8TWhPggWtw3PfE+T506tePx87aVoYzgBXokeIHmMgVvzMbGxnD+/Pkd7ylWcGNvb1H7comI3/X19cmR5m8EL9AjwQs0ly14y8T1ePf7jWkRwwfd89vjCF6gR4IXaC5r8MbEam9cWmx6m0OxuLg4LC8vb4ZihhG8QI8EL9Bc5uAdT+zhjfdVRAxnG8EL9EjwAs0dl+A9DiN4gR4JXqA5wZtnBC/QI8ELNCd484zgBXokeIHmBG+eEbxAjwQv0JzgzTOCF+iR4AWaE7x5RvACPRK8QHOCN88IXqBHghdoTvDmmVrwfuM3f9Nw6/ZnDu3Olzaq/34A7kXwAs0J3jxTC96vf+yx4b0vXTm0T976h+q/H4B7EbxAc4I3zwheoEeCF2hO8OYZwQv0SPACzQnePCN4gR4JXqA5wZtnBC/QI8ELNCd484zgBXokeIHmBG+eEbxAjwQv0JzgzTOCF+iR4AWaE7x5RvACPRK8QHO9Be/rr78+PPfcc8MTTzyx7TU9++yzm/eZ3UfwAj0SvEBzvQTvm2++OTz11FM7Xsu0iOF4rNk5ghfokeAFmusheCNgx+d/5ZVXhtu3b0/uHYZbt24NL7zwwtb9sdprdo7gBXokeIHmegjesrJ7+vTpPVdvX3311WavcR5G8AI9ErxAc62DN/bllvPuZ6tCbGmIx9ZWeWNVeHr/bzyu9n7KinHEdpy3HDfE8+P+6YmV5nL/blOOE6vURz2CF+iR4AWaax28JRDjz/1MxGmE5/TEa55+H2PTxy/BG6vKYfrx5b7pKTFdew0x5bnjLRlHNYIX6JHgBZprHbwlNg9zzojL8trjeCVGI47He3/Hq67j20P8XlaY43Hj28dT7nv++ecnt3xtSnTXQvkoRvACPRK8QHOtg7ec8zCXHIv4jGPE6mttW0SJ2/FWhHHwTkdtzDh6x1PiuratoeV2hhjBC/RI8ALNZQjeskocH2qrTURwOU9Z/R0H725T7p9+beV809sayuNbbGeIEbxAjwQv0FyG4N3PMcqVIMpjSvDG7btNec70im1tW0Pr7QwxghfokeAFmmsdvCUqD3PO8rofVPBOb3koK8bjbQ2ttzPECF6gR4IXaK518Jb9t/u9SkNMPD5itGwdKK/7QQVvbatEXO4s7ivbGuLn0Go7Q4zgBXokeIHmWgdvBGM5735isfZhsge9h3d6r25M+RKMCPaynaH1N8AJXqBHghdornXwxpSV1AjX2lUWyozjeLzNYL9XaQhlxrfVQrmEde1qDDElouM1l/PvFtxHNYIX6JHgBZrrIXjLpb6KiNHxqmr8XKIyTH8wbPz8uK88N6J0HLbj/bXj20OJ1XjOeBV5r4gt+3YjiuPPvWL9KEbwAj0SvEBzPQRvTERq2Zqwl1gNroVl2Vawm92+aa3Eas299hWPz9l6O0OM4AV6JHiB5noJ3jKxolo+EDYWt+31obSYWOktq67j59XeTwneCOh43viccdt+/w7Kc1pvZ4gRvECPBC/QXG/Be1QzDt5Zp+zjDa23M8QIXqBHghdoTvDOHryxqhvHOMgl1R7kCF6gR4IXaE7wHix4y0pubK8of1/jD9i1HMEL9EjwAs0J3oMF7/Qe4V5Wd2MEL9AjwQs0J3gPFrxlG0OIS6X1sHe3jOAFeiR4geaOa/BmHMEL9EjwAs0J3jwjeIEeCV6gOcGbZwQv0CPBCzQnePOM4AV6JHiB5gRvnhG8QI8EL9Cc4M0zghfokeAFmhO8eUbwAj0SvEBzgjfPCF6gR4IXaE7w5hnBC/RI8ALNCd48I3iBHgleoDnBm2cEL9AjwQs0J3jzjOAFeiR4geYEb54RvECPBC/QnODNM4IX6JHgBZoTvHlG8AI9ErxAc4I3zwheoEeCF2hO8OYZwQv0SPACzWUP3jt37gxXrlwZzp49O5w5c2bz/Z06dWrz54sXLw7r6+uTR87/CF6gR4IXaC5r8N69e3dYWVnZ8d5qIoYjjOd9BC/QI8ELNJcxeK9duzYsLi7ueF97WVhYGFZXVydHmM8RvECPBC/QXLbgra3qxgpuxGy8r1j5vXHjxrC2tjacP39+x2PPnTs3OdL8jeAFeiR4geYyBW+87vH7OHny5OZq714T8bu0tLTteZcuXZrcO18jeIEeCV6guSzBGyu3420Msaq7sbExuXfviedeuHBh67knTpyYyw+zCV6gR4IXaC5L8I6DNVZ2Z/kQWly9oRwjfp63EbxAjwQv0FyG4I24Hb/+q1evTu452MSqbqzuluPEPt95GsEL9EjwAs1lCN64zm557bGV4TAT1+Ytx4oPtc3TCF6gR4IXaK4WvPPssJcWu3nzZvW480rwAq0JXqC5bMEbwXrYqR13XgleoDXBCzSXLXjjf+sfdg76pRU9E7xAa4IXaC5b8N6Py4nVjjuvBC/QmuAFmssWvIe9skLtg1/zTPACrQleIK1btz9TDacH4e3vfMdW4B32ygrxLWvlWN/zfd9bPd9xJHiBWQleIK2jDN7ffM/XLiUW4uuCZ5n4Zrb40opynJ/7hZ+vnu84ErzArAQvkNZRBm/4zu/+rq1QXVpa2vy64IPO008/vXWM2Arwey++t3qu40jwArMSvEBaRx28v/27vzM88ugjW8EaWxsOEr2XL1/eem5413Pvrp7nuBK8wKwEL5DWUQdviC0I42iNld57bW+IbQznzp3b9rzTP/q26vGPM8ELzErwAmm1CN4QHzQbx2tYXl7ecbmyuBpDrOqO9+yGx7/yu60MOwleYFaCF0irVfCGd/70ueHhhx/eFrL78eNv/0mxuwvBC8xK8AJptQzeEFduWPy2b62G7bRY1f3V3/j16nH29uJX//zA9O35CF5gVoIXSKt18Ba/+CsXhh/6kR/e9oG2ECvA3//WHxjO/9KzB1rVfV/ltrH3vfT+6u3zTvACsxK8QFq9BG87k9XfJAQvMCvBC6QleHMRvMCsBC+QluCdSLK/V/ACsxK8QFqCNxfBC8xK8AJpCd5cBC8wK8ELpCV4cxG8wKwEL5CW4M1F8AKzErxAWoJ3u9+v3DZPBC8wK8ELpPWv//2F4W8/+fdza+2jH66G36z+4kN/XT3PvPiXL/xb9b8zwL0IXoBOxYpmLVxnFdFYOw9AdoIXoFP//O+fq4brrG5++p+q5wHITvACdOo//+9/hvf/8cvVeJ3FnS9tVM8DkJ3gBejYazc+Vo3Xg/qrax+sHh/gOBC8AB2LVd6X//yVasTu1x+8/IHND/DVjg9wHAhegM4d9vJqLucFHHeCF2AOXP/E31Vj9l4++DcfqR4P4DgRvABz4h8/9+nhpT/9o2rYTosPu1nZBfgqwQswR/7jf/9r+MjHr1cjt1h77cOuyAAwIngB5lCEb6zgjr+JLH4XugA7CV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDUBC8AAKkJXgAAUhO8AACkJngBAEhN8AIAkJrgBQAgNcELAEBqghcAgNQELwAAqQleAABSE7wAAKQmeAEASE3wAgCQmuAFACA1wQsAQGqCFwCA1AQvAACpCV4AAFITvAAApCZ4AQBITfACAJCa4AUAIDXBCwBAaoIXAIDEvjz8P04XcPMgmu77AAAAAElFTkSuQmCC"}}]);