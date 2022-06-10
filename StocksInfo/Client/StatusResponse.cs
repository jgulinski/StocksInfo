using System.Net;

namespace Client;

public class StatusResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public object Content { get; set; }
}