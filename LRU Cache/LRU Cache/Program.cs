using System;


public class Node
{
    public int Key { get; set; }
    public int Value { get; set; }
    public Node Next { get; set; }
    public Node Prev { get; set; }

    public Node(int key, int value)
    {
        this.Key = key;
        this.Value = value;
        this.Next = null;
        this.Prev = null;
    }
}

public class LRUCache
{
    private int _capacity;
    private Dictionary<int, Node> _map;
    private Node _head;
    private Node _tail;

    public LRUCache(int capacity)
    {
        this._capacity = capacity;
        this._map = new Dictionary<int, Node>();

        this._head = new Node(0, 0);
        this._tail = new Node(0, 0);

        this._head.Next = this._tail;
        this._tail.Prev = this._head;
    }
    public void AddToFront(Node node)
    {
        node.Next = this._head.Next;    
        node.Prev = this._head;          
        this._head.Next.Prev = node;     
        this._head.Next = node;           
    }

    public void RemoveNode(Node node)
    {
        node.Prev.Next = node.Next;       
        node.Next.Prev = node.Prev;      
    }

    public int Get(int key)
    {
        if(!_map.ContainsKey(key))
        {
            return -1;
        }

        Node node = _map[key];
        RemoveNode(node);
        AddToFront(node);
        return node.Value;
    }
    public void Put(int key, int value)
    {
        if (_map.ContainsKey(key))
        {
            Node node = _map[key];
            node.Value = value;
            RemoveNode(node);
            AddToFront(node);
            _map[key] = node;
            return;
        }

        if (_map.Count >= _capacity)
        {
            Node lru = _tail.Prev;
            RemoveNode(lru);
            _map.Remove(lru.Key);
        }

        Node newNode = new Node(key, value);
        AddToFront(newNode);
        _map[key] = newNode;
    }
}

public class LRUSystem
{
    public static void Main(string[] args)
    {
        LRUCache cache = new LRUCache(3);

        cache.Put(1, 10);
        cache.Put(2, 20);
        cache.Put(3, 30);

        Console.WriteLine(cache.Get(1));  

        cache.Put(4, 40);                 

        Console.WriteLine(cache.Get(2));  
        Console.WriteLine(cache.Get(3));  
        Console.WriteLine(cache.Get(4));  
        Console.ReadLine();
    }
}