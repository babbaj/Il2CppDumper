using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Il2CppDumper.Il2CppConstants;

namespace Il2CppDumper
{
    
    // pretty bad for merging upstream changes 
    public class CoolHeaderGenerator
    {
        private Il2CppExecutor executor;
        private Metadata metadata;
        private Il2Cpp il2Cpp;
        private Dictionary<Il2CppTypeDefinition, int> typeDefImageIndices = new Dictionary<Il2CppTypeDefinition, int>();
        private HashSet<string> structNameHashSet = new HashSet<string>(StringComparer.Ordinal);
        private List<StructInfo> structInfoList = new List<StructInfo>();
        private Dictionary<string, StructInfo> structInfoWithStructName = new Dictionary<string, StructInfo>();
        private Dictionary<Il2CppTypeDefinition, string> structNameDic = new Dictionary<Il2CppTypeDefinition, string>();
        private Dictionary<ulong, string> genericClassStructNameDic = new Dictionary<ulong, string>();
        private Dictionary<string, Il2CppType> nameGenericClassDic = new Dictionary<string, Il2CppType>();
        private List<ulong> genericClassList = new List<ulong>();

        // TODO: these shouldnt be here
        private StringBuilder arrayClassPreHeader = new StringBuilder();
        private StringBuilder arrayClassHeader = new StringBuilder();
        private static HashSet<string> keyword = new HashSet<string>(StringComparer.Ordinal)
        { "klass", "monitor", "register", "_cs", "auto", "friend", "template", "near", "far", "flat", "default", "_ds", "interrupt", "inline", "unsigned", "signed"};

        public CoolHeaderGenerator(Il2CppExecutor il2CppExecutor)
        {
            executor = il2CppExecutor;
            metadata = il2CppExecutor.metadata;
            il2Cpp = il2CppExecutor.il2Cpp;
            initializeState();
        }

        public void WriteHeader(string outputDir)
        {
            File.WriteAllText(outputDir + "il2cpp.h", generateHeader());
        }

        string generateHeader()
        {
            StringBuilder sb = new StringBuilder();

            var preHeader = new StringBuilder();
            var headerStructs = new StringBuilder();

            foreach (StructInfo info in structInfoList) 
            {
                preHeader.Append($"struct {info.TypeName}_o;\n");
            }


            IEnumerable<StructInfo> valueTypes = structInfoList.Where(info => info.IsValueType); // TODO: this needs to be sorted
            IEnumerable<StructInfo> classes = structInfoList.Where(info => !info.IsValueType).OrderBy(info => info.typeDef, Comparer<Il2CppTypeDefinition>.Create(orderByInheritance));

            foreach(StructInfo info in valueTypes)
            {
                headerStructs.Append(RecursiveDefineValueTypes(info));
            }

            foreach (StructInfo info in classes)
            {
                headerStructs.Append(DefineStruct(info));
            }
            

            sb.Append(head());
            sb.Append(arrayClassPreHeader);
            sb.Append(preHeader);
            sb.Append(headerStructs);
            sb.Append(arrayClassHeader);
            sb.Append(tail());
            return sb.ToString();
        }

        public static string pointer(string type)
        {
            return $"::pointer<{type}>";
        }

        // code at th top of the header
        string head()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#include <cstdint>\n");
            sb.Append("#include \"pointer.hpp\"\n");
            sb.Append('\n');
            sb.Append("namespace rust {\n");

            // copy/pasted
            sb.Append(HeaderConstants2.GenericHeader);
            switch (il2Cpp.Version)
            {
                case 22f:
                    sb.Append(HeaderConstants2.HeaderV22);
                    break;
                case 23f:
                case 24f:
                    sb.Append(HeaderConstants2.HeaderV240);
                    break;
                case 24.1f:
                    sb.Append(HeaderConstants2.HeaderV241);
                    break;
                case 24.2f:
                case 24.3f:
                    sb.Append(HeaderConstants2.HeaderV242);
                    break;
                default:
                    throw new Exception($"This il2cpp version [{il2Cpp.Version}] does not support generating .h files");
            }


