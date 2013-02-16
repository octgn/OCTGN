namespace Octgn.Online.Library.SignalR
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    public class DynamicProxy<T> 
    {
        public static DynamicProxy<T> Get()
        {
            return new DynamicProxy<T>();
        }

        public T Instance { get; private set; }

        private Dictionary<int, DynamicProxyOnBuilder> ProxyCalls { get; set; } 

        public DynamicProxy()
        {
            ProxyCalls = new Dictionary<int, DynamicProxyOnBuilder>();
            Instance = CreateInstance();
            // Since HandleCall is called dynamically, call it once so it doesn't
            // get optimized away
            this.HandleCall(0);
        }

        public DynamicProxyOnBuilder On(Expression<Func<T, Action>> expression)
        {
            // Vodo to get the method we want.
            //var exp = expression.Body;
            //while (exp.NodeType != ExpressionType.Call)
            //{
            //    switch (exp.NodeType)
            //    {
            //        case ExpressionType.Call:
            //            break;
            //        case ExpressionType.Lambda:
            //            var la = expression as LambdaExpression;
            //            exp = la.Body;
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}
            //var unaryExpression = exp as UnaryExpression;

            //var methodCallExpression = unaryExpression.Operand as MethodCallExpression;
            //var obj = methodCallExpression.Object as ConstantExpression;
            MethodCallExpression methExpression = null;
            var obj = (expression.Body is UnaryExpression ? ((UnaryExpression)expression.Body).Operand : expression.Body);
            if (obj is MethodCallExpression) methExpression = (MethodCallExpression)obj;
            else if (obj is LambdaExpression) methExpression = (obj as LambdaExpression).Body as MethodCallExpression;
            var methodInfo = methExpression.Method;
            return ProxyCalls[methodInfo.GetHashCode()];
        }

        //public DynamicProxyOnBuilder On(Expression<Func<T, Action<dynamic,dynamic>>>  expression)
        //{
        //    // Vodo to get the method we want.
        //    var unaryExpression = expression.Body as UnaryExpression;
        //    var methodCallExpression = unaryExpression.Operand as MethodCallExpression;
        //    var obj = methodCallExpression.Object as ConstantExpression;
        //    var methodInfo = obj.Value as MethodInfo;
        //    return ProxyCalls[methodInfo.GetHashCode()];
        //}

        public DynamicProxyOnBuilder OnAll()
        {
            var builder = new DynamicProxyOnBuilder();
            foreach (var m in typeof(T).GetMethods())
            {
                ProxyCalls[m.GetHashCode()] = builder;
            }
            return builder;
        }

        /// <summary>
        /// Don't call this...
        /// </summary>
        /// <param name="code"></param>
        public void HandleCall(int code)
        {
            //TODO Make this internal or private somehow
            DynamicProxyOnBuilder builder = null;
            if (ProxyCalls.TryGetValue(code, out builder))
            {
                builder.ThisCalls.Invoke();
            }
        }

        private T CreateInstance()
        {
            var typeOfT = typeof(T);
            var methodInfos = typeOfT.GetMethods();
            var assName = new AssemblyName(Guid.NewGuid().ToString());
            var assBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assName, AssemblyBuilderAccess.RunAndSave);
            var moduleBuilder = assBuilder.DefineDynamicModule("DynamicModule", "test.dll");
            var typeBuilder = moduleBuilder.DefineType(typeOfT.Name + "Proxy", TypeAttributes.Public);

            #region blah
            typeBuilder.AddInterfaceImplementation(typeOfT);
            var proxyField = typeBuilder.DefineField("proxy", this.GetType(), FieldAttributes.Public);
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,CallingConventions.HasThis,new Type[] { this.GetType() });
            var ilGenerator = ctorBuilder.GetILGenerator();

            //Loads the first argument into a field.
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg, (short)1);
            ilGenerator.Emit(OpCodes.Stfld, proxyField);
            ilGenerator.Emit(OpCodes.Ret);

            foreach (var methodInfo in methodInfos)
            {
                var methodParams = methodInfo.GetParameters().Select(p => p.ParameterType).ToArray();
                var methodBuilder = typeBuilder.DefineMethod(
                    methodInfo.Name,
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    methodInfo.ReturnType, 
                    methodParams
                    );
                var methodIlGen = methodBuilder.GetILGenerator();

                // Add proxy call to method
                var emptyAction = new DynamicProxyOnBuilder();
                ProxyCalls.Add(methodInfo.GetHashCode(), emptyAction);
                // Drop method info hash into local variable 0 
                var hash = methodInfo.GetHashCode();
                methodIlGen.Emit(OpCodes.Ldc_I4, hash);
                methodIlGen.DeclareLocal(typeof(Int32), false);
                methodIlGen.Emit(OpCodes.Stloc_0);

                // Loads the 'proxy' public field
                methodIlGen.Emit(OpCodes.Ldarg_0);
                methodIlGen.Emit(OpCodes.Ldfld, proxyField);
                // Load the local variable 0
                methodIlGen.Emit(OpCodes.Ldloc_0);
                methodIlGen.Emit(OpCodes.Callvirt, this.GetType().GetMethods().First(x => x.Name == "HandleCall"));
                methodIlGen.Emit(OpCodes.Nop);
                if (methodInfo.ReturnType == typeof(void))
                {
                    methodIlGen.Emit(OpCodes.Ret);
                }
                else
                {
                    if (methodInfo.ReturnType.IsValueType || methodInfo.ReturnType.IsEnum)
                    {
                        var getMethod = typeof(Activator).GetMethod("CreateInstance", new[] { typeof(Type) });
                        var lb = methodIlGen.DeclareLocal(methodInfo.ReturnType);
                        methodIlGen.Emit(OpCodes.Ldtoken, lb.LocalType);
                        methodIlGen.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                        methodIlGen.Emit(OpCodes.Callvirt, getMethod);
                        methodIlGen.Emit(OpCodes.Unbox_Any, lb.LocalType);
                    }
                    else
                    {
                        methodIlGen.Emit(OpCodes.Ldnull);
                    }
                    methodIlGen.Emit(OpCodes.Ret);
                }
                typeBuilder.DefineMethodOverride(methodBuilder, methodInfo);
            }
            #endregion

            var constructedType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(constructedType,this);
            return (T)instance;
        }
    }
}