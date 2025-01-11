using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
//架构的抽象基 类
public abstract class Architexture<T> : IArchitexture where T:new()
{
    private IOCContainer iocContainer = new IOCContainer();
    private GameEventSystem gameEventSystem = new GameEventSystem();

    /*
        抽象类不能实例化。
        抽象类可以包含抽象方法和抽象访问器。
        不能用 sealed 修饰符修饰抽象类，因为这两个修饰符的含义是相反的。 采用 sealed 修饰符的类无法继 承，而 abstract 修饰符要求对类进行继承。
        从抽象类派生的非抽象类必须包括继承的所有抽象方法和抽象访问器的实际实现。
     */
    public Architexture() { Init(); }
    //子类不会继承父类的构造方法，实例化子类的时候会先调用父类的构造函数，
    //再调用子类的构造函数
    protected abstract void Init();
    public void InitAllModules()
    {
        iocContainer.InitAllModules();
    }
    public void RegistEvent<U>(Action<object> onEvent) where U : new()
    {
       gameEventSystem.Regist<U>(onEvent);
    }

    public void RegistSystem<U>(U instance) where U : ISystem
    {
        iocContainer.Register<U>(instance);
    }

    public void RegistModel<U>(U instance) where U : IModel
    {
        iocContainer.Register<U>(instance);
    }

    public void RegistUtility<U>(U instance) where U : IUtility
    {
        iocContainer.Register<U>(instance);
    }
    public void SendCommand<U>(object dataObj) where U : ICommand,new()
    {
        var command = new U();
        command.Execute(dataObj);
    }   
    public void SendEvent<U>(object dataObj) where U : new()
    {
        gameEventSystem.Send<U>(dataObj);
    }  
    public void UnRegistEvent<U>(Action<object> onEvent) where U : new()
    {
        gameEventSystem.UnRegist<U>(onEvent);
    }
    public U GetSystem<U>() where U:class,ISystem
    {
        return iocContainer.Get<U>();
    }

    public U GetModel<U>() where U : class,IModel 
    {
        return iocContainer.Get<U>();
    }

    public U GetUtility<U>() where U : class, IUtility
    {
        return iocContainer.Get<U>();
    }

  
 

 

   

}

