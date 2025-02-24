using Arad.SMS.Core.DotNetSDK.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Arad.Portal.Services.Interfaces;

public interface IMessage
{
    Task<string> GetToken(string username, string password, string scope, CancellationToken cancellationToken);

    Task<HttpResponseMessage> Send(List<AradA2PMessage> aradA2PMessages, string token, bool returnLongId, CancellationToken cancellationToken);

    Task<HttpResponseMessage> SendBulk(AradBulkMessage aradBulkMessage, string token, bool returnLongId, CancellationToken cancellationToken);

    Task<List<DLRStatus>> GetDLR(List<string> ids, string token, bool returnLongId, CancellationToken cancellationToken);

    Task<List<Inbox>> GetMO(bool returnId, string token, CancellationToken cancellationToken);

    Task<List<Inbox>> GetMOByDate(DateTime startDateTime, DateTime endDateTime, bool returnId, string token, CancellationToken cancellationToken);
}