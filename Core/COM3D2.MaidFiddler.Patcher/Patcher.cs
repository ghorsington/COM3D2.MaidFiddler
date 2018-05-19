using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Inject;

namespace COM3D2.MaidFiddler.Patcher
{
    public class Patcher
    {
        public static readonly string[] TargetAssemblyNames = {"Assembly-CSharp.dll"};

        private const string HOOK_NAME = "COM3D2.MaidFiddler.Hooks";

        private static AssemblyDefinition HookDefinition;

        public static void Patch(AssemblyDefinition ass)
        {
            string assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            HookDefinition = AssemblyLoader.LoadAssembly(Path.Combine(assemblyDir, $"{HOOK_NAME}.dll"));

            PatchMaidStatus(ass);
            PatchCharacterMgr(ass);
            PatchGameMain(ass);
        }

        private static void PatchGameMain(AssemblyDefinition ass)
        {
            TypeDefinition gameMainHooks = HookDefinition.MainModule.GetType($"{HOOK_NAME}.GameMainHooks");

            TypeDefinition gameMain = ass.MainModule.GetType("GameMain");

            MethodDefinition deserialize = gameMain.GetMethod("Deserialize");

            deserialize.InjectWith(gameMainHooks.GetMethod("OnPreDeserialize"));

            MethodReference onPostDeserialize = ass.MainModule.Import(gameMainHooks.GetMethod("OnPostDeserialize"));

            ILProcessor il = deserialize.Body.GetILProcessor();

            for (int index = 0; index < deserialize.Body.Instructions.Count; index++)
            {
                Instruction ins = deserialize.Body.Instructions[index];
                if (ins.OpCode.Code == Code.Ret)
                {
                    il.InsertBefore(ins, il.Create(OpCodes.Call, onPostDeserialize));
                    index++;
                }
            }
        }

        private static void PatchCharacterMgr(AssemblyDefinition ass)
        {
            TypeDefinition characterMgrHooks = HookDefinition.MainModule.GetType($"{HOOK_NAME}.CharacterMgrHooks");

            MethodDefinition onBanishment = characterMgrHooks.GetMethod("OnCharacterBanished");
            MethodDefinition onAdd = characterMgrHooks.GetMethod("OnCharacterAdded");
            TypeDefinition characterMgr = ass.MainModule.GetType("CharacterMgr");

            characterMgr.GetMethod("AddStock").InjectWith(onAdd,
                                                          flags: InjectFlags.PassParametersVal | InjectFlags.PassLocals,
                                                          codeOffset: -2,
                                                          localsID: new[] { 1 });

            characterMgr.GetMethod("BanishmentMaid", "Maid", "System.Boolean")
                        .InjectWith(onBanishment, flags: InjectFlags.PassParametersVal);
        }

        private static void PatchMaidStatus(AssemblyDefinition ass)
        {
            TypeDefinition maidStatusHooks = HookDefinition.MainModule.GetType($"{HOOK_NAME}.MaidStatusHooks");

            MethodDefinition onPropertySetPrefix = maidStatusHooks.GetMethod("OnPropertySetPrefix");
            MethodDefinition onPropertySetPostfix = maidStatusHooks.GetMethod("OnPropertySetPostfix");

            TypeDefinition maidStatus = ass.MainModule.GetType("MaidStatus.Status");
            TypeDefinition maid = ass.MainModule.GetType("Maid");

            FieldDefinition bonusStatus = maidStatus.GetField("bonusStatus");
            bonusStatus.IsPublic = true;
            bonusStatus.IsPrivate = false;

            foreach (MethodDefinition setter in maidStatus.Methods.Where(m => m.Name.StartsWith("set_") && m.IsPublic))
            {
                string tag = setter.Name.Substring("set_".Length);
                Console.WriteLine($"Patching {tag}");
                setter.InjectWith(onPropertySetPrefix,
                                  tag: tag,
                                  flags: InjectFlags.PassInvokingInstance
                                         | InjectFlags.ModifyReturn
                                         | InjectFlags.PassStringTag);
                setter.InjectWith(onPropertySetPostfix,
                                  tag: tag,
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassStringTag,
                                  codeOffset: -1);
            }

            maidStatus.GetMethod("AddFeature", "MaidStatus.Feature/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnFeatureAdd"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -2);

            maidStatus.GetMethod("RemoveFeature", "MaidStatus.Feature/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnFeatureRemove"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -1);

            maidStatus.GetMethod("AddPropensity", "MaidStatus.Propensity/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnPropensityAdd"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -2);

            maidStatus.GetMethod("RemovePropensity", "MaidStatus.Propensity/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnPropensityRemove"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -1);

            maidStatus.GetMethod("ChangeJobClass", "MaidStatus.JobClass/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnJobClassChange"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -2);

            maidStatus.GetMethod("ChangeYotogiClass", "MaidStatus.YotogiClass/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnYotogiClassChange"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -2);

            maidStatus.GetMethod("SetPersonal", "MaidStatus.Personal/Data")
                      .InjectWith(maidStatusHooks.GetMethod("OnPersonalSet"),
                                  flags: InjectFlags.PassInvokingInstance | InjectFlags.PassParametersVal,
                                  codeOffset: -2);

            maidStatus.GetMethod("SetSeikeikenBack").InjectWith(maidStatusHooks.GetMethod("OnSetSeikeiken"),
                                                                tag: "back",
                                                                flags: InjectFlags.PassInvokingInstance
                                                                       | InjectFlags.PassParametersVal
                                                                       | InjectFlags.PassStringTag,
                                                                codeOffset: -1);

            maidStatus.GetMethod("SetSeikeikenFront").InjectWith(maidStatusHooks.GetMethod("OnSetSeikeiken"),
                                                                 tag: "front",
                                                                 flags: InjectFlags.PassInvokingInstance
                                                                        | InjectFlags.PassParametersVal
                                                                        | InjectFlags.PassStringTag,
                                                                 codeOffset: -1);

            maidStatus.GetMethod("SetWorkDataLevel").InjectWith(maidStatusHooks.GetMethod("OnWorkDataUpdate"),
                                                                flags: InjectFlags.PassInvokingInstance
                                                                       | InjectFlags.PassParametersVal,
                                                                codeOffset: -1);

            maidStatus.GetMethod("AddWorkDataPlayCount").InjectWith(maidStatusHooks.GetMethod("OnWorkDataUpdate"),
                                                                    flags: InjectFlags.PassInvokingInstance
                                                                           | InjectFlags.PassParametersVal,
                                                                    codeOffset: -1);

            maidStatus.GetMethod("UpdateClassBonusStatus").InjectWith(maidStatusHooks.GetMethod("OnBonusStatusChange"),
                                                                      flags: InjectFlags.PassInvokingInstance,
                                                                      codeOffset: -1);


            maid.GetMethod("ThumShot").InjectWith(maidStatusHooks.GetMethod("OnThumShot"), flags: InjectFlags.PassInvokingInstance, codeOffset: -1);
        }
    }
}