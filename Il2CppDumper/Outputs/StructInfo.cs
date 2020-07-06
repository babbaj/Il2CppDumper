using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Il2CppDumper
{
    public class StructInfo
    {
        public string TypeName;
        public bool IsValueType;
        [ObsoleteAttribute] public List<string> Parents = new List<string>(); // not removing to not have to change ScriptGenerator.cs (should probably just replace this)
        public string parent;
        public Il2CppTypeDefinition parentTypeDef;
        public Il2CppTypeDefinition typeDef;
        public Il2CppGenericContext context;
        public List<StructFieldInfo> Fields = new List<StructFieldInfo>();
        public List<StructFieldInfo> StaticFields = new List<StructFieldInfo>();
        public List<StructVTableMethodInfo> VTableMethod = new List<StructVTableMethodInfo>();
        public List<StructRGCTXInfo> RGCTXs = new List<StructRGCTXInfo>();
    }

    public class StructFieldInfo
    {
        public Il2CppType type;
        public string FieldTypeName;
        public string FieldName;
        public bool IsValueType;
    }

    public class StructVTableMethodInfo
    {
        public string MethodName;
    }

    public class StructRGCTXInfo
    {
        public Il2CppRGCTXDataType Type;
        public string TypeName;
        public string ClassName;
        public string MethodName;
    }
}
