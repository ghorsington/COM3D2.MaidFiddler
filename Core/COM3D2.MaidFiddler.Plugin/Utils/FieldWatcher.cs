using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace COM3D2.MaidFiddler.Core.Utils
{
    public interface IWatcher<in T>
    {
        Action<string, object> OnValueChanged { get; set; }
        void Set(T instance);
        void Update(T instance);
    }

    public static class FieldWatcher
    {
        public static IWatcher<T> CreateWatcher<T>()
        {
            var type = typeof(T);
            var asm = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName($"maid_watcher_{type.FullName}_{DateTime.Now.Ticks}"),
                AssemblyBuilderAccess.RunAndSave);
            var module = asm.DefineDynamicModule("main_module", "main_module.dll");

            var typeBuilder = module.DefineType($"{type.FullName}_Watcher",
                TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
                TypeAttributes.BeforeFieldInit, typeof(object),
                new[] {typeof(IWatcher<T>)});


            // Generate code for delegate
            var onValChangedField = typeBuilder.DefineField("_onValChanged_BackingField",
                typeof(Action<string, object>), FieldAttributes.Private);

            var onValChangedProp = typeBuilder.DefineProperty(nameof(IWatcher<T>.OnValueChanged),
                PropertyAttributes.None, typeof(Action<string, object>), new[] {typeof(Action<string, object>)});

            var setOnValChanged = typeBuilder.DefineMethod($"set_{nameof(IWatcher<T>.OnValueChanged)}",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                CallingConventions.HasThis, typeof(void), new[] {typeof(Action<string, object>)});

            var getOnValChanged = typeBuilder.DefineMethod($"get_{nameof(IWatcher<T>.OnValueChanged)}",
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.SpecialName | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                CallingConventions.HasThis, typeof(Action<string, object>), new Type[0]);

            var il = setOnValChanged.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, onValChangedField);
            il.Emit(OpCodes.Ret);

            il = getOnValChanged.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, onValChangedField);
            il.Emit(OpCodes.Ret);

            onValChangedProp.SetGetMethod(getOnValChanged);
            onValChangedProp.SetSetMethod(setOnValChanged);

            // Generate observable fields
            var generatedFields = new Dictionary<string, FieldInfo>();
            var fieldsToObserve = type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f =>
                f.FieldType.IsValueType || f.FieldType.IsEnum || f.FieldType == typeof(string));

            foreach (var fieldToObserve in fieldsToObserve)
            {
                var field = typeBuilder.DefineField(fieldToObserve.Name, fieldToObserve.FieldType,
                    fieldToObserve.Attributes);
                generatedFields[field.Name] = field;
            }


            // Generate Set method
            var setMethod = typeBuilder.DefineMethod(nameof(IWatcher<T>.Set),
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void),
                new[] {type});

            il = setMethod.GetILGenerator();
            foreach (var kv in generatedFields)
            {
                string name = kv.Key;
                var field = kv.Value;

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, type.GetField(name, BindingFlags.Instance | BindingFlags.Public));
                il.Emit(OpCodes.Stfld, field);
            }

            il.Emit(OpCodes.Ret);


            // Generate Update method

            var updateMethod = typeBuilder.DefineMethod(nameof(IWatcher<T>.Update),
                MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig |
                MethodAttributes.NewSlot | MethodAttributes.Virtual, CallingConventions.HasThis, typeof(void),
                new[] {type});

            il = updateMethod.GetILGenerator();

            foreach (var kv in generatedFields)
            {
                string name = kv.Key;
                var field = kv.Value;
                var originalField = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
                var skipLabel = il.DefineLabel();
                var invokeDelegateLabel = il.DefineLabel();
                var updateFieldLabel = il.DefineLabel();


                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, originalField);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(OpCodes.Beq, skipLabel);

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, onValChangedField);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Brtrue, invokeDelegateLabel);

                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Br, updateFieldLabel);

                il.MarkLabel(invokeDelegateLabel);
                il.Emit(OpCodes.Ldstr, name);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, originalField);
                il.Emit(OpCodes.Box, field.FieldType);
                il.Emit(OpCodes.Callvirt, typeof(Action<string, object>).GetMethod("Invoke"));

                il.MarkLabel(updateFieldLabel);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Ldfld, originalField);
                il.Emit(OpCodes.Stfld, field);

                il.MarkLabel(skipLabel);
            }

            il.Emit(OpCodes.Ret);

            var t = typeBuilder.CreateType();

            return (IWatcher<T>) Activator.CreateInstance(t);
        }
    }
}