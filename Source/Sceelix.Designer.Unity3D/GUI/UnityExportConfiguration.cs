using System;

namespace Sceelix.Designer.Unity3D.GUI
{
    public class UnityExportConfiguration
    {
        public UnityExportConfiguration()
        {
            UnityAssetsPath = "Path/to/Unity/Assets/Folder";
            PackageSubPath = "Resources/SceelixPackage.bytes";
        }



        //public String PackageName { get; set; }
        public String UnityAssetsPath
        {
            get;
            set;
        }



        public string PackageSubPath
        {
            get;
            set;
        }



        public override string ToString()
        {
            return string.Format("UnityAssetsPath: {0}, PackageSubPath: {1}", UnityAssetsPath, PackageSubPath);
        }
    }
}