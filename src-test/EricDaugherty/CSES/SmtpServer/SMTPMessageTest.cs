using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using log4net.Config;
using NUnit.Framework;
using EricDaugherty.CSES.Net;

namespace EricDaugherty.CSES.SmtpServer
{
		
	[TestFixture]
	public class SMTPMessageTest
	{
		#region Constants

		private static string ATTACHMENT_1_HEADER_DATA = 
			"Content-Type: application/pdf;\r\n" +
			"	name=\"000000001.pdf\"\r\n" +
			"Content-Transfer-Encoding: base64\r\n" +
			"Content-Disposition: attachment;\r\n" +
			"	filename=\"000000001.pdf\"\r\n" +
			"\r\n";

		private static string ATTACHMENT_1_BODY_DATA = 
			"JVBERi0xLjIKekdf1fnfSqQYt7AjczYfpmRSIEyEcx8KMSAwIG9iago8PAovVHlwZSAvQ2F0YWxv\r\n" +
			"ZwovUGFnZXMgMyAwIFIKL091dGxpbmVzIDIgMCBSCj4+CmVuZG9iagoyIDAgb2JqCjw8Ci9UeXBl\r\n" +
			"IC9PdXRsaW5lcwovQ291bnQgMQovRmlyc3QgOCAwIFIKL0xhc3QgOCAwIFIKPj4KZW5kb2JqCjMg\r\n" +
			"MCBvYmoKPDwKL1R5cGUgL1BhZ2VzCi9Db3VudCAxCi9LaWRzIFsgNSAwIFIgXQo+PgplbmRvYmoK\r\n" +
			"NCAwIG9iagpbL1BERiAvVGV4dCAvSW1hZ2VCIF0KZW5kb2JqCjUgMCBvYmoKPDwKL1R5cGUgL1Bh\r\n" +
			"Z2UKL1BhcmVudCAzIDAgUgovUmVzb3VyY2VzIDw8Ci9YT2JqZWN0IDw8Ci9JTWNwZGYwIDcgMCBS\r\n" +
			"Cj4+Ci9Qcm9jU2V0IDQgMCBSID4+Ci9NZWRpYUJveCBbMCAwIDM5OSA1NjFdCi9Dcm9wQm94IFsw\r\n" +
			"IDAgMzk5IDU2MV0KL1JvdGF0ZSAwCi9Db250ZW50cyA2IDAgUgo+PgplbmRvYmoKNiAwIG9iago8\r\n" +
			"PAovTGVuZ3RoIDQ3Ci9GaWx0ZXIgWy9GbGF0ZURlY29kZV0KPj4Kc3RyZWFtCnic4yrkMtQzAAIF\r\n" +
			"AxQKq2ByLpexpSU2GVMzXBr0PX2TC1LSDBRc8rkCuQCBJhXPCmVuZHN0cmVhbQplbmRvYmoKNyAw\r\n" +
			"IG9iago8PAovVHlwZSAvWE9iamVjdAovU3VidHlwZSAvSW1hZ2UKL05hbWUgL0lNY3BkZjAKL1dp\r\n" +
			"ZHRoIDE2NjQKL0hlaWdodCAyMzM4Ci9GaWx0ZXIgL0ZsYXRlRGVjb2RlCi9CaXRzUGVyQ29tcG9u\r\n" +
			"ZW50IDEKL0NvbG9yU3BhY2UgL0RldmljZUdyYXkKL0xlbmd0aCAyNTAxMgo+PgpzdHJlYW0KeJzs\r\n" +
			"vV1sJEl+4BfZ2epsnSjmyGvguBBV0cIKXr+Z47HhGlxtxQgyTjBwwD74xfCXejyG51FsD2xVQ6WK\r\n" +
			"\r\n";

