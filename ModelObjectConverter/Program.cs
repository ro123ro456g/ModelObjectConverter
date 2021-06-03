using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

namespace ModelObjectConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!Directory.Exists("./target"))
            {
                Directory.CreateDirectory("./target");
            }

            Console.WriteLine("請將Model檔放入tartget資料夾");

            Console.WriteLine("請輸入來源的Model(Vo、Bo、Do) 目前只有Do有能用 : ");

            string target = GetInputStr(new string[] { "Vo", "Bo", "Do" });

            Console.WriteLine("請輸入要轉成的Model(Vo、Bo) : ");

            string output = GetInputStr(new string[] { "Vo", "Bo" });


            Console.WriteLine("請輸入要變更的Namespace , 不變更請輸入空值");

            string nameSpace = Console.ReadLine();

            Console.WriteLine("是否要繼承Base(Y、N) : ");

            string f = GetInputStr(new string[] { "Y", "N" });

            bool baseFlag = f == "Y";
            string usingNameSpace = string.Empty;
            if (baseFlag)
            {
                Console.WriteLine("要引用的namespace");

                usingNameSpace = Console.ReadLine();
            }

            Console.WriteLine();

            ConvertProcess(target, output, nameSpace, baseFlag, usingNameSpace);

        }

        private static void ConvertProcess(string target, string output, string nameSpace, bool baseFlag, string usingNameSpace)
        {
            var a = Directory.GetFiles("./target");
            if (a == null || a.Length <= 0)
            {
                Console.WriteLine("請將Model檔放入tartget資料夾 , 案任意鈕後繼續");

                Console.ReadKey();
            }

            a = Directory.GetFiles("./target");

            int targetCount = 0;
            List<string> ls = new List<string>();

            foreach (var b in a)
            {
                var c = b.ToLower();
                if (target != "Do")
                {
                    if (c.EndsWith(target.ToLower() + ".cs"))
                    {
                        ls.Add(b);
                        targetCount++;
                    }
                }
                else
                {
                    ls.Add(b);
                    targetCount++;
                }
            }

            Console.WriteLine("一共找到 " + targetCount + " 個檔案 , 確認後按任意鍵繼續");

            Console.ReadKey();

            bool nameSpaceFlag = !string.IsNullOrEmpty(nameSpace);

            int successCount = 0;

            if (!Directory.Exists("./output"))
            {
                Directory.CreateDirectory("./output");
            }

            //先轉並記錄className
            List<Tuple<string, string>> listClassNameTuple = new List<Tuple<string, string>>();
            foreach (var path in ls)
            {
                string className = string.Empty;
                string newClassName = string.Empty;

                className = path.Substring(path.LastIndexOf("\\") + 1, path.LastIndexOf(".cs") - path.LastIndexOf("\\") - 1);

                if (target == "Do")
                {
                    newClassName = className + output;
                }
                else
                {
                    newClassName = className.Substring(0, className.IndexOf(target)) + output + className.Substring(className.IndexOf(target) + target.Length);
                }

                listClassNameTuple.Add(new Tuple<string, string>(className, newClassName));
            }

            foreach (var path in ls)
            {
                string className = string.Empty;
                string newClassName = string.Empty;
                string program = File.ReadAllText(path);

                className = path.Substring(path.LastIndexOf("\\") + 1, path.LastIndexOf(".cs") - path.LastIndexOf("\\") - 1);

                if (target == "Do")
                {
                    newClassName = className + output;
                }
                else
                {
                    newClassName = className.Substring(0, className.IndexOf(target)) + output + className.Substring(className.IndexOf(target) + target.Length);
                }

                if (nameSpaceFlag)
                {
                    //program = program.Substring(0, program.IndexOf("namespace ")) + nameSpace + program.Substring(program.IndexOf("namespace ") + 10);

                    var nsIndex = program.IndexOf("namespace ") + 10;
                    string orgNs = program.Substring(nsIndex, program.IndexOf("\r\n", nsIndex) - nsIndex);

                    program = program.Substring(0, nsIndex) + nameSpace + program.Substring(program.IndexOf("\r\n", nsIndex));
                }

                if (baseFlag)
                {
                    //引用命名空間
                    string strUsingNs = "using " + usingNameSpace + ";\r\n";
                    program = strUsingNs + program;

                    var classIndex = program.IndexOf("class");
                    var classRowIndex = program.IndexOf("\r\n", classIndex);
                    string strBase = " : Base" + output;

                    program = program.Substring(0, classRowIndex) + strBase + program.Substring(classRowIndex);
                }

                program = program.Replace(className, newClassName);

                foreach (var x in listClassNameTuple)
                {
                    if (x.Item1 == className)
                    {
                        continue;
                    }

                    program = ReplaceInOrder(program, x.Item1, x.Item2, 0);

                }


                File.WriteAllText("./output/" + newClassName + ".cs", program);

                successCount++;
            }

            Console.WriteLine("轉換完成, 共轉換 " + successCount + " 個檔案");
        }

        private static string GetInputStr(string[] sa)
        {
            string inputStr = string.Empty;
            bool inloopFlag = true;
            do
            {
                Console.Write("請輸入");
                for (int i = 0; i < sa.Length; i++)
                {
                    Console.Write(sa[i]);
                    if (i + 1 < sa.Length)
                    {
                        Console.Write('或');
                    }
                }
                Console.WriteLine(" 請注意大小寫");

                //Console.WriteLine("請輸入Vo或Bo或Do 請注意大小寫");
                inputStr = Console.ReadLine();

                inloopFlag = !sa.Contains(inputStr);
            }
            while (
                string.IsNullOrEmpty(inputStr) ||
                inloopFlag
                );

            return inputStr;
        }

        public static string ReplaceInOrder(string text, string search, string replace, int startIndex)
        {
            int findIndex = text.IndexOf(search, startIndex);

            if (findIndex > -1)
            {
                //條件
                if (text[findIndex - 1] == '<')
                {
                    text = text.Substring(0, findIndex) + replace + text.Substring(findIndex + search.Length);
                }

                int nextFindIndex = text.IndexOf(search, findIndex + 1);
                if (nextFindIndex > -1)
                {
                    text = ReplaceInOrder(text, search, replace, findIndex + 1);
                }
            }

            return text;
        }
    }
}