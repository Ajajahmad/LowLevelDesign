


using System;
using System.Xml.Linq;

public interface ISubscriber
{
    string Name { get; set; }
    void OnMessage(Message message);
}

public class EmailSubscriber : ISubscriber
{
    public string Name { get; set; }    
    public EmailSubscriber(string name)
    {
        this.Name = name;
    }
    public void OnMessage(Message message)
    {
        Console.WriteLine($"[EMAIL] {Name} received: {message.Content}");
    }
}
public class ConsoleSubscriber : ISubscriber
{
    public string Name { get; set; }

    public ConsoleSubscriber(string name)
    {
        this.Name = name;
    }

    public void OnMessage(Message message)
    {
        Console.WriteLine($"[CONSOLE] {Name} received: {message.Content}");
    }
}

public class Message
{
    public string Content { get; set;}
    public DateTime time { get; set;}   
    public Message(string content)
    {
        this.Content = content;
        this.time = DateTime.Now;
    }
}
public class Topic
{
    public string Name { get; set; }    
    public List<Message> messages { get; set; } = new List<Message>();
    public List<ISubscriber> Subscribers { get; set; } = new List<ISubscriber>();
    public Topic(string name)
    {
        this.Name = name;
    }
    public void Subscribe(ISubscriber subscriber)
    {
        Subscribers.Add(subscriber);
        Console.WriteLine($"{subscriber.Name} subscribed to {Name}");
    }

    public void Unsubscribe(ISubscriber subscriber)
    {
        Subscribers.Remove(subscriber);
        Console.WriteLine($"{subscriber.Name} unsubscribed from {Name}");
    }
    public void Publish(string content)
    {
        Message message = new Message(content); 
        messages.Add(message);
        NotifyAll(message);
    }
    private void NotifyAll(Message message)
    {
        foreach (ISubscriber sub in Subscribers)
        {
            sub.OnMessage(message);
        }
    }
}

public class Publisher
{
    public string Name { get; set; }
    private PubSubService _service;
    public Publisher(string name, PubSubService service)
    {
        this.Name = name;
        this._service = service;
    }
    public void Publish(string topicName, string content)
    {
        _service.Publish(topicName, content);
        Console.WriteLine($"[{Name}] published to {topicName}: {content}");
    }
}

public class PubSubService
{
    private Dictionary<string, Topic> _topics = new Dictionary<string, Topic>();
    public void CreateTopic(string topicName)
    {
        if (!_topics.ContainsKey(topicName))
        {
            _topics[topicName] = new Topic(topicName);
            Console.WriteLine($"Topic '{topicName}' created.");
        }
    }

    public void Subscribe(string topicName, ISubscriber subcriber)
    {
        if (!_topics.ContainsKey(topicName))
            throw new Exception("Topic not found!");
        _topics[topicName].Subscribe(subcriber);
    }
    public void Unsubscribe(string topicName, ISubscriber subscriber)
    {
        if (!_topics.ContainsKey(topicName))
            throw new Exception("Topic not found!");
        _topics[topicName].Unsubscribe(subscriber);
    }
    public void Publish(string topicName, string content)
    {
        if (!_topics.ContainsKey(topicName))
            throw new Exception("Topic not found!");
        _topics[topicName].Publish(content);
    }
}

public class PubSubSystem
{
    public static void Main(string[] args)
    {
        PubSubService service = new PubSubService();

        service.CreateTopic("Technology");
        service.CreateTopic("Sports");

        ISubscriber user1 = new EmailSubscriber("Ajaj");
        ISubscriber user2 = new ConsoleSubscriber("Rahul");
        ISubscriber user3 = new EmailSubscriber("Priya");

        service.Subscribe("Technology", user1);
        service.Subscribe("Technology", user2);
        service.Subscribe("Sports", user2);
        service.Subscribe("Sports", user3);

        Publisher pub1 = new Publisher("TechPublisher", service);
        Publisher pub2 = new Publisher("SportsPublisher", service);

        Console.WriteLine("\n--- Publishing Messages ---");
        pub1.Publish("Technology", "AI is revolutionizing the world!");
        pub2.Publish("Sports", "India won the match!");

        Console.WriteLine("\n--- Rahul unsubscribes from Technology ---");
        service.Unsubscribe("Technology", user2);
        pub1.Publish("Technology", "New iPhone launched!");
    }
}