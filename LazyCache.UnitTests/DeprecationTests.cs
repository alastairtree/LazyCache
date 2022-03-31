using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace LazyCache.UnitTests
{
    /// <summary>
    /// Tests to confirm that methods intended to be deprecated have been adequately marked as such
    /// </summary>
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
            CollectionAssert.IsEmpty(classesWithoutObsoleteTag, "Types which do not have the Add() method marked as Obsolete");
        }

        [Test]
        public void AllImplementationsOfIAppCache_HaveTheAddMethod_MarkedAsNeverBrowsable()
        {
            var classesWhereAddAppearsBrowsable = new List<Type>();

            foreach (var type in GetTypesWithIAppCache())
            {
                var editorBrowsableState = GetAddMethodsEditorBrowsableState(type);
                if (editorBrowsableState != EditorBrowsableState.Never)
                {
                    classesWhereAddAppearsBrowsable.Add(type);
                }
            }
            CollectionAssert.IsEmpty(classesWhereAddAppearsBrowsable, "Types which have the Add() method not marked as Never browsable");
        }

        [Test]
        public void AppCacheExtensions_AddMethods_AreObsolete()
        {
            var addMethodsWithoutObsoleteAttribute =
                typeof(AppCacheExtensions).GetMethods()
                    .Where(m => m.Name == nameof(AppCacheExtensions.Add)
                                && m.GetCustomAttribute(typeof(ObsoleteAttribute), true) == null);

            CollectionAssert.IsEmpty(addMethodsWithoutObsoleteAttribute, "Add methods not marked as obsolete");
        }

        [Test]
        public void AppCacheExtensions_AddMethods_AreNotBrowsable()
        {
            var addMethodsThatAreBrowsable =
                typeof(AppCacheExtensions)
                    .GetMethods()
                    .Where(m => m.Name == nameof(AppCacheExtensions.Add))
                    .Select(m => (Method: m, Attribute: GetEditorBrowsableAttribute(m)))
                    .Where(ma => ma.Attribute.State != EditorBrowsableState.Never);

            CollectionAssert.IsEmpty(addMethodsThatAreBrowsable.Select(ma => ma.Method), "Add methods not marked as Never browsable");

            static EditorBrowsableAttribute GetEditorBrowsableAttribute(MethodInfo m)
            {
                return m.GetCustomAttributes(typeof(EditorBrowsableAttribute), true).Single() as EditorBrowsableAttribute;
            }
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

        private EditorBrowsableState GetAddMethodsEditorBrowsableState(Type type)
        {
            var addMethod = nameof(IAppCache.Add);
            var method = type.GetMethods()
                .SingleOrDefault(m => m.Name == addMethod);
            var attribute = method?.GetCustomAttributes(typeof(EditorBrowsableAttribute), true)
                .SingleOrDefault() as EditorBrowsableAttribute;
            if (attribute == null)
            {
                throw new InvalidOperationException($"{type.Name}'s {addMethod} method does not have {nameof(EditorBrowsableAttribute)}");
            }
            return attribute.State;
        }
    }
}