            return sb.ToString();
        }

        string tail()
        {
            return "}\n"; // namespace rust
        }


        void initializeState()
        {
            // 生成唯一名称 (Generate a unique name)
            for (var imageIndex = 0; imageIndex < metadata.imageDefs.Length; imageIndex++)
            {
                var imageDef = metadata.imageDefs[imageIndex];
                var typeEnd = imageDef.typeStart + imageDef.typeCount;
                for (int typeIndex = imageDef.typeStart; typeIndex < typeEnd; typeIndex++)
                {
                    var typeDef = metadata.typeDefs[typeIndex];
                    typeDefImageIndices.Add(typeDef, imageIndex);
                    CreateStructNameDic(typeDef);
                }
            }

            // 生成后面处理泛型实例要用到的字典 (Generate a dictionary to be used later for processing generic instances)
            foreach (var il2CppType in il2Cpp.types.Where(x => x.type == Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST))
            {
                var genericClass = il2Cpp.MapVATR<Il2CppGenericClass>(il2CppType.data.generic_class);
                if (genericClass.typeDefinitionIndex == 4294967295 || genericClass.typeDefinitionIndex == -1)
                {
                    continue;
                }
                var typeDef = metadata.typeDefs[genericClass.typeDefinitionIndex];
                var typeBaseName = structNameDic[typeDef];
                var typeToReplaceName = FixName(executor.GetTypeDefName(typeDef, true, true));
                var typeReplaceName = FixName(executor.GetTypeName(il2CppType, true, false));
                var typeStructName = typeBaseName.Replace(typeToReplaceName, typeReplaceName);
                nameGenericClassDic[typeStructName] = il2CppType;
                genericClassStructNameDic[il2CppType.data.generic_class] = typeStructName;
            }

            // 处理函数 (Handler function)
            for (var imageIndex = 0; imageIndex < metadata.imageDefs.Length; imageIndex++)
            {
                var imageDef = metadata.imageDefs[imageIndex];
                var typeEnd = imageDef.typeStart + imageDef.typeCount;
                for (int typeIndex = imageDef.typeStart; typeIndex < typeEnd; typeIndex++)
                {
                    var typeDef = metadata.typeDefs[typeIndex];
                    AddStruct(typeDef);
                }
            }


            // I think this is header stuff
            for (int i = 0; i < genericClassList.Count; i++)
            {
                var pointer = genericClassList[i];
                AddGenericClassStruct(pointer);
            }
            foreach (var info in structInfoList)
            {
                structInfoWithStructName.Add(info.TypeName + "_o", info);
            }
        }


        Il2CppTypeDefinition getParent(Il2CppTypeDefinition typeDef)
        {
            if (typeDef.parentIndex < 0) return null;
            var parent = il2Cpp.types[typeDef.parentIndex];
            return GetTypeDefinition(parent);
        }

        // returns the given type and its parents
        IEnumerable<Il2CppTypeDefinition> parents(Il2CppTypeDefinition typeDef)
        {
            for (var parent = typeDef; parent != null; parent = getParent(parent))
            {
                yield return parent;
            }
        }

        IEnumerable<Il2CppTypeDefinition> parents(StructInfo info)
        {
            return parents(info.typeDef);
        }

        int orderByInheritance(Il2CppTypeDefinition a, Il2CppTypeDefinition b)
        {
            if (parents(a).Contains(b)) return 1;
            else if (parents(b).Contains(a)) return -1;
            else return 0;
        }


        ulong? offsetOfClass(Il2CppTypeDefinition def)
        {
            foreach (var i in metadata.metadataUsageDic[1]) //kIl2CppMetadataUsageTypeInfo
            {
                if (i.Value == def.elementTypeIndex)
                {
                    return il2Cpp.GetRVA(il2Cpp.metadataUsages[i.Key]);
                }
            }
            return null;
        }

