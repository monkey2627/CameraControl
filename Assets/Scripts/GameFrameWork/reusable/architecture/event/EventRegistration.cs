using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class EventRegistration:IEventRegistration
{
    //Action<object> 表参数为object类型，没有返回值的委托
    public Action<object> OnEvent = obj => { };
    //知识点 lambda表达式和匿名函数
    //1.匿名函数
    //2.lambda表达式：一种匿名函数的实现方式
    /* => 是lambda表达式的标志 （）表参数 Func是内置的有返回值的委托
        Func<int, string,int> C = (int i, string s) =>
        {
            int q = Int32.Parse(s) + i;
            return q;
        };
        // 委托已经指定类型，可以省略参数类型
        Func<int, string,int> C = (i, s) =>
        {
            int q = Int32.Parse(s) + i;
            return q;
        };
     */
}

