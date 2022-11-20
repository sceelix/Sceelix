"use strict";(self.webpackChunksceelix=self.webpackChunksceelix||[]).push([[8953],{7780:(e,t,o)=>{o.r(t),o.d(t,{assets:()=>s,contentTitle:()=>l,default:()=>d,frontMatter:()=>i,metadata:()=>a,toc:()=>p});var r=o(7462),n=(o(7294),o(3905));const i={},l="Project Explorer",a={unversionedId:"Working With Sceelix/ProjectExplorer",id:"Working With Sceelix/ProjectExplorer",title:"Project Explorer",description:"Like many 3D environments, Sceelix\u2019s workflow revolves around the manipulation of resources organized into projects. Such organization allows for easy and consistent manipulation of files, which may relate and reference one another. Examples of supported resources are images, 3D models or graph files.",source:"@site/docs/03-Working With Sceelix/02-ProjectExplorer.md",sourceDirName:"03-Working With Sceelix",slug:"/Working With Sceelix/ProjectExplorer",permalink:"/docs/Working With Sceelix/ProjectExplorer",draft:!1,editUrl:"https://github.com/Sceelix/Sceelix/edit/master/website/docs/03-Working With Sceelix/02-ProjectExplorer.md",tags:[],version:"current",sidebarPosition:2,frontMatter:{},sidebar:"tutorialSidebar",previous:{title:"Interface, Layouts and Settings",permalink:"/docs/Working With Sceelix/Interface"},next:{title:"Inspector",permalink:"/docs/Working With Sceelix/Inspector"}},s={},p=[{value:"Creating Projects",id:"creating-projects",level:2},{value:"Opening Projects",id:"opening-projects",level:2},{value:"Managing Resources",id:"managing-resources",level:2}],c={toc:p};function d(e){let{components:t,...i}=e;return(0,n.kt)("wrapper",(0,r.Z)({},c,i,{components:t,mdxType:"MDXLayout"}),(0,n.kt)("h1",{id:"project-explorer"},"Project Explorer"),(0,n.kt)("p",null,"Like many 3D environments, Sceelix\u2019s workflow revolves around the manipulation of resources organized into ",(0,n.kt)("strong",{parentName:"p"},"projects"),". Such organization allows for easy and consistent manipulation of files, which may relate and reference one another. Examples of supported resources are images, 3D models or graph files."),(0,n.kt)("p",null,"Projects are structured into files and folders in a recursive way, like any file system does. These are displayed together in the same tree-like fashion, simply featuring different icons for easy visual differentiation."),(0,n.kt)("p",null,(0,n.kt)("img",{src:o(5573).Z,width:"368",height:"391"})),(0,n.kt)("p",null,"It is important to note that the resources listed in the project may not reflect the full contents of the underlying file system contents. In other words, the files and folders of the project DO exist in the file system, but there might be other files in the same location that are NOT part of the project. Moving or copying a file through the operating system to the project folder will NOT add that file to the project. It has to be done inside Sceelix, as shown below."),(0,n.kt)("h2",{id:"creating-projects"},"Creating Projects"),(0,n.kt)("p",null,"In order to create a new project, you should use the window top menu (not the Sceelix main menu) and click \u201cNew\u2026\u201d. This will open the \u201cNew Sceelix Project\u201d window."),(0,n.kt)("p",null,(0,n.kt)("img",{src:o(3604).Z,width:"560",height:"300"})),(0,n.kt)("p",null,"As indicated, having indicated a project name and location, a new folder with the project name will be created within the location. In the example above, this would create a folder named"),(0,n.kt)("pre",null,(0,n.kt)("code",{parentName:"pre"},"C:\\Users\\pedro\\Desktop\\Sceelix 0.8.5.0\\Projects\\MyProject\n")),(0,n.kt)("p",null,"and inside, a file called MyProject.slxp. This file contains an index of the items that belong to the project. All Sceelix project files have the .slxp extension."),(0,n.kt)("h2",{id:"opening-projects"},"Opening Projects"),(0,n.kt)("p",null,"Opening an existing project is also achieved via the Project Explorer top menu, by clicking the \u201cOpen..\u201d menu that features the following options:"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Existing Project:")," This opens a dialog window which will allow you to search your file system for .slxp files and open them."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Recent..:")," Shows the list of the last loaded projects, allowing you to toggle between different projects rapidly."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Tutorial Samples:")," Extracts and opens the ",(0,n.kt)("a",{parentName:"li",href:"../Setting%20Up/ExploringSamples"},"sample project")," for the current Sceelix version.")),(0,n.kt)("h2",{id:"managing-resources"},"Managing Resources"),(0,n.kt)("p",null,"Once a project has been created and/or opened, you can create new files or copy existing ones. You can do so by right-clicking a folder, which will open its context menu, as shown in the image below:"),(0,n.kt)("p",null,(0,n.kt)("img",{src:o(4709).Z,width:"415",height:"280"})),(0,n.kt)("p",null,"The available options are as follows:"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Add:")," Allows for files or folders to be added or created inside this folder.",(0,n.kt)("ul",{parentName:"li"},(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"New Item:")," Allows the creation of a new file from one of the available formats supported by Sceelix."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Existing Item:")," Allows one more more files to be selected from the file system and brought into the project. The files will be copied and listed as project resources."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"New Folder:")," Allows the creation of a new folder."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Existing Folder and Contents:")," Allows a full folder tree (including all subfiles and subfolders) to be imported at once."))),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Open in Explorer:")," Shows the contents of the selected folder in the file explorer of the Operating System."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Copy Path:")," Copies the full path of the folder to the clipboard."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Rename:")," Renames the folder."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Exclude:")," Removes the folder and its subitems from the project, but leaves them in the disk."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Delete:")," Deletes the selected folder from the project and from the disk.")),(0,n.kt)("p",null,"On the other hand, right-clicking a file brings up a context menu with the following options:"),(0,n.kt)("ul",null,(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Open:")," Opens the file editor or viewer in the ",(0,n.kt)("a",{parentName:"li",href:"DocumentArea"},"Document Area"),", if applicable."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Open Folder in Explorer:")," Shows the contents of this file\u2019s parent folder in the file explorer of the Operating System."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Copy Path:")," Copies the full path of the file to the clipboard."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Rename:")," Renames the file."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Duplicate:")," Duplicates the file and its contents and adds it inside the same folder."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Exclude:")," Removes the file from the project, but leaves it in the disk."),(0,n.kt)("li",{parentName:"ul"},(0,n.kt)("strong",{parentName:"li"},"Delete:")," Deletes the selected file from the project and from the disk.")))}d.isMDXComponent=!0},3604:(e,t,o)=>{o.d(t,{Z:()=>r});const r=o.p+"assets/images/NewProjectWindow-1520330d63aa14e32ddedcacd831e13f.png"},4709:(e,t,o)=>{o.d(t,{Z:()=>r});const r=o.p+"assets/images/ProjectExplorerItemContext-668d5978ce7687c2643044a27a016a32.png"},5573:(e,t,o)=>{o.d(t,{Z:()=>r});const r=o.p+"assets/images/ProjectExplorerWindow-698e6db51e5852aa6b0ed018839725b9.png"}}]);