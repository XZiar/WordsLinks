using System;
using Main.Util;
using System.Collections.Generic;
using System.Diagnostics;

namespace Main.Service
{
    public static class TestService
    {
        interface iface
        {
            string GetStr();
        }
        abstract class bclass : iface
        {
            public abstract string GetStr();
            public override int GetHashCode()
            {
                return GetStr().GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is bclass)
                    return GetStr() == (obj as bclass).GetStr();
                else
                    return GetHashCode() == obj.GetHashCode();
            }
        }
        class theclass : bclass
        {
            public string str;
            public int id;
            public override string GetStr() => str;
        }
        class tmpclass : bclass
        {
            public string str;
            public tmpclass(string name)
            {
                str = name;
            }
            public override string GetStr() => str;
        }
        static TestService()
        {
        }

        public static void Test()
        {
            Dictionary<bclass, int> dict = new Dictionary<bclass, int>();
            var obj1 = new theclass() { str = "text", id = 1 };
            var obj2 = new theclass() { str = "text", id = 2 };
            var obj3 = new theclass() { str = "str", id = 3 };
            dict.Add(obj1, obj1.id);
            dict.Add(obj3, obj3.id);
            Debug.WriteLine($"read obj2 : {dict[obj2]}");
            obj1.id = 10086;
            Debug.WriteLine($"read obj1 afterCHG : {dict[obj1]}");
            foreach (var o in dict)
            {
                Debug.WriteLine($"foreach afterCHG : ({o.Key.GetStr()},{(o.Key as theclass).id})->{o.Value}");
            }
            Debug.WriteLine($"read txt : {dict[new tmpclass("str")]}");
        }
    }
}