        private string DefineStruct(StructInfo info)
        {
            StringBuilder sb = new StringBuilder();
            if (!info.IsValueType)
            {
                sb.Append(StaticFieldsStruct(info));
                sb.Append(VTableStruct(info));
                sb.Append(ClassStruct(info));
                sb.Append(ObjectStruct(info));
            }
            else
            {
                // Value types need to be ordered differently because static_fields can have instances of it
                sb.Append(ObjectStruct(info));
                sb.Append(StaticFieldsStruct(info));
                sb.Append(VTableStruct(info));
                sb.Append(ClassStruct(info));
            }
            
            sb.Append('\n');

            return sb.ToString();
        }

        private string RecursiveDefineValueTypes(StructInfo info)
        {
            return RecursiveDefineValueTypes(info, new HashSet<StructInfo>());
        }
        private string RecursiveDefineValueTypes(StructInfo info, HashSet<StructInfo> structCache)
        {
            if (!structCache.Add(info)) return string.Empty;

            var pre = new StringBuilder();

            foreach (var field in info.Fields)
            {
                if (field.IsValueType)
                {
                    var fieldInfo = structInfoWithStructName[field.FieldTypeName];
                    pre.Append(RecursiveDefineValueTypes(fieldInfo, structCache));
                }
            }
            foreach (var field in info.StaticFields)
            {
                if (field.IsValueType)
                {
                    var fieldInfo = structInfoWithStructName[field.FieldTypeName];
                    pre.Append(RecursiveDefineValueTypes(fieldInfo, structCache));
                }
            }

            return pre.Append(DefineStruct(info)).ToString();
        }

