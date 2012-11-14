using System;
using System.Net.Mail;
using NUnit.Framework;

namespace SmtpServer.Tests.Common
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
            var address = new MailAddress(addressString);

            Assert.AreEqual("user", address.User, "Username incorrect");
            Assert.AreEqual("mydomain.com", address.Host, "Domain incorrect");
            Assert.AreEqual(addressString, address.Address, "Address incorrect");
            Assert.AreEqual(addressString, address.ToString(), "ToString incorrect");
        }

        [Test]
        public void TestAddress1()
        {
            const string addressString = "eric@ericdomain.com";
            var address = new MailAddress(addressString);

            Assert.AreEqual("eric", address.User, "Username incorrect");
            Assert.AreEqual("ericdomain.com", address.Host, "Domain incorrect");
            Assert.AreEqual(addressString, address.Address, "Address incorrect");
            Assert.AreEqual(addressString, address.ToString(), "ToString incorrect");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestAddressInvalid1()
        {
            new MailAddress("us@er@mydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestAddressInvalid2()
        {
            new MailAddress("user@mydo:main.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestAddressNoAt()
        {
            new MailAddress("usermydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestAddressNoDomain()
        {
            new MailAddress("user@");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestAddressNoUsername()
        {
            new MailAddress("@mydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestEmptyDomain()
        {
            new MailAddress("user", "");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestEmptyUsername()
        {
            new MailAddress("", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidDomain1()
        {
            new MailAddress("user", "mydom]in.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidDomain2()
        {
            new MailAddress("user", "mydo;main.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidDomain3()
        {
            new MailAddress("user", "mydo\"main.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidUsername2()
        {
            new MailAddress("m<e", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestInvalidUsername3()
        {
            new MailAddress("m:e", "mydomain.com");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void TestNullDomain()
        {
            new MailAddress("user", null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullUsername()
        {
            new MailAddress(null, "mydomain.com");
        }

        [Test]
        public void TestValidUsername()
        {
            var address = new MailAddress("user@mydomain.com");

            Assert.IsTrue("user".Equals(address.User), "Username incorrect");
            Assert.IsTrue("mydomain.com".Equals(address.Host), "Domain incorrect");
            Assert.IsTrue("user@mydomain.com".Equals(address.Address), "Address incorrect");
            Assert.IsTrue("user@mydomain.com".Equals(address.ToString()), "ToString incorrect");

            address = new MailAddress("my_name@my_domain.com");
            address = new MailAddress("my_name100@mydomain.com");
        }
    }
}