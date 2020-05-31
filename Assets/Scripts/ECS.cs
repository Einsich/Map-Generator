using System;
using System.Collections.Generic;
using System.Reflection;
namespace ECS
{
    /*
     Данная реализация ECS никак не вводин формальные ограничения на то, чем должны являться компоненты, 
     но рациональнее всего использовать ссылочные типы, 
     поскольку типы-значения в объекте сущности нельзя будет изменять.
         */
    /// <summary>
    /// Пул для переиспользования компонентов, вызывает Constructor() при инициализации компонента и Destruct() при уничтожении. 
    /// </summary>
    /// <typeparam name="T"> Хранит список сущностей, имеющих компонент T</typeparam>
    static class ComponentPool<T> where T : new()
    {
        static Stack<T> DestroyComponents = new Stack<T>();//Компоненты  для переработки
        static List<Entity> Entities = new List<Entity>();//Сущности имеющий этот компонент
        public static List<Entity> GetEntities() => Entities;
        public static void DestroyComponent(T t, Entity owner)
        {
            t.GetType().GetMethod("Destructor")?.Invoke(t, null);
            DestroyComponents.Push(t);
            Entities.Remove(owner);
        }
        public static T GetComponent(Entity owner) 
        {
            T t = DestroyComponents.Count == 0 ? new T() : DestroyComponents.Pop();
            t.GetType().GetMethod("Constructor")?.Invoke(t, null);
            Entities.Add(owner);
            return t;
        }
    }
    /// <summary>
    /// Сущность - класс контейнер для компонент.
    /// </summary>
    public class Entity
    {
        static class ArrayPool<T>//пул для массивов object и Type, которые все время создаются.
        {
            static Stack<T[]>[] TypePool = new Stack<T[]>[10];//Исскуственное ограничение на кол-во элементов в массиве = 10, предполагается, что больше не будет
            static ArrayPool()
            {
                for (int i = 0; i < TypePool.Length; i++)
                    TypePool[i] = new Stack<T[]>();
            }
            public static T[] GetArray(int length)
            {
                return TypePool[length].Count == 0 ? new T[length] : TypePool[length].Pop();
            }
            public static void SetArray(T[] array)
            {
                TypePool[array.Length].Push(array);
            }
        }
        private object[] Components;//массив компонент
       
        public T GetComponent<T>() 
        {
            for (int i = 0; i < Components.Length; i++)
                if (Components[i] is T result)
                    return result;
            return default;
        }
       
        /// <summary>
        /// Возвращает массив объектов, соответсвующий массиву переданных типов, если тип является Entity,
        /// то в нужный элемент массива положится сама ссылка на текущую сущность. 
        /// Если сущность не имеет компонент запрашеваемого типа, то функция вернет null. 
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public object[] GetComponents(Type[] types)
        {
            object[] components = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                if(types[i] == typeof(Entity))
                {
                    components[i] = this;
                    continue;
                }
                for (int j = 0; j < Components.Length; j++)
                    if (types[i] == Components[j].GetType())
                    {
                        components[i] = Components[j];
                        break;
                    }
                if (components[i] == null)
                {
                    return null;
                }
            }
            return components;
        }
        static Stack<Entity> EntityPool = new Stack<Entity>();
        static Entity GetClearEntity()
        {
            Entity entity = EntityPool.Count == 0 ? new Entity() : EntityPool.Pop();
            return entity;
        }
        public T AddComponent<T>() where T : new()
        {
            T component = ComponentPool<T>.GetComponent(this);
            object[] array = ArrayPool<object>.GetArray(Components.Length + 1);
            Components.CopyTo(array, 0);
            array[Components.Length] = component;
            ArrayPool<object>.SetArray(Components);
            Components = array;
            return component;
        }
        /// <summary>
        /// Пытается удалить компонент и возвращает количество удаленных компонент.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int RemoveComponent<T>()
        {
            int newLength = Components.Length;
            for (int i = 0; i < Components.Length; i++)
                if (Components[i].GetType() == typeof(T))
                    newLength--;
            
            if (Components.Length == newLength)
                return 0;
            object[] array = ArrayPool<object>.GetArray(newLength);
            int d = 0;
            for (int i = 0; i < Components.Length; i++)
            {
                if (Components[i].GetType() == typeof(T))
                {
                    PushToPool((dynamic)Components[i]);
                    d++;
                }
                else
                {
                    array[i - d] = Components[i];
                }
            }

            ArrayPool<object>.SetArray(Components);
            Components = array;
            return d;
        }
        public void Destroy()
        {
            for (int i = 0; i < Components.Length; i++)
                PushToPool((dynamic)Components[i]);
            ArrayPool<object>.SetArray(Components);
            EntityPool.Push(this);
        }
        private void PushToPool<T>(T t) where T : new()
        {
            ComponentPool<T>.DestroyComponent(t, this);
        }