        // _c
        private string ClassStruct(StructInfo info)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"struct {info.TypeName}_c {{\n");
            var address = offsetOfClass(info.typeDef);
            if (address != null)
            {
                var offset = offsetOfClass(info.typeDef);
                sb.Append($"\tstatic constexpr auto offset = 0x{offset:X};\n");
            }
            sb.Append(
                $"\tIl2CppClass_1 _1;\n" +
                $"\t{pointer($"{info.TypeName}_StaticFields")} static_fields;\n" +
                $"\t{pointer("void")} rgctx_data;\n" + //$"\t{pointer($"{info.TypeName}_RGCTXs")} rgctx_data;\n" +
                $"\tIl2CppClass_2 _2;\n" +
                $"\t{info.TypeName}_VTable vtable;\n" +
                $"}};\n"
                );

            return sb.ToString();
        }

        // _o
        // TODO: inherit from parent and only list fields for this class
        private string ObjectStruct(StructInfo info)
        {
            StringBuilder sb = new StringBuilder();
           
            //sb.Append($"struct {info.TypeName}_o : {structNameDic[info.parentTypeDef]} {{\n");
            if (info.parentTypeDef != null) {
                // This is a subclass
                sb.Append($"struct {info.TypeName}_o : {structNameDic[info.parentTypeDef] + "_o"} {{\n");
            } else {
                // This is a base class
                sb.Append($"struct {info.TypeName}_o {{\n");
                if (!info.IsValueType)
                {
                    sb.Append($"\t{pointer($"{info.TypeName}_c")} klass;\n");
                    sb.Append($"\t{pointer("void")} monitor;\n");
                }
            }
            

            foreach (var field in info.Fields)
            {
                sb.Append($"\t{field.FieldTypeName} {field.FieldName};\n");
            }
            sb.Append("};\n");

            return sb.ToString();
        }

        // _StaticFields
        private string StaticFieldsStruct(StructInfo info)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"struct {info.TypeName}_StaticFields {{\n");
            foreach (var field in info.StaticFields)
            {
                sb.Append($"\t{field.FieldTypeName} {field.FieldName};\n");
            }
            sb.Append("};\n");

            return sb.ToString();
        }

        // _VTable
        private string VTableStruct(StructInfo info)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"struct {info.TypeName}_VTable {{\n");
            foreach (var method in info.VTableMethod)
            {
                sb.Append($"\tVirtualInvokeData {method.MethodName};\n");
            }
            sb.Append("};\n");

            return sb.ToString();
        }

        // everything below here is copy/pasted


        private static string FixName(string str)
        {
            if (keyword.Contains(str))
            {
                str = "_" + str;
            }
            if (Regex.IsMatch(str, "^[0-9]"))
            {
                return "_" + str;
            }
            else
            {
                return Regex.Replace(str, "[^a-zA-Z0-9_]", "_");
            }
        }

        private string ParseType(Il2CppType il2CppType, Il2CppGenericContext context = null)
        {
            switch (il2CppType.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
                    return "void";
                case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                    return "bool";
                case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
                    return "uint16_t"; //Il2CppChar
                case Il2CppTypeEnum.IL2CPP_TYPE_I1:
                    return "int8_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                    return "uint8_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                    return "int16_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                    return "uint16_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                    return "int32_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                    return "uint32_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                    return "int64_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                    return "uint64_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_R4:
                    return "float";
                case Il2CppTypeEnum.IL2CPP_TYPE_R8:
                    return "double";
                case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                    return pointer("System_String_o");
                case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
                    {
                        var oriType = il2Cpp.GetIl2CppType(il2CppType.data.type);
                        return pointer(ParseType(oriType));
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    {
                        var typeDef = metadata.typeDefs[il2CppType.data.klassIndex];
                        if (typeDef.IsEnum)
                        {
                            return ParseType(il2Cpp.types[typeDef.elementTypeIndex]);
                        }
                        return structNameDic[typeDef] + "_o";
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
                    {
                        var typeDef = metadata.typeDefs[il2CppType.data.klassIndex];
                        return pointer(structNameDic[typeDef] + "_o");
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.class_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return ParseType(type);
                        }
                        return pointer("Il2CppObject");
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
                    {
                        var arrayType = il2Cpp.MapVATR<Il2CppArrayType>(il2CppType.data.array);
                        var elementType = il2Cpp.GetIl2CppType(arrayType.etype);
                        var elementStructName = GetIl2CppStructName(elementType, context);
                        var typeStructName = elementStructName + "_array";
                        if (structNameHashSet.Add(typeStructName))
                        {
                            ParseArrayClassStruct(elementType, context);
                        }
                        return pointer(typeStructName);
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    {
                        var genericClass = il2Cpp.MapVATR<Il2CppGenericClass>(il2CppType.data.generic_class);
                        var typeDef = metadata.typeDefs[genericClass.typeDefinitionIndex];
                        var typeStructName = genericClassStructNameDic[il2CppType.data.generic_class];
                        if (structNameHashSet.Add(typeStructName))
                        {
                            genericClassList.Add(il2CppType.data.generic_class);
                        }
                        if (typeDef.IsValueType)
                        {
                            if (typeDef.IsEnum)
                            {
                                return ParseType(il2Cpp.types[typeDef.elementTypeIndex]);
                            }
                            return typeStructName + "_o";
                        }
                        return pointer(typeStructName + "_o");
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
                    return pointer("Il2CppObject");
                case Il2CppTypeEnum.IL2CPP_TYPE_I:
                    return "intptr_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_U:
                    return "uintptr_t";
                case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
                    return pointer("Il2CppObject");
                case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
                    {
                        var elementType = il2Cpp.GetIl2CppType(il2CppType.data.type);
                        var elementStructName = GetIl2CppStructName(elementType, context);
                        var typeStructName = elementStructName + "_array";
                        if (structNameHashSet.Add(typeStructName))
                        {
                            ParseArrayClassStruct(elementType, context);
                        }
                        return pointer(typeStructName);
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.method_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return ParseType(type);
                        }
                        return pointer("Il2CppObject");
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private void AddStruct(Il2CppTypeDefinition typeDef)
        {
            var structInfo = new StructInfo();
            structInfoList.Add(structInfo);
            structInfo.TypeName = structNameDic[typeDef];
            structInfo.IsValueType = typeDef.IsValueType;
            SetParent(typeDef, structInfo);
            AddFields(typeDef, structInfo, null);
            AddVTableMethod(structInfo, typeDef);
            AddRGCTX(structInfo, typeDef);
            structInfo.typeDef = typeDef;
        }

        private void AddGenericClassStruct(ulong pointer)
        {
            var genericClass = il2Cpp.MapVATR<Il2CppGenericClass>(pointer);
            var typeDef = metadata.typeDefs[genericClass.typeDefinitionIndex];
            var structInfo = new StructInfo();
            structInfoList.Add(structInfo);
            structInfo.TypeName = genericClassStructNameDic[pointer];
            structInfo.IsValueType = typeDef.IsValueType;
            SetParent(typeDef, structInfo);
            AddFields(typeDef, structInfo, genericClass.context);
            AddVTableMethod(structInfo, typeDef);
            structInfo.typeDef = typeDef;
        }

        private void SetParent(Il2CppTypeDefinition typeDef, StructInfo structInfo)
        {
            if (!typeDef.IsValueType && !typeDef.IsEnum)
            {
                if (typeDef.parentIndex >= 0)
                {
                    var parent = il2Cpp.types[typeDef.parentIndex];
                    var parentDef = GetTypeDefinition(parent);
                    if (parentDef != null)
                    {
                        SetParent(parentDef, structInfo);
                        if (parentDef.field_count > 0) // Not sure if this should be here
                        {
                            var fieldEnd = parentDef.fieldStart + parentDef.field_count;
                            for (var i = parentDef.fieldStart; i < fieldEnd; ++i)
                            {
                                var fieldDef = metadata.fieldDefs[i];
                                var fieldType = il2Cpp.types[fieldDef.typeIndex];
                                if ((fieldType.attrs & FIELD_ATTRIBUTE_LITERAL) == 0 && (fieldType.attrs & FIELD_ATTRIBUTE_STATIC) == 0)
                                {
                                    structInfo.parent = GetIl2CppStructName(parent);
                                    structInfo.parentTypeDef = parentDef;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


        private void AddFields(Il2CppTypeDefinition typeDef, StructInfo structInfo, Il2CppGenericContext context)
        {
            if (typeDef.field_count > 0)
            {
                var fieldEnd = typeDef.fieldStart + typeDef.field_count;
                for (var i = typeDef.fieldStart; i < fieldEnd; ++i)
                {
                    var fieldDef = metadata.fieldDefs[i];
                    var fieldType = il2Cpp.types[fieldDef.typeIndex];
                    if ((fieldType.attrs & FIELD_ATTRIBUTE_LITERAL) != 0)
                    {
                        continue;
                    }
                    var structFieldInfo = new StructFieldInfo();
                    structFieldInfo.FieldTypeName = ParseType(fieldType, context);
                    var fieldName = FixName(metadata.GetStringFromIndex(fieldDef.nameIndex));
                    structFieldInfo.FieldName = fieldName;
                    structFieldInfo.IsValueType = IsValueType(fieldType, context);
                    if ((fieldType.attrs & FIELD_ATTRIBUTE_STATIC) != 0)
                    {
                        structInfo.StaticFields.Add(structFieldInfo);
                    }
                    else
                    {
                        structInfo.Fields.Add(structFieldInfo);
                    }
                }
            }
        }

        private void AddVTableMethod(StructInfo structInfo, Il2CppTypeDefinition typeDef)
        {
            var dic = new SortedDictionary<int, Il2CppMethodDefinition>();
            for (int i = 0; i < typeDef.vtable_count; i++)
            {
                var vTableIndex = typeDef.vtableStart + i;
                var encodedMethodIndex = metadata.vtableMethods[vTableIndex];
                var usage = metadata.GetEncodedIndexType(encodedMethodIndex);
                var index = metadata.GetDecodedMethodIndex(encodedMethodIndex);
                Il2CppMethodDefinition methodDef;
                if (usage == 6) //kIl2CppMetadataUsageMethodRef
                {
                    var methodSpec = il2Cpp.methodSpecs[index];
                    methodDef = metadata.methodDefs[methodSpec.methodDefinitionIndex];
                }
                else
                {
                    methodDef = metadata.methodDefs[index];
                }
                dic[methodDef.slot] = methodDef;
            }
            foreach (var i in dic)
            {
                var methodInfo = new StructVTableMethodInfo();
                structInfo.VTableMethod.Add(methodInfo);
                var methodDef = i.Value;
                methodInfo.MethodName = $"_{methodDef.slot}_{FixName(metadata.GetStringFromIndex(methodDef.nameIndex))}";
            }
        }

        private void AddRGCTX(StructInfo structInfo, Il2CppTypeDefinition typeDef)
        {
            var imageIndex = typeDefImageIndices[typeDef];
            var collection = executor.GetTypeRGCTXDefinition(typeDef, imageIndex);
            if (collection != null)
            {
                foreach (var definitionData in collection)
                {
                    var structRGCTXInfo = new StructRGCTXInfo();
                    structInfo.RGCTXs.Add(structRGCTXInfo);
                    structRGCTXInfo.Type = definitionData.type;
                    switch (definitionData.type)
                    {
                        case Il2CppRGCTXDataType.IL2CPP_RGCTX_DATA_TYPE:
                            {
                                var il2CppType = il2Cpp.types[definitionData.data.typeIndex];
                                structRGCTXInfo.TypeName = FixName(executor.GetTypeName(il2CppType, true, false));
                                break;
                            }
                        case Il2CppRGCTXDataType.IL2CPP_RGCTX_DATA_CLASS:
                            {
                                var il2CppType = il2Cpp.types[definitionData.data.typeIndex];
                                structRGCTXInfo.ClassName = FixName(executor.GetTypeName(il2CppType, true, false));
                                break;
                            }
                        case Il2CppRGCTXDataType.IL2CPP_RGCTX_DATA_METHOD:
                            {
                                var methodSpec = il2Cpp.methodSpecs[definitionData.data.methodIndex];
                                (var methodSpecTypeName, var methodSpecMethodName) = executor.GetMethodSpecName(methodSpec, true);
                                structRGCTXInfo.MethodName = FixName(methodSpecTypeName + "." + methodSpecMethodName);
                                break;
                            }
                    }
                }
            }
        }

        private void ParseArrayClassStruct(Il2CppType il2CppType, Il2CppGenericContext context)
        {
            var structName = GetIl2CppStructName(il2CppType, context);
            arrayClassPreHeader.Append($"struct {structName}_array;\n");
            arrayClassHeader.Append($"struct {structName}_array {{\n" +
                $"\tIl2CppObject obj;\n" +
                $"\t{pointer("Il2CppArrayBounds")} bounds;\n" +
                $"\til2cpp_array_size_t max_length;\n" +
                $"\t{ParseType(il2CppType, context)} m_Items[65535];\n" +
                $"}};\n");
        }

        private Il2CppTypeDefinition GetTypeDefinition(Il2CppType il2CppType)
        {
            switch (il2CppType.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    return metadata.typeDefs[il2CppType.data.klassIndex];
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    var genericClass = il2Cpp.MapVATR<Il2CppGenericClass>(il2CppType.data.generic_class);
                    return metadata.typeDefs[genericClass.typeDefinitionIndex];
                case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
                    return null;
                default:
                    throw new NotSupportedException();
            }
        }

        private void CreateStructNameDic(Il2CppTypeDefinition typeDef)
        {
            var typeName = executor.GetTypeDefName(typeDef, true, true);
            var typeStructName = FixName(typeName);
            var uniqueName = GetUniqueName(typeStructName);
            structNameDic.Add(typeDef, uniqueName);
        }

        private string GetUniqueName(string name)
        {
            var fixName = name;
            int i = 1;
            while (!structNameHashSet.Add(fixName))
            {
                fixName = $"{name}_{i++}";
            }
            return fixName;
        }

        

        private string GetIl2CppStructName(Il2CppType il2CppType, Il2CppGenericContext context = null)
        {
            switch (il2CppType.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_VOID:
                case Il2CppTypeEnum.IL2CPP_TYPE_BOOLEAN:
                case Il2CppTypeEnum.IL2CPP_TYPE_CHAR:
                case Il2CppTypeEnum.IL2CPP_TYPE_I1:
                case Il2CppTypeEnum.IL2CPP_TYPE_U1:
                case Il2CppTypeEnum.IL2CPP_TYPE_I2:
                case Il2CppTypeEnum.IL2CPP_TYPE_U2:
                case Il2CppTypeEnum.IL2CPP_TYPE_I4:
                case Il2CppTypeEnum.IL2CPP_TYPE_U4:
                case Il2CppTypeEnum.IL2CPP_TYPE_I8:
                case Il2CppTypeEnum.IL2CPP_TYPE_U8:
                case Il2CppTypeEnum.IL2CPP_TYPE_R4:
                case Il2CppTypeEnum.IL2CPP_TYPE_R8:
                case Il2CppTypeEnum.IL2CPP_TYPE_STRING:
                case Il2CppTypeEnum.IL2CPP_TYPE_TYPEDBYREF:
                case Il2CppTypeEnum.IL2CPP_TYPE_I:
                case Il2CppTypeEnum.IL2CPP_TYPE_U:
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                case Il2CppTypeEnum.IL2CPP_TYPE_CLASS:
                case Il2CppTypeEnum.IL2CPP_TYPE_OBJECT:
                    {
                        var typeDef = metadata.typeDefs[il2CppType.data.klassIndex];
                        return structNameDic[typeDef];
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_PTR:
                    {
                        var oriType = il2Cpp.GetIl2CppType(il2CppType.data.type);
                        return GetIl2CppStructName(oriType);
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_ARRAY:
                    {
                        var arrayType = il2Cpp.MapVATR<Il2CppArrayType>(il2CppType.data.array);
                        var elementType = il2Cpp.GetIl2CppType(arrayType.etype);
                        var elementStructName = GetIl2CppStructName(elementType, context);
                        var typeStructName = elementStructName + "_array";
                        if (structNameHashSet.Add(typeStructName))
                        {
                            ParseArrayClassStruct(elementType, context);
                        }
                        return typeStructName;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_SZARRAY:
                    {
                        var elementType = il2Cpp.GetIl2CppType(il2CppType.data.type);
                        var elementStructName = GetIl2CppStructName(elementType, context);
                        var typeStructName = elementStructName + "_array";
                        if (structNameHashSet.Add(typeStructName))
                        {
                            ParseArrayClassStruct(elementType, context);
                        }
                        return typeStructName;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    {
                        var typeStructName = genericClassStructNameDic[il2CppType.data.generic_class];
                        if (structNameHashSet.Add(typeStructName))
                        {
                            genericClassList.Add(il2CppType.data.generic_class);
                        }
                        return typeStructName;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.class_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return GetIl2CppStructName(type);
                        }
                        return "System_Object";
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.method_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return GetIl2CppStructName(type);
                        }
                        return "System_Object";
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private bool IsValueType(Il2CppType il2CppType, Il2CppGenericContext context)
        {
            switch (il2CppType.type)
            {
                case Il2CppTypeEnum.IL2CPP_TYPE_VALUETYPE:
                    {
                        var typeDef = metadata.typeDefs[il2CppType.data.klassIndex];
                        return !typeDef.IsEnum;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_GENERICINST:
                    {
                        var genericClass = il2Cpp.MapVATR<Il2CppGenericClass>(il2CppType.data.generic_class);
                        var typeDef = metadata.typeDefs[genericClass.typeDefinitionIndex];
                        return typeDef.IsValueType && !typeDef.IsEnum;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_VAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.class_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return IsValueType(type, null);
                        }
                        return false;
                    }
                case Il2CppTypeEnum.IL2CPP_TYPE_MVAR:
                    {
                        if (context != null)
                        {
                            var genericParameter = metadata.genericParameters[il2CppType.data.genericParameterIndex];
                            var genericInst = il2Cpp.MapVATR<Il2CppGenericInst>(context.method_inst);
                            var pointers = il2Cpp.MapVATR<ulong>(genericInst.type_argv, genericInst.type_argc);
                            var pointer = pointers[genericParameter.num];
                            var type = il2Cpp.GetIl2CppType(pointer);
                            return IsValueType(type, null);
                        }
                        return false;
                    }
                default:
                    return false;
            }
        }

        
    }
}
