#if UNITY_EDITOR && ANNULUS_CODEGEN && MODULE_CORE_MVVM_ADAPTERS_GENERATOR

using System;
using System.Globalization;
using EncosyTower.Modules.CodeGen;
using UnityCodeGen;
using UnityEngine;

namespace EncosyTower.Modules.Editor.Mvvm.ViewBinding.Adapters.ResourceKeys
{
    [Generator]
    internal class AnyResourcesAdaptersGenerator : ICodeGenerator
    {
        private readonly static string[] s_unityTypes = new string[] {
            nameof(Sprite),
            nameof(GameObject),
            nameof(AudioClip),
        };

        public void Execute(GeneratorContext context)
        {
            var nameofGenerator = nameof(AnyResourcesAdaptersGenerator);

            if (CodeGenAPI.TryGetOutputFolderPath(nameofGenerator, out var outputPath) == false)
            {
                context.OverrideFolderPath("Assets");
                return;
            }

            var p = Printer.DefaultLarge;
            p.PrintAutoGeneratedBlock(nameofGenerator);
            p.PrintEndLine();

            p.PrintLine(@"#pragma warning disable

using System;
using UnityEngine;
");

            p.PrintLine("namespace EncosyTower.Modules.Mvvm.ViewBinding.Adapters.ResourceKeys");
            p.OpenScope();
            {
                var unityTypes = s_unityTypes.AsSpan();
                var serializables = new bool[] { false };

                for (var i = 0; i < unityTypes.Length; i++)
                {
                    var type = unityTypes[i];

                    p.PrintBeginLine("#region    ").PrintEndLine(type.ToUpper(CultureInfo.InvariantCulture));
                    p.PrintBeginLine("#endregion ").Print('=', type.Length).PrintEndLine();
                    p.PrintEndLine();

                    foreach (var serializable in serializables)
                    {
                        var subType = serializable ? ".Serializable" : "";
                        var affix = serializable ? "Serializable" : "";

                        p.PrintLine($"[Serializable]");
                        p.PrintLine($"[Label(\"ResourceKey{subType}<{type}>.Load()\", \"Default\")]");
                        p.PrintLine($"[Adapter(sourceType: typeof(ResourceKey{subType}<{type}>), destType: typeof({type}), order: 0)]");
                        p.PrintLine($"public sealed class ResourceKeyTyped{affix}To{type}Adapter : ResourceKeyTyped{affix}Adapter<{type}> {{ }}");
                        p.PrintEndLine();

                        p.PrintLine($"[Serializable]");
                        p.PrintLine($"[Label(\"ResourceKey{subType}.Load<{type}>()\", \"Default\")]");
                        p.PrintLine($"[Adapter(sourceType: typeof(ResourceKey{subType}), destType: typeof({type}), order: 1)]");
                        p.PrintLine($"public sealed class ResourceKeyUntyped{affix}To{type}Adapter : ResourceKeyUntyped{affix}Adapter<{type}> {{ }}");
                        p.PrintEndLine();
                    }

                    p.PrintLine($"[Serializable]");
                    p.PrintLine($"[Label(\"Resources.Load<{type}>(string)\", \"Default\")]");
                    p.PrintLine($"[Adapter(sourceType: typeof(string), destType: typeof({type}), order: 2)]");
                    p.PrintLine($"public sealed class ResourceStringTo{type}Adapter : ResourceStringAdapter<{type}> {{ }}");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            context.OverrideFolderPath(outputPath);
            context.AddCode($"AnyResourcesAdapters.gen.cs", p.Result);
        }
    }
}

#endif
