
using System.Net.Sockets;
using System.Net;
using System.Text;
using System;
using System.Text.Json;

public class Server
{
    private readonly int _port;

    private CategoryList _categoryList = new CategoryList();

    public Server(int port)
    {
        _port = port;


    }


    public void Run()
    {

        var server = new TcpListener(IPAddress.Loopback, _port); // IPv4 127.0.0.1 IPv6 ::1
        server.Start();

        Console.WriteLine($"Server started on port {_port}");

        while (true)
        {
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected!!!");

            Task.Run(() => HandleClient(client));


        }

    }

    private void HandleClient(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            string msg = ReadFromStream(stream);

            Console.WriteLine("Message from client: " + msg);

            if (msg == "{}")
            {
                var response = new Response { Status = "missing method" };

                var json = ToJson(response);
                WriteToStream(stream, json);
            }
            else
            {
                var request = FromJson(msg);

                // null request 
                if (request == null)
                {
                    var response = new Response { Status = "illegal request" };
                }
                else
                {
                    string[] validMethods = ["create", "read", "update", "delete", "echo"];

                    // invalid method
                    if (!validMethods.Contains(request.Method))
                    {
                        var response = new Response { Status = "illegal method" };

                        var json = ToJson(response);
                        WriteToStream(stream, json);
                    }
                    else
                    {

                        // missing resourcecs
                        if (request.Path == null)
                        {
                            var response = new Response { Status = "missing resources" };

                            var json = ToJson(response);
                            WriteToStream(stream, json);
                        }
                        else
                        {
                            Console.WriteLine("Performing operation" + request.Method);
                            var response = PerformCategoryOperation(request);

                            var json = ToJson(response);
                            WriteToStream(stream, json);


                        }

                    }
                }


            }

        }
        catch { }
    }

    private Response PerformCategoryOperation(Request request)
    {

        Response response = new Response();
        string pathId = request.Path.Split("/").ElementAtOrDefault(3) ?? string.Empty;

        switch (request.Method.ToLower())
        {
            case "read":

                if (pathId == null)
                {
                    Category[] fetchedCategories = _categoryList.ListCategories();
                    response = new Response { Status = "1 Ok", Body = CategoryArrayToJson(fetchedCategories) };
                    return response;
                }
                else
                {
                    Category? fetchedCategory = _categoryList.ReadCategory(int.Parse(pathId));

                    if (fetchedCategory == null)
                    {
                        response = new Response { Status = "5 not found" };
                        return response;
                    }
                    response = new Response { Status = "1 Ok", Body = CategoryToJson(fetchedCategory) };
                    return response;
                }
            case "update":
                Category? convertedCategory = CategoryFromJson(request.Body);
                if (convertedCategory != null && convertedCategory.Cid == int.Parse(pathId))
                {
                    _categoryList.UpdateCategory(convertedCategory);
                    response = new Response { Status = "3 updated" };
                    return response;
                }

                return new Response { Status = "5 not found" };

            case "create":
                Console.WriteLine("Create" + request.Body);

                string? newCategoryName = CategoryFromJson(request.Body)?.Name;

                if (!string.IsNullOrEmpty(newCategoryName))
                {
                    Category? newCategory = _categoryList.CreateCategory(newCategoryName!);

                    if (newCategory == null)
                    {
                        response = new Response { Status = "4 Bad Request" };
                        return response;
                    }

                    response = new Response { Status = "2 created", Body = CategoryToJson(newCategory) };
                    return response;
                }

                return new Response { Status = "5 not found" };
            case "delete":
                Category? deletedCategory = _categoryList.DeleteCategory(int.Parse(pathId));
                if (deletedCategory == null)
                {
                    response = new Response { Status = "5 not found" };
                }
                else
                {
                    response = new Response { Status = "1 Ok" };
                }

                return response;




            default:
                return new Response { Status = "illegal method" };

        };
    }

    private string ReadFromStream(NetworkStream stream)
    {
        var buffer = new byte[1024];
        var readCount = stream.Read(buffer);
        return Encoding.UTF8.GetString(buffer, 0, readCount);
    }

    private void WriteToStream(NetworkStream stream, string msg)
    {
        var buffer = Encoding.UTF8.GetBytes(msg);
        stream.Write(buffer);
    }

    public static string ToJson(Response response)
    {
        return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Request? FromJson(string element)
    {
        return JsonSerializer.Deserialize<Request>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static string CategoryToJson(Category category)
    {
        return JsonSerializer.Serialize(category, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static string CategoryArrayToJson(Category[] categories)
    {
        return JsonSerializer.Serialize(categories, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }

    public static Category? CategoryFromJson(string element)
    {
        return JsonSerializer.Deserialize<Category>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }


    public static Category[] CategoryArrayFromJson(string element)
    {
        return JsonSerializer.Deserialize<Category[]>(element, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) ?? Array.Empty<Category>();
    }


}
