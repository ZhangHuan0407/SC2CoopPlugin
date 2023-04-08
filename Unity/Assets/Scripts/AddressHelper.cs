using System;
using System.Runtime.InteropServices;

namespace ForceSerialize
{
    public class AddressHelper
    {
        private ObjectReinterpreter reinterpreter;

        public AddressHelper()
        {
            reinterpreter = new ObjectReinterpreter();
            reinterpreter.AsObject = new ObjectWrapper();
        }

        public IntPtr GetAddress(object obj)
        {
            reinterpreter.AsObject.Object = obj;
            IntPtr address = reinterpreter.AsIntPtr.Value;
            reinterpreter.AsObject.Object = null;
            return address;
        }

#if UNITY_EDITOR
        //public static T GetInstance<T>(IntPtr address)
        //{
        //    AddressHelper.reinterpreter.AsIntPtr.Value = address;
        //    T obj = (T)AddressHelper.reinterpreter.AsObject.Object;
        //    AddressHelper.reinterpreter.AsObject.Object = null;
        //    return obj;
        //}
#endif

        // I bet you thought C# was type-safe.
        [StructLayout(LayoutKind.Explicit)]
        private struct ObjectReinterpreter
        {
            [FieldOffset(0)] public ObjectWrapper AsObject;
            [FieldOffset(0)] public IntPtrWrapper AsIntPtr;
        }

        private class ObjectWrapper
        {
            public object Object;
        }

        private class IntPtrWrapper
        {
            public IntPtr Value;
        }
    }
}