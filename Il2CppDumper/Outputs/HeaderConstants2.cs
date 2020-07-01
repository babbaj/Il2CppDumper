//using static Il2CppDumper.CoolHeaderGenerator;

namespace Il2CppDumper
{
    // should probably just edit the original
    public static class HeaderConstants2
    {
        public readonly static string GenericHeader =
//@"typedef void(*Il2CppMethodPointer)();
@"using Il2CppMethodPointer = ::pointer<void()>;

struct MethodInfo;

struct VirtualInvokeData
{
    Il2CppMethodPointer methodPtr;
    ::pointer<const MethodInfo> method;
};

struct Il2CppType
{
    ::pointer<void> data;
    unsigned int bits;
};

struct Il2CppClass;

struct Il2CppObject
{
    ::pointer<Il2CppClass> klass;
    ::pointer<void> monitor;
};

union Il2CppRGCTXData
{
    ::pointer<void> rgctxDataDummy;
    ::pointer<const MethodInfo> method;
    ::pointer<const Il2CppType> type;
    ::pointer<Il2CppClass> klass;
};

";

        public readonly static string HeaderV242 =
@"struct Il2CppClass_1
{
    ::pointer<void> image;
    ::pointer<void> gc_desc;
    ::pointer<const char> name;
    ::pointer<const char> namespaze;
    Il2CppType byval_arg;
    Il2CppType this_arg;
    ::pointer<Il2CppClass> element_class;
    ::pointer<Il2CppClass> castClass;
    ::pointer<Il2CppClass> declaringType;
    ::pointer<Il2CppClass> parent;
    ::pointer<void> generic_class;
    ::pointer<void> typeDefinition;
    ::pointer<void> interopData;
    ::pointer<Il2CppClass> klass;
    ::pointer<void> fields;
    ::pointer<void> events;
    ::pointer<void> properties;
    ::pointer<void> methods;
    ::pointer<::pointer<Il2CppClass>> nestedTypes;
    ::pointer<::pointer<Il2CppClass>> implementedInterfaces;
    ::pointer<void> interfaceOffsets;
};

struct Il2CppClass_2
{
    ::pointer<::pointer<Il2CppClass>> typeHierarchy;
    ::pointer<void> unity_user_data;
    uint32_t initializationExceptionGCHandle;
    uint32_t cctor_started;
    uint32_t cctor_finished;
    size_t cctor_thread;
    int32_t genericContainerIndex;
    uint32_t instance_size;
    uint32_t actualSize;
    uint32_t element_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
    int32_t thread_static_fields_offset;
    uint32_t flags;
    uint32_t token;
    uint16_t method_count;
    uint16_t property_count;
    uint16_t field_count;
    uint16_t event_count;
    uint16_t nested_type_count;
    uint16_t vtable_count;
    uint16_t interfaces_count;
    uint16_t interface_offsets_count;
    uint8_t typeHierarchyDepth;
    uint8_t genericRecursionDepth;
    uint8_t rank;
    uint8_t minimumAlignment;
    uint8_t naturalAligment;
    uint8_t packingSize;
    uint8_t bitflags1;
    uint8_t bitflags2;
};

struct Il2CppClass
{
    Il2CppClass_1 _1;
    ::pointer<void> static_fields;
    ::pointer<Il2CppRGCTXData> rgctx_data;
    Il2CppClass_2 _2;
    VirtualInvokeData vtable[255];
};

typedef uintptr_t il2cpp_array_size_t;
typedef int32_t il2cpp_array_lower_bound_t;
struct Il2CppArrayBounds
{
    il2cpp_array_size_t length;
    il2cpp_array_lower_bound_t lower_bound;
};

struct MethodInfo
{
    Il2CppMethodPointer methodPointer;
    ::pointer<void> invoker_method;
    ::pointer<const char> name;
    ::pointer<Il2CppClass> klass;
    ::pointer<const Il2CppType> return_type;
    ::pointer<const void> parameters;
    union
    {
        ::pointer<const Il2CppRGCTXData> rgctx_data;
        ::pointer<const void> methodDefinition;
    };
    union
    {
        ::pointer<const void> genericMethod;
        ::pointer<const void> genericContainer;
    };
    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint8_t parameters_count;
    uint8_t bitflags;
};

";

