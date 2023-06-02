namespace DI_Container
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new DependencyContainer();
            container.AddTransient<ServiceConsumer>();
            container.AddTransient<HelloService>();
            container.AddSingltone<MessageService>();

            var resolver = new DependencyResolver(container);

            var service1 = resolver.GetService<ServiceConsumer>();
            service1.Print();

            var service2 = resolver.GetService<ServiceConsumer>();
            service2.Print();

            var service3 = resolver.GetService<ServiceConsumer>();
            service3.Print();

        }


        public class DependencyResolver
        {
            DependencyContainer _container;
            public DependencyResolver(DependencyContainer container)
            {
                _container = container;
            }
            public T GetService<T>()
            {
                return (T)GetService(typeof(T));
            }
            public object GetService(Type type)
            {
                var dependency = _container.GetDependency(type);
                var constructor = dependency.Type.GetConstructors().Single();
                var parameters = constructor.GetParameters().ToArray();

                if (parameters.Length > 0)
                {
                    var parameterImplementations = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameterImplementations[i] = GetService(parameters[i].ParameterType);
                    }
                    return CreateImplementation(dependency, t => Activator.CreateInstance(t, parameterImplementations));
                }
                return CreateImplementation(dependency, t => Activator.CreateInstance(t));
            }
            public object CreateImplementation(Dependency dependency, Func<Type,object> factory)
            {
                if (dependency.Implemented)
                {
                    return dependency.Implementation;
                }

                var implementation = factory(dependency.Type);

                if (dependency.Lifetime == DependencyLifetime.Singletone)
                {
                    dependency.AddImplementation(implementation);
                }
                return implementation;
            }
        }
        public class DependencyContainer
        {
            List<Dependency> _dependencies;
            public DependencyContainer()
            {
                _dependencies = new List<Dependency>();
            }
            public void AddSingltone<T>()
            {
                _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Singletone));
            }
            public void AddTransient<T>()
            {
                _dependencies.Add(new Dependency(typeof(T), DependencyLifetime.Transient));
            }
            public Dependency GetDependency(Type type)
            {
                return _dependencies.First(x => x.Type.Name == type.Name);
            }
        }

        public class Dependency
        {
            public Type Type { get; set; }
            public Dependency(Type t, DependencyLifetime l)
            {
                Type = t;
                Lifetime = l;
            }
            public DependencyLifetime Lifetime { get; set; }
            public object Implementation { get; set; }
            public bool Implemented { get; set; }
            public void AddImplementation(object i)
            {
                Implementation = i;
                Implemented = true;
            }
        }

        public enum DependencyLifetime
        {
            Singletone = 0, Transient = 1
        }
        public class ServiceConsumer
        {
            HelloService _hello;
            public ServiceConsumer(HelloService hello)
            {
                _hello = hello;
            }
            public void Print()
            {
                _hello.Print();
            }
        }

        public class HelloService
        {
            MessageService _message;
            int _random;
            public HelloService(MessageService message)
            {
                _random = new Random().Next();
                _message = message;
            }
            public void Print()
            {
                Console.WriteLine($"Hello #{_random} World! {_message.Message()}");
            }
        }
        public class MessageService
        {
            public int _random;
            public MessageService()
            {
                _random = new Random().Next();
            }
            public string Message()
            {
                return $"Yo #{_random}";
            }
        }
    }
}