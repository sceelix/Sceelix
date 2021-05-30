namespace Sceelix.Core.Graphs.Functions
{
    public static class PrimitiveOperators
    {
        #region op_Addition

        public static int op_Addition(int op1, int op2)
        {
            return op1 + op2;
        }



        public static uint op_Addition(uint op1, uint op2)
        {
            return op1 + op2;
        }



        public static long op_Addition(long op1, long op2)
        {
            return op1 + op2;
        }



        public static ulong op_Addition(ulong op1, ulong op2)
        {
            return op1 + op2;
        }



        public static float op_Addition(float op1, float op2)
        {
            return op1 + op2;
        }



        public static double op_Addition(double op1, double op2)
        {
            return op1 + op2;
        }



        public static string op_Addition(string op1, string op2)
        {
            return op1 + op2;
        }

        #endregion

        #region op_Subtraction

        public static int op_Subtraction(int op1, int op2)
        {
            return op1 - op2;
        }



        public static uint op_Subtraction(uint op1, uint op2)
        {
            return op1 - op2;
        }



        public static long op_Subtraction(long op1, long op2)
        {
            return op1 - op2;
        }



        public static ulong op_Subtraction(ulong op1, ulong op2)
        {
            return op1 - op2;
        }



        public static float op_Subtraction(float op1, float op2)
        {
            return op1 - op2;
        }



        public static double op_Subtraction(double op1, double op2)
        {
            return op1 - op2;
        }

        #endregion

        #region op_Multiplication

        public static int op_Multiplication(int x, int y)
        {
            return x * y;
        }



        public static uint op_Multiplication(uint x, uint y)
        {
            return x * y;
        }



        public static long op_Multiplication(long x, long y)
        {
            return x * y;
        }



        public static ulong op_Multiplication(ulong x, ulong y)
        {
            return x * y;
        }



        public static float op_Multiplication(float x, float y)
        {
            return x * y;
        }



        public static double op_Multiplication(double x, double y)
        {
            return x * y;
        }

        #endregion

        #region op_Division

        public static int op_Division(int x, int y)
        {
            return x / y;
        }



        public static uint op_Division(uint x, uint y)
        {
            return x / y;
        }



        public static long op_Division(long x, long y)
        {
            return x / y;
        }



        public static ulong op_Division(ulong x, ulong y)
        {
            return x / y;
        }



        public static float op_Division(float x, float y)
        {
            return x / y;
        }



        public static double op_Division(double x, double y)
        {
            return x / y;
        }

        #endregion

        #region op_Remainder

        public static int op_Remainder(int x, int y)
        {
            return x % y;
        }



        public static uint op_Remainder(uint x, uint y)
        {
            return x % y;
        }



        public static long op_Remainder(long x, long y)
        {
            return x % y;
        }



        public static ulong op_Remainder(ulong x, ulong y)
        {
            return x % y;
        }



        public static float op_Remainder(float x, float y)
        {
            return x % y;
        }



        public static double op_Remainder(double x, double y)
        {
            return x % y;
        }

        #endregion

        #region op_ConditionalAndOr

        public static bool op_ConditionalOr(bool x, bool y)
        {
            return x || y;
        }



        public static bool op_ConditionalAnd(bool x, bool y)
        {
            return x && y;
        }

        #endregion

        #region op_Equal

        public static bool op_Equal(int x, int y)
        {
            return x == y;
        }



        public static bool op_Equal(uint x, uint y)
        {
            return x == y;
        }



        public static bool op_Equal(long x, long y)
        {
            return x == y;
        }



        public static bool op_Equal(ulong x, ulong y)
        {
            return x == y;
        }



        public static bool op_Equal(float x, float y)
        {
            return x == y;
        }



        public static bool op_Equal(double x, double y)
        {
            return x == y;
        }



        public static bool op_Equal(bool x, bool y)
        {
            return x == y;
        }



        public static bool op_Equal(string x, string y)
        {
            return x == y;
        }

        #endregion

        #region op_NotEqual

        public static bool op_NotEqual(int x, int y)
        {
            return x != y;
        }



        public static bool op_NotEqual(uint x, uint y)
        {
            return x != y;
        }



        public static bool op_NotEqual(long x, long y)
        {
            return x != y;
        }



        public static bool op_NotEqual(ulong x, ulong y)
        {
            return x != y;
        }



        public static bool op_NotEqual(float x, float y)
        {
            return x != y;
        }



        public static bool op_NotEqual(double x, double y)
        {
            return x != y;
        }



        public static bool op_NotEqual(bool x, bool y)
        {
            return x != y;
        }



        public static bool op_NotEqual(string x, string y)
        {
            return x != y;
        }

        #endregion

        #region op_LesserThan

        public static bool op_LesserThan(int x, int y)
        {
            return x < y;
        }



        public static bool op_LesserThan(uint x, uint y)
        {
            return x < y;
        }



        public static bool op_LesserThan(long x, long y)
        {
            return x < y;
        }



        public static bool op_LesserThan(ulong x, ulong y)
        {
            return x < y;
        }



        public static bool op_LesserThan(float x, float y)
        {
            return x < y;
        }



        public static bool op_LesserThan(double x, double y)
        {
            return x < y;
        }

        #endregion

        #region op_GreaterThan

        public static bool op_GreaterThan(int x, int y)
        {
            return x > y;
        }



        public static bool op_GreaterThan(uint x, uint y)
        {
            return x > y;
        }



        public static bool op_GreaterThan(long x, long y)
        {
            return x > y;
        }



        public static bool op_GreaterThan(ulong x, ulong y)
        {
            return x > y;
        }



        public static bool op_GreaterThan(float x, float y)
        {
            return x > y;
        }



        public static bool op_GreaterThan(double x, double y)
        {
            return x > y;
        }

        #endregion

        #region op_LesserOrEqualThan

        public static bool op_LesserOrEqualThan(int x, int y)
        {
            return x <= y;
        }



        public static bool op_LesserOrEqualThan(uint x, uint y)
        {
            return x <= y;
        }



        public static bool op_LesserOrEqualThan(long x, long y)
        {
            return x <= y;
        }



        public static bool op_LesserOrEqualThan(ulong x, ulong y)
        {
            return x <= y;
        }



        public static bool op_LesserOrEqualThan(float x, float y)
        {
            return x <= y;
        }



        public static bool op_LesserOrEqualThan(double x, double y)
        {
            return x <= y;
        }

        #endregion

        #region op_GreaterOrEqualThan

        public static bool op_GreaterOrEqualThan(int x, int y)
        {
            return x >= y;
        }



        public static bool op_GreaterOrEqualThan(uint x, uint y)
        {
            return x >= y;
        }



        public static bool op_GreaterOrEqualThan(long x, long y)
        {
            return x >= y;
        }



        public static bool op_GreaterOrEqualThan(ulong x, ulong y)
        {
            return x >= y;
        }



        public static bool op_GreaterOrEqualThan(float x, float y)
        {
            return x >= y;
        }



        public static bool op_GreaterOrEqualThan(double x, double y)
        {
            return x >= y;
        }

        #endregion

        /*int operator &(int x, int y);
        uint operator &(uint x, uint y);
        long operator &(long x, long y);
        ulong operator &(ulong x, ulong y);

        int operator |(int x, int y);
        uint operator |(uint x, uint y);
        long operator |(long x, long y);
        ulong operator |(ulong x, ulong y);

        int operator ^(int x, int y);
        uint operator ^(uint x, uint y);
        long operator ^(long x, long y);
        ulong operator ^(ulong x, ulong y);*/
    }
}