		private string TEST_SINGLE_BASE64 = 
			"Received: from development02 (development02 [127.0.0.1])\r\n" +
			"     by adexs.com (Eric Daugherty's C# Email Server)\r\n" +
			"     4/16/2004 10:35:58 AM\r\n" +
			"From: \"Eric Daugherty\" <edaugherty@adexs.com>\r\n" +
			"To: <test@test.com>\r\n" +
			"Subject: CofAs\r\n" +
			"Date: Fri, 16 Apr 2004 10:35:55 -0500\r\n" +
			"Message-ID: <LIEBLGPCEJMNGHIPABGIGEABCAAA.edaugherty@adexs.com>\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: multipart/mixed;\r\n" +
			"	boundary=\"----=_NextPart_000_0000_01C4239E.999350F0\"\r\n" +
			"X-Priority: 3 (Normal)\r\n" +
			"X-MSMail-Priority: Normal\r\n" +
			"X-Mailer: Microsoft Outlook IMO, Build 9.0.2416 (9.0.2911.0)\r\n" +
			"Importance: Normal\r\n" +
			"X-MimeOLE: Produced By Microsoft MimeOLE V6.00.2800.1409\r\n" +
			"\r\n" +
			"This is a multi-part message in MIME format.\r\n" +
			"\r\n" +
			"------=_NextPart_000_0000_01C4239E.999350F0\r\n" +
			ATTACHMENT_1_HEADER_DATA +
			ATTACHMENT_1_BODY_DATA +
			"------=_NextPart_000_0000_01C4239E.999350F0--\r\n" +
			"\r\n";

		private string TEST_DOUBLE_BASE64 = 
			"Received: from development02 (development02 [127.0.0.1])\r\n" +
			"     by adexs.com (Eric Daugherty's C# Email Server)\r\n" +
			"     4/16/2004 10:35:58 AM\r\n" +
			"From: \"Eric Daugherty\" <edaugherty@adexs.com>\r\n" +
			"To: <test@test.com>\r\n" +
			"Subject: CofA\r\n" +
			"Date: Fri, 16 Apr 2004 10:35:55 -0500\r\n" +
			"Message-ID: <LIEBLGPCEJMNGHIPABGIGEABCAAA.edaugherty@adexs.com>\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: multipart/mixed;\r\n" +
			"	boundary=\"----=_NextPart_000_0000_01C4239E.999350F0\"\r\n" +
			"X-Priority: 3 (Normal)\r\n" +
			"X-MSMail-Priority: Normal\r\n" +
			"X-Mailer: Microsoft Outlook IMO, Build 9.0.2416 (9.0.2911.0)\r\n" +
			"Importance: Normal\r\n" +
			"X-MimeOLE: Produced By Microsoft MimeOLE V6.00.2800.1409\r\n" +
			"\r\n" +
			"This is a multi-part message in MIME format.\r\n" +
			"\r\n" +
			"------=_NextPart_000_0000_01C4239E.999350F0\r\n" +
			ATTACHMENT_1_HEADER_DATA +
			ATTACHMENT_1_BODY_DATA +
			"------=_NextPart_000_0000_01C4239E.999350F0\r\n" +
			"Content-Type: application/pdf;\r\n" +
			"	name=\"test.pdf\"\r\n" +
			"Content-Transfer-Encoding: 7bit\r\n" +
			"Content-Disposition: attachment;\r\n" +
			"	filename=\"test.pdf\"\r\n" +
			"\r\n" +
			"PAovTGVuZ3RoIDQ3Ci9GaWx0ZXIgWy9GbGF0ZURlY29kZV0KPj4Kc3RyZWFtCnic4yrkMtQzAAIF\r\n" +
			"AxQKq2ByLpexpSU2GVMzXBr0PX2TC1LSDBRc8rkCuQCBJhXPCmVuZHN0cmVhbQplbmRvYmoKNyAw\r\n" +
			"IG9iago8PAovVHlwZSAvWE9iamVjdAovU3VidHlwZSAvSW1hZ2UKL05hbWUgL0lNY3BkZjAKL1dp\r\n" +
			"\r\n" +
			"------=_NextPart_000_0000_01C4239E.999350F0--\r\n" +
			"\r\n";

