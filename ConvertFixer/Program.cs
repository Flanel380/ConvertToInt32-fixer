using System;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace ConvertFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            ModuleDefMD module;
            string filepath;
            try
            {
                filepath = args[0];
                module = ModuleDefMD.Load(filepath);
            }
            catch
            {
                Console.WriteLine("Can't load module, maybe it's not .NET assembly, try to drag on drop another file..");
                Console.ReadLine();
                return;
            }
            int ConvertFixed = 0;
            Console.Title = "Convert.ToInt32 fixer by NCP";
            Console.WriteLine("Convert.ToInt32 fixer by NCP");
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.HasBody && method.Body.HasInstructions)
                    {
                        for (var i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            try
                            {
                                if (method.Body.Instructions[i].OpCode == OpCodes.Ldc_R4 && method.Body.Instructions[i+1].OpCode == OpCodes.Call && method.Body.Instructions[i+1].Operand.ToString().Contains("Convert"))
                                {
                                    float result = (float)method.Body.Instructions[i].Operand;
                                    method.Body.Instructions[i + 1].OpCode = OpCodes.Nop;
                                    method.Body.Instructions[i].OpCode = OpCodes.Ldc_I4;
                                    method.Body.Instructions[i].Operand = (int)result;
                                    ConvertFixed++;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"[{ConvertFixed}] Fixed Convert.ToInt32 in " + method.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Can't fix - " + ex.Message);
                            }
                        }
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (ConvertFixed != 0)
            {
                Console.WriteLine("Fixed " + ConvertFixed + " Convert.ToInt32 calls");
            }
            Console.WriteLine("Fix finished!Module wrote to " + filepath.Split('.')[0] + "-ConvertFixed.exe");
            module.Write(filepath.Split('.')[0] + "-ConvertFixed.exe");
            Console.ReadLine();
        }
    }
}
