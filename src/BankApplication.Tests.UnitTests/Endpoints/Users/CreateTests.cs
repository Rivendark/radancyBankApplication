﻿using BankApplication.Api.Endpoints.Users;
using BankApplication.Application.Commands.Users;
using BankApplication.Application.DTOs;
using BankApplication.Core.Exceptions;
using MediatR;
using Moq;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BankApplication.Tests.UnitTests.Endpoints.Users;

[TestFixture]
public class CreateTests
{
    private Mock<ISender> _senderMock;
    private Create _endpoint;
    private CancellationTokenSource _cts;
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly UserDto _userDto = new ()
    {
        Email = "firstUser@test.com",
        FirstName = "first",
        LastName = "user",
        Id = UserId
    };

    [SetUp]
    public void BeforeEachTest()
    {
        _cts = new CancellationTokenSource();
        _senderMock = new Mock<ISender>();
        _endpoint = new Create(_senderMock.Object);
    }
    
    [Test]
    public async Task Should_CreateUser()
    {
        var cmd = new CreateUserCommand
        {
            CorrelationId = Guid.NewGuid(),
            Email = _userDto.Email,
            FirstName = _userDto.FirstName,
            LastName = _userDto.LastName,
            SendingSystemId = Guid.NewGuid()
        };
        
        _senderMock.Setup(x => x.Send(cmd, _cts.Token))
            .ReturnsAsync(_userDto)
            .Verifiable();
        
        var result = await _endpoint.HandleAsync(cmd, _cts.Token);
        
        Assert.Multiple(() =>
        {
            _senderMock.Verify();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UserDto>(), "Result not instance of UserDto.");
            Assert.That(result.FirstName, Is.EqualTo(_userDto.FirstName), "First names are not equal.");
        });
    }

    [Test]
    public void Should_ThrowUserExistsException_When_UserIdExists()
    {
        var cmd = new CreateUserCommand
        {
            CorrelationId = Guid.NewGuid(),
            Email = _userDto.Email,
            FirstName = _userDto.FirstName,
            LastName = _userDto.LastName,
            SendingSystemId = Guid.NewGuid()
        };
        
        _senderMock.Setup(x => x.Send(cmd, _cts.Token))
            .ThrowsAsync(new UserExistsException())
            .Verifiable();
        
        Assert.ThrowsAsync<UserExistsException>(async () => await _endpoint.HandleAsync(cmd, _cts.Token));
    }
}