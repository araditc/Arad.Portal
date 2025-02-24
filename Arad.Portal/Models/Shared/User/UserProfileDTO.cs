using Arad.Portal.DataLayer.Models.Shared;
using Arad.Portal.DataLayer.Models.Shared.User;

using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Arad.Portal.Models.Shared.User;

public class UserProfileDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? FullName { get; init; }
    public string FatherName { get; init; }
    public Gender Gender { get; init; }
    public string Email { get; init; }
    public string GenderId { get; init; }
    public List<AddressDto> Addresses { get; init; } = [];

    //???
    public string NationalCode { get; init; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
    public DateTime? BirthDate { get; init; }
    public string PersianBirthDate { get; set; }
    public Image ProfilePhoto { get; init; } = new();

    public string CompanyName { get; init; }
    public BankAccount BankAccount { get; init; } = new();

    public UserType UserType { get; init; }
    public SpecialAccess? Access { get; init; }
    public string DefaultCurrencyId { get; init; }
    public string DefaultCurrencyName { get; set; }
    public string DefaultLanguageId { get; init; }
    public string DefaultLanguageName { get; set; }
    public string FileContent { get; init; }
    public string FileName { get; init; }
}