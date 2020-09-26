using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Reflection;
using System.IO;

namespace Encryption
{
    //https://www.windowscentral.com/how-create-automated-task-using-task-scheduler-windows-10
    public class Program
    {
        public static void Main()
        {
            string code =
@"using System;
namespace Main{
	public static class Main
	{
		public static int Main(string[] args)
		{
			Console.WriteLine('H');
		}
	}
}";

            string[] sourceCode = { code };
            string key = "pW#WJSL7%_Bg(j_";

            string source = sourceCode[0];
            string encrypted = SAREncryption.Encrypt(source, key);

            Console.WriteLine(encrypted);

            Console.WriteLine(SAREncryption.Decrypt(encrypted, key));

            Console.ReadLine();
        }
    }

    public static class SAREncryption
    {
        public static string Encrypt(string source, string key)
        {
            string output = string.Empty;

            char[] chars = source.ToCharArray();
            char[] keyChars = key.ToCharArray();

            int[] encoded = new int[chars.Length];

            for (int i = 0, k = 0; i < chars.Length; i++)
            {
                if (k >= keyChars.Length)
                    k = 0;

                encoded[i] = (byte)chars[i] + (byte)keyChars[k];

                output += (char)encoded[i];

                k++;
            }

            output.Reverse();

            return output;
        }

        public static string Decrypt(string source, string key)
        {
            source.Reverse();

            string output = string.Empty;

            char[] chars = source.ToCharArray();
            char[] keyChars = key.ToCharArray();

            int[] decoded = new int[chars.Length];

            for (int i = 0, k = 0; i < chars.Length; i++)
            {
                if (k >= keyChars.Length)
                    k = 0;

                decoded[i] = (byte)chars[i] - (byte)keyChars[k];
                output += (char)decoded[i];

                k++;
            }

            return output;
        }

        public static void EncryptToFile(string source, string key, string path)
        {
            string encrypted = Encrypt(source, key);

            File.WriteAllText(encrypted, path);
        }

        public static string DecryptFromFile(string key, string path)
        {
            string source = File.ReadAllText(path);

            return Decrypt(source, key);
        }
    }
}