        public readonly static string HeaderV241 =
@"struct Il2CppClass_1
{
    ::pointer<void> image;
    ::pointer<void> gc_desc;
    ::pointer<const char> name;
    ::pointer<const char> namespaze;
    Il2CppType byval_arg;
    Il2CppType this_arg;
    ::pointer<Il2CppClass> element_class;
    ::pointer<Il2CppClass> castClass;
    ::pointer<Il2CppClass> declaringType;
    ::pointer<Il2CppClass> parent;
    ::pointer<void> generic_class;
    ::pointer<void> typeDefinition;
    ::pointer<void> interopData;
    Il2CppClass> klass;
    ::pointer<void> fields;
    ::pointer<void> events;
    ::pointer<void> properties;
    ::pointer<void> methods;
    ::pointer<::pointer<Il2CppClass>> nestedTypes;
    ::pointer<::pointer<Il2CppClass>> implementedInterfaces;
    ::pointer<void> interfaceOffsets;
};

struct Il2CppClass_2
{
    ::pointer<::pointer<Il2CppClass>> typeHierarchy;
    uint32_t initializationExceptionGCHandle;
    uint32_t cctor_started;
    uint32_t cctor_finished;
    uint64_t cctor_thread;
    int32_t genericContainerIndex;
    uint32_t instance_size;
    uint32_t actualSize;
    uint32_t element_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
    int32_t thread_static_fields_offset;
    uint32_t flags;
    uint32_t token;
    uint16_t method_count;
    uint16_t property_count;
    uint16_t field_count;
    uint16_t event_count;
    uint16_t nested_type_count;
    uint16_t vtable_count;
    uint16_t interfaces_count;
    uint16_t interface_offsets_count;
    uint8_t typeHierarchyDepth;
    uint8_t genericRecursionDepth;
    uint8_t rank;
    uint8_t minimumAlignment;
    uint8_t naturalAligment;
    uint8_t packingSize;
    uint8_t bitflags1;
    uint8_t bitflags2;
};

struct Il2CppClass
{
    Il2CppClass_1 _1;
    ::pointer<void> static_fields;
    ::pointer<Il2CppRGCTXData> rgctx_data;
    Il2CppClass_2 _2;
    VirtualInvokeData vtable[255];
};

typedef uintptr_t il2cpp_array_size_t;
typedef int32_t il2cpp_array_lower_bound_t;
struct Il2CppArrayBounds
{
    il2cpp_array_size_t length;
    il2cpp_array_lower_bound_t lower_bound;
};

struct MethodInfo
{
    Il2CppMethodPointer methodPointer;
    void> invoker_method;
    ::pointer<const char> name;
    ::pointer<Il2CppClass> klass;
    ::pointer<const Il2CppType> return_type;
    ::pointer<const void> parameters;
    union
    {
        ::pointer<const Il2CppRGCTXData> rgctx_data;
        ::pointer<const void> methodDefinition;
    };
    union
    {
        ::pointer<const void> genericMethod;
        ::pointer<const void> genericContainer;
    };
    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint8_t parameters_count;
    uint8_t bitflags;
};

";

        public readonly static string HeaderV240 =
@"struct Il2CppClass_1
{
    ::pointer<void> image;
    ::pointer<void> gc_desc;
    ::pointer<const char> name;
    ::pointer<const char> namespaze;
    ::pointer<Il2CppType> byval_arg;
    ::pointer<Il2CppType> this_arg;
    ::pointer<Il2CppClass> element_class;
    ::pointer<Il2CppClass> castClass;
    ::pointer<Il2CppClass> declaringType;
    ::pointer<Il2CppClass> parent;
    ::pointer<void> generic_class;
    ::pointer<void> typeDefinition;
    ::pointer<void> interopData;
    ::pointer<void> fields;
    ::pointer<void> events;
    ::pointer<void> properties;
    ::pointer<void> methods;
    ::pointer<::pointer<Il2CppClass>> nestedTypes;
    ::pointer<::pointer<Il2CppClass>> implementedInterfaces;
    void> interfaceOffsets;
};

struct Il2CppClass_2
{
    ::pointer<::pointer<Il2CppClass>> typeHierarchy;
    uint32_t cctor_started;
    uint32_t cctor_finished;
    uint64_t cctor_thread;
    int32_t genericContainerIndex;
    int32_t customAttributeIndex;
    uint32_t instance_size;
    uint32_t actualSize;
    uint32_t element_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
    int32_t thread_static_fields_offset;
    uint32_t flags;
    uint32_t token;
    uint16_t method_count;
    uint16_t property_count;
    uint16_t field_count;
    uint16_t event_count;
    uint16_t nested_type_count;
    uint16_t vtable_count;
    uint16_t interfaces_count;
    uint16_t interface_offsets_count;
    uint8_t typeHierarchyDepth;
    uint8_t genericRecursionDepth;
    uint8_t rank;
    uint8_t minimumAlignment;
    uint8_t packingSize;
    uint8_t bitflags1;
    uint8_t bitflags2;
};

struct Il2CppClass
{
    Il2CppClass_1 _1;
    ::pointer<void> static_fields;
    ::pointer<Il2CppRGCTXData> rgctx_data;
    Il2CppClass_2 _2;
    VirtualInvokeData vtable[255];
};

typedef int32_t il2cpp_array_size_t;
typedef int32_t il2cpp_array_lower_bound_t;
struct Il2CppArrayBounds
{
    il2cpp_array_size_t length;
    il2cpp_array_lower_bound_t lower_bound;
};

struct MethodInfo
{
    Il2CppMethodPointer methodPointer;
    ::pointer<void> invoker_method;
    ::pointer<const char> name;
    ::pointer<Il2CppClass> declaring_type;
    ::pointer<const Il2CppType> return_type;
    ::pointer<const void> parameters;
    union
    {
        ::pointer<const Il2CppRGCTXData> rgctx_data;
        ::pointer<const void> methodDefinition;
    };
    union
    {
        ::pointer<const void> genericMethod;
        ::pointer<const void> genericContainer;
    };
    int32_t customAttributeIndex;
    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint8_t parameters_count;
    uint8_t bitflags;
};

";

