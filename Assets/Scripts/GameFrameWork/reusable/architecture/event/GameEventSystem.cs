using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//总体管理事件系统的类
public  class GameEventSystem :IEventSystem
{
    private Dictionary<Type,IEventRegistration> eventRegistrationsDict = new Dictionary<Type, IEventRegistration>();
    public void Regist<T>(Action<object> onEvent) {
        var type = typeof(T);
        IEventRegistration eventRegistration;
        //取出T对应的那个eventRegistration类，将事件注册到里面
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistration))
        {
            //T只是用来标志事件类型的标识符，其本身不起作用，而是要新生成一个事件类型的实例来帮助管理
            //as操作符不会做过的转换操作，
            //当需要转化对象的类型属于转换目标类型或者转换目标类型的派生类型时，
            //那么此转换操作才能成功，而且并不产生新的对象
            //【当不成功的时候，会返回null】。因此用as进行类型转换是安全的。
            (eventRegistration as EventRegistration).OnEvent += onEvent;
        }
        else
        {
            Debug.Log("注册了一个新的事件类型");
            eventRegistration = new EventRegistration() {
                OnEvent = onEvent};
            eventRegistrationsDict.Add(type, eventRegistration);
    }
        
    }
    //
    public void UnRegist<T>(Action<object> onEvent)
    {
        var type = typeof(T);
        IEventRegistration eventRegistration;
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistration))
        {
            //as操作符不会做过的转换操作，
            //当需要转化对象的类型属于转换目标类型或者转换目标类型的派生类型时，
            //那么此转换操作才能成功，而且并不产生新的对象
            //【当不成功的时候，会返回null】。因此用as进行类型转换是安全的。
            (eventRegistration as EventRegistration).OnEvent -= onEvent;
        }
    }
    public void Send<T>(object obj) where T : new()
    {
        var type = typeof(T);
        IEventRegistration eventRegistration;
        if (eventRegistrationsDict.TryGetValue(type, out eventRegistration))
        {
            //as操作符不会做过的转换操作，
            //当需要转化对象的类型属于转换目标类型或者转换目标类型的派生类型时，
            //那么此转换操作才能成功，而且并不产生新的对象
            //【当不成功的时候，会返回null】。因此用as进行类型转换是安全的。
            (eventRegistration as EventRegistration).OnEvent.Invoke(obj);
        }
    }
}