        //Способы создания сущности
        public static Entity Create<T1>() where T1 : new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(1);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            return entity;
        }
        public static Entity Create<T1,T2>() where T1 :  new() where T2 :  new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(2);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            entity.Components[1] = ComponentPool<T2>.GetComponent(entity);
            return entity;
        }
        public static Entity Create<T1, T2, T3>() where T1 :  new() where T2 :  new() where T3 :  new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(3);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            entity.Components[1] = ComponentPool<T2>.GetComponent(entity);
            entity.Components[2] = ComponentPool<T3>.GetComponent(entity);
            return entity;
        }
        public static Entity Create<T1, T2, T3,T4>() where T1 :  new() where T2 :  new() where T3 :  new() where T4 :  new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(4);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            entity.Components[1] = ComponentPool<T2>.GetComponent(entity);
            entity.Components[2] = ComponentPool<T3>.GetComponent(entity);
            entity.Components[3] = ComponentPool<T4>.GetComponent(entity);
            return entity;
        }
        public static Entity Create<T1, T2, T3, T4, T5>() where T1 :  new() where T2 :  new() where T3 :  new() where T4 :  new() where T5 :  new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(5);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            entity.Components[1] = ComponentPool<T2>.GetComponent(entity);
            entity.Components[2] = ComponentPool<T3>.GetComponent(entity);
            entity.Components[3] = ComponentPool<T4>.GetComponent(entity);
            entity.Components[4] = ComponentPool<T5>.GetComponent(entity);
            return entity;
        }
        public static Entity Create<T1, T2, T3, T4, T5, T6>() where T1 : new() where T2 :  new() where T3 :  new() where T4 :  new() where T5 :  new() where T6 :  new()
        {
            Entity entity = GetClearEntity();
            entity.Components = ArrayPool<object>.GetArray(6);
            entity.Components[0] = ComponentPool<T1>.GetComponent(entity);
            entity.Components[1] = ComponentPool<T2>.GetComponent(entity);
            entity.Components[2] = ComponentPool<T3>.GetComponent(entity);
            entity.Components[3] = ComponentPool<T4>.GetComponent(entity);
            entity.Components[4] = ComponentPool<T5>.GetComponent(entity);
            entity.Components[5] = ComponentPool<T6>.GetComponent(entity);
            return entity;
        }
    }
    /// <summary>
    /// Базовый класс для систем. Реализуйте функцию Update(...), чтобы выша система начала работать.
    /// </summary>
    public abstract class BaseSystem:IComparable<BaseSystem>
    {
        readonly int Order;
        public Type[] types;

        public MethodInfo UpdateMethod;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Порядок вызова систем</param>
        public BaseSystem(int order)
        {
            Order = order;
            UpdateMethod = GetType().GetMethod("Update");
            types = Array.ConvertAll(UpdateMethod.GetParameters(), (x) => x.ParameterType);
        }

        public int CompareTo(BaseSystem other)
        {
            return Order - other.Order;
        }
        public abstract void Start();
        public abstract void End();
    }
    /// <summary>
    /// Общий менеджер всех систем. Добавляйте и удаляйте системы через AddSystem() и RemoveSystem()
    /// </summary>
    public static class EntitySystem
    {
        static List<BaseSystem> Systems = new List<BaseSystem>();
        public static void AddSystem<S>() where S : BaseSystem, new()
        {
            S s = new S();
            for (int i = 0; i < Systems.Count; i++)
                if (Systems[i].CompareTo(s) > 0)
                {
                    Systems.Insert(i, s);
                    s.Start();

                    return;
                } else
                {
                    if (Systems[i] is S)
                        throw new Exception("Added more than one similar System");
                }
            Systems.Add(s);
            s.Start();
        }
        public static void RemoveSystem<S>() where S : BaseSystem, new()
        {
            for (int i = 0; i < Systems.Count; i++)
                if (Systems[i] is S)
                {
                    BaseSystem system = Systems[i];
                    system.End();
                    Systems.Remove(system);
                    return;
                }
        }
        static public void Update()
        {
            int count = Systems.Count;
            for (int i = 0; i < count; i++)
            {
                UpdateSystem(Systems[i]);
            }
        }
        static Dictionary<Type, MethodInfo> GetEntitiesMethods = new Dictionary<Type, MethodInfo>();//Reflection основанно решение, чтобы получать списки сущностей

        static MethodInfo GetEntities(Type type)
        {
            if (GetEntitiesMethods.ContainsKey(type))
                return GetEntitiesMethods[type];
            MethodInfo method = typeof(ComponentPool<>).MakeGenericType(type).GetMethod("GetEntities");
            GetEntitiesMethods.Add(type, method);
            return method;
        }
        /// <summary>
        /// Применяет функцию ко всем сущностям, имеющим данные Типы компоненты.
        /// </summary>
        /// <param name="types"></param>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <param name="firstApply"></param>
        static void Apply(Type[] types, MethodInfo method, object target, bool firstApply = false)
        {
            List<Entity> minList = null;
            for (int i = 0; i < types.Length; i++) //оптимизация по выбору самого маленького списка.
            {
                if (types[i] == typeof(Entity))
                    continue;
                var list = (List<Entity>) GetEntities(types[i]).Invoke(null, null);
                if (minList == null || minList.Count > list.Count)
                    minList = list;
            }
            if (minList == null)
                return;
            int count = minList.Count;
            for (int i = 0; i < count; i++)
            {

                var components = minList[i].GetComponents(types);
                if (components != null)
                {
                    method.Invoke(target, components);
                    if (firstApply)
                        return;
                }
                
            }            
        }
        static void UpdateSystem(BaseSystem system)
        {
            if (system.UpdateMethod != null)
                Apply(system.types, system.UpdateMethod, system);
        }
        /// <summary>
        /// Query запрос. Можно производить операции над нужными сущностями с нужными наборами компонент в любом месте.
        /// </summary>
        /// <param name="func"></param>
        public static void Query(Delegate func)
        {
            MethodInfo method = func.GetMethodInfo();
            if (method != null)
                Apply(Array.ConvertAll(method.GetParameters(), (x) => x.ParameterType), method, func.Target);
        }
        /// <summary>
        /// Query, которая выполнит переданную функцию только над первой подходящей сущность.
        /// </summary>
        /// <param name="func"></param>
        public static void FirstQuery(Delegate func)
        {
            MethodInfo method = func.GetMethodInfo();
            if (method != null)
                Apply(Array.ConvertAll(method.GetParameters(), (x) => x.ParameterType), method, func.Target, true);
        }
        /// <summary>
        /// Получение первой попавшейся сущности с нужным набором компонент. 
        /// </summary>
        /// <param name="types">Пишите typeof(T)</param>
        /// <returns></returns>
        public static Entity GetEntity(params Type[] types)
        {
            List<Entity> minList = null;
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == typeof(Entity))
                    continue;
                var list = (List<Entity>)GetEntities(types[i]).Invoke(null, null);
                if (minList == null || minList.Count > list.Count)
                    minList = list;
            }
            if (minList == null)
                return null;
            int count = minList.Count;
            for (int i = 0; i < count; i++)
                if (minList[i].GetComponents(types) != null)                
                    return minList[i];

            return null;
        }
    }
}