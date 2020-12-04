using System;
using System.Reflection;
using UnityEngine;

namespace Utils {

    public static partial class Handy {

        public static T DuplicateComponent<T>(T original, GameObject destination) 
            where T : Component {

            Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in fields) {
                field.SetValue(copy, field.GetValue(original));
            }

            return copy as T;
        }
    }
}
