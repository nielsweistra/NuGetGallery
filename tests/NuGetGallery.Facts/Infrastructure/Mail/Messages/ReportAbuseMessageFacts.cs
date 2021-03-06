﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Moq;
using NuGetGallery.Infrastructure.Mail.Requests;
using Xunit;

namespace NuGetGallery.Infrastructure.Mail.Messages
{
    public class ReportAbuseMessageFacts : MarkdownMessageBuilderFacts
    {
        public class TheConstructor
        {
            public static IEnumerable<object[]> ConstructorArguments
            {
                get
                {
                    yield return new object[] { null, new ReportPackageRequest(), It.IsAny<bool>() };
                    yield return new object[] { Configuration, null, It.IsAny<bool>() };
                }
            }

            [Theory]
            [MemberData(nameof(ConstructorArguments))]
            public void GivenANullArgument_ItShouldThrow(
                IMessageServiceConfiguration configuration,
                ReportPackageRequest request,
                bool alreadyContactedOwners)
            {
                Assert.Throws<ArgumentNullException>(() => new ReportAbuseMessage(
                    configuration,
                    request,
                    alreadyContactedOwners));
            }
        }

        public class TheGetRecipientsMethod
        {
            [Fact]
            public void AddsGalleryOwnerToToList()
            {
                var message = CreateMessage();
                var recipients = message.GetRecipients();

                Assert.Equal(1, recipients.To.Count);
                Assert.Contains(Configuration.GalleryOwner, recipients.To);
            }

            [Fact]
            public void WhenCopySenderFalse_HasEmptyCCList()
            {
                var message = CreateMessage(copySender: false);
                var recipients = message.GetRecipients();

                Assert.Empty(recipients.CC);
            }

            [Fact]
            public void WhenCopySenderTrue_AddsFromAddressToCCList()
            {
                var message = CreateMessage(copySender: true);
                var recipients = message.GetRecipients();

                Assert.Equal(1, recipients.CC.Count);
                Assert.Contains(Fakes.FromAddress, recipients.CC);
            }

            [Fact]
            public void HasEmptyBccList()
            {
                var message = CreateMessage();
                var recipients = message.GetRecipients();

                Assert.Empty(recipients.Bcc);
            }

            [Fact]
            public void AddsFromAddressToReplyToList()
            {
                var message = CreateMessage();
                var recipients = message.GetRecipients();

                Assert.Equal(1, recipients.ReplyTo.Count);
                Assert.Contains(Fakes.FromAddress, recipients.ReplyTo);
            }
        }

        public class TheGetBodyMethod
        {
            [Theory]
            [InlineData(EmailFormat.Markdown, _expectedMarkdownBody)]
            [InlineData(EmailFormat.PlainText, _expectedPlainTextBody)]
            public void ReturnsExpectedBody(EmailFormat format, string expectedString)
            {
                var message = CreateMessage();

                var body = message.GetBody(format);
                Assert.Equal(expectedString, body);
            }
        }

        [Fact]
        public void SetsGalleryOwnerAsSender()
        {
            var message = CreateMessage();

            Assert.Equal(Configuration.GalleryOwner, message.Sender);
        }

        private static ReportAbuseMessage CreateMessage(
            bool copySender = false,
            bool alreadyContactedOwners = false)
        {
            var request = new ReportPackageRequest
            {
                FromAddress = Fakes.FromAddress,
                Package = Fakes.Package,
                PackageUrl = Fakes.PackageUrl,
                PackageVersionUrl = Fakes.PackageVersionUrl,
                Reason = "reason",
                Message = "message",
                RequestingUser = Fakes.RequestingUser,
                RequestingUserUrl = Fakes.ProfileUrl,
                Signature = "signature",
                CopySender = copySender
            };

            return new ReportAbuseMessage(
                Configuration,
                request,
                alreadyContactedOwners);
        }

        private const string _expectedMarkdownBody =
            @"**Email**: Sender (sender@gallery.org)

**Signature**: signature

**Package**: PackageId
packageUrl

**Version**: 1.0.0
packageVersionUrl

**User:** requestingUser (requestUser@gallery.org)
profileUrl

**Reason**:
reason

**Has the package owner been contacted?**
No

**Message:**
message


Message sent from NuGetGallery";
        private const string _expectedPlainTextBody =
            @"Email: Sender (sender@gallery.org)

Signature: signature

Package: PackageId
packageUrl

Version: 1.0.0
packageVersionUrl

User: requestingUser (requestUser@gallery.org)
profileUrl

Reason:
reason

Has the package owner been contacted?
No

Message:
message

Message sent from NuGetGallery";
    }
}
