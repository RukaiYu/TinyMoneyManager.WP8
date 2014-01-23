namespace TinyMoneyManager.Component
{
    using System;

    public class DatabaseVersion
    {
        public readonly static int v1_6_5 = 0;
        public readonly static int v1_7_0 = 2;
        public readonly static int v1_7_0_beta = 1;
        public readonly static int v1_8_0_to_5 = 3;
        public readonly static int v1_8_9 = 4;
        public readonly static int v1_9_0 = 5;
        public readonly static int v1_9_1 = 6;
        public readonly static int v1_9_3 = 7;

        /// <summary>
        /// 8
        /// </summary>
        public readonly static int v1_9_6 = 8;

        /// <summary>
        /// 9
        /// </summary>
        public readonly static int v1_9_7 = 9;

        /// <summary>
        /// 10
        /// </summary>
        public static readonly int v1_9_8 = 10;

        /// <summary>
        /// 11
        /// </summary>
        public static readonly int v1_9_9 = 11;

        /// <summary>
        /// 12
        /// </summary>
        public static readonly int v1_9_9_2 = 12;

        //TODO: Need to set the LatestVersion eachTime the scheme is modified.
        /// <summary>
        /// 
        /// </summary>
        public static readonly int CurrentVersion = v1_9_9_2;


    }
}

