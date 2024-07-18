using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions.Execution;
using NSubstitute.Core;
using NSubstitute.Core.Arguments;

namespace NSubstitute
{
    public static class Verify
    {
        private static readonly ArgumentFormatter DefaultArgumentFormatter = new ArgumentFormatter();
        /// <summary>
        /// Enqueues a matcher for the method argument in current position and returns the value which should be
        /// passed back to the method you invoke.
        /// </summary>
        /// <remarks>Temporary replacement for <see cref="ArgumentMatcher.Enqueue{T}"/> because of issue in NSubstitute output for expected parameter description for custom <see cref="IArgumentMatcher{T}"/> implementations. (https://github.com/nsubstitute/NSubstitute/issues/796) </remarks>
        internal static ref T? Enqueue<T>(IArgumentMatcher argumentMatcher)
        {
            SubstitutionContext.Current.ThreadContext.EnqueueArgumentSpecification(new ArgumentSpecification(typeof(T), argumentMatcher));
            return ref new DefaultValueContainer<T>().Value;
        }

        private class DefaultValueContainer<T>
        {
            public T? Value;
        }
        
        /// <summary>
        /// Verify the NSubstitute call using FluentAssertions.
        /// </summary>
        /// <example>
        /// <code>
        /// var sub = Substitute.For&lt;ISomeInterface&gt;();
        /// sub.InterestingMethod("Hello hello");
        ///
        /// sub.Received().InterestingMethod(Verify.That&lt;string&gt;(s => s.Should().StartWith("hello").And.EndWith("goodbye")));
        /// </code>
        /// Results in the failure message:
        /// <code>
        /// Expected to receive a call matching:
        ///     SomeMethod("
        /// Expected string to start with
        /// "hello", but
        /// "Hello hello" differs near "Hel" (index 0).
        /// Expected string
        /// "Hello hello" to end with
        /// "goodbye".")
        /// Actually received no matching calls.
        /// Received 1 non-matching call(non-matching arguments indicated with '*' characters):
        ///     SomeMethod(*"Hello hello"*)
        /// </code>
        /// </example>
        /// <typeparam name="T">Type of argument to verify.</typeparam>
        /// <param name="action">Action in which to perform FluentAssertions verifications.</param>
        /// <returns></returns>
        public static ref T? That<T>(Action<T?> action)
            => ref Enqueue<T>(new AssertionMatcher<T>(action));

        private class AssertionMatcher<T> : IArgumentMatcher, IArgumentMatcher<T>
        {
            private readonly Action<T?> assertion;
            private string allFailures = "";

            public AssertionMatcher(Action<T?> assertion)
                => this.assertion = assertion;
            
            bool IArgumentMatcher.IsSatisfiedBy(object? argument) => IsSatisfiedBy((T?)argument);

            public bool IsSatisfiedBy(T? argument)
            {
                using (var scope = new AssertionScope())
                {
                    try
                    {
                        assertion(argument);
                    }
                    catch (Exception exception)
                    {
                        var f = scope.Discard();
                        allFailures = f.Any() ? AggregateFailures(f) : exception.Message;
                        return false;
                    }

                    var failures = scope.Discard().ToList();

                    if (failures.Count == 0) return true;

                    allFailures = AggregateFailures(failures);

                    return false;
                }
            }

            private string AggregateFailures(IEnumerable<string> discard)
                => discard.Aggregate(allFailures, (a, b) => a + "\n" + b);

            public override string ToString()
                => DefaultArgumentFormatter.Format(allFailures, false);
        }
    }
}
