using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace LazyCache.UnitTests
{
    [TestFixture]
    public class ServiceCacheTests
    {
        [TearDown]
        public void TearDown()
        {
            MemoryCache.Default.Remove(TestKey);
        }

        private class ComplexTestObject
        {
            public const string SomeMessage = "testing123";
            public readonly IList<object> SomeItems = new object[] {1, 2, 3, "testing123"};
        }

        private const string TestKey = "testKey";

        [Test]
        public void AddComplexObjectThenGetGenericReturnsCachedObject()
        {
            var sut = new CachingService();
            sut.Add(TestKey, new ComplexTestObject());
            var actual = sut.Get<ComplexTestObject>(TestKey);
            var expected = new ComplexTestObject();
            Assert.AreEqual(ComplexTestObject.SomeMessage, ComplexTestObject.SomeMessage);
            Assert.AreEqual(expected.SomeItems, actual.SomeItems);
        }

        [Test]
        public void AddObjectRaisesItemAddedEvent()
        {
            var addedEventFired = false;

            var sut = new CachingService();
            sut.ItemAddedEvent += (sender, args) => {
                addedEventFired = true;
                Assert.AreEqual(TestKey, args.Key);
                Assert.AreEqual(CacheChangedEventArgs.ChangeType.Added, args.Change);
            };

            sut.Add(TestKey, new ComplexTestObject());
            Assert.IsTrue(addedEventFired);
        }

        [Test]
        [Ignore("Not a simple way to handle updates at the moment with introducing more locking.")]
        public void AddObjectAlreadyCachedDoesNotRaiseItemAddedEvent()
        {
            var sut = new CachingService();
            sut.Add(TestKey, new ComplexTestObject());

            sut.ItemAddedEvent += (sender, args) => {
                Assert.Fail("Mistakes were made.");
            };

            // really this will "update" the item already cached by this key
            sut.Add(TestKey, new ComplexTestObject());
        }

        [Test]
        public void GetOrAddObjectRaisesItemAddedEvent()
        {
            var addedEventFired = false;

            var sut = new CachingService();
            sut.ItemAddedEvent += (sender, args) => {
                addedEventFired = true;
                Assert.AreEqual(TestKey, args.Key);
                Assert.AreEqual(CacheChangedEventArgs.ChangeType.Added, args.Change);
            };

            sut.GetOrAdd(TestKey, () => new ComplexTestObject());
            Assert.IsTrue(addedEventFired);
        }

        [Test]
        public void GetOrAddObjectAlreadyCachedDoesNotRaiseItemAddedEvent()
        {
            var sut = new CachingService();
            sut.GetOrAdd(TestKey, () => new ComplexTestObject());

            sut.ItemAddedEvent += (sender, args) => {
                Assert.Fail("Mistakes were made.");
            };

            sut.GetOrAdd(TestKey, () => new ComplexTestObject());
        }

        [Test]
        public void GetOrAddObjectAlreadyCachedRaisesItemRetrievedEvent()
        {
            var eventFired = false;
            var sut = new CachingService();

            sut.ItemRetrievedEvent += (sender, args) => {
                eventFired = true;
                Assert.AreEqual(TestKey, args.Key);
                Assert.AreEqual(CacheChangedEventArgs.ChangeType.Retrieved, args.Change);
            };

            sut.GetOrAdd(TestKey, () => new ComplexTestObject());
            Assert.IsFalse(eventFired);

            sut.GetOrAdd(TestKey, () => new ComplexTestObject());
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void GetObjectAlreadyCachedRaisesItemRetrievedEvent()
        {
            var eventFired = false;
            var sut = new CachingService();

            sut.ItemRetrievedEvent += (sender, args) => {
                eventFired = true;
                Assert.AreEqual(TestKey, args.Key);
                Assert.AreEqual(CacheChangedEventArgs.ChangeType.Retrieved, args.Change);
            };

            sut.Add(TestKey, new ComplexTestObject());

            sut.Get<ComplexTestObject>(TestKey);
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void RemoveItemRaisesItemRemovedEvent()
        {
            var eventFired = false;

            var sut = new CachingService();
            sut.ItemRemovedEvent += (sender, args) => {
                eventFired = true;
                Assert.AreEqual(TestKey, args.Key);
                Assert.AreEqual(CacheChangedEventArgs.ChangeType.Removed, args.Change);
            };

            sut.Add(TestKey, new ComplexTestObject());
            sut.Remove(TestKey);
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void AddComplexObjectTwiceThenGetReturnsUpdatedObject()
        {
            var sut = new CachingService();
            sut.Add(TestKey, new ComplexTestObject());

            var o = new ComplexTestObject();
            o.SomeItems[2] = "updated";
            sut.Add(TestKey, o);

            var actual = sut.Get<ComplexTestObject>(TestKey);
            
            Assert.AreEqual(o.SomeItems[2], actual.SomeItems[2]);
            Assert.AreEqual("updated", actual.SomeItems[2]);
        }

        [Test]
        public void AddComplexObjectThenGetReturnsCachedObject()
        {
            var sut = new CachingService();
            sut.Add(TestKey, new ComplexTestObject());
            var actual = sut.Get<ComplexTestObject>(TestKey);
            var expected = new ComplexTestObject();
            Assert.AreEqual(ComplexTestObject.SomeMessage, ComplexTestObject.SomeMessage);
            Assert.AreEqual(expected.SomeItems, actual.SomeItems);
        }

        [Test]
        public void AddEmptyKeyThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.Add("", new object());
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithOffsetExpiration()
        {
            var sut = new CachingService();
            Action act = () => sut.Add("", new object(), DateTimeOffset.Now.AddHours(1));
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithSlidingExpiration()
        {
            var sut = new CachingService();
            Action act = () => sut.Add("", new object(), TimeSpan.FromHours(1));
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithPolicy()
        {
            var sut = new CachingService();
            Action act = () => sut.Add("", new object(), new CacheItemPolicy());
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithSliding()
        {
            var sut = new CachingService();
            Action act = () => sut.Add("", new object(), new TimeSpan(1000));
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void ConstructWithNullCacheThrowsException()
        {
            Action act = () => new CachingService(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.Add(null, new object());
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithOffsetExpiration()
        {
            var sut = new CachingService();
            Action act = () => sut.Add(null, new object(), DateTimeOffset.Now.AddHours(1));
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithSlidingExpiration()
        {
            var sut = new CachingService();
            Action act = () => sut.Add(null, new object(), TimeSpan.FromHours(1));
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithPolicy()
        {
            var sut = new CachingService();
            Action act = () => sut.Add(null, new object(), new CacheItemPolicy());
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithSliding()
        {
            var sut = new CachingService();
            Action act = () => sut.Add(null, new object(), new TimeSpan(1000));
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.Add<object>(TestKey, null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddThenGetReturnsCachedObject()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject");
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithOffsetReturnsCachedItem()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject", DateTimeOffset.Now.AddSeconds(1));
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }
        
        [Test]
        public void AddWithOffsetThatExpiresReturnsNull()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject", DateTimeOffset.Now.AddSeconds(1));
            Thread.Sleep(1500);
            Assert.IsNull(sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithPolicyReturnsCachedItem()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject", new CacheItemPolicy());
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithSlidingReturnsCachedItem()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject", new TimeSpan(5000));
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithSlidingThatExpiresReturnsNull()
        {
            var sut = new CachingService();
            sut.Add(TestKey, "testObject", new TimeSpan(750));
            Thread.Sleep(1500);
            Assert.IsNull(sut.Get<string>(TestKey));
        }

        [Test]
        public void GetCachedNullableStructTypeParamReturnsType()
        {
            var sut = new CachingService();
            DateTime? cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached.Value, sut.Get<DateTime>(TestKey));
        }

        [Test]
        public void GetEmptyKeyThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.Get<object>("");
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void GetOrAddNullFuncThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.GetOrAdd<object>(TestKey, null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void GetFromCacheTwiceAtSameTimeOnlyAddsOnce()
        {
            var times = 0;
            var sut = new CachingService();
            var t1 = Task.Factory.StartNew(() =>
            {
                sut.GetOrAdd(TestKey, () =>
                {
                    Interlocked.Increment(ref times);
                    return new DateTime(2001, 01, 01);
                });
            });

            var t2 = Task.Factory.StartNew(() =>
            {
                sut.GetOrAdd(TestKey, () =>
                {
                    Interlocked.Increment(ref times);
                    return new DateTime(2001, 01, 01);
                });
            });

            Task.WaitAll(t1, t2);

            Assert.AreEqual(1, times);
        }

        [Test]
        public void GetNullKeyThrowsException()
        {
            var sut = new CachingService();
            Action act = () => sut.Get<object>(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void GetOrAddAndThenGetObjectReturnsCorrectType()
        {
            var sut = new CachingService();
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetOrAddAndThenGetValueObjectReturnsCorrectType()
        {
            var sut = new CachingService();
            Func<int> fetch = () => 123;
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<int>(TestKey);
            Assert.AreEqual(123, actual);
        }

        [Test]
        public void GetOrAddAndThenGetWrongtypeObjectReturnsNull()
        {
            var sut = new CachingService();
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<Exception>(TestKey);
            Assert.IsNull(actual);
        }

        [Test]
        public void GetOrAddWillAddOnFirstCall()
        {
            var times = 0;
            var sut = new CachingService();

            var expected = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            });
            Assert.AreEqual(2001, expected.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        public void GetOrAddWillAddOnFirstCallButReturnCachedOnSecond()
        {
            var times = 0;
            var sut = new CachingService();

            var expectedFirst = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            });

            var expectedSecond = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2002, 01, 01);
            });

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        public void GetOrAddWillNotAddIfExistingData()
        {
            var times = 0;
            var sut = new CachingService();
            var cached = new DateTime(1999, 01, 01);
            sut.Add(TestKey, cached);

            var expected = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            });
            Assert.AreEqual(1999, expected.Year);
            Assert.AreEqual(0, times);
        }

        [Test]
        public void GetOrAddWithOffsetWillAddAndReturnCached()
        {
            var sut = new CachingService();

            var expectedFirst = sut.GetOrAdd(
                TestKey,
                () => new DateTime(2001, 01, 01),
                DateTimeOffset.Now.AddSeconds(5)
                );
            var expectedSecond = sut.Get<DateTime>(TestKey);

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
        }

        [Test]
        public void GetOrAddWithPolicyAndThenGetObjectReturnsCorrectType()
        {
            var sut = new CachingService();
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch, new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddHours(1), Priority = CacheItemPriority.NotRemovable});
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetOrAddWithPolicyAndThenGetValueObjectReturnsCorrectType()
        {
            var sut = new CachingService();
            Func<int> fetch = () => 123;
            sut.GetOrAdd(TestKey, fetch, new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddHours(1), Priority = CacheItemPriority.NotRemovable});
            var actual = sut.Get<int>(TestKey);
            Assert.AreEqual(123, actual);
        }

        [Test]
        public void GetOrAddWithPolicyWillAddOnFirstCallButReturnCachedOnSecond()
        {
            var times = 0;
            var sut = new CachingService();

            var expectedFirst = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            }, new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddHours(1), Priority = CacheItemPriority.NotRemovable});

            var expectedSecond = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2002, 01, 01);
            }, new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddHours(1), Priority = CacheItemPriority.NotRemovable});

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        public void GetWithClassTypeParamReturnsType()
        {
            var sut = new CachingService();
            var cached = new EventArgs();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<EventArgs>(TestKey));
        }

        [Test]
        public void GetWithIntRetunsDefualtIfNotCached()
        {
            var sut = new CachingService();
            Assert.AreEqual(default(int), sut.Get<int>(TestKey));
        }

        [Test]
        public void GetWithNullableIntRetunsCachedNonNullableInt()
        {
            var sut = new CachingService();
            const int expected = 123;
            sut.Add(TestKey, expected);
            Assert.AreEqual(expected, sut.Get<int?>(TestKey));
        }

        [Test]
        public void GetWithNullableStructTypeParamReturnsType()
        {
            var sut = new CachingService();
            var cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<DateTime?>(TestKey));
        }

        [Test]
        public void GetWithStructTypeParamReturnsType()
        {
            var sut = new CachingService();
            var cached = new DateTime(2000, 1, 1);
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<DateTime>(TestKey));
        }

        [Test]
        public void GetWithValueTypeParamReturnsType()
        {
            var sut = new CachingService();
            const int cached = 3;
            sut.Add(TestKey, cached);
            Assert.AreEqual(3, sut.Get<int>(TestKey));
        }

        [Test]
        public void GetWithWrongClassTypeParamReturnsNull()
        {
            var sut = new CachingService();
            var cached = new EventArgs();
            sut.Add(TestKey, cached);
            Assert.IsNull(sut.Get<ArgumentNullException>(TestKey));
        }

        [Test]
        public void GetWithWrongStructTypeParamReturnsNull()
        {
            var sut = new CachingService();
            var cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(new TimeSpan(), sut.Get<TimeSpan>(TestKey));
        }

        [Test]
        public void RemovedItemCannotBeRetrievedFromCache()
        {
            var sut = new CachingService();
            sut.Add(TestKey, new object());
            Assert.NotNull(sut.Get<object>(TestKey));
            sut.Remove(TestKey);
            Assert.Null(sut.Get<object>(TestKey));
        }

        [Test, Timeout(20000)]
        public void GetOrAddWithPolicyWithCallbackOnRemovedReturnsTheOriginalCachedObject()
        {
            var sut = new CachingService();
            Func<int> fetch = () => 123;
            CacheEntryRemovedArguments removedCallbackArgs = null;
            CacheEntryRemovedCallback callback = (args) => removedCallbackArgs = args;
            
            
            sut.GetOrAdd(TestKey, fetch, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100), RemovedCallback = callback});
            var actual = sut.Get<int>(TestKey);
            
            sut.Remove(TestKey); //force removed callback to fire
            while(removedCallbackArgs == null)
                Thread.Sleep(500);
            
            Assert.AreEqual(123, removedCallbackArgs.CacheItem.Value); 
        }

        [Test, Timeout(20000)]
        public void GetOrAddWithCallbackOnRemovedReturnsTheOriginalCachedObjectEvenIfNotGettedBeforehand()
        {
            var sut = new CachingService();
            Func<int> fetch = () => 123;
            CacheEntryRemovedArguments removedCallbackArgs = null;
            CacheEntryRemovedCallback callback = (args) => removedCallbackArgs = args;
            sut.GetOrAdd(TestKey, fetch, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100), RemovedCallback = callback });

            sut.Remove(TestKey); //force removed callback to fire
            while (removedCallbackArgs == null)
                Thread.Sleep(500);

            Assert.AreEqual(123, removedCallbackArgs.CacheItem.Value);
        }
    }
}