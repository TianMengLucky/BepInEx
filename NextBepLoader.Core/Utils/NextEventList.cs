using System;
using System.Collections.Generic;

namespace NextBepLoader.Core.Utils;

public class NextEventList<T> : List<T>, ICollection<T> where T : class
{
    public event Func<NextEventListEventArgs<T>, bool> OnEvent;
    bool ICollection<T>.IsReadOnly => false;

    void ICollection<T>.Add(T? item)
    {
        if (item == null) return;
        if (!StartEvent(ListEventType.Add, item)) return;
        base.Add(item);
    }

    void ICollection<T>.Clear()
    {
        if (Count <= 0) return;
        if (!StartEvent(ListEventType.Clear, null)) return;
        base.Clear();
    }

    bool ICollection<T>.Contains(T? item)
    {
        if (item == null) return false;
        var e = new NextEventListEventArgs<T>(this, ListEventType.Contains, item);
        var r = OnEvent.Invoke(e);
        if (r)
            return e.Contains;

        var value = base.Contains(item);
        e.Contains = value;
        e.OnContained(e);
        return value;
    }

    bool ICollection<T>.Remove(T? item)
    {
        if (item == null) return false;
        if (Count <= 0) return false;
        var e = new NextEventListEventArgs<T>(this, ListEventType.Remove, item);
        var r = OnEvent.Invoke(e);

        if (r)
            return e.Remove;

        var value = base.Remove(item);
        e.Remove = value;
        e.OnRemoved(e);
        return value;
    }

    public bool BaseRemove(T item) => base.Remove(item);

    private bool StartEvent(ListEventType type, T? value) =>
        OnEvent.Invoke(new NextEventListEventArgs<T>(this, type, value));
}

public enum ListEventType
{
    Add,
    Clear,
    Contains,
    Remove
}

public class NextEventListEventArgs<TEvent>(NextEventList<TEvent> list, ListEventType eventType, TEvent? value)
    : EventArgs where TEvent : class
{
    public NextEventList<TEvent> List { get; } = list;
    public ListEventType Type { get; } = eventType;
    public TEvent? Value { get; } = value;

    public bool Contains { get; set; }
    
    public bool Remove { get; set; }
    
    public Action<NextEventListEventArgs<TEvent>> OnRemoved { get; set; }
    public Action<NextEventListEventArgs<TEvent>> OnContained { get; set; }
}
