namespace Sceelix.Designer.Utils
{
    public enum Platform
    {
        Windows,
        Linux,
        MacOS
    }

    public class BuildPlatform
    {
        public static Platform Enum
        {
            get
            {
#if LINUX
                return Platform.Linux;
#elif MACOS
                return Platform.MacOS;
#else
                return Platform.Windows;
#endif
            }
        }

        public static bool IsLinux
        {
            get
            {
#if LINUX
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsMacOS
        {
            get
            {
#if MACOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsWindows
        {
            get
            {
#if WINDOWS
                return true;
#else
                return false;
#endif
            }
        }
    }
}
