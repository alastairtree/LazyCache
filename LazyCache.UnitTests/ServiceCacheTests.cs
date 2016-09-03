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

        [SetUp]
        public void BeforeEachTest()
        {

            sut = new CachingService();
        }

        private CachingService sut;

        private readonly CacheItemPolicy oneHourNonRemoveableCacheItemPolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
            Priority = CacheItemPriority.NotRemovable
        };

        private class ComplexTestObject
        {
            public const string SomeMessage = "testing123";
            public readonly IList<object> SomeItems = new object[] {1, 2, 3, "testing123"};
        }

        private const string TestKey = "testKey";


        [Test]
        public void AddComplexObjectThenGetGenericReturnsCachedObject()
        {
            sut.Add(TestKey, new ComplexTestObject());
            var actual = sut.Get<ComplexTestObject>(TestKey);
            var expected = new ComplexTestObject();
            Assert.AreEqual(ComplexTestObject.SomeMessage, ComplexTestObject.SomeMessage);
            Assert.AreEqual(expected.SomeItems, actual.SomeItems);
        }

        [Test]
        public void AddComplexObjectThenGetReturnsCachedObject()
        {
            sut.Add(TestKey, new ComplexTestObject());
            var actual = sut.Get<ComplexTestObject>(TestKey);
            var expected = new ComplexTestObject();
            Assert.AreEqual(ComplexTestObject.SomeMessage, ComplexTestObject.SomeMessage);
            Assert.AreEqual(expected.SomeItems, actual.SomeItems);
        }

        [Test]
        public void AddEmptyKeyThrowsException()
        {
            Action act = () => sut.Add("", new object());
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithExpiration()
        {
            Action act = () => sut.Add("", new object(), DateTimeOffset.Now.AddHours(1));
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithPolicy()
        {
            Action act = () => sut.Add("", new object(), new CacheItemPolicy());
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithSliding()
        {
            Action act = () => sut.Add("", new object(), new TimeSpan(1000));
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddNullKeyThrowsException()
        {
            Action act = () => sut.Add(null, new object());
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithExpiration()
        {
            Action act = () => sut.Add(null, new object(), DateTimeOffset.Now.AddHours(1));
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithPolicy()
        {
            Action act = () => sut.Add(null, new object(), new CacheItemPolicy());
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithSliding()
        {
            Action act = () => sut.Add(null, new object(), new TimeSpan(1000));
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullThrowsException()
        {
            Action act = () => sut.Add<object>(TestKey, null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void AddThenGetReturnsCachedObject()
        {
            sut.Add(TestKey, "testObject");
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithOffsetReturnsCachedItem()
        {
            sut.Add(TestKey, "testObject", DateTimeOffset.Now.AddSeconds(1));
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithOffsetThatExpiresReturnsNull()
        {
            sut.Add(TestKey, "testObject", DateTimeOffset.Now.AddSeconds(1));
            Thread.Sleep(1500);
            Assert.IsNull(sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithPolicyReturnsCachedItem()
        {
            sut.Add(TestKey, "testObject", new CacheItemPolicy());
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithSlidingReturnsCachedItem()
        {
            sut.Add(TestKey, "testObject", new TimeSpan(5000));
            Assert.AreEqual("testObject", sut.Get<string>(TestKey));
        }

        [Test]
        public void AddWithSlidingThatExpiresReturnsNull()
        {
            sut.Add(TestKey, "testObject", new TimeSpan(750));
            Thread.Sleep(1500);
            Assert.IsNull(sut.Get<string>(TestKey));
        }

        [Test]
        public void GetCachedNullableStructTypeParamReturnsType()
        {
            DateTime? cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached.Value, sut.Get<DateTime>(TestKey));
        }

        [Test]
        public void GetEmptyKeyThrowsException()
        {
            Action act = () => sut.Get<object>("");
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void GetFromCacheTwiceAtSameTimeOnlyAddsOnce()
        {
            var times = 0;

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
            Action act = () => sut.Get<object>(null);
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void GetOrAddAndThenGetObjectReturnsCorrectType()
        {
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public async Task GetOrAddAsyncTaskAndThenGetTaskOfObjectReturnsCorrectType()
        {
            var cachedAsyncResult = new ComplexTestObject();
            Func<Task<ComplexTestObject>> fetch = () => Task.FromResult(cachedAsyncResult);
            await sut.GetOrAddAsync(TestKey, fetch);
            var actual = sut.Get<Task<ComplexTestObject>>(TestKey);
            Assert.IsNotNull(actual);
            Assert.That(actual.Result, Is.EqualTo(cachedAsyncResult));
        }

        [Test]
        public void GetOrAddAndThenGetValueObjectReturnsCorrectType()
        {
            Func<int> fetch = () => 123;
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<int>(TestKey);
            Assert.AreEqual(123, actual);
        }

        [Test]
        public void GetOrAddAndThenGetWrongtypeObjectReturnsNull()
        {
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch);
            var actual = sut.Get<ApplicationException>(TestKey);
            Assert.IsNull(actual);
        }

        [Test]
        public async Task GetOrAddAsyncTaskAndThenGetTaskOfAnotherTypeReturnsNull()
        {
            var cachedAsyncResult = new ComplexTestObject();
            Func<Task<ComplexTestObject>> fetch = () => Task.FromResult(cachedAsyncResult);
            await sut.GetOrAddAsync(TestKey, fetch);
            var actual = sut.Get<Task<ApplicationException>>(TestKey);
            Assert.Null(actual);
        }

        [Test]
        public async Task GetOrAddAyncAllowsCachingATask()
        {
            var cachedResult = new ComplexTestObject();
            Func<Task<ComplexTestObject>> fetchAsync = () => Task.FromResult(cachedResult);

            var actualResult = await sut.GetOrAddAsync(TestKey, fetchAsync, oneHourNonRemoveableCacheItemPolicy);

            Assert.That(actualResult, Is.EqualTo(cachedResult));
        }

        [Test]
        public void GetOrAddWillAddOnFirstCall()
        {
            var times = 0;


            var expected = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            });
            Assert.AreEqual(2001, expected.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        public async Task GetOrAddAsyncWillAddOnFirstCall()
        {
            var times = 0;

            var expected = await sut.GetOrAddAsync(TestKey, () =>
            {
                times++;
                return Task.FromResult(new DateTime(2001, 01, 01));
            });
            Assert.AreEqual(2001, expected.Year);
            Assert.AreEqual(1, times);
        }


        [Test]
        public void GetOrAddWillAddOnFirstCallButReturnCachedOnSecond()
        {
            var times = 0;

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
        public async Task GetOrAddAsyncWillAddOnFirstCallButReturnCachedOnSecond()
        {
            var times = 0;

            var expectedFirst = await sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return Task.FromResult(new DateTime(2001, 01, 01));
            });

            var expectedSecond = await sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return Task.FromResult(new DateTime(2002, 01, 01));
            });

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        public void GetOrAddWillNotAddIfExistingData()
        {
            var times = 0;

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
        public async Task GetOrAddAsyncWillNotAddIfExistingData()
        {
            var times = 0;

            var cached = new DateTime(1999, 01, 01);
            sut.Add(TestKey, cached);

            var expected = await sut.GetOrAddAsync(TestKey, () =>
            {
                times++;
                return Task.FromResult(new DateTime(2001, 01, 01));
            });
            Assert.AreEqual(1999, expected.Year);
            Assert.AreEqual(0, times);
        }

        [Test, Timeout(20000)]
        public void GetOrAddWithCallbackOnRemovedReturnsTheOriginalCachedObjectEvenIfNotGettedBeforehand()
        {
            Func<int> fetch = () => 123;
            CacheEntryRemovedArguments removedCallbackArgs = null;
            CacheEntryRemovedCallback callback = args => removedCallbackArgs = args;
            sut.GetOrAdd(TestKey, fetch,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100),
                    RemovedCallback = callback
                });

            sut.Remove(TestKey); //force removed callback to fire
            while (removedCallbackArgs == null)
                Thread.Sleep(500);

            Assert.AreEqual(123, removedCallbackArgs.CacheItem.Value);
        }

        [Test, Timeout(20000)]
        public async Task GetOrAddAsyncWithCallbackOnRemovedReturnsTheOriginalCachedObjectEvenIfNotGettedBeforehand()
        {
            Func<Task<int>> fetch = () => Task.FromResult(123);
            CacheEntryRemovedArguments removedCallbackArgs = null;
            CacheEntryRemovedCallback callback = args => removedCallbackArgs = args;
            await sut.GetOrAddAsync(TestKey, fetch,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100),
                    RemovedCallback = callback
                });

            sut.Remove(TestKey); //force removed callback to fire
            while (removedCallbackArgs == null)
                Thread.Sleep(500);

            var callbackResult = removedCallbackArgs.CacheItem.Value;
            Assert.That(callbackResult, Is.AssignableTo<Task<int>>());
            var callbackResultValue = await (Task<int>) removedCallbackArgs.CacheItem.Value;

            Assert.AreEqual(123, callbackResultValue);
        }

        [Test]
        public void GetOrAddWithOffsetWillAddAndReturnCached()
        {
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
        public async Task GetOrAddAsyncWithOffsetWillAddAndReturnTaskOfCached()
        {
            var expectedFirst = await sut.GetOrAddAsync(
                TestKey,
                () => Task.FromResult(new DateTime(2001, 01, 01)),
                DateTimeOffset.Now.AddSeconds(5)
                );
            var expectedSecond = await sut.Get<Task<DateTime>>(TestKey);

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
        }

        [Test]
        public void GetOrAddWithPolicyAndThenGetObjectReturnsCorrectType()
        {
            Func<ComplexTestObject> fetch = () => new ComplexTestObject();
            sut.GetOrAdd(TestKey, fetch,
                oneHourNonRemoveableCacheItemPolicy);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public async Task GetOrAddAsyncWithPolicyAndThenGetTaskObjectReturnsCorrectType()
        {
            var item = new ComplexTestObject();
            Func<Task<ComplexTestObject>> fetch = () => Task.FromResult(item);
            await sut.GetOrAddAsync(TestKey, fetch,
                oneHourNonRemoveableCacheItemPolicy);
            var actual = await sut.Get<Task<ComplexTestObject>>(TestKey);
            Assert.That(actual, Is.EqualTo(item));
        }

        [Test]
        public void GetOrAddWithPolicyAndThenGetValueObjectReturnsCorrectType()
        {
            Func<int> fetch = () => 123;
            sut.GetOrAdd(TestKey, fetch, oneHourNonRemoveableCacheItemPolicy);
            var actual = sut.Get<int>(TestKey);
            Assert.AreEqual(123, actual);
        }

        [Test]
        public void GetOrAddWithPolicyWillAddOnFirstCallButReturnCachedOnSecond()
        {
            var times = 0;


            var expectedFirst = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2001, 01, 01);
            }, oneHourNonRemoveableCacheItemPolicy);

            var expectedSecond = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2002, 01, 01);
            }, oneHourNonRemoveableCacheItemPolicy);

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
            Assert.AreEqual(1, times);
        }

        [Test, Timeout(20000)]
        public void GetOrAddWithPolicyWithCallbackOnRemovedReturnsTheOriginalCachedObject()
        {
            Func<int> fetch = () => 123;
            CacheEntryRemovedArguments removedCallbackArgs = null;
            CacheEntryRemovedCallback callback = args => removedCallbackArgs = args;


            sut.GetOrAdd(TestKey, fetch,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100),
                    RemovedCallback = callback
                });
            var actual = sut.Get<int>(TestKey);

            sut.Remove(TestKey); //force removed callback to fire
            while (removedCallbackArgs == null)
                Thread.Sleep(500);

            Assert.AreEqual(123, removedCallbackArgs.CacheItem.Value);
        }

        [Test]
        public void GetWithClassTypeParamReturnsType()
        {
            var cached = new EventArgs();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<EventArgs>(TestKey));
        }

        [Test]
        public void GetWithIntRetunsDefaultIfNotCached()
        {
            Assert.AreEqual(default(int), sut.Get<int>(TestKey));
        }

        [Test]
        public void GetWithNullableIntRetunsCachedNonNullableInt()
        {
            const int expected = 123;
            sut.Add(TestKey, expected);
            Assert.AreEqual(expected, sut.Get<int?>(TestKey));
        }

        [Test]
        public void GetWithNullableStructTypeParamReturnsType()
        {
            var cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<DateTime?>(TestKey));
        }

        [Test]
        public void GetWithStructTypeParamReturnsType()
        {
            var cached = new DateTime(2000, 1, 1);
            sut.Add(TestKey, cached);
            Assert.AreEqual(cached, sut.Get<DateTime>(TestKey));
        }

        [Test]
        public void GetWithValueTypeParamReturnsType()
        {
            const int cached = 3;
            sut.Add(TestKey, cached);
            Assert.AreEqual(3, sut.Get<int>(TestKey));
        }

        [Test]
        public void GetWithWrongClassTypeParamReturnsNull()
        {
            var cached = new EventArgs();
            sut.Add(TestKey, cached);
            Assert.IsNull(sut.Get<ArgumentNullException>(TestKey));
        }

        [Test]
        public void GetWithWrongStructTypeParamReturnsNull()
        {
            var cached = new DateTime();
            sut.Add(TestKey, cached);
            Assert.AreEqual(new TimeSpan(), sut.Get<TimeSpan>(TestKey));
        }

        [Test]
        public void RemovedItemCannotBeRetrievedFromCache()
        {
            sut.Add(TestKey, new object());
            Assert.NotNull(sut.Get<object>(TestKey));
            sut.Remove(TestKey);
            Assert.Null(sut.Get<object>(TestKey));
        }
    }
}