        public readonly static string HeaderV22 =
@"struct Il2CppClass_1
{
    ::pointer<void> image;
    ::pointer<void> gc_desc;
    ::pointer<const char> name;
    ::pointer<const char> namespaze;
    ::pointer<Il2CppType> byval_arg;
    ::pointer<Il2CppType> this_arg;
    ::pointer<Il2CppClass> element_class;
    ::pointer<Il2CppClass> castClass;
    ::pointer<Il2CppClass> declaringType;
    ::pointer<Il2CppClass> parent;
    ::pointer<void> generic_class;
    ::pointer<void> typeDefinition;
    ::pointer<void> fields;
    ::pointer<void> events;
    ::pointer<void> properties;
    ::pointer< void> methods;
    ::pointer<::pointer<Il2CppClass>> nestedTypes;
    ::pointer<::pointer<Il2CppClass>> implementedInterfaces;
    ::pointer<void> interfaceOffsets;
};

struct Il2CppClass_2
{
    ::pointer<::pointer<Il2CppClass>> typeHierarchy;
    uint32_t cctor_started;
    uint32_t cctor_finished;
    uint64_t cctor_thread;
    int32_t genericContainerIndex;
    int32_t customAttributeIndex;
    uint32_t instance_size;
    uint32_t actualSize;
    uint32_t element_size;
    int32_t native_size;
    uint32_t static_fields_size;
    uint32_t thread_static_fields_size;
    int32_t thread_static_fields_offset;
    uint32_t flags;
    uint32_t token;
    uint16_t method_count;
    uint16_t property_count;
    uint16_t field_count;
    uint16_t event_count;
    uint16_t nested_type_count;
    uint16_t vtable_count;
    uint16_t interfaces_count;
    uint16_t interface_offsets_count;
    uint8_t typeHierarchyDepth;
    uint8_t genericRecursionDepth;
    uint8_t rank;
    uint8_t minimumAlignment;
    uint8_t packingSize;
    uint8_t bitflags1;
    uint8_t bitflags2;
};

struct Il2CppClass
{
    Il2CppClass_1 _1;
    ::pointer<void> static_fields;
    ::pointer<Il2CppRGCTXData> rgctx_data;
    Il2CppClass_2 _2;
    VirtualInvokeData vtable[255];
};

typedef int32_t il2cpp_array_size_t;
typedef int32_t il2cpp_array_lower_bound_t;
struct Il2CppArrayBounds
{
    il2cpp_array_size_t length;
    il2cpp_array_lower_bound_t lower_bound;
};

struct MethodInfo
{
    Il2CppMethodPointer methodPointer;
    ::pointer<void> invoker_method;
    ::pointer<const char> name;
    ::pointer<Il2CppClass> declaring_type;
    ::pointer<const Il2CppType> return_type;
    ::pointer<const void> parameters;
    union
    {
        ::pointer<const Il2CppRGCTXData> rgctx_data;
        ::pointer<const void> methodDefinition;
    };
    union
    {
        ::pointer<const void> genericMethod;
        ::pointer<const void> genericContainer;
    };
    int32_t customAttributeIndex;
    uint32_t token;
    uint16_t flags;
    uint16_t iflags;
    uint16_t slot;
    uint8_t parameters_count;
    uint8_t bitflags;
};

";
    }
}
