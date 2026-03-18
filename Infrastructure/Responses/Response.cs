using System.Net;

public class Response<T>
{
    public int StatusCode{get;set;}=0;
    public List<string> Description{get;set;}=[];
    public T? Data{get;set;}
    public Response(HttpStatusCode statusCode,string messeage,T data)
    {
        StatusCode=(int)statusCode;
        Description.Add(messeage);
        Data=data;
    }
    public Response(HttpStatusCode statusCode,string messeage)
    {
        StatusCode=(int)statusCode;
        Description.Add(messeage);
    }
    public Response(HttpStatusCode statusCode,List<string> messeage,T data)
    {
        StatusCode=(int)statusCode;
        Description=messeage;
        Data=data;
    }
    public Response(HttpStatusCode statusCode,List<string> messeage)
    {
        StatusCode=(int)statusCode;
        Description=messeage;
    }
}