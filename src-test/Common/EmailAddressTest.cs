using NUnit.Framework;
using src.Common;

namespace src_test.Common
{
    /// <summary>
    /// Test cases for the EmailAddress class.  These tests
    /// also covers the InvalidEmailAddressException class.
    /// </summary>
    /// <remarks>
    /// Created By: Eric Daugherty
    /// </remarks>
    [TestFixture]
    public class EmailAddressTest
    {
        [Test]
        public void TestAddress()
        {
            const string addressString = "user@mydomain.com";
            var address = new EmailAddress(addressString);

            Assert.AreEqual("user", address.Username, "Username incorrect");
            Assert.AreEqual("mydomain.com", address.Domain, "Domain incorrect");
            Assert.AreEqual(addressString, address.Address, "Address incorrect");
            Assert.AreEqual(addressString, address.ToString(), "ToString incorrect");
        }

        [Test]
        public void TestAddress1()
        {
            const string addressString = "eric@ericdomain.com";
            var address = new EmailAddress(addressString);

            Assert.AreEqual("eric", address.Username, "Username incorrect");
            Assert.AreEqual("ericdomain.com", address.Domain, "Domain incorrect");
            Assert.AreEqual(addressString, address.Address, "Address incorrect");
            Assert.AreEqual(addressString, address.ToString(), "ToString incorrect");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestAddressInvalid1()
        {
            new EmailAddress("us@er@mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestAddressInvalid2()
        {
            new EmailAddress("user@mydo:main.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestAddressNoAt()
        {
            new EmailAddress("usermydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestAddressNoDomain()
        {
            new EmailAddress("user@");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestAddressNoUsername()
        {
            new EmailAddress("@mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestEmptyDomain()
        {
            new EmailAddress("user", "");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestEmptyUsername()
        {
            new EmailAddress("", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidDomain1()
        {
            new EmailAddress("user", "mydom]in.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidDomain2()
        {
            new EmailAddress("user", "mydo;main.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidDomain3()
        {
            new EmailAddress("user", "mydo\"main.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidUsername1()
        {
            new EmailAddress("m@e", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidUsername2()
        {
            new EmailAddress("m<e", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestInvalidUsername3()
        {
            new EmailAddress("m:e", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestNullDomain()
        {
            new EmailAddress("user", null);
        }

        [Test]
        [ExpectedException(typeof (InvalidEmailAddressException))]
        public void TestNullUsername()
        {
            new EmailAddress(null, "mydomain.com");
        }

        [Test]
        public void TestValidUsername()
        {
            var address = new EmailAddress("us er", "mydomain.com");

            Assert.IsTrue("us er".Equals(address.Username), "Username incorrect");
            Assert.IsTrue("mydomain.com".Equals(address.Domain), "Domain incorrect");
            Assert.IsTrue("us er@mydomain.com".Equals(address.Address), "Address incorrect");
            Assert.IsTrue("us er@mydomain.com".Equals(address.ToString()), "ToString incorrect");

            address = new EmailAddress("my_name", "my_domain.com");
            address = new EmailAddress("my_name100@mydomain.com");
        }
    }
}