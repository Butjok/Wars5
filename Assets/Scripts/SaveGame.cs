using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace SaveGame {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    sealed class DontSaveAttribute : Attribute { }

    public class DefaultValue {
        public static readonly DefaultValue Instance = new();
    }

    public class Command {
        public int depth;
    }
    public class PushNull : Command { }
    public class PushInt : Command {
        public int value;
    }
    public class PushFloat : Command {
        public float value;
    }
    public class PushDouble : Command {
        public double value;
    }
    public class PushString : Command {
        public string value;
    }
    public class PushBool : Command {
        public bool value;
    }
    public class PushType : Command {
        public string typeName;
    }
    public class PushColor : Command {
        public float r, g, b, a;
    }
    public class PushVector2 : Command {
        public float x, y;
    }
    public class PushVector3 : Command {
        public float x, y, z;
    }
    public class PushVector4 : Command {
        public float x, y, z, w;
    }
    public class PushVector2Int : Command {
        public int x, y;
    }
    public class PushVector3Int : Command {
        public int x, y, z;
    }
    public class PushQuaternion : Command {
        public float x, y, z, w;
    }
    public class PushEnum : Command {
        public string typeName;
        public string value;
    }
    public class PushMatrix4X4 : Command {
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;
    }
    public class ReferenceTo : Command {
        public int index;
    }
    public class HashSet : Command {
        public int count;
        public string elementTypeName;
        public int id;
    }
    public class HashSetItem : Command { }
    public class EndHashSetItem : Command { }
    public class EndHashSet : Command { }
    public class StackCommand : Command {
        public int count;
        public string elementTypeName;
        public int id;
    }
    public class StackItem : Command { }
    public class EndStackItem : Command { }
    public class EndStack : Command { }
    public class QueueCommand : Command {
        public int count;
        public string elementTypeName;
        public int id;
    }
    public class QueueItem : Command { }
    public class EndQueueItem : Command { }
    public class EndQueue : Command { }
    public class Array : Command {
        public int length;
        public string elementTypeName;
        public int id;
    }
    public class ArrayItem : Command {
        public int index;
    }
    public class EndArrayItem : Command { }
    public class EndArray : Command { }

    public class List : Command {
        public int length;
        public string elementTypeName;
        public int id;
    }
    public class ListItem : Command { }
    public class EndListItem : Command { }
    public class EndList : Command { }

    public class LinkedList : Command {
        public string elementTypeName;
        public int id;
    }
    public class LinkedListItem : Command { }
    public class EndLinkedListItem : Command { }
    public class EndLinkedList : Command { }

    public class Dictionary : Command {
        public int count;
        public string keyTypeName, valueTypeName;
        public int id;
    }
    public class KeyValue : Command { }
    public class EndKeyValue : Command { }
    public class EndDictionary : Command { }
    public class ClassInstance : Command {
        public string typeName;
        public int id;
    }
    public class EndClassInstance : Command { }
    public class StructInstance : Command {
        public string typeName;
    }
    public class EndStructInstance : Command { }
    public class Field : Command {
        public string fieldName;
    }
    public class EndField : Command { }
    public class Property : Command {
        public string propertyName;
    }
    public class EndProperty : Command { }

    public class TextFormatter {
        public static void Format(TextWriter output, IReadOnlyList<Command> commands) {
            // set invariant culture
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            foreach (var command in commands) {
                output.Write(new string(' ', command.depth * 2));
                output.WriteLine(command switch {
                    PushNull _ => "Null",
                    PushInt pushInt => $"Int {pushInt.value}",
                    PushFloat pushFloat => $"Float {pushFloat.value}",
                    PushDouble pushDouble => $"Double {pushDouble.value}",
                    PushString pushString => $"String {EscapeString(pushString.value)}",
                    PushBool pushBool => $"Bool {(pushBool.value ? "True" : "False")}",
                    PushType pushType => $"Type {EscapeString(pushType.typeName)}",
                    PushColor pushColor => $"Color {pushColor.r} {pushColor.g} {pushColor.b} {pushColor.a}",
                    PushVector2 pushVector2 => $"Vector2 {pushVector2.x} {pushVector2.y}",
                    PushVector3 pushVector3 => $"Vector3 {pushVector3.x} {pushVector3.y} {pushVector3.z}",
                    PushVector4 pushVector4 => $"Vector4 {pushVector4.x} {pushVector4.y} {pushVector4.z} {pushVector4.w}",
                    PushVector2Int pushVector2Int => $"Vector2Int {pushVector2Int.x} {pushVector2Int.y}",
                    PushVector3Int pushVector3Int => $"Vector3Int {pushVector3Int.x} {pushVector3Int.y} {pushVector3Int.z}",
                    PushQuaternion pushQuaternion => $"Quaternion {pushQuaternion.x} {pushQuaternion.y} {pushQuaternion.z} {pushQuaternion.w}",
                    PushEnum pushEnum => $"Enum {EscapeString(pushEnum.typeName)} {EscapeString(pushEnum.value)}",
                    PushMatrix4X4 pushMatrix4X4 => $"Matrix4x4 {pushMatrix4X4.m00} {pushMatrix4X4.m01} {pushMatrix4X4.m02} {pushMatrix4X4.m03} {pushMatrix4X4.m10} {pushMatrix4X4.m11} {pushMatrix4X4.m12} {pushMatrix4X4.m13} {pushMatrix4X4.m20} {pushMatrix4X4.m21} {pushMatrix4X4.m22} {pushMatrix4X4.m23} {pushMatrix4X4.m30} {pushMatrix4X4.m31} {pushMatrix4X4.m32} {pushMatrix4X4.m33}",
                    ReferenceTo getReference => $"ReferenceTo {getReference.index}",
                    HashSet hashSet => $"HashSet {hashSet.count} {EscapeString(hashSet.elementTypeName)} ReferenceId {hashSet.id}",
                    HashSetItem _ => "HashSetItem",
                    EndHashSetItem _ => "EndHashSetItem",
                    EndHashSet _ => "EndHashSet",
                    Array array => $"Array {array.length} {EscapeString(array.elementTypeName)} ReferenceId {array.id}",
                    ArrayItem arrayItem => $"ArrayItem {arrayItem.index}",
                    EndArrayItem => "EndArrayItem",
                    EndArray _ => "EndArray",
                    List list => $"List {list.length} {EscapeString(list.elementTypeName)} ReferenceId {list.id}",
                    ListItem _ => "ListItem",
                    EndListItem _ => "EndListItem",
                    EndList _ => "EndList",
                    LinkedList linkedList => $"LinkedList {EscapeString(linkedList.elementTypeName)} ReferenceId {linkedList.id}",
                    LinkedListItem _ => "LinkedListItem",
                    EndLinkedListItem _ => "EndLinkedListItem",
                    EndLinkedList _ => "EndLinkedList",
                    Dictionary dictionary => $"Dictionary {dictionary.count} {EscapeString(dictionary.keyTypeName)} {EscapeString(dictionary.valueTypeName)} ReferenceId {dictionary.id}",
                    KeyValue _ => "KeyValue",
                    EndKeyValue _ => "EndKeyValue",
                    EndDictionary _ => "EndDictionary",
                    ClassInstance classInstance => $"ClassInstance {EscapeString(classInstance.typeName)} ReferenceId {classInstance.id}",
                    StructInstance structInstance => $"StructInstance {EscapeString(structInstance.typeName)}",
                    EndStructInstance _ => "EndStructInstance",
                    EndClassInstance _ => "EndClassInstance",
                    Field beginFieldValue => $"Field {beginFieldValue.fieldName}",
                    EndField => "EndField",
                    Property beginPropertyValue => $"Property {beginPropertyValue.propertyName}",
                    EndProperty => "EndProperty",
                    StackCommand stackCommand => $"Stack {stackCommand.count} {EscapeString(stackCommand.elementTypeName)} ReferenceId {stackCommand.id}",
                    StackItem _ => "StackItem",
                    EndStackItem _ => "EndStackItem",
                    EndStack _ => "EndStack",
                    QueueCommand queueCommand => $"Queue {queueCommand.count} {EscapeString(queueCommand.elementTypeName)} ReferenceId {queueCommand.id}",
                    QueueItem _ => "QueueItem",
                    EndQueueItem _ => "EndQueueItem",
                    EndQueue _ => "EndQueue",
                    _ => throw new NotImplementedException(command.GetType().ToString())
                });
            }
        }
        public static List<Command> Parse(TextReader input) {
            // set invariant culture
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            // read line by line 
            var commands = new List<Command>();
            while (input.ReadLine() is { } line) {
                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                commands.Add(parts[0] switch {
                    "Null" => new PushNull(),
                    "Int" => new PushInt { value = int.Parse(parts[1]) },
                    "Float" => new PushFloat { value = float.Parse(parts[1]) },
                    "Double" => new PushDouble { value = double.Parse(parts[1]) },
                    "String" => new PushString { value = UnescapeString(parts[1]) },
                    "Bool" => new PushBool { value = parts[1] == "True" },
                    "Type" => new PushType { typeName = UnescapeString(parts[1]) },
                    "Color" => new PushColor { r = float.Parse(parts[1]), g = float.Parse(parts[2]), b = float.Parse(parts[3]), a = float.Parse(parts[4]) },
                    "Vector2" => new PushVector2 { x = float.Parse(parts[1]), y = float.Parse(parts[2]) },
                    "Vector3" => new PushVector3 { x = float.Parse(parts[1]), y = float.Parse(parts[2]), z = float.Parse(parts[3]) },
                    "Vector4" => new PushVector4 { x = float.Parse(parts[1]), y = float.Parse(parts[2]), z = float.Parse(parts[3]), w = float.Parse(parts[4]) },
                    "Vector2Int" => new PushVector2Int { x = int.Parse(parts[1]), y = int.Parse(parts[2]) },
                    "Vector3Int" => new PushVector3Int { x = int.Parse(parts[1]), y = int.Parse(parts[2]), z = int.Parse(parts[3]) },
                    "Quaternion" => new PushQuaternion { x = float.Parse(parts[1]), y = float.Parse(parts[2]), z = float.Parse(parts[3]), w = float.Parse(parts[4]) },
                    "Enum" => new PushEnum { typeName = UnescapeString(parts[1]), value = UnescapeString(parts[2]) },
                    "Matrix4x4" => new PushMatrix4X4 {
                        m00 = float.Parse(parts[1]), m01 = float.Parse(parts[2]), m02 = float.Parse(parts[3]), m03 = float.Parse(parts[4]),
                        m10 = float.Parse(parts[5]), m11 = float.Parse(parts[6]), m12 = float.Parse(parts[7]), m13 = float.Parse(parts[8]),
                        m20 = float.Parse(parts[9]), m21 = float.Parse(parts[10]), m22 = float.Parse(parts[11]), m23 = float.Parse(parts[12]),
                        m30 = float.Parse(parts[13]), m31 = float.Parse(parts[14]), m32 = float.Parse(parts[15]), m33 = float.Parse(parts[16])
                    },
                    "ReferenceTo" => new ReferenceTo { index = int.Parse(parts[1]) },
                    "HashSet" => new HashSet { count = int.Parse(parts[1]), elementTypeName = UnescapeString(parts[2]) },
                    "HashSetItem" => new HashSetItem(),
                    "EndHashSetItem" => new EndHashSetItem(),
                    "EndHashSet" => new EndHashSet(),
                    "Array" => new Array { length = int.Parse(parts[1]), elementTypeName = UnescapeString(parts[2]) },
                    "ArrayItem" => new ArrayItem { index = int.Parse(parts[1]) },
                    "EndArrayItem" => new EndArrayItem(),
                    "EndArray" => new EndArray(),
                    "List" => new List { length = int.Parse(parts[1]), elementTypeName = UnescapeString(parts[2]) },
                    "ListItem" => new ListItem(),
                    "EndListItem" => new EndListItem(),
                    "EndList" => new EndList(),
                    "LinkedList" => new LinkedList { elementTypeName = UnescapeString(parts[1]) },
                    "LinkedListItem" => new LinkedListItem(),
                    "EndLinkedListItem" => new EndLinkedListItem(),
                    "EndLinkedList" => new EndLinkedList(),
                    "Dictionary" => new Dictionary { count = int.Parse(parts[1]), keyTypeName = UnescapeString(parts[2]), valueTypeName = UnescapeString(parts[3]) },
                    "KeyValue" => new KeyValue(),
                    "EndKeyValue" => new EndKeyValue(),
                    "EndDictionary" => new EndDictionary(),
                    "ClassInstance" => new ClassInstance { typeName = UnescapeString(parts[1]) },
                    "EndClassInstance" => new EndClassInstance(),
                    "StructInstance" => new StructInstance { typeName = UnescapeString(parts[1]) },
                    "EndStructInstance" => new EndStructInstance(),
                    "Field" => new Field { fieldName = parts[1] },
                    "EndField" => new EndField(),
                    "Property" => new Property { propertyName = parts[1] },
                    "EndProperty" => new EndProperty(),
                    "Stack" => new StackCommand { count = int.Parse(parts[1]), elementTypeName = UnescapeString(parts[2]) },
                    "StackItem" => new StackItem(),
                    "EndStackItem" => new EndStackItem(),
                    "EndStack" => new EndStack(),
                    "Queue" => new QueueCommand { count = int.Parse(parts[1]), elementTypeName = UnescapeString(parts[2]) },
                    "QueueItem" => new QueueItem(),
                    "EndQueueItem" => new EndQueueItem(),
                    "EndQueue" => new EndQueue(),
                    _ => throw new NotImplementedException(parts[0])
                });
            }
            return commands;
        }

        private static StringBuilder sb = new();
        // escape string by replacing whitespace and % with %xx
        public static string EscapeString(string input) {
            sb.Clear();
            sb.Append('"');
            foreach (var c in input)
                if (char.IsWhiteSpace(c) || c == '%') {
                    sb.Append('%');
                    sb.Append((char)('0' + c / 16));
                    sb.Append((char)('0' + c % 16));
                }
                else
                    sb.Append(c);
            sb.Append('"');
            return sb.ToString();
        }
        public static string UnescapeString(string input) {
            sb.Clear();
            Assert.IsTrue(input.Length > 0);
            Assert.IsTrue(input[0] == '"');
            Assert.IsTrue(input[^1] == '"');
            for (var i = 1; i < input.Length - 1; i++)
                if (input[i] == '%') {
                    Assert.IsTrue(i + 2 < input.Length - 1);
                    var hi = input[i + 1] - '0';
                    var lo = input[i + 2] - '0';
                    var hex = hi * 16 + lo;
                    sb.Append((char)hex);
                    i += 2;
                }
                else
                    sb.Append(input[i]);
            return sb.ToString();
        }
    }

    public class Loader {

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly List<object> referencedObjects = new();
        private readonly Stack<object> stack = new();
        private readonly Stack<FieldInfo> fieldInfos = new();
        private readonly Stack<PropertyInfo> propertyInfos = new();
        private readonly Stack<int> arrayIndices = new();
        public static T Load<T>(IReadOnlyList<Command> commands) {
            var loader = new Loader();
            loader.ExecuteCommands(commands);
            Assert.IsTrue(loader.stack.Count == 1);
            return (T)loader.stack.Pop();
        }
        public static T Load<T>(string text) {
            var commands = TextFormatter.Parse(new StringReader(text));
            return Load<T>(commands);
        }
        private void ExecuteCommands(IReadOnlyList<Command> commands) {
            foreach (var command in commands)
                switch (command) {
                    case PushNull _:
                        stack.Push(null);
                        break;
                    case PushInt pushInt:
                        stack.Push(pushInt.value);
                        break;
                    case PushFloat pushFloat:
                        stack.Push(pushFloat.value);
                        break;
                    case PushDouble pushDouble:
                        stack.Push(pushDouble.value);
                        break;
                    case PushString pushString:
                        stack.Push(pushString.value);
                        break;
                    case PushBool pushBool:
                        stack.Push(pushBool.value);
                        break;
                    case PushType pushType:
                        stack.Push(Type.GetType(pushType.typeName));
                        break;
                    case PushColor pushColor:
                        stack.Push(new Color(pushColor.r, pushColor.g, pushColor.b, pushColor.a));
                        break;
                    case PushVector2 pushVector2:
                        stack.Push(new Vector2(pushVector2.x, pushVector2.y));
                        break;
                    case PushVector3 pushVector3:
                        stack.Push(new Vector3(pushVector3.x, pushVector3.y, pushVector3.z));
                        break;
                    case PushVector4 pushVector4:
                        stack.Push(new Vector4(pushVector4.x, pushVector4.y, pushVector4.z, pushVector4.w));
                        break;
                    case PushVector2Int pushVector2Int:
                        stack.Push(new Vector2Int(pushVector2Int.x, pushVector2Int.y));
                        break;
                    case PushVector3Int pushVector3Int:
                        stack.Push(new Vector3Int(pushVector3Int.x, pushVector3Int.y, pushVector3Int.z));
                        break;
                    case PushQuaternion pushQuaternion:
                        stack.Push(new Quaternion(pushQuaternion.x, pushQuaternion.y, pushQuaternion.z, pushQuaternion.w));
                        break;
                    case PushMatrix4X4 pushMatrix4X4:
                        stack.Push(new Matrix4x4 {
                            m00 = pushMatrix4X4.m00, m01 = pushMatrix4X4.m01, m02 = pushMatrix4X4.m02, m03 = pushMatrix4X4.m03,
                            m10 = pushMatrix4X4.m10, m11 = pushMatrix4X4.m11, m12 = pushMatrix4X4.m12, m13 = pushMatrix4X4.m13,
                            m20 = pushMatrix4X4.m20, m21 = pushMatrix4X4.m21, m22 = pushMatrix4X4.m22, m23 = pushMatrix4X4.m23,
                            m30 = pushMatrix4X4.m30, m31 = pushMatrix4X4.m31, m32 = pushMatrix4X4.m32, m33 = pushMatrix4X4.m33
                        });
                        break;
                    case ReferenceTo getReference:
                        stack.Push(referencedObjects[getReference.index]);
                        break;
                    case EndArray or ListItem or EndList or KeyValue or EndDictionary or HashSetItem or EndHashSet or EndClassInstance or EndStructInstance or StackItem or EndStack or QueueItem or EndQueue or LinkedListItem or EndLinkedList:
                        break;
                    case Array array1: {
                        var type = Type.GetType(array1.elementTypeName);
                        if (type == null)
                            Debug.LogError($"Type {array1.elementTypeName} not found");
                        var array = type != null ? System.Array.CreateInstance(type, array1.length) : null;
                        referencedObjects.Add(array);
                        stack.Push(array);
                        break;
                    }
                    case ArrayItem arrayItem:
                        arrayIndices.Push(arrayItem.index);
                        break;
                    case EndArrayItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is System.Array array)
                            array.SetValue(value, arrayIndices.Pop());
                        else
                            Debug.LogError($"{stack.Peek()} is not an array");
                        break;
                    }
                    case List list1: {
                        var type = Type.GetType(list1.elementTypeName);
                        if (type == null)
                            Debug.LogError($"Type {list1.elementTypeName} not found");
                        var list = type != null ? (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type), list1.length) : null;
                        referencedObjects.Add(list);
                        stack.Push(list);
                        break;
                    }
                    case EndListItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is IList list)
                            list.Add(value);
                        else
                            Debug.LogError($"{stack.Peek()} is not a list");
                        break;
                    }
                    case LinkedList linkedList1: {
                        var type = Type.GetType(linkedList1.elementTypeName);
                        if (type == null)
                            Debug.LogError($"Type {linkedList1.elementTypeName} not found");
                        var linkedList = type != null ? (IEnumerable)Activator.CreateInstance(typeof(LinkedList<>).MakeGenericType(type)) : null;
                        referencedObjects.Add(linkedList);
                        stack.Push(linkedList);
                        break;
                    }
                    case EndLinkedListItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is IEnumerable linkedList) {
                            var methods = linkedList.GetType().GetMethods();
                            var method = methods.Single(m => m.Name == "AddLast" && m.GetParameters()[0].ParameterType == value.GetType());
                            method.Invoke(linkedList, new[] { value });
                        }
                        else
                            Debug.LogError($"{stack.Peek()} is not a linked list");
                        LinkedList<Vector2Int> list;
                        break;
                    }
                    case Dictionary dictionary1: {
                        var keyType = Type.GetType(dictionary1.keyTypeName);
                        if (keyType == null)
                            Debug.LogError($"Type {dictionary1.keyTypeName} not found");
                        var valueType = Type.GetType(dictionary1.valueTypeName);
                        if (valueType == null)
                            Debug.LogError($"Type {dictionary1.valueTypeName} not found");
                        var dictionary = keyType != null && valueType != null ? (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType), dictionary1.count) : null;
                        referencedObjects.Add(dictionary);
                        stack.Push(dictionary);
                        break;
                    }
                    case EndKeyValue _: {
                        var value = stack.Pop();
                        var key = stack.Pop();
                        if (stack.Peek() is IDictionary dictionary)
                            dictionary.Add(key, value);
                        else
                            Debug.LogError($"{stack.Peek()} is not a dictionary");
                        break;
                    }
                    case HashSet set: {
                        var type = Type.GetType(set.elementTypeName);
                        if (type == null)
                            Debug.LogError($"Type {set.elementTypeName} not found");
                        var hashSet = type != null ? (IEnumerable)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(type)) : null;
                        referencedObjects.Add(hashSet);
                        stack.Push(hashSet);
                        break;
                    }
                    case EndHashSetItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is IEnumerable hashSet) {
                            var methodInfo = hashSet.GetType().GetMethod("Add");
                            if (methodInfo != null)
                                methodInfo.Invoke(hashSet, new[] { value });
                            else
                                Debug.LogError($"{stack.Peek()} does not have Add method");
                        }
                        else
                            Debug.LogError($"{stack.Peek()} is not a IEnumerable");
                        break;
                    }
                    case ClassInstance beginClassInstance: {
                        var type = Type.GetType(beginClassInstance.typeName);
                        if (type == null)
                            Debug.LogError($"Type {beginClassInstance.typeName} not found");
                        var obj = type != null ? Activator.CreateInstance(type) : null;
                        referencedObjects.Add(obj);
                        stack.Push(obj);
                        break;
                    }
                    case StructInstance beginStructInstance: {
                        var type = Type.GetType(beginStructInstance.typeName);
                        if (type == null)
                            Debug.LogError($"Type {beginStructInstance.typeName} not found");
                        var obj = type != null ? Activator.CreateInstance(type) : null;
                        stack.Push(obj);
                        break;
                    }
                    case Field beginFieldValue:
                        fieldInfos.Push(stack.Peek()?.GetType()?.GetField(beginFieldValue.fieldName, flags));
                        break;
                    case EndField _: {
                        var value = stack.Pop();
                        var obj = stack.Peek();
                        var fieldInfo = fieldInfos.Pop();
                        if (fieldInfo != null && (value == null || fieldInfo.FieldType.IsAssignableFrom(value.GetType())))
                            fieldInfo.SetValue(obj, value);
                        break;
                    }
                    case Property beginPropertyValue:
                        propertyInfos.Push(stack.Peek()?.GetType()?.GetProperty(beginPropertyValue.propertyName, flags));
                        break;
                    case EndProperty _: {
                        var value = stack.Pop();
                        var obj = stack.Peek();
                        var propertyInfo = propertyInfos.Pop();
                        if (propertyInfo != null && (value == null || propertyInfo.PropertyType.IsAssignableFrom(value.GetType())))
                            propertyInfo.SetValue(obj, value);
                        break;
                    }
                    case PushEnum pushEnum: {
                        var enumType = Type.GetType(pushEnum.typeName);
                        if (Enum.TryParse(enumType, pushEnum.value, out var result))
                            stack.Push(result);
                        else
                            Debug.LogError($"Failed to parse {pushEnum.value} as {enumType}");
                        break;
                    }
                    case StackCommand stackCommand: {
                        var type = Type.GetType(stackCommand.elementTypeName);
                        if (type == null)
                            Debug.LogError($"Type {stackCommand.elementTypeName} not found");
                        var stackInstance = type != null ? Activator.CreateInstance(typeof(Stack<>).MakeGenericType(type)) : null;
                        referencedObjects.Add(stackInstance);
                        stack.Push(stackInstance);
                        break;
                    }
                    case EndStackItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is IEnumerable stack2) {
                            var methodInfo = stack2.GetType().GetMethod("Push");
                            if (methodInfo != null)
                                methodInfo.Invoke(stack2, new[] { value });
                            else
                                Debug.LogError($"{stack.Peek()} does not have Push method");
                        }
                        else
                            Debug.LogError($"{stack.Peek()} is not a IEnumerable");
                        break;
                    }
                    case QueueCommand queueCommand: {
                        var queueInstance = (Queue)Activator.CreateInstance(typeof(Queue<>).MakeGenericType(Type.GetType(queueCommand.elementTypeName)));
                        referencedObjects.Add(queueInstance);
                        stack.Push(queueInstance);
                        break;
                    }
                    case EndQueueItem _: {
                        var value = stack.Pop();
                        if (stack.Peek() is IEnumerable queue) {
                            var methodInfo = queue.GetType().GetMethod("Enqueue");
                            if (methodInfo != null)
                                methodInfo.Invoke(queue, new[] { value });
                            else
                                Debug.LogError($"{stack.Peek()} does not have Enqueue method");
                        }
                        else
                            Debug.LogError($"{stack.Peek()} is not a IEnumerable");
                        break;
                    }
                    default:
                        Debug.LogError($"Unknown command {command.GetType()}");
                        break;
                }
        }
    }

    public class CommandEmitter {

        private const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private readonly List<object> referencedObjects = new();
        private readonly Dictionary<object, int> getIndex = new();

        private int NewReferenceId(object obj) {
            referencedObjects.Add(obj);
            getIndex.Add(obj, referencedObjects.Count - 1);
            return referencedObjects.Count - 1;
        }

        public static List<Command> Emit<T>(T obj) {
            var writer = new CommandEmitter();
            return writer.Emit(obj);
        }

        private List<Command> Emit(object obj, List<Command> output = null, int depth = 0) {
            Debug.Assert(depth < 1000, "Depth limit reached");

            output ??= new List<Command>();

            switch (obj) {
                case null:
                    output.Add(new PushNull { depth = depth });
                    break;
                case int intValue:
                    output.Add(new PushInt { depth = depth, value = intValue });
                    break;
                case float floatValue:
                    output.Add(new PushFloat { depth = depth, value = floatValue });
                    break;
                case double doubleValue:
                    output.Add(new PushDouble { depth = depth, value = doubleValue });
                    break;
                case string stringValue:
                    output.Add(new PushString { depth = depth, value = stringValue });
                    break;
                case bool boolValue:
                    output.Add(new PushBool { depth = depth, value = boolValue });
                    break;
                case Type typeValue:
                    output.Add(new PushType { depth = depth, typeName = typeValue.AssemblyQualifiedName });
                    break;
                case Color colorValue:
                    output.Add(new PushColor { depth = depth, r = colorValue.r, g = colorValue.g, b = colorValue.b, a = colorValue.a });
                    break;
                case Vector2 vector2Value:
                    output.Add(new PushVector2 { depth = depth, x = vector2Value.x, y = vector2Value.y });
                    break;
                case Vector3 vector3Value:
                    output.Add(new PushVector3 { depth = depth, x = vector3Value.x, y = vector3Value.y, z = vector3Value.z });
                    break;
                case Vector4 vector4Value:
                    output.Add(new PushVector4 { depth = depth, x = vector4Value.x, y = vector4Value.y, z = vector4Value.z, w = vector4Value.w });
                    break;
                case Vector2Int vector2IntValue:
                    output.Add(new PushVector2Int { depth = depth, x = vector2IntValue.x, y = vector2IntValue.y });
                    break;
                case Vector3Int vector3IntValue:
                    output.Add(new PushVector3Int { depth = depth, x = vector3IntValue.x, y = vector3IntValue.y, z = vector3IntValue.z });
                    break;
                case Quaternion quaternionValue:
                    output.Add(new PushQuaternion { depth = depth, x = quaternionValue.x, y = quaternionValue.y, z = quaternionValue.z, w = quaternionValue.w });
                    break;
                case Enum enumValue:
                    output.Add(new PushEnum { depth = depth, typeName = enumValue.GetType().AssemblyQualifiedName, value = enumValue.ToString() });
                    break;
                case Matrix4x4 matrix4X4Value:
                    output.Add(new PushMatrix4X4 {
                        depth = depth,
                        m00 = matrix4X4Value.m00, m01 = matrix4X4Value.m01, m02 = matrix4X4Value.m02, m03 = matrix4X4Value.m03,
                        m10 = matrix4X4Value.m10, m11 = matrix4X4Value.m11, m12 = matrix4X4Value.m12, m13 = matrix4X4Value.m13,
                        m20 = matrix4X4Value.m20, m21 = matrix4X4Value.m21, m22 = matrix4X4Value.m22, m23 = matrix4X4Value.m23,
                        m30 = matrix4X4Value.m30, m31 = matrix4X4Value.m31, m32 = matrix4X4Value.m32, m33 = matrix4X4Value.m33
                    });
                    break;
                default: {
                    if (getIndex.TryGetValue(obj, out var id)) {
                        output.Add(new ReferenceTo { depth = depth, index = id });
                        break;
                    }

                    var type = obj.GetType();

                    // arrays
                    if (type.IsArray) {
                        var array = (System.Array)obj;
                        id = NewReferenceId(obj);
                        output.Add(new Array {
                            depth = depth,
                            length = array.Length,
                            elementTypeName = array.GetType().GetElementType().AssemblyQualifiedName,
                            id = id
                        });
                        for (var i = 0; i < array.Length; i++) {
                            output.Add(new ArrayItem { depth = depth + 1, index = i });
                            Emit(array.GetValue(i), output, depth + 2);
                            output.Add(new EndArrayItem { depth = depth + 1 });
                        }
                        output.Add(new EndArray { depth = depth });
                        break;
                    }

                    // generic
                    if (type.IsGenericType) {
                        if (type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                            Emit(type.GetProperty("Value").GetValue(obj), output, depth + 1);
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(List<>)) {
                            var list = (IList)obj;
                            id = NewReferenceId(obj);
                            output.Add(new List {
                                depth = depth,
                                length = list.Count,
                                elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                id = id
                            });
                            foreach (var item in list) {
                                output.Add(new ListItem { depth = depth + 1 });
                                Emit(item, output, depth + 2);
                                output.Add(new EndListItem { depth = depth + 1 });
                            }
                            output.Add(new EndList { depth = depth });
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(LinkedList<>)) {
                            var linkedList = (IEnumerable)obj;
                            id = NewReferenceId(obj);
                            output.Add(new LinkedList {
                                depth = depth,
                                elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                id = id
                            });
                            foreach (var item in linkedList) {
                                output.Add(new LinkedListItem { depth = depth + 1 });
                                Emit(item, output, depth + 2);
                                output.Add(new EndLinkedListItem { depth = depth + 1 });
                            }
                            output.Add(new EndLinkedList { depth = depth });
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
                            var dictionary = (IDictionary)obj;
                            id = NewReferenceId(obj);
                            output.Add(new Dictionary {
                                depth = depth,
                                count = dictionary.Count,
                                keyTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                valueTypeName = type.GetGenericArguments()[1].AssemblyQualifiedName,
                                id = id
                            });
                            foreach (var key in dictionary.Keys) {
                                output.Add(new KeyValue { depth = depth + 1 });
                                Emit(key, output, depth + 2);
                                Emit(dictionary[key], output, depth + 2);
                                output.Add(new EndKeyValue { depth = depth + 1 });
                            }
                            output.Add(new EndDictionary { depth = depth });
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(HashSet<>)) {
                            var hashSet = (IEnumerable)obj;
                            id = NewReferenceId(obj);
                            output.Add(new HashSet {
                                depth = depth,
                                count = 0,
                                elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                id = id
                            });
                            foreach (var item in hashSet) {
                                output.Add(new HashSetItem { depth = depth + 1 });
                                Emit(item, output, depth + 2);
                                output.Add(new EndHashSetItem { depth = depth + 1 });
                            }
                            output.Add(new EndHashSet { depth = depth });
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(Stack<>)) {
                            var stack = (IEnumerable)obj;
                            id = NewReferenceId(obj);
                            output.Add(new StackCommand {
                                depth = depth,
                                count = 0,
                                elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                id = id
                            });
                            var values = new List<object>();
                            foreach (var item in stack)
                                values.Add(item);
                            values.Reverse();
                            foreach (var item in values) {
                                output.Add(new StackItem { depth = depth + 1 });
                                Emit(item, output, depth + 2);
                                output.Add(new EndStackItem { depth = depth + 1 });
                            }
                            output.Add(new EndStack { depth = depth });
                            break;
                        }

                        if (type.GetGenericTypeDefinition() == typeof(Queue<>)) {
                            var queue = (IEnumerable)obj;
                            id = NewReferenceId(obj);
                            output.Add(new QueueCommand {
                                depth = depth,
                                count = 0,
                                elementTypeName = type.GetGenericArguments()[0].AssemblyQualifiedName,
                                id = id
                            });
                            foreach (var item in queue) {
                                output.Add(new QueueItem { depth = depth + 1 });
                                Emit(item, output, depth + 2);
                                output.Add(new EndQueueItem { depth = depth + 1 });
                            }
                            output.Add(new EndQueue { depth = depth });
                            break;
                        }
                    }

                    if (type.IsClass) {
                        Assert.IsFalse(obj is UnityEngine.Object, $"Cannot save {type.Name} since it is UnityEngine.Object");
                        id = NewReferenceId(obj);
                        output.Add(new ClassInstance {
                            depth = depth,
                            typeName = type.AssemblyQualifiedName,
                            id = id
                        });
                    }
                    else
                        output.Add(new StructInstance { depth = depth, typeName = type.AssemblyQualifiedName });

                    foreach (var field in type.GetFields(flags))
                        if (field.GetCustomAttribute<DontSaveAttribute>() == null && field.GetCustomAttribute<CompilerGeneratedAttribute>() == null) {
                            output.Add(new Field { depth = depth + 1, fieldName = field.Name });
                            Emit(field.GetValue(obj), output, depth + 2);
                            output.Add(new EndField { depth = depth + 1 });
                        }

                    foreach (var property in type.GetProperties(flags))
                        if (property.GetCustomAttribute<DontSaveAttribute>() == null) {
                            Assert.IsTrue(property.GetMethod != null, $"Property {type.Name}.{property.Name} has no getter");
                            Assert.IsTrue(property.SetMethod != null, $"Property {type.Name}.{property.Name} has no setter");
                            output.Add(new Property { depth = depth + 1, propertyName = property.Name });
                            Emit(property.GetValue(obj), output, depth + 2);
                            output.Add(new EndProperty { depth = depth + 1 });
                        }

                    if (type.IsClass)
                        output.Add(new EndClassInstance { depth = depth });
                    else
                        output.Add(new EndStructInstance { depth = depth });

                    break;
                }
            }

            return output;
        }
    }
}