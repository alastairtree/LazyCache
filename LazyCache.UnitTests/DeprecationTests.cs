using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LazyCache.UnitTests
{
    public class DeprecationTests
    {
        [Test]
        public void AllImplementationsOfIAppCache_HaveTheAddMethod_MarkedAsObsolete()
        {
            var classesWithoutObsoleteTag = new List<Type>();

            foreach (var type in GetTypesWithIAppCache())
            {
                if (MethodDoesNotHaveObsoleteAttribute(type))
                {
                    classesWithoutObsoleteTag.Add(type);
                }
            }
            CollectionAssert.IsEmpty(classesWithoutObsoleteTag);
        }

        private IEnumerable<Type> GetTypesWithIAppCache()
        {
            var iAppCache = typeof(IAppCache);
            return Assembly.GetAssembly(iAppCache)
                .GetTypes()
                .Where(iAppCache.IsAssignableFrom);
        }

        private bool MethodDoesNotHaveObsoleteAttribute(Type type)
        {
            var method = type.GetMethods()
                .SingleOrDefault(m => m.Name == nameof(IAppCache.Add));
            var attribute = method?.GetCustomAttributes(typeof(ObsoleteAttribute), true)
                .SingleOrDefault() as ObsoleteAttribute;
            return attribute == null;
        }

    }
}