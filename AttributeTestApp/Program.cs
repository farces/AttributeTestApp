using System;
using System.Collections.Generic;
using System.Reflection;

namespace SampleAttributeUsage
{
    [AttributeUsage(AttributeTargets.Method)]
    class VersAttribute : Attribute
    {
        private readonly string _version;
        private readonly string _fieldname;

        public VersAttribute(string version, string name)
        {
            _version = version;
            _fieldname = name;
        }

        public string Version { get { return _version;  } }
        public string Name { get { return _fieldname;  } }
    }

    /* bare interface */
    interface IMyData
    {
        string Output();
    }

    class v5678 : IMyData
    {
        public string Name;
        public int X;
        public int Y;
        public v5678(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }

        public string Output()
        {
            return String.Format("VERSION 5678 : Name={0}, x={1}, y={2}", Name, X, Y);
        }
    }

    class v1234 : IMyData
    {
        public int X;
        public int Y;
        public int Z;
        public v1234(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public string Output()
        {
            return String.Format("VERSION 1234 : x={0}, y={1}, z={2}", X, Y, Z);
        }
    }

    class Program
    {
        static readonly Dictionary<int,Dictionary<string,MethodInfo>> MethodDictionary = new Dictionary<int,Dictionary<string,MethodInfo>>();

        static void Main(string[] args)
        {
            const string input1 = "1234";
            const string input2 = "5678";
            const string input3 = "5555"; //unimplemented version

            LoadMethods();

            var data1 = ProcessInput(input1);
            if (data1 != null)
            { 
                Console.WriteLine(data1.Output());
            }
            var data2 = ProcessInput(input2);
            if (data2 != null)
            {
                Console.WriteLine(data2.Output());
            }
            var data3 = ProcessInput(input3);
            if (data3 != null)
            {
                Console.WriteLine(data3.Output());
            }

            Console.ReadLine();
        }

        /* Loads test data for version 1234 */
        [Vers("1234","Load")]
        private static IMyData LoadData1234()
        {
            Console.WriteLine("Loading Data: Version 1234");
            return new v1234(500,700,2);
        }

        /* Loads test data for version 5678 */
        [Vers("5678","Load")]
        private static IMyData LoadData5678()
        {
            Console.WriteLine("Loading Data: Version 5678");
            return new v5678("Test Object",9999,80);
        }

        /* load all methods with a Vers attribute, their name (function), and version number 
         * into a Dictionary that can be efficiently accessed when the data version changes */
        private static void LoadMethods()
        {
            foreach (MethodInfo method in (typeof (Program)).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
            {
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is VersAttribute)
                    {
                        var fullattr = (VersAttribute) attribute;
                        int version = Convert.ToInt32(fullattr.Version);
                        if (!MethodDictionary.ContainsKey(version))
                           MethodDictionary.Add(version,new Dictionary<string, MethodInfo>());
                        MethodDictionary[version].Add(fullattr.Name, method);
                    }
                }
            }
        }

        /* loads data set using the methods defined for the version number provided.
         * currently is just dummy code that does nothing but check version and finds the
         * correct method to load it into an IMyData compatible class */
        private static IMyData ProcessInput(string version)
        {
            MethodInfo loadMethod = null;
            try
            {
                loadMethod = MethodDictionary[Convert.ToInt32(version)]["Load"];
            } catch (KeyNotFoundException)
            {
                Console.WriteLine(String.Format("No Method Found for {0}", version));
                return null;
            }

            return (IMyData)loadMethod.Invoke(null, new object[] {});
        }       
    }
}
