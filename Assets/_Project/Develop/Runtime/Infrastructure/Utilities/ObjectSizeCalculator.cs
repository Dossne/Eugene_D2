using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Infrastructure.BroTweens;
using UnityEngine;

namespace Infrastructure.Utilities
{
    public class ObjectSizeCalculator : MonoBehaviour
    {
#if UNITY_EDITOR


        [TriInspector.Button]
        private void Calculate()
        {
            var calc = ClassSizeCalculator.CalculateClassSize(typeof(BroTweenBase));
            Debug.Log($"{calc}");
        }


#endif
    }

    public static class ClassSizeCalculator
    {
        private static readonly StringBuilder stringBuilder = new();
        private static readonly StringBuilder structDetail = new();


        /// <summary>
        /// Get calculated size by fields
        /// </summary>
        public static string CalculateClassSize(Type type)
        {
            stringBuilder.Clear();
            stringBuilder.AppendLine("----------------START DETAILED INFO----------------");
            int offset = 0;

            List<FieldInfo> fields = GetAllInstanceFields(type); // in declared order
            for (var i = 0; i < fields.Count; i++)
            {
                structDetail.Clear();

                var f = fields[i];
                int fieldSize = GetFieldSizeCLR(f.FieldType);
                int alignment = GetFieldAlignmentCLR(f.FieldType);
                int originalOffset = offset;
                offset = Align(offset, alignment);
                int padding = offset - originalOffset;
                offset += fieldSize;

                string paddingResult = padding > 0 ? $" {padding}".ToColor(ColorHex.Red) : $"{padding}".ToColor(ColorHex.Green);
                stringBuilder.AppendLine($"{i + 1}.Type: {f.FieldType}; Name: {f.Name}; Size: {fieldSize} byte; AddedPadding: {paddingResult}; CurrentOffset: {offset}");

                if (structDetail.Length > 0)
                    stringBuilder.Append(structDetail.ToString());

                stringBuilder.AppendLine("---");
            }

            stringBuilder.AppendLine("----------------END DETAILED INFO----------------");

            // align whole object
            offset = Align(offset, IntPtr.Size);

            bool hasHeader = !type.IsValueType;
            int headerSize = hasHeader ? IntPtr.Size * 2 : 0; // 16 bytes on 64-bit CLR
            int totalSize = headerSize + offset;
            string headerInfo = hasHeader ? $"header: {headerSize} + " : null;
            stringBuilder.Insert(0, $"Total size of {type.Name} = {totalSize} byte ({headerInfo}fields: {offset}) bytes\n");
            return stringBuilder.ToString();
        }


        /// <summary>
        /// Get approx size by total memory allocated
        /// </summary>
        public static (long total, double perObj) GetSizeByTotalMemory<T>() where T : class, new()
        {
            int count = 50000;
            long before = GC.GetTotalMemory(true);
            T[] arr = new T[count];
            for (int i = 0; i < count; i++)
                arr[i] = new T(); // constructor empty!

            long after = GC.GetTotalMemory(true);
            long delta = after - before;
            double perObject = (double)delta / count;
            return (delta, perObject); //bytes
        }


        private static int Align(int offset, int alignment)
        {
            int mod = offset % alignment;
            return mod == 0 ? offset : offset + (alignment - mod);
        }


        private static int GetFieldAlignmentCLR(Type t)
        {
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte))
                return 1;

            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char))
                return 2;

            if (t == typeof(int) || t == typeof(uint) || t == typeof(float))
                return 4;

            if (t == typeof(long) || t == typeof(ulong) || t == typeof(double))
                return 8;

            if (t.IsEnum)
                return GetFieldAlignmentCLR(Enum.GetUnderlyingType(t));

            if (t.IsValueType)
                return GetStructAlignment(t);

            return IntPtr.Size; // reference type
        }


        private static int GetFieldSizeCLR(Type t)
        {
            if (t == typeof(bool) || t == typeof(byte) || t == typeof(sbyte))
                return 1;

            if (t == typeof(short) || t == typeof(ushort) || t == typeof(char))
                return 2;

            if (t == typeof(int) || t == typeof(uint) || t == typeof(float))
                return 4;

            if (t == typeof(long) || t == typeof(ulong) || t == typeof(double))
                return 8;

            if (t.IsEnum)
                return GetFieldSizeCLR(Enum.GetUnderlyingType(t));

            if (t.IsValueType)
                return GetStructSizeCLR(t);

            return IntPtr.Size;
        }


        private static int GetStructSizeCLR(Type t)
        {
            int offset = 0;

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            structDetail.AppendLine("----");

            for (var i = 0; i < fields.Length; i++)
            {
                var f = fields[i];
                int fieldSize = GetFieldSizeCLR(f.FieldType);
                int alignment = GetFieldAlignmentCLR(f.FieldType);
                int originalOffset = offset;
                offset = Align(offset, alignment);
                int padding = offset - originalOffset;
                offset += fieldSize;

                string paddingResult = padding > 0 ? $" {padding}".ToColor(ColorHex.Red) : $"{padding}".ToColor(ColorHex.Green);
                structDetail.AppendLine($"----StructDetail. {i + 1}.Type: {f.FieldType}; Name: {f.Name}; Size: {fieldSize} byte; AddedPadding: {paddingResult}; CurrentOffset: {offset}");
                structDetail.AppendLine("----");
            }

            int originalOffsetTotal = offset;

            offset = Align(offset, 8); // struct alignment rule
            int paddingTotal = offset - originalOffsetTotal;
            string paddingResultTotal = paddingTotal > 0 ? $" {paddingTotal}".ToColor(ColorHex.Red) : $"{paddingTotal}".ToColor(ColorHex.Green);

            structDetail.AppendLine($"----StructDetail Total.  AddedPadding: {paddingResultTotal};");
            structDetail.AppendLine("----");
            return offset;
        }


        private static int GetStructAlignment(Type t)
        {
            int alignment = 1;

            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var f in fields)
            {
                alignment = Math.Max(alignment, GetFieldAlignmentCLR(f.FieldType));
            }

            return alignment;
        }


        private static List<FieldInfo> GetAllInstanceFields(Type type)
        {
            var result = new List<FieldInfo>();

            while (type != null && type != typeof(object))
            {
                var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                                 .Where(f => !f.IsStatic);

                result.AddRange(fields);
                type = type.BaseType;
            }

            return result;
        }
    }
}