		private string TEST_BODY_BASE64 = 
			"Received: from development02 (development02 [127.0.0.1])\r\n" +
			"     by adexs.com (Eric Daugherty's C# Email Server)\r\n" +
			"     4/22/2004 4:36:14 PM\r\n" +
			"From: \"Eric Daugherty\" <edaugherty@adexs.com>\r\n" +
			"To: <cc_1000@test.com>\r\n" +
			"Subject: CofAs\r\n" +
			"Date: Thu, 22 Apr 2004 16:36:14 -0500\r\n" +
			"Message-ID: <LIEBLGPCEJMNGHIPABGIKEAHCAAA.edaugherty@adexs.com>\r\n" +
			"MIME-Version: 1.0\r\n" +
			"Content-Type: application/pdf;\r\n" +
			"	name=\"000000002.pdf\"\r\n" +
			"Content-Transfer-Encoding: base64\r\n" +
			"Content-Disposition: attachment;\r\n" +
			"	filename=\"000000002.pdf\"\r\n" +
			"X-Priority: 3 (Normal)\r\n" +
			"X-MSMail-Priority: Normal\r\n" +
			"X-Mailer: Microsoft Outlook IMO, Build 9.0.2416 (9.0.2911.0)\r\n" +
			"Importance: Normal\r\n" +
			"X-MimeOLE: Produced By Microsoft MimeOLE V6.00.2800.1409\r\n" +
			"" +
			ATTACHMENT_1_BODY_DATA;
			

		#endregion

		#region SetUp/TearDown
		
		[SetUp]
		public void Setup()
		{
		}
	
		[TearDown]
		public void Teardown()
		{
		}
		
		#endregion

		#region Tests

		[Test]
		public void MessageHeaders()
		{
			SMTPMessage message = new SMTPMessage();
			message.AddData( TEST_SINGLE_BASE64 );

			Assertion.AssertEquals( "Received", "from development02 (development02 [127.0.0.1]) by adexs.com (Eric Daugherty's C# Email Server) 4/16/2004 10:35:58 AM", message.Headers["Received"] );
			Assertion.AssertEquals( "From", "\"Eric Daugherty\" <edaugherty@adexs.com>", message.Headers["From" ] );
			Assertion.AssertEquals( "Subject", "CofAs", message.Headers["Subject"] );
			Assertion.AssertEquals( "Date", "Fri, 16 Apr 2004 10:35:55 -0500", message.Headers["Date"] );
			Assertion.AssertEquals( "X-MimeOLE", "Produced By Microsoft MimeOLE V6.00.2800.1409", message.Headers["X-MimeOLE"] );
		}

		[Test]
		public void SingleBase64Attachment()
		{
			SMTPMessage message = new SMTPMessage();
			message.AddData( TEST_SINGLE_BASE64 );
		  
		  SMTPMessagePart[] messageParts = message.MessageParts;

			Assertion.AssertEquals( "AttachmentCount", 1, messageParts.Length );
			Assertion.AssertEquals( "AttachmentHeaderData", ATTACHMENT_1_HEADER_DATA, messageParts[0].HeaderData );
			Assertion.AssertEquals( "AttachmentBodyData", ATTACHMENT_1_BODY_DATA, messageParts[0].BodyData );
			Assertion.AssertEquals( "AttachmentContentType", "application/pdf; name=\"000000001.pdf\"", messageParts[0].Headers["Content-Type"] );
			Assertion.AssertEquals( "AttachmentContentDisposition", "attachment; filename=\"000000001.pdf\"", messageParts[0].Headers["Content-Disposition"] );

		}

		[Test]
		public void DoubleBase64Attachment()
		{
			SMTPMessage message = new SMTPMessage();
			message.AddData( TEST_DOUBLE_BASE64 );
		  
		  SMTPMessagePart[] messageParts = message.MessageParts;

			Assertion.AssertEquals( "AttachmentCount", 2, messageParts.Length );
			Assertion.AssertEquals( "AttachmentHeaderData1", ATTACHMENT_1_HEADER_DATA, messageParts[0].HeaderData );
			Assertion.AssertEquals( "AttachmentBodyData1", ATTACHMENT_1_BODY_DATA, messageParts[0].BodyData );
			Assertion.AssertEquals( "AttachmentContentType1", "application/pdf; name=\"000000001.pdf\"", messageParts[0].Headers["Content-Type"] );
			Assertion.AssertEquals( "AttachmentContentDisposition1", "attachment; filename=\"000000001.pdf\"", messageParts[0].Headers["Content-Disposition"] );
		}

		[Test]
		public void BodyBase64()
		{
			SMTPMessage message = new SMTPMessage();
			message.AddData( TEST_BODY_BASE64 );
		  
		  SMTPMessagePart[] messageParts = message.MessageParts;
		  
			Assertion.AssertEquals( "AttachmentCount", 0, messageParts.Length );
			Assertion.AssertEquals( "ContentDisposition", "attachment; filename=\"000000002.pdf\"", message.Headers["Content-Disposition"] );
		}

		#endregion
				
	}
}