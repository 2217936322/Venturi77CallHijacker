﻿using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using Newtonsoft.Json;
using Venturi77CallHijacker;
using System.Threading;
using static Venturi77Hijacker.CallHijacker;

namespace Venturi77Hijacker {
    class Program {
        static void Main(string[] args) {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[1] Inject Dll\n[2] Build Config\n[3] Build Debug Config\n[4] Parse #Koi Header");
            var key = Console.ReadKey();
            Console.Clear();
            if(key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1) {
                InjectDll();
                return;
            }
            if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2) {
                CreateConfig();
                return;
            }
            if (key.Key == ConsoleKey.D3 || key.Key == ConsoleKey.NumPad3) {
                CreateDebugConfig();
                return;
            }
            if (key.Key == ConsoleKey.D4 || key.Key == ConsoleKey.NumPad4) {
                ParseKoiHeader();
                return;
            }
        }
        public static void ParseKoiHeader() {
            Console.WriteLine("Path: ");
            string Path = Console.ReadLine().Replace("\"", "");
            Console.Clear();
            var data = KoiHeader.Parse(Path);
            if(data != null) {
                Console.WriteLine("Strings Found: ");
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.White;
                foreach (KeyValuePair<uint, string> keyValuePair in data.strings) {
                    try {
                        Console.WriteLine("[+] " + keyValuePair.Key + "|" + keyValuePair.Value);
                    } catch {
                    }
                }
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("References Found: ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                foreach (KeyValuePair<uint, KoiShit.VMData.RefInfo> keyValuePair2 in data.references) {
                    try {
                        Console.WriteLine("[+] " + keyValuePair2.Key + "|" + keyValuePair2.Value.Member.ToString());
                    } catch {
                    }
                }
                Console.WriteLine();
             /*   Console.WriteLine("Exports Found: ");
                foreach (var str in data.exports) {
                    WriteLine(str.Value.CodeOffset);
                }
                Console.WriteLine();*/
            } else {
                Console.WriteLine("Found nothing :(");
            }
            Console.ReadLine();
        }
        public static void WriteLine(string ToWrite) {
            Console.ForegroundColor = ConsoleColor.White;
            try {
                Console.WriteLine("[+] " +ToWrite);
            } catch { }
            Console.ForegroundColor = ConsoleColor.Cyan;
        }
        public static void InjectDll() {
            Obfuscator obf = Obfuscator.Unknown;
            Console.WriteLine("Path: ");
            string Path = Console.ReadLine().Replace("\"", "");
            ModuleDefMD Module = ModuleDefMD.Load(Path);
            Console.Clear();
            Console.WriteLine("[1] Detect Obfuscator\n[2] Select Obfuscator");
            var key = Console.ReadKey();
            Console.Clear();
            if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1) {
                obf = DetectObfuscator(Module,Path);
                Console.WriteLine("Detected: " + obf);
               
            }
            if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2) {
                Console.WriteLine("[1] KoiVM\n[2] EazVM\n[3] AgileVM");
                obf = (Obfuscator)(Convert.ToInt32(Console.ReadLine())-1);
                Console.Clear();
            }
            if(obf == Obfuscator.KoiVM) {
                if (InjectKoiVM(Module, Path)) {
                    Console.WriteLine("Successfully Injected KoiVM");
                } else {
                    Console.WriteLine("Failed Injecting KoiVM");
                }
                Console.ReadLine();
                return;
            }
            if(obf == Obfuscator.EazVM) {
                if (InjectEazVM(Module, Path)) {
                    Console.WriteLine("Successfully Injected EazVM");
                } else {
                    Console.WriteLine("Failed Injecting EazVM");
                }
                Console.ReadLine();
                return;
            }
            if(obf == Obfuscator.AgileVM) {
                /*  if (InjectAgileVM(Module, Path)) {
                      Console.WriteLine("Successfully Injected AgileVM");
                  } else {
                      Console.WriteLine("Failed Injecting AgileVM");
                  }*/
                Console.WriteLine("Some Issues With Agile Will Fix In A Bit!");
                Console.ReadLine();
                return;
            }
            if(obf == Obfuscator.KoiVMDll) {
                if (InjectKoiVM(ModuleDefMD.Load(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), "KoiVM.Runtime.dll")), System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path), "KoiVM.Runtime.dll"))) {
                    Console.WriteLine("Successfully Injected KoiVM");
                } else {
                    Console.WriteLine("Failed Injecting KoiVM");
                }
            }
            if(obf == Obfuscator.Unknown) {
                Console.WriteLine("Unknown Obfuscator!");
                Console.ReadLine();
                return;
            }

        }
        public static int WriteShit() {
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("[1] New Function");
            Console.WriteLine("[2] Save");
            Console.ForegroundColor = ConsoleColor.Cyan;

            int parse2 = Convert.ToInt32(WriteInput());
            Console.Clear();
            return parse2;
        }
        public static void CreateDebugConfig() {

            CallHijacker.Config Config = new CallHijacker.Config();
            Config.Debug = true;
            File.AppendAllText("Config.Json", JsonConvert.SerializeObject(Config, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            }));

        }
        public static void CreateConfig() {
            CallHijacker.Config Config = new CallHijacker.Config();
            Config.Debug = false;
            Config.Functions = new CallHijacker.Function();
            Config.Functions.MDToken = new List<CallHijacker.MDToken>();
            Config.Functions.Methods = new List<Method>();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Config.Functions = CreateFunction(Config.Functions);
            SaveConfig(Config);
            Console.WriteLine("Saved!");
            Console.ReadLine();
            Console.Clear();

        }
        public static void SaveConfig(Config config) {
            File.AppendAllText("Config.Json", JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore
            }));
        }
        public static CallHijacker.Parameter GenerateParam() {
            CallHijacker.Parameter param = new CallHijacker.Parameter();
            Console.WriteLine("Parameter Index:");
            param.ParameterIndex = Convert.ToInt32(WriteInput());
            Console.Clear();
            Console.WriteLine("Replace Parameter Value With: (*null* if do nothing)");
            string input2 = WriteInput();
            param.ReplaceWith = ParseString(input2);
            Console.Clear();
            return param;
        }
        public static CallHijacker.Function CreateFunction(CallHijacker.Function func) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Search By: " + Environment.NewLine);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[1] MethodName" + Environment.NewLine + "[2] MDToken" + Environment.NewLine);
            int parse3 = Convert.ToInt32(WriteInput());
            if(parse3 == 1) {
                Method method = new Method();
                Console.WriteLine("Method Name:");
                method.MethodName = WriteInput();
                Console.Clear();
                Console.WriteLine("Replace Result With: (*null* if do nothing)");
                method.ReplaceResultWith = ParseString(WriteInput());
                Console.Clear();
                Console.WriteLine("[1] Add Edit Parameter\n[2]Continue");
                if (Convert.ToInt32(WriteInput()) == 1) {
                    method.Param = new List<CallHijacker.Parameter>();
                    for (; ; )
                        {
                        method.Param.Add(GenerateParam());
                        Console.WriteLine("[1] Add Edit Parameter\n[2]Continue");
                        if (Convert.ToInt32(WriteInput()) != 1)
                            break;
                    }
                }
                Console.Clear();
                func.Methods.Add(method);
                Console.WriteLine("[1] Continue\n[2] Save");
                int parse2  = Convert.ToInt32(WriteInput());
                if(parse2 == 1) {
                    CreateFunction(func);
                } else {
                    return func;
                }
            } else {
                if (parse3 == 3) {
                   
                   
                } else {
                    if (parse3 == 2) {
                        CallHijacker.MDToken MDTok = new CallHijacker.MDToken();
                        Console.WriteLine("MDToken:");
                        MDTok.MDTokenInt = Convert.ToInt32(WriteInput());
                        Console.Clear();
                        Console.WriteLine("Replace Result With: (*null* if do nothing)");
                        string input = WriteInput();
                        MDTok.ReplaceResultWith = ParseString(input);
                        Console.Clear();
                        Console.WriteLine();
                        Console.WriteLine("[1] Add Edit Parameter\n[2]Continue");
                        if(Convert.ToInt32(WriteInput()) == 1) {
                            MDTok.Param = new List<CallHijacker.Parameter>();
                            for (; ; )
                                {
                                MDTok.Param.Add(GenerateParam());
                                Console.WriteLine("[1] Add Edit Parameter\n[2] Continue");
                                if (Convert.ToInt32(WriteInput()) != 1)
                                    break;
                            }
                        }
                        func.MDToken.Add(MDTok);
                        Console.Clear();
                        Console.WriteLine("[1] Continue\n[2] Save");
                        int parse2 = Convert.ToInt32(WriteInput());
                        if (parse2 == 1) {
                            CreateFunction(func);
                        } else {
                            return func;
                        }
                    }
                }
            }
            

           
            return func;

        }
        public static object ParseString(string input) {
            if (input == "*null*")
                return null;
            if (input == "*true*")
                return true;
            if (input == "*false*")
                return false;
            return input;
            
        }
        public static string WriteInput() {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Input: ");
            string input = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();
            return input;
        }
        public static bool InjectAgileVM(ModuleDefMD Module, string Pathh) {
            bool injected = false;
            TypeDef[] array = Module.Types.ToArray<TypeDef>();
            for (int i = 0; i < array.Length; i++) {
                foreach (MethodDef method in array[i].Methods.ToArray<MethodDef>()) {
                    if(method.HasBody && method.Body.HasInstructions) {
                        for (int d = 0; d < method.Body.Instructions.Count(); d++) {

                            if(method.Body.Instructions[d].OpCode == OpCodes.Callvirt) {
                                if (method.Body.Instructions[d].ToString().Contains("System.Reflection.MethodBase::Invoke(System.Object,System.Object[])") && method.Body.Instructions[d + 1].IsStloc() && method.Body.Instructions[d - 1].IsLdarg() && method.Body.Instructions[d - 3].IsLdarg()) {
                                    method.Body.Instructions[d].Operand = method.Module.Import(ModuleDefMD.Load("Venturi77CallHijacker.dll").Types.Single(t => t.IsPublic && t.Name == "Handler").Methods.Single(m => m.Name == "HandleInvoke"));
                                    method.Body.Instructions[d].OpCode = OpCodes.Call;
                                    injected = true;
                                }
                        }
                           
                        }
                    }
                }
            }
            if(!injected) {
                return injected;
            }
            ModuleWriterOptions nativeModuleWriterOptions = new ModuleWriterOptions(Module);
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Cor20HeaderOptions.Flags = new ComImageFlags?(ComImageFlags.ILOnly);
            
          //  var otherstrteams = Module.Metadata.AllStreams.Where(a => a.GetType() == typeof(DotNetStream));
          //  nativeModuleWriterOptions.MetadataOptions.PreserveHeapOrder(Module, addCustomHeaps: true);
            Module.Write(Pathh, nativeModuleWriterOptions);



            // File.Delete(Pathh);
          
            File.Copy("Venturi77CallHijacker.dll", Path.GetDirectoryName(Pathh) + "\\Venturi77CallHijacker.dll");
            File.Copy("Newtonsoft.Json.dll", Path.GetDirectoryName(Pathh)+ "\\Newtonsoft.Json.dll");
            return injected;
        }
        public static bool InjectEazVM(ModuleDefMD Module, string Pathh) {
            bool injected = false;
            TypeDef[] array = Module.Types.ToArray<TypeDef>();
            for (int i = 0; i < array.Length; i++) {
                foreach (MethodDef method in array[i].Methods.ToArray<MethodDef>()) {
                    bool flag = method.HasBody && method.Body.HasInstructions && method.Body.Instructions.Count() >=15 && !method.FullName.Contains("My.") && !method.FullName.Contains(".My") && !method.IsConstructor && !method.DeclaringType.IsGlobalModuleType;
                    if (flag) {
                        for (int j = 0; j < method.Body.Instructions.Count - 1; j++) {
                            if (method.Body.Instructions[j].OpCode == OpCodes.Callvirt) {
                                string operand = method.Body.Instructions[j].Operand.ToString();
                                if (operand.Contains("System.Object System.Reflection.MethodBase::Invoke(System.Object,System.Object[])") && method.Body.Instructions[j - 1].IsLdarg() && method.Body.Instructions[j - 2].IsLdarg() && method.Body.Instructions[j - 3].IsLdarg()) {
                                    Importer importer = new Importer(Module);
                                    IMethod myMethod;
                                    myMethod = importer.Import(typeof(Venturi77CallHijacker.Handler).GetMethod("HandleInvoke"));
                                    method.Body.Instructions[j].Operand = Module.Import(myMethod);
                                    method.Body.Instructions[j].OpCode = OpCodes.Call;
                                    injected = true;
                                 
                                }
                            }
                        }
                    }
                }
            
        
            }
            if (!injected) {
                return injected;
            }
            ModuleWriterOptions moduleWriterOptions = new ModuleWriterOptions(Module);
            moduleWriterOptions.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
            moduleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            moduleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            moduleWriterOptions.MetadataOptions.PreserveHeapOrder(Module, true);
            moduleWriterOptions.Cor20HeaderOptions.Flags = new ComImageFlags?(ComImageFlags.ILOnly | ComImageFlags.Bit32Required);
            Module.Write(Pathh + "_Injected.exe", moduleWriterOptions);
            File.Copy("Venturi77CallHijacker.dll", Path.GetDirectoryName(Pathh) + "\\Venturi77CallHijacker.dll");
            File.Copy("Newtonsoft.Json.dll", Path.GetDirectoryName(Pathh )+ "\\Newtonsoft.Json.dll");
            return injected;
        }
        public static bool InjectKoiVM(ModuleDefMD Module,string Pathh) {
            bool injected = false;
            TypeDef[] array = Module.Types.ToArray<TypeDef>();
            for (int i = 0; i < array.Length; i++) {
                foreach (MethodDef method in array[i].Methods.ToArray<MethodDef>()) {
                    bool flag = method.HasBody && method.Body.HasInstructions && !method.FullName.Contains("My.") && !method.FullName.Contains(".My") && !method.IsConstructor && !method.DeclaringType.IsGlobalModuleType;
                    if (flag) {
                        for (int j = 0; j < method.Body.Instructions.Count - 1; j++) {
                            if (method.Body.Instructions[j].OpCode == OpCodes.Ldarg_2) {
                                if (method.Body.Instructions[j + 1].OpCode == OpCodes.Ldloc_2) {
                                    if (method.Body.Instructions[j + 2].OpCode == OpCodes.Ldloc_3) {
                                        if (method.Body.Instructions[j + 3].OpCode == OpCodes.Callvirt) {
                                            if (method.Body.Instructions[j + 4].OpCode == OpCodes.Stloc_S) {
                                                method.Body.Instructions[j + 3].OpCode = OpCodes.Call;
                                                Importer importer = new Importer(Module);
                                                IMethod Method;
                                                Method = importer.Import(typeof(Venturi77CallHijacker.Handler).GetMethod("HandleInvoke"));
                                                method.Body.Instructions[j+3].Operand = Module.Import(Method);
                                                method.Body.Instructions[j + 3].OpCode = OpCodes.Call;
                                                injected = true;
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    }
                }

            }
            if (!injected) {
                return injected;
            }
            ModuleWriterOptions nativeModuleWriterOptions = new ModuleWriterOptions(Module);
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
            nativeModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;
            nativeModuleWriterOptions.MetadataOptions.Flags = MetadataFlags.PreserveAll;
            nativeModuleWriterOptions.Cor20HeaderOptions.Flags = new ComImageFlags?(ComImageFlags.ILOnly);
            var otherstrteams = Module.Metadata.AllStreams.Where(a => a.GetType() == typeof(DotNetStream));
            nativeModuleWriterOptions.MetadataOptions.PreserveHeapOrder(Module, addCustomHeaps: true);
            if(Pathh.ToString().ToUpper().Contains(".DLL")) {
                Thread.Sleep(1000);
                Module.Write(Path.Combine(Path.GetDirectoryName(Pathh), Path.GetFileNameWithoutExtension(Pathh) + "_Inj" + ".dll"), nativeModuleWriterOptions);

            } else {
               
                Module.Write(Path.Combine(Path.GetDirectoryName(Pathh), Path.GetFileNameWithoutExtension(Pathh) + "_Injected" + ".exe"), nativeModuleWriterOptions);


            }
            File.Copy("Venturi77CallHijacker.dll", Path.GetDirectoryName(Pathh) + "\\Venturi77CallHijacker.dll");
            File.Copy("Newtonsoft.Json.dll", Path.GetDirectoryName(Pathh)+ "\\Newtonsoft.Json.dll");
            return injected;
        }
        public static Obfuscator DetectObfuscator(ModuleDefMD Module,string Path) {
            if(Module.GetAssemblyRefs().Where(q=>q.ToString().Contains("AgileDotNet.VMRuntime")).ToArray().Count() > 0){
                return Obfuscator.AgileVM;
            }
            if(Module.Types.SingleOrDefault(t => t.HasFields && t.Fields.Count == 119) != null) {
                return Obfuscator.KoiVM;
            }
            if (Module.GetAssemblyRefs().Where(q => q.ToString().Contains("KoiVM.Runtime")).ToArray().Count() > 0) {
                return Obfuscator.KoiVMDll;
            }

            foreach (var type in Module.Types) {
                foreach(var method in type.Methods) {
                    if(method.HasBody && method.Body.HasInstructions&& method.Body.Instructions.Count() >= 6) {
                       if(method.Body.Instructions[0].IsLdarg()) {
                            if(method.Body.Instructions[1].IsLdarg()) {
                                if (method.Body.Instructions[2].IsLdarg()) {
                                    if (method.Body.Instructions[3].IsLdarg()) {
                                        if (method.Body.Instructions[5].OpCode == OpCodes.Pop) {
                                            if (method.Body.Instructions[6].OpCode == OpCodes.Ret) {
                                                if (method.Body.Instructions[4].OpCode == OpCodes.Call) {
                                                    
                                                    if(method.Body.Instructions[4].ToString().Contains("(System.IO.Stream,System.String,System.Object[])")) {
                                                         return Obfuscator.EazVM;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Obfuscator.Unknown;
        }
        
    }
}
