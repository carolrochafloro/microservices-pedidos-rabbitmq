﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using dotenv.net;
using Email.Models;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Email.Services;
internal class Consumer
{
    protected readonly string? hostName = Environment.GetEnvironmentVariable("HOST_NAME");
    protected ConnectionFactory factory;
    protected IConnection connection;
    protected IModel channel;
    private readonly SendEmail _sendEmail;
    public ConcurrentQueue<string> Messages { get; } = new ConcurrentQueue<string>();
    // logger


    public Consumer()
    {
        factory = new ConnectionFactory { HostName = "localhost" };
        connection = factory.CreateConnection();
        channel = connection.CreateModel();
        _sendEmail = new SendEmail();
    }

    public async Task Consume(string queueName)
    {

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
            );

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Messages.Enqueue(message);
            //logger
        };

        channel.BasicConsume(queue: queueName,
                            autoAck: true,
                            consumer: consumer);


        //logger - mensagem
    }

    public async Task ProcessMessages()
    {
        if (Messages.TryDequeue(out var message))
        {
            await _sendEmail.Send(message);
        }
    }
}