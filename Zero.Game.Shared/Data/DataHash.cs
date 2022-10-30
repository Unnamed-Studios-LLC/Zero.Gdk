using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Zero.Game.Shared
{
    internal static class DataHash<T>
    {
        private enum ExtendedTypeCodes
        {
            Pointer = 20
        }

        private static readonly Type s_fixedBufferAttributeType = typeof(FixedBufferAttribute);

        public static void ApplyHashElement(Type elementType, ref long hash)
        {
            if (elementType.IsEnum)
            {
                elementType = Enum.GetUnderlyingType(elementType);
            }

            if (elementType.IsPointer)
            {
                AlterHash(ref hash, (long)ExtendedTypeCodes.Pointer);
                return;
            }

            var typeCode = Type.GetTypeCode(elementType);
            if (elementType.IsPrimitive ||
                typeCode == TypeCode.Decimal)
            {
                AlterHash(ref hash, (long)typeCode);
                return;
            }

            // else this is a nested struct
            ApplyHashStruct(elementType, ref hash);
        }

        private static void AlterHash(ref long hash, long value)
        {
            hash *= (1779033703 + 2 * value);
        }

        private static void ApplyHashField(FieldInfo field, ref long hash)
        {
            var attributes = field.GetCustomAttributes(s_fixedBufferAttributeType, false);
            if (attributes.Length > 0)
            {
                var attribute = (FixedBufferAttribute)attributes[0];
                AlterHash(ref hash, attribute.Length);
                ApplyHashElement(attribute.ElementType, ref hash);
            }
            else
            {
                ApplyHashElement(field.FieldType, ref hash);
            }
        }

        private static void ApplyHashStruct(Type type, ref long hash)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(x => (GetFieldOffset(x), x))
                .OrderBy(x => x.Item1);

            AlterHash(ref hash, type.StructLayoutAttribute.Size);
            foreach (var (offset, field) in fields)
            {
                AlterHash(ref hash, offset);
                ApplyHashField(field, ref hash);
            }
        }

        /// <summary>
        /// from https://stackoverflow.com/questions/30817924/obtain-non-explicit-field-offset
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        private static int GetFieldOffset(FieldInfo fi) => GetFieldOffset(fi.FieldHandle);
        private static int GetFieldOffset(RuntimeFieldHandle h) => Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;
    }
}
