namespace Sceelix.Designer.Utils
{
    public enum Distribution
    {
        Standard,
        Steam
    }

    public class BuildDistribution
    {
        public static Distribution Enum
        {
            get
            {
#if STEAM
                return Distribution.Steam;
#else
                return Distribution.Standard;
#endif
            }
        }

        public static bool IsSteam
        {
            get
            {
#if STEAM
                return true;
#else
                return false;
#endif
            }
        }

        public static bool IsStandard
        {
            get
            {
#if !STEAM
                return true;
#else
                return false;
#endif
            }
        }
    }
}
