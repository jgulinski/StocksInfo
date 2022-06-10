using System.Net;

namespace Server;

public class StatusResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public object Content { get; set; }
}