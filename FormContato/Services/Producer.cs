﻿using FormContato.DTOs;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;

namespace FormContato.Services;

public class Producer : IDisposable
{
    protected readonly string? hostName = Environment.GetEnvironmentVariable("HOST_NAME");
    protected ConnectionFactory factory;
    protected IConnection connection;
    protected IModel channel;

    // conexão com o servidor no construtor
    public Producer()
    {
        factory = new ConnectionFactory { HostName = "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
    }

    public void Produce(string queueName, ContactDTO contact)
    {
        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
            );
        string message = JsonConvert.SerializeObject(contact);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
    }


    public void Dispose()
    {
        channel.Close();
        connection.Close();
    }

}