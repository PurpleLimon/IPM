using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Проекты_8_9_Классы
{
    public static class Extensions
    {

        public static bool IsNullOrEmpty(this IEnumerable<char> items)
        {
            return items == null || !items.Except(new char[] { ' ' }).Any();
        }

        public static byte SumBytes(this IEnumerable<byte> arr)
        {
            int res = 0;
            foreach (var item in arr)
                res += item;

            return (byte)res;
        }

        /// <summary>
        /// Возвращает строку полученую повтором исходной строки count раз
        /// </summary>
        /// <param name="str"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string Multiply(this string str, int count)
        {
            if (count < 0)
            {
                throw new Exception("Недопустимое значение переменной count");
            }

            string res = "";

            for (int i = 0; i < count; i++)
            {
                res += str;
            }

            return res;
        }

        public static Int16[] ArrayCastToInt16s<T>(this T[] arr)
        {
            Int16[] res = new short[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                res[i] = Convert.ToInt16(arr[i]);

            return res;
        }

        public static byte[] ArrayCastToBytes<T>(this T[] arr)
        {
            byte[] res = new byte[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                res[i] = Convert.ToByte(arr[i]);

            return res;
        }

        public static UInt16[] ArrayCastToUInt16s<T>(this T[] arr)
        {
            UInt16[] res = new UInt16[arr.Length];

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].ToString() == "")
                {
                    res[i] = 0;
                    continue;
                }
                res[i] = Convert.ToUInt16(arr[i]);
            }

            return res;
        }

        public static string[] ArrayCastToStrings<T>(this T[] arr)
        {
            string[] res = new string[arr.Length];

            for (int i = 0; i < arr.Length; i++)
                res[i] = Convert.ToString(arr[i]);

            return res;
        }

        public static void SumWith(this float[] actualArr, float[] sumWithArr, int actualFrom, int sumWithFrom)
        {
            for (int i = actualFrom; i < actualArr.Length; i++)
            {
                actualArr[i] += sumWithArr[sumWithFrom];
                actualFrom++;
            }
        }
        public static void SumWith(this int[] actualArr, int[] sumWithArr, int actualFrom, int sumWithFrom)
        {
            for (int i = actualFrom; i < actualArr.Length; i++)
            {
                actualArr[i] += sumWithArr[sumWithFrom];
                actualFrom++;
            }
        }

        public static void SumWith(this int[] actualArr, byte[] sumWithArr, int actualFrom, int sumWithFrom)
        {
            for (int i = actualFrom; i < actualArr.Length; i++)
            {
                actualArr[i] += sumWithArr[sumWithFrom];
                actualFrom++;
            }
        }

        public static byte[] DivideAllBy(int[] arr, float divideBy)
        {
            byte[] res = new byte[arr.Length];

            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (byte)Math.Round(arr[i] / divideBy, MidpointRounding.AwayFromZero);
            }
            return res;
        }

        public static int IntArrayCastToOne_ByScales(this UInt16[] arr, params int[] scales)
        {
            int res = 0;
            for (int i = 0; i < Math.Min(arr.Length, scales.Length); i++)
            {
                res += arr[i] * scales[i];
            }

            return res;
        }
    }

    public static class ExceptionExtension
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        public static bool IsFileLocked(this Exception exception)
        {
            int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }
    }



}
