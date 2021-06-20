using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;

namespace LazyCache.UnitTests
{
    [TestFixture]
    public class CachingServiceMemoryCacheProviderTests
    {
        private static CachingService BuildCache()
        {
            return new CachingService(new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions())));
        }

        private IAppCache sut;

        private readonly MemoryCacheEntryOptions oneHourNonRemoveableMemoryCacheEntryOptions =
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1),
                Priority = CacheItemPriority.NeverRemove
            };

        private ComplexTestObject testObject = new ComplexTestObject();

        private class ComplexTestObject
        {
            public readonly IList<object> SomeItems = new List<object> { 1, 2, 3, "testing123" };
            public string SomeMessage = "testing123";
        }

        private const string TestKey = "testKey";

        [SetUp]
        public void BeforeEachTest()
        {
            sut = BuildCache();
            testObject = new ComplexTestObject();
        }


        [Test]
        public void AddComplexObjectThenGetGenericReturnsCachedObject()
        {
            testObject.SomeItems.Add("Another");
            testObject.SomeMessage = "changed-it-up";
            sut.Add(TestKey, testObject);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            var expected = testObject;
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual);
            testObject.SomeItems.Should().Contain("Another");
            testObject.SomeMessage.Should().Be("changed-it-up");
        }

        [Test]
        public void AddComplexObjectThenGetReturnsCachedObject()
        {
            sut.Add(TestKey, testObject);
            var actual = sut.Get<object>(TestKey) as ComplexTestObject;
            var expected = testObject;
            Assert.NotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddEmptyKeyThrowsException()
        {
            Action act = () => sut.Add("", new object());
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithExpiration()
        {
            Action act = () => sut.Add("", new object(), DateTimeOffset.Now.AddHours(1));
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithPolicy()
        {
            Action act = () => sut.Add("", new object(), new MemoryCacheEntryOptions());
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddEmptyKeyThrowsExceptionWithSliding()
        {
            Action act = () => sut.Add("", new object(), new TimeSpan(1000));
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void AddNullKeyThrowsException()
        {
            Action act = () => sut.Add(null, new object());
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithExpiration()
        {
            Action act = () => sut.Add(null, new object(), DateTimeOffset.Now.AddHours(1));
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithPolicy()
        {
            Action act = () => sut.Add(null, new object(), new MemoryCacheEntryOptions());
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddNullKeyThrowsExceptionWithSliding()
        {
            Action act = () => sut.Add(null, new object(), new TimeSpan(1000));
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void AddNullDoesNotThrowException()
        {
            Action act = () => sut.Add<object>(TestKey, null);
            act.Should().NotThrow<ArgumentNullException>();
        }

        [Test]
        public void AddNullThenGetReturnsCachedNullReference()
        {
            const string testValue = null;

            sut.Add(TestKey, testValue);

            Assert.IsNull(sut.Get<string>(TestKey));
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
            sut.Add(TestKey, "testObject", new MemoryCacheEntryOptions());
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
        public void CacheProviderIsNotNull()
        {
            sut.CacheProvider.Should().NotBeNull();
        }

        [Test]
        public void DefaultContructorThenGetOrAddFromSecondCachingServiceHasSharedUnderlyingCache()
        {
            var cacheOne = new CachingService();
            var cacheTwo = new CachingService();

            var resultOne = cacheOne.GetOrAdd(TestKey, () => "resultOne");
            var resultTwo = cacheTwo.GetOrAdd(TestKey, () => "resultTwo"); // should not get executed

            resultOne.Should().Be("resultOne", "GetOrAdd should execute the delegate");
            resultTwo.Should().Be("resultOne", "CachingService should use a shared cache by default");
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
            act.Should().Throw<ArgumentOutOfRangeException>();
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
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void GetOrAddAndThenGetObjectReturnsCorrectType()
        {
            sut.GetOrAdd(TestKey, () => testObject);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetOrAddAndThenGetOrAddDifferentTypeDoesLastInWins()
        {
            var first = sut.GetOrAdd(TestKey, () => new object());
            var second = sut.GetOrAdd(TestKey, () => testObject);
            Assert.IsNotNull(second);
            Assert.IsInstanceOf<ComplexTestObject>(second);
        }

        [Test]
        public void GetOrAddAndThenGetValueObjectReturnsCorrectType()
        {
            sut.GetOrAdd(TestKey, () => 123);
            var actual = sut.Get<int>(TestKey);
            Assert.AreEqual(123, actual);
        }

        [Test]
        public void GetOrAddAndThenGetWrongtypeObjectReturnsNull()
        {
            sut.GetOrAdd(TestKey, () => testObject);
            var actual = sut.Get<ApplicationException>(TestKey);
            Assert.IsNull(actual);
        }



        [Test]
        public void GetOrAddAsyncACancelledTaskDoesNotCacheIt()
        {
            Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await sut.GetOrAddAsync(TestKey, AsyncHelper.CreateCancelledTask<ComplexTestObject>));

            var stillCached = sut.Get<Task<ComplexTestObject>>(TestKey);

            Assert.That(stillCached, Is.Null);
        }

        [Test]
        public void GetOrAddAsyncACancelledTaskReturnsTheCacelledTaskToConsumer()
        {
            var cancelledTask = sut.GetOrAddAsync(TestKey, AsyncHelper.CreateCancelledTask<ComplexTestObject>);

            Assert.That(cancelledTask, Is.Not.Null);

            Assert.Throws<AggregateException>(cancelledTask.Wait);

            Assert.That(cancelledTask.IsCanceled, Is.True);
        }

        [Test]
        public void GetOrAddAsyncAFailingTaskDoesNotCacheIt()
        {
            Task<ComplexTestObject> FetchAsync()
            {
                return Task<ComplexTestObject>.Factory.StartNew(() => throw new ApplicationException());
            }

            Assert.ThrowsAsync<ApplicationException>(async () => await sut.GetOrAddAsync(TestKey, FetchAsync));

            var stillCached = sut.Get<Task<ComplexTestObject>>(TestKey);

            Assert.That(stillCached, Is.Null);
        }

        [Test]
        public async Task GetOrAddAsyncAndThenGetAsyncObjectReturnsCorrectType()
        {
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(testObject));
            var actual = await sut.GetAsync<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
            Assert.That(actual, Is.EqualTo(testObject));
        }

        [Test]
        public async Task GetOrAddAsyncAndThenGetOrAddAsyncDifferentTypeDoesLastInWins()
        {
            var first = await sut.GetOrAddAsync(TestKey, () => Task.FromResult(new object()));
            var second = await sut.GetOrAddAsync(TestKey, () => Task.FromResult(testObject));
            Assert.IsNotNull(second);
            Assert.IsInstanceOf<ComplexTestObject>(second);
        }

        [Test]
        public async Task GetOrAddAsyncAndThenGetAsyncWrongObjectReturnsNull()
        {
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(testObject));
            var actual = await sut.GetAsync<ApplicationException>(TestKey);
            Assert.IsNull(actual);
        }

        [Test]
        public async Task GetOrAddAsyncFollowinGetOrAddReturnsTheFirstObjectAndIgnoresTheSecondTask()
        {
            ComplexTestObject FetchSync()
            {
                return testObject;
            }

            Task<ComplexTestObject> FetchAsync()
            {
                return Task.FromResult(new ComplexTestObject());
            }

            var actualSync = sut.GetOrAdd(TestKey, FetchSync);
            var actualAsync = await sut.GetOrAddAsync(TestKey, FetchAsync);

            Assert.IsNotNull(actualSync);
            Assert.That(actualSync, Is.EqualTo(testObject));

            Assert.IsNotNull(actualAsync);
            Assert.That(actualAsync, Is.EqualTo(testObject));

            Assert.AreEqual(actualAsync, actualSync);
        }

        [Test]
        public async Task GetOrAddAsyncTaskAndThenGetTaskOfAnotherTypeReturnsNull()
        {
            var cachedAsyncResult = testObject;
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(cachedAsyncResult));
            var actual = sut.Get<Task<ApplicationException>>(TestKey);
            Assert.Null(actual);
        }

        [Test]
        public async Task GetOrAddAsyncTaskAndThenGetTaskOfObjectReturnsCorrectType()
        {
            var cachedAsyncResult = testObject;
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(cachedAsyncResult));
            var actual = sut.Get<Task<ComplexTestObject>>(TestKey);
            Assert.IsNotNull(actual);
            Assert.That(actual.Result, Is.EqualTo(cachedAsyncResult));
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

        [Test]
        [MaxTime(1000)]
        public void GetOrAddAsyncWithALongTaskReturnsBeforeTaskCompletes()
        {
            var cachedResult = testObject;

            Task<ComplexTestObject> FetchAsync()
            {
                return Task.Delay(TimeSpan.FromMinutes(1))
                    .ContinueWith(x => cachedResult);
            }

            var actualResult = sut.GetOrAddAsync(TestKey, FetchAsync);

            Assert.That(actualResult, Is.Not.Null);
            Assert.That(actualResult.IsCompleted, Is.Not.True);
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
        public async Task GetOrAddAsyncWithPolicyAndThenGetTaskObjectReturnsCorrectType()
        {
            var item = testObject;
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(item),
                oneHourNonRemoveableMemoryCacheEntryOptions);
            var actual = await sut.Get<Task<ComplexTestObject>>(TestKey);
            Assert.That(actual, Is.EqualTo(item));
        }

        [Test]
        [MaxTime(20000)]
        public async Task GetOrAddAsyncWithPostEvictionCallbacksReturnsTheOriginalCachedKeyEvenIfNotGettedBeforehand()
        {
            string callbackKey = null;
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100)
            };
            memoryCacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                callbackKey = key.ToString();
            });
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(123), memoryCacheEntryOptions);

            sut.Remove(TestKey); //force removed callback to fire
            while (callbackKey == null)
                Thread.Sleep(500);

            callbackKey.Should().Be(TestKey);
        }

        [Test]
        [MaxTime(20000)]
        public async Task
            GetOrAddAsyncWithPostEvictionCallbacksReturnsTheOriginalCachedObjectEvenIfNotGettedBeforehand()
        {
            object callbackValue = null;
            var memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100)
            };
            memoryCacheEntryOptions.RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                callbackValue = value;
            });
            await sut.GetOrAddAsync(TestKey, () => Task.FromResult(123), memoryCacheEntryOptions);

            sut.Remove(TestKey); //force removed callback to fire
            while (callbackValue == null)
                Thread.Sleep(500);

            Assert.That(callbackValue, Is.AssignableTo<Task<int>>());
            var callbackResultValue = await (Task<int>)callbackValue;
            Assert.AreEqual(123, callbackResultValue);
        }

        [Test]
        public async Task GetOrAddAyncAllowsCachingATask()
        {
            var cachedResult = testObject;

            Task<ComplexTestObject> FetchAsync()
            {
                return Task.FromResult(cachedResult);
            }

            var actualResult =
                await sut.GetOrAddAsync(TestKey, FetchAsync, oneHourNonRemoveableMemoryCacheEntryOptions);

            Assert.That(actualResult, Is.EqualTo(cachedResult));
        }

        [Test]
        public async Task GetOrAddFollowinGetOrAddAsyncReturnsTheFirstObjectAndUnwrapsTheFirstTask()
        {
            Task<ComplexTestObject> FetchAsync()
            {
                return Task.FromResult(testObject);
            }

            ComplexTestObject FetchSync()
            {
                return new ComplexTestObject();
            }

            var actualAsync = await sut.GetOrAddAsync(TestKey, FetchAsync);
            var actualSync = sut.GetOrAdd(TestKey, FetchSync);

            Assert.IsNotNull(actualAsync);
            Assert.That(actualAsync, Is.EqualTo(testObject));

            Assert.IsNotNull(actualSync);
            Assert.That(actualSync, Is.EqualTo(testObject));

            Assert.AreEqual(actualAsync, actualSync);
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
        public void GetOrAddWithAbsoluteOffsetExpiryAsDateTimeOffsetDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = sut.GetOrAdd(
                TestKey,
                () => new ComplexTestObject(),
                DateTimeOffset.Now.AddMilliseconds(millisecondsCacheDuration)
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public void GetOrAddWithAbsoluteOffsetExpiryAsTimeSpanDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = sut.GetOrAdd(
                TestKey,
                () => new ComplexTestObject(),
                TimeSpan.FromMilliseconds(millisecondsCacheDuration)
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public void GetOrAddWithAbsoluteOffsetExpiryInTheDelegateDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = sut.GetOrAdd(
                TestKey,
                entry =>
                {
                    entry.SetAbsoluteExpiration(DateTimeOffset.Now.AddMilliseconds(millisecondsCacheDuration));
                    return new ComplexTestObject();
                }
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public void GetOrAddWithAbsoluteOffsetExpiryInTheDelegateUsingTimeSpanDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = sut.GetOrAdd(
                TestKey,
                entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMilliseconds(millisecondsCacheDuration));
                    return new ComplexTestObject();
                }
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public async Task GetOrAddAsyncWithAbsoluteOffsetExpiryAsDateTimeOffsetDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                () => Task.FromResult(new ComplexTestObject()),
                DateTimeOffset.Now.AddMilliseconds(millisecondsCacheDuration)
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public async Task GetOrAddAsyncWithAbsoluteOffsetExpiryAsTimeSpanDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                () => Task.FromResult(new ComplexTestObject()),
                TimeSpan.FromMilliseconds(millisecondsCacheDuration)
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public async Task GetOrAddAsyncWithAbsoluteOffsetExpiryInTheDelegateDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                entry =>
                {
                    entry.SetAbsoluteExpiration(DateTimeOffset.Now.AddMilliseconds(millisecondsCacheDuration));
                    return Task.FromResult(new ComplexTestObject());
                }
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public async Task GetOrAddAsyncWithAbsoluteOffsetExpiryInTheDelegateUsingTimeSpanDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                entry =>
                {
                    entry.SetAbsoluteExpiration(TimeSpan.FromMilliseconds(millisecondsCacheDuration));
                    return Task.FromResult(new ComplexTestObject());
                }
            );
            // pass expiry time with a delay
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));
            var expiredResult = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(expiredResult, Is.Null);
        }

        [Test]
        public void GetOrAddWithCancellationExpiryBasedOnTimerAndCallbackInTheDelegateDoesExpireItemsAndFireTheCallback()
        {
            var millisecondsCacheDuration = 100;
            var callbackHasFired = false;
            var tokenSource = new CancellationTokenSource(millisecondsCacheDuration);
            var expireToken = new CancellationChangeToken(tokenSource.Token);
            var validResult = sut.GetOrAdd(
                TestKey,
            entry =>
            {
                return new ComplexTestObject();
            }, new MemoryCacheEntryOptions()
                    .AddExpirationToken(expireToken)
                    .RegisterPostEvictionCallback((key, value, reason, state) => callbackHasFired = true));
            // trigger expiry
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));

            Assert.That(validResult, Is.Not.Null);
            Assert.That(callbackHasFired, Is.True);
        }

        [Test]
        public void GetOrAddWithImmediateExpirationAndCallbackInTheDelegateDoesExpireItemsAndFireTheCallback()
        {
            var millisecondsCacheDuration = 100;
            var callbackHasFired = false;
            var validResult = sut.GetOrAdd(
                TestKey,
                entry =>
                {
                    return new ComplexTestObject();
                }, LazyCacheEntryOptions
                    .WithImmediateAbsoluteExpiration(TimeSpan.FromMilliseconds(millisecondsCacheDuration))
                    .RegisterPostEvictionCallback((key, value, reason, state) => callbackHasFired = true));
            // trigger expiry
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));

            Assert.That(validResult, Is.Not.Null);
            Assert.That(callbackHasFired, Is.True);
        }

        [Test]
        public async Task GetOrAddAsyncWithImmediateExpirationAndCallbackInTheDelegateDoesExpireItemsAndFireTheCallback()
        {
            var millisecondsCacheDuration = 1000;
            var callbackHasFired = false;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                entry =>
                {
                    return Task.FromResult(new ComplexTestObject());
                }, LazyCacheEntryOptions
                    .WithImmediateAbsoluteExpiration(TimeSpan.FromMilliseconds(millisecondsCacheDuration))
                    .RegisterPostEvictionCallback((key, value, reason, state) => callbackHasFired = true));
            // trigger expiry
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 1000));

            Assert.That(validResult, Is.Not.Null);
            Assert.That(callbackHasFired, Is.True);
        }

        [Test]
        public async Task AutoRefresh()
        {
            var key = "someKey";
            var refreshInterval = TimeSpan.FromSeconds(1);
            var timesGenerated = 0;

            // this is the Func what we are caching 
            ComplexTestObject GetStuff()
            {
                timesGenerated++;
                return new ComplexTestObject();
            }

            // this sets up options that will recreate the entry on eviction
            MemoryCacheEntryOptions GetOptions()
            {
                var options = new LazyCacheEntryOptions()
                    .SetAbsoluteExpiration(refreshInterval, ExpirationMode.ImmediateEviction);
                options.RegisterPostEvictionCallback((keyEvicted, value, reason, state) =>
                {
                    if (reason == EvictionReason.Expired || reason == EvictionReason.TokenExpired)
                        sut.GetOrAdd(key, _ => GetStuff(), GetOptions());
                });
                return options;
            }

            for (var i = 0; i < 3; i++)
            {
                var thing = sut.GetOrAdd(key, () => GetStuff(), GetOptions());
                Assert.That(thing, Is.Not.Null);
                await Task.Delay(2 * refreshInterval);
            }

            // refreshed every second in 6 seconds so generated 6 times
            // even though we only fetched it every other second which would be 3 times
            Assert.That(timesGenerated, Is.EqualTo(6));
        }

        [Test]
        public async Task GetOrAddAsyncWithImmediateExpirationDoesExpireItems()
        {
            var millisecondsCacheDuration = 100;
            var validResult = await sut.GetOrAddAsync(
                TestKey,
                () =>
                {
                    return Task.FromResult(new ComplexTestObject());
                }, DateTimeOffset.UtcNow.AddMilliseconds(millisecondsCacheDuration), ExpirationMode.ImmediateEviction);
            // trigger expiry
            Thread.Sleep(TimeSpan.FromMilliseconds(millisecondsCacheDuration + 50));

            var actual = sut.Get<ComplexTestObject>(TestKey);

            Assert.That(validResult, Is.Not.Null);
            Assert.That(actual, Is.Null);
        }

        [Test]
        [Ignore("Not a real unit tests - just used for hammering the cache")]
        public async Task PerfTest()
        {
            var watch = new Stopwatch();
            watch.Start();
            var asyncThreads = 10;
            var syncThreads = 10;
            var uniqueCacheItems = 20;
            int cacheMiss = 0;
            int hits = 0;
            var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            async Task<ComplexTestObject> GetStuffAsync()
            {
                await Task.Delay(25);
                Interlocked.Increment(ref cacheMiss);
                return new ComplexTestObject();
            }

            ComplexTestObject GetStuff()
            {
                Thread.Sleep(25);
                Interlocked.Increment(ref cacheMiss);
                return new ComplexTestObject();
            }


            var asyncActions = Task.Run(() =>
            {
                Parallel.For(1, asyncThreads, async i =>
                {
                    while (!cancel.IsCancellationRequested)
                    {
                        var key = $"stuff-{hits % uniqueCacheItems}";
                        var cached = await sut.GetOrAddAsync(key, () => GetStuffAsync(), DateTimeOffset.UtcNow.AddSeconds(1));
                        if (!cancel.IsCancellationRequested) Interlocked.Increment(ref hits);
                    }
                });
            });

            var syncActions = Task.Run(() =>
            {
                Parallel.For(1, syncThreads, i =>
                {
                    while (!cancel.IsCancellationRequested)
                    {
                        var key = $"stuff-{hits % uniqueCacheItems}";
                        var cached = sut.GetOrAdd(key, () => GetStuff(), DateTimeOffset.UtcNow.AddSeconds(1));
                        if (!cancel.IsCancellationRequested) Interlocked.Increment(ref hits);
                    }
                });
            });

            await Task.WhenAll(asyncActions, syncActions);

            watch.Stop();
            Console.WriteLine(watch.Elapsed);
            Console.WriteLine("miss " + cacheMiss);
            Console.WriteLine("hit " + hits);
        }


        [Test]
        public void GetOrAddWithPolicyAndThenGetObjectReturnsCorrectType()
        {
            sut.GetOrAdd(TestKey, () => testObject,
                oneHourNonRemoveableMemoryCacheEntryOptions);
            var actual = sut.Get<ComplexTestObject>(TestKey);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void GetOrAddWithPolicyAndThenGetValueObjectReturnsCorrectType()
        {
            int Fetch()
            {
                return 123;
            }

            sut.GetOrAdd(TestKey, Fetch, oneHourNonRemoveableMemoryCacheEntryOptions);
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
            }, oneHourNonRemoveableMemoryCacheEntryOptions);

            var expectedSecond = sut.GetOrAdd(TestKey, () =>
            {
                times++;
                return new DateTime(2002, 01, 01);
            }, oneHourNonRemoveableMemoryCacheEntryOptions);

            Assert.AreEqual(2001, expectedFirst.Year);
            Assert.AreEqual(2001, expectedSecond.Year);
            Assert.AreEqual(1, times);
        }

        [Test]
        [MaxTime(20000)]
        public void GetOrAddWithPostEvictionCallbackdReturnsTheOriginalCachedObjectEvenIfNotGettedBeforehand()
        {
            object cacheValue = null;
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMilliseconds(100)
            }.RegisterPostEvictionCallback((key, value, reason, state) => cacheValue = value);
            sut.GetOrAdd(TestKey, () => 123,
                cacheEntryOptions);

            sut.Remove(TestKey); //force removed callback to fire
            while (cacheValue == null)
                Thread.Sleep(500);

            cacheValue.Should().BeOfType<int>();
            cacheValue.Should().Be(123);
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

        [Test]
        public void TryGetReturnsCachedValueAndTrue()
        {
            string val = "Test Value";
            string key = "testkey";
            sut.Add(key, val);

            var contains = sut.TryGetValue<string>(key, out var value);

            Assert.IsTrue(contains);
            Assert.AreEqual(value, val);

            var contains2 = sut.TryGetValue<string>("invalidkey", out var value2);

            Assert.IsFalse(contains2);
        }
    }
}