namespace Tests
{
    using FluentAssertions;
    using NSubstitute;
    using System;
    using Xunit;

    public class TestAssertionMessages
    {
        [Fact]
        public void Successful_test_has_no_failure_message()
        {
            var sub = Substitute.For<Bob>();
            sub.SingleParam(1);

            var actual = MessageFor(() => sub.Received().SingleParam(Verify.That<int>(i => i.Should().Be(1))));

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Missing_call_failure_does_not_include_verification_details()
        {
            var sub = Substitute.For<Bob>();

            var actual = MessageFor(() => sub.Received().SingleParam(Verify.That<int>(i => i.Should().Be(1))));

            actual.Should().StartWith("Expected to receive a call matching:\n	SingleParam(\"\")\r\nActually received no matching calls.\r\n");
        }

        [Fact]
        public void Wrong_call_failure_does_includes_verification_details()
        {
            var sub = Substitute.For<Bob>();
            sub.SingleParam(2);

            var actual = MessageFor(() => sub.Received().SingleParam(Verify.That<int>(i => i.Should().Be(1))));

            actual.Should().StartWith("Expected to receive a call matching:\n	SingleParam(\"\nExpected value to be 1, but found 2.\")\r\nActually received no matching calls.\r\n");
        }

        [Fact]
        public void Documentation_example()
        {
            var sub = Substitute.For<Bob>();
            sub.SomeMethod("Hello hello");

            var actual = MessageFor(() => sub.Received().SomeMethod(Verify.That<string>(s => s.Should().StartWith("hello").And.EndWith("goodbye"))));

            actual.Should().Contain("\"Hello hello\" differs near \"Hel\" (index 0).");
        }

        public static string MessageFor(Action act)
        {
            try
            {
                act();
                return "";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public interface Bob
        {
            int SingleParam(int a);

            void SomeMethod(string m);

            int DoubleParam(int a, double y);
        }
    }
}
