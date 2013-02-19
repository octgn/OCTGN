namespace Octgn.Online.Library.SignalR.TypeSafe
{
    using System;
    using System.Collections.Generic;
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
            this.ProxyCalls = new Dictionary<int, DynamicProxyOnBuilder>();
            this.Instance = this.CreateInstance();
            // Since HandleCall is called dynamically, call it once so it doesn't
            // get optimized away
            this.HandleCall(0);
        }

        public DynamicProxyOnBuilder On(Expression<Func<T, Action>> expression)
        {
            MethodCallExpression methExpression = null;
            var obj = (expression.Body is UnaryExpression ? ((UnaryExpression)expression.Body).Operand : expression.Body);
            var callExpression = obj as MethodCallExpression;
            if (callExpression != null) methExpression = callExpression;
            else if (obj is LambdaExpression) methExpression = (obj as LambdaExpression).Body as MethodCallExpression;
            if (methExpression != null)
            {
                var methodInfo = methExpression.Method;
                return this.ProxyCalls[methodInfo.GetHashCode()];
            }
            throw new Exception("I'm confused...");
        }

        public DynamicProxyOnBuilder OnAll()
        {
            var builder = new DynamicProxyOnBuilder();
            foreach (var m in typeof(T).GetMethods())
            {
                this.ProxyCalls[m.GetHashCode()] = builder;
            }
            return builder;
        }

        /// <summary>
        /// Don't call this...
        /// </summary>
        /// <param name="code">Hash code of method</param>
        /// <param name="args">Arguments passed to method</param>
        public void HandleCall(int code, params object[] args)
        {
            //TODO Make this internal or private somehow
            DynamicProxyOnBuilder builder;
            if (this.ProxyCalls.TryGetValue(code, out builder))
            {
                var methodInfo = typeof(T).GetMethods().First(x => x.GetHashCode() == code);
                var mi = new MethodCallInfo { Args = args, Method = methodInfo };
                builder.ThisCalls.Invoke(mi);
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
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public,CallingConventions.HasThis,new[] { this.GetType() });
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
                this.ProxyCalls.Add(methodInfo.GetHashCode(), emptyAction);
                // Drop method info hash into local variable 0 
                var hash = methodInfo.GetHashCode();
                methodIlGen.Emit(OpCodes.Ldc_I4, hash);
                methodIlGen.DeclareLocal(typeof(Int32), false);
                methodIlGen.Emit(OpCodes.Stloc_0);

                // Loads the 'proxy' public field
                methodIlGen.Emit(OpCodes.Ldarg_0);
                methodIlGen.Emit(OpCodes.Ldfld, proxyField);
                // Load the local variable 0 and pass it as first parameter
                methodIlGen.Emit(OpCodes.Ldloc_0);

                // Create object array for the rest of the parameters
                methodIlGen.Emit(OpCodes.Ldc_I4,methodInfo.GetParameters().Length);
                methodIlGen.Emit(OpCodes.Newarr,typeof(Object));
                methodIlGen.DeclareLocal(typeof(Object[]), false);
                methodIlGen.Emit(OpCodes.Stloc_1);
                // Load the rest of the passed parameters in
                var currentLoc = 0;
                var args = methodInfo.GetParameters();
                foreach (var arg in args)
                {
                    methodIlGen.Emit(OpCodes.Ldloc_1);// Load array onto stack
                    methodIlGen.Emit(OpCodes.Ldc_I4, currentLoc); // Load array index onto stack
                    methodIlGen.Emit(OpCodes.Ldarg, (short)currentLoc + 1); //Load array value to stack
                    if (IsSimpleType(arg.ParameterType))
                        methodIlGen.Emit(OpCodes.Box, arg.ParameterType);// Box if primitive
                    methodIlGen.Emit(OpCodes.Stelem_Ref);// Add ref to array
                    currentLoc++;
                }
                methodIlGen.Emit(OpCodes.Ldloc_1);
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
#if(DEBUG)
            //Debug.WriteLine(Directory.GetCurrentDirectory());
            //assBuilder.Save("test.dll");
#endif
            return (T)instance;
        }
        public static bool IsSimpleType(Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new[] { 
				//typeof(String),
				typeof(Decimal),
				typeof(DateTime),
				typeof(DateTimeOffset),
				typeof(TimeSpan),
				typeof(Guid)
			}.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }
    }
}