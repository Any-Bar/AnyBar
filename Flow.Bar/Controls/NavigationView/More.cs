using iNKORE.UI.WPF.Modern.Common;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xaml;

namespace Flow.Bar.Controls.NavigationView;

public interface IControlProtected
{
    DependencyObject GetTemplateChild(string childName);
}

internal class FrameworkElementSizeChangedRevoker : EventRevoker<FrameworkElement, SizeChangedEventHandler>
{
    public FrameworkElementSizeChangedRevoker(FrameworkElement source, SizeChangedEventHandler handler) : base(source, handler)
    {
    }

    protected override void AddHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged += handler;
    }

    protected override void RemoveHandler(FrameworkElement source, SizeChangedEventHandler handler)
    {
        source.SizeChanged -= handler;
    }
}

internal abstract class EventRevoker<TSource, TDelegate>
        where TSource : class
        where TDelegate : Delegate
{
    private WeakReference<TSource> _source;
    private WeakReference<TDelegate> _handler;

    protected EventRevoker(TSource source, TDelegate handler)
    {
        _source = new WeakReference<TSource>(source);
        _handler = new WeakReference<TDelegate>(handler);
        AddHandler(source, handler);
    }

    protected abstract void AddHandler(TSource source, TDelegate handler);
    protected abstract void RemoveHandler(TSource source, TDelegate handler);

    public void Revoke()
    {
        if (_source != null && _handler != null &&
            _source.TryGetTarget(out var source) &&
            _handler.TryGetTarget(out var handler))
        {
            RemoveHandler(source, handler);
        }

        _source = null;
        _handler = null;
    }
}

internal class SplitVector<T, SplitVectorID>
{
    public SplitVector(SplitVectorID id, Func<T, int> indexOfFunction)
    {
        m_vectorID = id;
        m_indexFunctionFromDataSource = indexOfFunction;

        // TODO: WPF
        /*
        m_vector.set(winrt::make<Vector<T, MakeVectorParam<VectorFlag::Observable, VectorFlag::DependencyObjectBase>()>>(
            [this](const T& value)
               {
                    return IndexOf(value);
               }));
        */
        m_vector = new ObservableCollection<T>();
    }

    public SplitVectorID GetVectorIDForItem() { return m_vectorID; }

    public IList GetVector() { return m_vector; }

    public void OnRawDataRemove(int indexInOriginalVector, SplitVectorID vectorID)
    {
        if (Equals(m_vectorID, vectorID))
        {
            RemoveAt(indexInOriginalVector);
        }

        for (int i = 0; i < m_indexesInOriginalVector.Count; i++)
        {
            var v = m_indexesInOriginalVector[i];
            if (v > indexInOriginalVector)
            {
                m_indexesInOriginalVector[i]--;
            }
        }

    }

    public void OnRawDataInsert(int preferIndex, int indexInOriginalVector, T value, SplitVectorID vectorID)
    {
        for (int i = 0; i < m_indexesInOriginalVector.Count; i++)
        {
            var v = m_indexesInOriginalVector[i];
            if (v > indexInOriginalVector)
            {
                m_indexesInOriginalVector[i]++;
            }
        }

        if (Equals(m_vectorID, vectorID))
        {
            InsertAt(preferIndex, indexInOriginalVector, value);
        }
    }

    public void InsertAt(int preferIndex, int indexInOriginalVector, T value)
    {
        Debug.Assert(preferIndex >= 0);
        Debug.Assert(indexInOriginalVector >= 0);
        m_vector.Insert(preferIndex, value);
        m_indexesInOriginalVector.Insert(preferIndex, indexInOriginalVector);
    }

    public void Replace(int indexInOriginalVector, T value)
    {
        Debug.Assert(indexInOriginalVector >= 0);

        var index = IndexFromIndexInOriginalVector(indexInOriginalVector);
        var vector = m_vector;
        vector.RemoveAt(index);
        vector.Insert(index, value);
    }

    public void Clear()
    {
        m_vector.Clear();
        m_indexesInOriginalVector.Clear();
    }

    public void RemoveAt(int indexInOriginalVector)
    {
        Debug.Assert(indexInOriginalVector >= 0);
        var index = IndexFromIndexInOriginalVector(indexInOriginalVector);
        Debug.Assert(index < m_indexesInOriginalVector.Count);
        m_vector.RemoveAt(index);
        m_indexesInOriginalVector.RemoveAt(index);
    }

    public int IndexOf(T value)
    {
        int indexInOriginalVector = m_indexFunctionFromDataSource(value);
        return IndexFromIndexInOriginalVector(indexInOriginalVector);
    }

    public int IndexToIndexInOriginalVector(int index)
    {
        Debug.Assert(index >= 0 && index < Size());
        return m_indexesInOriginalVector[index];
    }

    public int IndexFromIndexInOriginalVector(int indexInOriginalVector)
    {
        var pos = m_indexesInOriginalVector.IndexOf(indexInOriginalVector);
        if (pos != -1)
        {
            return pos;
        }
        return -1;
    }

    int Size() { return m_indexesInOriginalVector.Count; }

    SplitVectorID m_vectorID;
    Collection<T> m_vector;
    List<int> m_indexesInOriginalVector = new List<int>();
    Func<T, int> m_indexFunctionFromDataSource;
}

internal class SplitDataSourceBase<T, SplitVectorID, AttachedDataType> where SplitVectorID : Enum
{
    static readonly int SplitVectorSize = Enum.GetNames(typeof(SplitVectorID)).Length;

    public SplitVectorID GetVectorIDForItem(int index)
    {
        Debug.Assert(index >= 0 && index < RawDataSize());
        return m_flags[index];
    }

    public AttachedDataType AttachedData(int index)
    {
        Debug.Assert(index >= 0 && index < RawDataSize());
        return m_attachedData[index];
    }

    public void AttachedData(int index, AttachedDataType attachedData)
    {
        Debug.Assert(index >= 0 && index < RawDataSize());
        m_attachedData[index] = attachedData;
    }

    public void ResetAttachedData()
    {
        ResetAttachedData(DefaultAttachedData());
    }

    public void ResetAttachedData(AttachedDataType attachedData)
    {
        for (int i = 0; i < RawDataSize(); i++)
        {
            m_attachedData[i] = attachedData;
        }
    }

    public SplitVector<T, SplitVectorID> GetVectorForItem(int index)
    {
        if (index >= 0 && index < RawDataSize())
        {
            return m_splitVectors[Convert.ToInt32(m_flags[index])];
        }
        return null;
    }

    public void MoveItemsToVector(SplitVectorID newVectorID)
    {
        MoveItemsToVector(0, RawDataSize(), newVectorID);
    }

    public void MoveItemsToVector(int start, int end, SplitVectorID newVectorID)
    {
        Debug.Assert(start >= 0 && end <= RawDataSize());
        for (int i = start; i < end; i++)
        {
            MoveItemToVector(i, newVectorID);
        }
    }

    public void MoveItemToVector(int index, SplitVectorID newVectorID)
    {
        Debug.Assert(index >= 0 && index < RawDataSize());

        if (!Equals(m_flags[index], newVectorID))
        {
            // remove from the old vector
            if (GetVectorForItem(index) is { } splitVector)
            {
                splitVector.RemoveAt(index);
            }

            // change flag
            m_flags[index] = newVectorID;

            // insert item to vector which matches with the newVectorID
            if (m_splitVectors[Convert.ToInt32(newVectorID)] is { } toVector)
            {
                int pos = GetPreferIndex(index, newVectorID);

                var value = GetAt(index);
                toVector.InsertAt(pos, index, value);
            }
        }
    }

    internal virtual int IndexOf(T value) => 0;
    internal virtual T GetAt(int index) => default;
    internal virtual int Size() => 0;
    internal virtual SplitVectorID DefaultVectorIDOnInsert() => default;
    internal virtual AttachedDataType DefaultAttachedData() => default;

    public int IndexOfImpl(T value, SplitVectorID vectorID)
    {
        int indexInOriginalVector = IndexOf(value);
        int index = -1;
        if (indexInOriginalVector != -1)
        {
            var vector = GetVectorForItem(indexInOriginalVector);
            if (vector != null && Equals(vector.GetVectorIDForItem(), vectorID))
            {
                index = vector.IndexFromIndexInOriginalVector(indexInOriginalVector);
            }
        }
        return index;
    }

    public void InitializeSplitVectors(List<SplitVector<T, SplitVectorID>> vectors)
    {
        foreach (var vector in vectors)
        {
            m_splitVectors[Convert.ToInt32(vector.GetVectorIDForItem())] = vector;
        }
    }

    public SplitVector<T, SplitVectorID> GetVector(SplitVectorID vectorID)
    {
        return m_splitVectors[Convert.ToInt32(vectorID)];
    }


    public void OnClear()
    {
        // Clear all vectors
        foreach (var vector in m_splitVectors)
        {
            if (vector != null)
            {
                vector.Clear();
            }
        }

        m_flags.Clear();
        m_attachedData.Clear();
    }

    public void OnRemoveAt(int startIndex, int count)
    {
        for (int i = startIndex + count - 1; i >= startIndex; i--)
        {
            OnRemoveAt(i);
        }
    }

    public void OnInsertAt(int startIndex, int count)
    {
        for (int i = startIndex; i < startIndex + count; i++)
        {
            OnInsertAt(i);
        }
    }

    public int RawDataSize()
    {
        return m_flags.Count;
    }

    public void SyncAndInitVectorFlagsWithID(SplitVectorID defaultID, AttachedDataType defaultAttachedData)
    {
        // Initialize the flags
        for (int i = 0; i < Size(); i++)
        {
            m_flags.Add(defaultID);
            m_attachedData.Add(defaultAttachedData);
        }
    }

    public void Clear()
    {
        OnClear();
    }

    void OnRemoveAt(int index)
    {
        var vectorID = m_flags[index];

        // Update mapping on all Vectors and Remove Item on vectorID vector;
        foreach (var vector in m_splitVectors)
        {
            if (vector != null)
            {
                vector.OnRawDataRemove(index, vectorID);
            }
        }

        m_flags.RemoveAt(index);
        m_attachedData.RemoveAt(index);
    }

    void OnReplace(int index)
    {
        if (GetVectorForItem(index) is { } splitVector)
        {
            var value = GetAt(index);
            splitVector.Replace(index, value);
        }
    }

    void OnInsertAt(int index)
    {
        var vectorID = DefaultVectorIDOnInsert();
        var defaultAttachedData = DefaultAttachedData();
        var preferIndex = GetPreferIndex(index, vectorID);
        var data = GetAt(index);

        // Update mapping on all Vectors and Insert Item on vectorID vector;
        foreach (var vector in m_splitVectors)
        {
            if (vector != null)
            {
                vector.OnRawDataInsert(preferIndex, index, data, vectorID);
            }
        }

        m_flags.Insert(index, vectorID);
        m_attachedData.Insert(index, defaultAttachedData);
    }

    int GetPreferIndex(int index, SplitVectorID vectorID)
    {
        return RangeCount(0, index, vectorID);
    }

    int RangeCount(int start, int end, SplitVectorID vectorID)
    {
        int count = 0;
        for (int i = start; i < end; i++)
        {
            if (Equals(m_flags[i], vectorID))
            {
                count++;
            }
        }
        return count;
    }

    // length is the same as data source, and used to identify which SplitVector it belongs to.
    List<SplitVectorID> m_flags = new List<SplitVectorID>();
    List<AttachedDataType> m_attachedData = new List<AttachedDataType>();
    SplitVector<T, SplitVectorID>[] m_splitVectors = new SplitVector<T, SplitVectorID>[SplitVectorSize];
}

internal class InspectingDataSource : ItemsSourceView
{
    public InspectingDataSource(object source) : base(source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IList vector)
        {
            m_vector = vector;
            ListenToCollectionChanges();
        }
        else
        {
            if (source is IEnumerable iterable)
            {
                m_vector = WrapIterable(iterable);
            }
            else
            {
                throw new ArgumentException("Argument 'source' is not a supported vector.");
            }
        }

        m_uniqueIdMaping = source as IKeyIndexMapping;
    }

    ~InspectingDataSource()
    {
        UnListenToCollectionChanges();
    }

    internal override int GetSizeCore()
    {
        return m_vector.Count;
    }

    internal override object GetAtCore(int index)
    {
        return m_vector[index];
    }

    internal override bool HasKeyIndexMappingCore()
    {
        return m_uniqueIdMaping != null;
    }

    internal override string KeyFromIndexCore(int index)
    {
        if (m_uniqueIdMaping != null)
        {
            return m_uniqueIdMaping.KeyFromIndex(index);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    internal override int IndexFromKeyCore(string id)
    {
        if (m_uniqueIdMaping != null)
        {
            return m_uniqueIdMaping.IndexFromKey(id);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    internal override int IndexOfCore(object value)
    {
        int index = -1;
        if (m_vector != null)
        {
            var v = m_vector.IndexOf(value);
            if (v >= 0)
            {
                index = v;
            }
        }
        return index;
    }

    private IList WrapIterable(IEnumerable iterable)
    {
        var vector = new List<object>();
        var iterator = iterable.GetEnumerator();
        while (iterator.MoveNext())
        {
            vector.Add(iterator.Current);
        }

        return vector;
    }

    private void UnListenToCollectionChanges()
    {
        if (m_vector is INotifyCollectionChanged incc)
        {
            CollectionChangedEventManager.RemoveHandler(incc, OnCollectionChanged);
        }
    }

    private void ListenToCollectionChanges()
    {
        Debug.Assert(m_vector != null);
        if (m_vector is INotifyCollectionChanged incc)
        {
            CollectionChangedEventManager.AddHandler(incc, OnCollectionChanged);
        }
    }

    private void OnCollectionChanged(
         object sender,
         NotifyCollectionChangedEventArgs e)
    {
        OnItemsSourceChanged(e);
    }

    private readonly IList m_vector;
    private readonly IKeyIndexMapping m_uniqueIdMaping = null;
}

public interface IKeyIndexMapping
{
    string KeyFromIndex(int index);
    int IndexFromKey(string key);
}

public class ItemsSourceView : INotifyCollectionChanged
{
    public ItemsSourceView(object source)
    {
    }

    public int Count
    {
        get
        {
            if (m_cachedSize == -1)
            {
                // Call the override the very first time. After this,
                // we can just update the size when there is a data source change.
                m_cachedSize = GetSizeCore();
            }

            return m_cachedSize;
        }
    }

    public object GetAt(int index)
    {
        return GetAtCore(index);
    }

    public bool HasKeyIndexMapping => HasKeyIndexMappingCore();

    public string KeyFromIndex(int index)
    {
        return KeyFromIndexCore(index);
    }

    public int IndexFromKey(string key)
    {
        return IndexFromKeyCore(key);
    }

    public int IndexOf(object value)
    {
        return IndexOfCore(value);
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    internal void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
    {
        m_cachedSize = GetSizeCore();
        CollectionChanged?.Invoke(this, args);
    }

    internal virtual int GetSizeCore()
    {
        throw new NotImplementedException();
    }

    internal virtual object GetAtCore(int index)
    {
        throw new NotImplementedException();
    }

    internal virtual bool HasKeyIndexMappingCore()
    {
        throw new NotImplementedException();
    }

    internal virtual string KeyFromIndexCore(int index)
    {
        throw new NotImplementedException();
    }

    internal virtual int IndexFromKeyCore(string id)
    {
        throw new NotImplementedException();
    }

    internal virtual int IndexOfCore(object value)
    {
        throw new NotImplementedException();
    }

    private int m_cachedSize = -1;

    internal class CollectionChangedRevoker : EventRevoker<ItemsSourceView, NotifyCollectionChangedEventHandler>
    {
        public CollectionChangedRevoker(ItemsSourceView source, NotifyCollectionChangedEventHandler handler) : base(source, handler)
        {
        }

        protected override void AddHandler(ItemsSourceView source, NotifyCollectionChangedEventHandler handler)
        {
            source.CollectionChanged += handler;
        }

        protected override void RemoveHandler(ItemsSourceView source, NotifyCollectionChangedEventHandler handler)
        {
            source.CollectionChanged -= handler;
        }
    }
}

public sealed class SplitViewPaneClosingEventArgs : EventArgs
{
    internal SplitViewPaneClosingEventArgs()
    {
    }

    public bool Cancel { get; set; }
}

public class ElementFactory : DependencyObject, IElementFactoryShim
{
    public ElementFactory()
    {
    }

    #region IElementFactory

    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        return GetElementCore(args);
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        RecycleElementCore(args);
    }

    #endregion

    protected virtual UIElement GetElementCore(ElementFactoryGetArgs args)
    {
        throw new NotImplementedException();
    }

    protected virtual void RecycleElementCore(ElementFactoryRecycleArgs args)
    {
        throw new NotImplementedException();
    }
}

public interface IElementFactoryShim
{
    UIElement GetElement(ElementFactoryGetArgs args);
    void RecycleElement(ElementFactoryRecycleArgs context);
}

internal class ItemTemplateWrapper : IElementFactoryShim
{
    public ItemTemplateWrapper(DataTemplate dataTemplate)
    {
        Template = dataTemplate;
    }

    public ItemTemplateWrapper(DataTemplateSelector dataTemplateSelector)
    {
        TemplateSelector = dataTemplateSelector;
    }

    public DataTemplate Template { get; set; }

    public DataTemplateSelector TemplateSelector { get; set; }

    #region IElementFactory

    public UIElement GetElement(ElementFactoryGetArgs args)
    {
        var selectedTemplate = Template ?? TemplateSelector.SelectTemplate(args.Data, null);
        // Check if selected template we got is valid
        if (selectedTemplate == null)
        {
            selectedTemplate = TemplateSelector.SelectTemplate(args.Data, null);

            if (selectedTemplate == null)
            {
                // Still nullptr, fail with a reasonable message now.
                throw new InvalidOperationException("Null encountered as data template. That is not a valid value for a data template, and can not be used.");
            }
        }
        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        UIElement element = null;

        if (recyclePool != null)
        {
            // try to get an element from the recycle pool.
            element = recyclePool.TryGetElement(string.Empty /* key */, args.Parent as FrameworkElement);
        }

        if (element == null)
        {
            // no element was found in recycle pool, create a new element
            element = selectedTemplate.LoadContent() as FrameworkElement;

            // Template returned null, so insert empty element to render nothing
            if (element == null)
            {
                element = new System.Windows.Shapes.Rectangle
                {
                    Width = 0,
                    Height = 0
                };
            }

            // Associate template with element
            element.SetValue(RecyclePool.OriginTemplateProperty, selectedTemplate);
        }

        return element;
    }

    public void RecycleElement(ElementFactoryRecycleArgs args)
    {
        var element = args.Element;
        DataTemplate selectedTemplate = Template ??
            element.GetValue(RecyclePool.OriginTemplateProperty) as DataTemplate;
        var recyclePool = RecyclePool.GetPoolInstance(selectedTemplate);
        if (recyclePool == null)
        {
            // No Recycle pool in the template, create one.
            recyclePool = new RecyclePool();
            RecyclePool.SetPoolInstance(selectedTemplate, recyclePool);
        }

        recyclePool.PutElement(args.Element, string.Empty /* key */, args.Parent);
    }

    #endregion
}

public class RecyclePool
{
    public void PutElement(
        UIElement element,
        string key)
    {
        PutElementCore(element, key, null /* owner */);
    }

    public void PutElement(
        UIElement element,
        string key,
        UIElement owner)
    {
        PutElementCore(element, key, owner);
    }

    public UIElement TryGetElement(
        string key)
    {
        return TryGetElementCore(key, null /* owner */);
    }

    public UIElement TryGetElement(
        string key,
        UIElement owner)
    {
        return TryGetElementCore(key, owner);
    }

    protected virtual void PutElementCore(
        UIElement element,
        string key,
        UIElement owner)
    {
        var winrtKey = key;
        var winrtOwner = owner;
        var winrtOwnerAsPanel = EnsureOwnerIsPanelOrNull(winrtOwner);

        ElementInfo elementInfo = new ElementInfo(element, winrtOwnerAsPanel);

        if (m_elements.TryGetValue(winrtKey, out var elements))
        {
            elements.Add(elementInfo);
        }
        else
        {
            List<ElementInfo> pool = new List<ElementInfo>();
            pool.Add(elementInfo);
            m_elements.Add(winrtKey, pool);
        }
    }

    protected virtual UIElement TryGetElementCore(
        string key,
        UIElement owner)
    {
        if (m_elements.TryGetValue(key, out var elements))
        {
            if (elements.Count > 0)
            {
                ElementInfo elementInfo = new ElementInfo(null, null);
                // Prefer an element from the same owner or with no owner so that we don't incur
                // the enter/leave cost during recycling.
                // TODO: prioritize elements with the same owner to those without an owner.
                var winrtOwner = owner;
                var index = elements.FindIndex(elemInfo => elemInfo.Owner == winrtOwner || elemInfo.Owner == null);

                if (index >= 0)
                {
                    elementInfo = elements[index];
                    elements.RemoveAt(index); // elements.erase(iter);
                }
                else
                {
                    elementInfo = elements.Last();
                    elements.RemoveLast();
                }

                var ownerAsPanel = EnsureOwnerIsPanelOrNull(winrtOwner);
                if (elementInfo.Owner != null && elementInfo.Owner != ownerAsPanel)
                {
                    // Element is still under its parent. remove it from its parent.
                    var panel = elementInfo.Owner;
                    if (panel != null)
                    {
                        int childIndex = panel.Children.IndexOf(elementInfo.Element);
                        bool found = childIndex >= 0;
                        if (!found)
                        {
                            throw new Exception("ItemsRepeater's child not found in its Children collection.");
                        }

                        panel.Children.RemoveAt(childIndex);
                    }
                }

                return elementInfo.Element;
            }
        }

        return null;
    }

    #region Properties

    internal static readonly DependencyProperty ReuseKeyProperty =
        DependencyProperty.RegisterAttached(
            "ReuseKey",
            typeof(string),
            typeof(RecyclePool),
            new PropertyMetadata(string.Empty));

    internal static string GetReuseKey(UIElement element)
    {
        return (string)element.GetValue(ReuseKeyProperty);
    }

    internal static void SetReuseKey(UIElement element, string value)
    {
        element.SetValue(ReuseKeyProperty, value);
    }

    private static readonly AttachableMemberIdentifier PoolInstanceProperty =
        new AttachableMemberIdentifier(
            typeof(RecyclePool),
            "PoolInstance");

    public static RecyclePool GetPoolInstance(DataTemplate dataTemplate)
    {
        AttachablePropertyServices.TryGetProperty<RecyclePool>(dataTemplate, PoolInstanceProperty, out var value);
        return value;
    }

    public static void SetPoolInstance(DataTemplate dataTemplate, RecyclePool value)
    {
        AttachablePropertyServices.SetProperty(dataTemplate, PoolInstanceProperty, value);
    }

    internal static readonly DependencyProperty OriginTemplateProperty =
        DependencyProperty.RegisterAttached(
            "OriginTemplate",
            typeof(DataTemplate),
            typeof(RecyclePool),
            null);

    #endregion

    private Panel EnsureOwnerIsPanelOrNull(UIElement owner)
    {
        Panel ownerAsPanel = null;
        if (owner != null)
        {
            ownerAsPanel = owner as Panel;
            if (ownerAsPanel == null)
            {
                throw new ArgumentException("owner must to be a Panel or null.");
            }
        }

        return ownerAsPanel;
    }

    private class ElementInfo
    {
        public ElementInfo(UIElement element, Panel owner)
        {
            Element = element;
            Owner = owner;
        }

        public UIElement Element { get; }
        public Panel Owner { get; }
    }

    private readonly Dictionary<string, List<ElementInfo>> m_elements = new Dictionary<string, List<ElementInfo>>();
}

internal static class ListExtensions
{
    public static void Resize<T>(this List<T> list, int size, T element = default)
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            list.AddRange(Enumerable.Repeat(element, size - count));
        }
    }

    public static T Last<T>(this List<T> list)
    {
        return list[list.Count - 1];
    }

    public static void RemoveLast<T>(this List<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }

    public static bool Empty<T>(this List<T> list)
    {
        return list.Count == 0;
    }
}

internal static class VisualStateUtil
{
    /*
    public static VisualStateGroup GetVisualStateGroup(FrameworkElement control, string groupName)
    {
        VisualStateGroup group = null;
        var visualStateGroups = VisualStateManager.GetVisualStateGroups(control);
        foreach (VisualStateGroup visualStateGroup in visualStateGroups)
        {
            if (visualStateGroup.Name == groupName)
            {
                group = visualStateGroup;
                return group;
            }
        }
        return group;
    }
    */

    public static void GoToStateIfGroupExists(Control control, string groupName, string stateName, bool useTransitions)
    {
        //var visualStateGroup = GetVisualStateGroup(control, groupName);
        //if (visualStateGroup != null)
        {
            VisualStateManager.GoToState(control, stateName, useTransitions);
        }
    }
}

internal static class SharedHelpers
{
    public static bool IsAnimationsEnabled => SystemParameters.ClientAreaAnimation &&
                                              RenderCapability.Tier > 0;

    public static bool IsRS1OrHigher() => true;

    public static bool IsRS2OrHigher() => true;

    public static bool IsRS3OrHigher() => true;

    public static bool IsRS4OrHigher() => true;

    public static bool IsRS5OrHigher() => true;

    public static bool IsControlCornerRadiusAvailable() => true;

    public static bool IsThemeShadowAvailable() => false;

    public static bool IsOnXbox() => false;

    public static void QueueCallbackForCompositionRendering(Action callback)
    {
        CompositionTarget.Rendering += onRendering;

        void onRendering(object sender, EventArgs e)
        {
            // Detach event or Rendering will keep calling us back.
            CompositionTarget.Rendering -= onRendering;

            callback();
        }
    }

    public static bool DoRectsIntersect(
        Rect rect1,
        Rect rect2)
    {
        var doIntersect =
            !(rect1.Width <= 0 || rect1.Height <= 0 || rect2.Width <= 0 || rect2.Height <= 0) &&
            (rect2.X <= rect1.X + rect1.Width) &&
            (rect2.X + rect2.Width >= rect1.X) &&
            (rect2.Y <= rect1.Y + rect1.Height) &&
            (rect2.Y + rect2.Height >= rect1.Y);
        return doIntersect;
    }

    public static object FindResource(string resource, ResourceDictionary resources, object defaultValue)
    {
        var boxedResource = resource;
        return resources.Contains(boxedResource) ? resources[boxedResource] : defaultValue;
    }

    public static object FindResource(string resource, FrameworkElement element, object defaultValue)
    {
        return element.TryFindResource(resource) ?? defaultValue;
    }

    public static object FindInApplicationResources(string resource, object defaultValue)
    {
        return SharedHelpers.FindResource(resource, Application.Current.Resources, defaultValue);
    }

    public static void SetBinding(
        string pathString,
        DependencyObject target,
        DependencyProperty targetProperty)
    {
        Binding binding = new Binding(pathString)
        {
            RelativeSource = RelativeSource.TemplatedParent
        };

        BindingOperations.SetBinding(target, targetProperty, binding);
    }

    public static bool IsFrameworkElementLoaded(FrameworkElement frameworkElement)
    {
        return frameworkElement.IsLoaded;
    }

    public static AncestorType GetAncestorOfType<AncestorType>(DependencyObject firstGuess) where AncestorType : DependencyObject
    {
        var obj = firstGuess;
        AncestorType matchedAncestor = null;
        while (obj != null && matchedAncestor == null)
        {
            matchedAncestor = obj as AncestorType;
            obj = VisualTreeHelper.GetParent(obj);
        }

        if (matchedAncestor != null)
        {
            return matchedAncestor;
        }
        else
        {
            return null;
        }
    }

    // TODO: WPF
    internal static void ForwardCollectionChange<T>(
        ObservableCollection<T> source,
        ObservableCollection<T> destination,
        NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                destination.Insert(args.NewStartingIndex, (T)args.NewItems[0]);
                break;
            case NotifyCollectionChangedAction.Remove:
                destination.RemoveAt(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                destination[args.NewStartingIndex] = (T)args.NewItems[0];
                break;
            case NotifyCollectionChangedAction.Move:
                destination.Move(args.OldStartingIndex, args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                CopyList(source, destination);
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    public static void RaiseAutomationPropertyChangedEvent(UIElement element, object oldValue, object newValue)
    {
        if (FrameworkElementAutomationPeer.FromElement(element) is AutomationPeer peer)
        {
            peer.RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue,
                newValue);
        }
    }

    public static IconElement MakeIconElementFrom(IconSource iconSource)
    {
        //if (iconSource is FontIconSource fontIconSource)
        //{
        //    FontIcon fontIcon = new FontIcon();

        //    fontIcon.Glyph = fontIconSource.Glyph;
        //    fontIcon.FontSize = fontIconSource.FontSize;
        //    var newForeground = fontIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        fontIcon.Foreground = newForeground;
        //    }

        //    if (fontIconSource.FontFamily != null)
        //    {
        //        fontIcon.FontFamily = fontIconSource.FontFamily;
        //    }

        //    fontIcon.FontWeight = fontIconSource.FontWeight;
        //    fontIcon.FontStyle = fontIconSource.FontStyle;
        //    //fontIcon.IsTextScaleFactorEnabled = fontIconSource.IsTextScaleFactorEnabled;
        //    //fontIcon.MirroredWhenRightToLeft = fontIconSource.MirroredWhenRightToLeft;

        //    return fontIcon;
        //}
        //else if (iconSource is SymbolIconSource symbolIconSource)
        //{
        //    SymbolIcon symbolIcon = new SymbolIcon();
        //    symbolIcon.Symbol = symbolIconSource.Symbol;
        //    var newForeground = symbolIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        symbolIcon.Foreground = newForeground;
        //    }
        //    return symbolIcon;
        //}
        //else if (iconSource is BitmapIconSource bitmapIconSource)
        //{
        //    BitmapIcon bitmapIcon = new BitmapIcon();

        //    if (bitmapIconSource.UriSource != null)
        //    {
        //        bitmapIcon.UriSource = bitmapIconSource.UriSource;
        //    }

        //    bitmapIcon.ShowAsMonochrome = bitmapIconSource.ShowAsMonochrome;
        //    var newForeground = bitmapIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        bitmapIcon.Foreground = newForeground;
        //    }
        //    return bitmapIcon;
        //}
        //else if (iconSource is ImageIconSource imageIconSource)
        //{
        //    ImageIcon imageIcon = new ImageIcon();
        //    var imageSource = imageIconSource.ImageSource;
        //    if (imageSource != null)
        //    {
        //        imageIcon.Source = imageSource;
        //    }
        //    var newForeground = imageIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        imageIcon.Foreground = newForeground;
        //    }
        //    return imageIcon;
        //}
        //else if (iconSource is PathIconSource pathIconSource)
        //{
        //    PathIcon pathIcon = new PathIcon();

        //    if (pathIconSource.Data != null)
        //    {
        //        pathIcon.Data = pathIconSource.Data;
        //    }
        //    var newForeground = pathIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        pathIcon.Foreground = newForeground;
        //    }
        //    return pathIcon;
        //}
        //else if (iconSource is AnimatedIconSource animatedIconSource)
        //{
        //    AnimatedIcon animatedIcon = new AnimatedIcon();
        //    var source = animatedIconSource.Source;
        //    if (source != null)
        //    {
        //        animatedIcon.Source = source;
        //    }
        //    var fallbackIconSource = animatedIconSource.FallbackIconSource;
        //    if (fallbackIconSource != null)
        //    {
        //        animatedIcon.FallbackIconSource = fallbackIconSource;
        //    }
        //    var newForeground = animatedIconSource.Foreground;
        //    if (newForeground != null)
        //    {
        //        animatedIcon.Foreground = newForeground;
        //    }
        //    animatedIcon.FontSize = animatedIconSource.FontSize;
        //    return animatedIcon;
        //}
        //return null;

        return iconSource?.CreateIconElement();
    }

    public static BindingExpressionBase SetBinding(
        this FrameworkElement element,
        DependencyProperty dp,
        DependencyProperty sourceDP,
        DependencyObject source)
    {
        return element.SetBinding(dp, new Binding { Path = new PropertyPath(sourceDP), Source = source });
    }

    public static void CopyList<T>(
        IList<T> source,
        IList<T> destination)
    {
        destination.Clear();

        foreach (var element in source)
        {
            destination.Add(element);
        }
    }

    /*public static Window GetActiveWindow()
    {
        var activeWindow = User32.GetActiveWindow();
        if (activeWindow != IntPtr.Zero)
        {
            return HwndSource.FromHwnd(activeWindow)?.RootVisual as Window;
        }
        return null;
    }*/

    public static string SafeSubstring(this string s, int startIndex)
    {
        return s.SafeSubstring(startIndex, s.Length - startIndex);
    }

    public static string SafeSubstring(this string s, int startIndex, int length)
    {
        if (s is null)
        {
            throw new ArgumentNullException(nameof(s));
        }

        if (startIndex > s.Length)
        {
            return string.Empty;
        }

        if (length > s.Length - startIndex)
        {
            length = s.Length - startIndex;
        }

        return s.Substring(startIndex, length);
    }

    public static bool IndexOf(this UIElementCollection collection, UIElement element, out int index)
    {
        int i = collection.IndexOf(element);
        if (i >= 0)
        {
            index = i;
            return true;
        }
        else
        {
            index = 0;
            return false;
        }
    }

    public static string TryGetStringRepresentationFromObject(object obj)
    {
        return obj?.ToString() ?? string.Empty;
    }
}

public static class CppWinRTHelpers
{
    public static WinRTReturn GetTemplateChildT<WinRTReturn>(string childName, IControlProtected controlProtected) where WinRTReturn : DependencyObject
    {
        DependencyObject childAsDO = controlProtected.GetTemplateChild(childName);

        if (childAsDO != null)
        {
            return childAsDO as WinRTReturn;
        }
        return null;
    }
}

internal static class InputHelper
{
    #region IsTapEnabled

    public static readonly DependencyProperty IsTapEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsTapEnabled",
            typeof(bool),
            typeof(InputHelper),
            new PropertyMetadata(false, OnIsTapEnabledChanged));

    public static bool GetIsTapEnabled(UIElement element)
    {
        return (bool)element.GetValue(IsTapEnabledProperty);
    }

    public static void SetIsTapEnabled(UIElement element, bool value)
    {
        element.SetValue(IsTapEnabledProperty, value);
    }

    private static void OnIsTapEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var element = (UIElement)d;
        var oldValue = (bool)e.OldValue;
        var newValue = (bool)e.NewValue;

        if (newValue)
        {
            element.MouseLeftButtonDown += OnMouseLeftButtonDown;
            element.MouseLeftButtonUp += OnMouseLeftButtonUp;
            element.LostMouseCapture += OnLostMouseCapture;
            element.MouseLeave += OnMouseLeave;
        }
        else
        {
            element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            element.LostMouseCapture -= OnLostMouseCapture;
            element.MouseLeave -= OnMouseLeave;
        }
    }

    #endregion

    #region IsPressed

    public static readonly DependencyProperty IsPressedProperty =
        DependencyProperty.RegisterAttached(
            "IsPressed",
            typeof(bool),
            typeof(InputHelper),
            new PropertyMetadata(false));

    private static bool GetIsPressed(UIElement element)
    {
        return (bool)element.GetValue(IsPressedProperty);
    }

    private static void SetIsPressed(UIElement element, bool value)
    {
        if (value)
        {
            element.SetValue(IsPressedProperty, value);
        }
        else
        {
            element.ClearValue(IsPressedProperty);
        }
    }

    #endregion

    #region Tapped

    public static readonly RoutedEvent TappedEvent =
        EventManager.RegisterRoutedEvent(
            "Tapped",
            RoutingStrategy.Bubble,
            typeof(TappedEventHandler),
            typeof(InputHelper));

    public static void AddTappedHandler(UIElement element, TappedEventHandler handler)
    {
        element.AddHandler(TappedEvent, handler);
    }

    public static void RemoveTappedHandler(UIElement element, TappedEventHandler handler)
    {
        element.RemoveHandler(TappedEvent, handler);
    }

    private static void RaiseTapped(UIElement element, int timestamp)
    {
        var e = new TappedRoutedEventArgs { RoutedEvent = TappedEvent, Source = element, Timestamp = timestamp };
        _lastTappedArgs = e;
        element.RaiseEvent(e);
    }

    #endregion

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var element = (UIElement)sender;

        if (!GetIsPressed(element))
        {
            SetIsPressed(element, true);
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        var element = (UIElement)sender;

        if (GetIsPressed(element))
        {
            SetIsPressed((UIElement)sender, false);

            var lastArgs = _lastTappedArgs;

            if (lastArgs != null && lastArgs.Handled && lastArgs.Timestamp == e.Timestamp)
            {
                // Handled by a child element, don't raise
            }
            else
            {
                var elementBounds = new System.Windows.Rect(new System.Windows.Point(), element.RenderSize);
                if (elementBounds.Contains(e.GetPosition(element)))
                {
                    RaiseTapped(element, e.Timestamp);
                }
            }
        }
    }

    private static void OnLostMouseCapture(object sender, MouseEventArgs e)
    {
        SetIsPressed((UIElement)sender, false);
    }

    private static void OnMouseLeave(object sender, MouseEventArgs e)
    {
        SetIsPressed((UIElement)sender, false);
    }

    private static TappedRoutedEventArgs _lastTappedArgs;
}

internal delegate void TappedEventHandler(object sender, TappedRoutedEventArgs e);

internal sealed class TappedRoutedEventArgs : RoutedEventArgs
{
    public TappedRoutedEventArgs()
    {
    }

    //public Point GetPosition(UIElement relativeTo);

    //public PointerDeviceType PointerDeviceType { get; }

    internal int Timestamp { get; set; }
}

internal delegate void DependencyPropertyChangedCallback(DependencyObject sender, DependencyProperty dp);

internal class ControlStrings : ResourceAccessor
{
    public ControlStrings(Type controlType, ModernControlCategory category) : base(GetControlBaseName(controlType, category), GetControlAssembly(controlType))
    {

    }


    internal static string GetControlBaseName(Type controlType, ModernControlCategory category)
    {
        var root = controlType.Assembly.GetName().Name;

        root = root + "." + category.ToString() + "." + controlType.Name;
        root = root + "." + "Strings.Resources";

        return root;
    }

    internal static Assembly GetControlAssembly(Type controlType)
    {
        return controlType.Assembly;
    }
}

internal enum ModernControlCategory
{
    Windows,
    Community,
    Extended
}

internal class ResourceAccessor
{
    #region Resource Keys

    public const string SR_BasicRatingString = "BasicRatingString";
    public const string SR_CommunityRatingString = "CommunityRatingString";
    public const string SR_RatingsControlName = "RatingsControlName";
    public const string SR_RatingControlName = "RatingControlName";
    public const string SR_RatingUnset = "RatingUnset";
    public const string SR_NavigationButtonClosedName = "NavigationButtonClosedName";
    public const string SR_NavigationButtonOpenName = "NavigationButtonOpenName";
    public const string SR_NavigationViewItemDefaultControlName = "NavigationViewItemDefaultControlName";
    public const string SR_NavigationBackButtonName = "NavigationBackButtonName";
    public const string SR_NavigationBackButtonToolTip = "NavigationBackButtonToolTip";
    public const string SR_NavigationCloseButtonName = "NavigationCloseButtonName";
    public const string SR_NavigationOverflowButtonName = "NavigationOverflowButtonName";
    public const string SR_NavigationOverflowButtonText = "NavigationOverflowButtonText";
    public const string SR_NavigationOverflowButtonToolTip = "NavigationOverflowButtonToolTip";
    public const string SR_SettingsButtonName = "SettingsButtonName";
    public const string SR_NavigationViewSearchButtonName = "NavigationViewSearchButtonName";
    public const string SR_TextAlphaLabel = "TextAlphaLabel";
    public const string SR_AutomationNameAlphaSlider = "AutomationNameAlphaSlider";
    public const string SR_AutomationNameAlphaTextBox = "AutomationNameAlphaTextBox";
    public const string SR_AutomationNameHueSlider = "AutomationNameHueSlider";
    public const string SR_AutomationNameSaturationSlider = "AutomationNameSaturationSlider";
    public const string SR_AutomationNameValueSlider = "AutomationNameValueSlider";
    public const string SR_TextBlueLabel = "TextBlueLabel";
    public const string SR_AutomationNameBlueTextBox = "AutomationNameBlueTextBox";
    public const string SR_AutomationNameColorModelComboBox = "AutomationNameColorModelComboBox";
    public const string SR_AutomationNameColorSpectrum = "AutomationNameColorSpectrum";
    public const string SR_TextGreenLabel = "TextGreenLabel";
    public const string SR_AutomationNameGreenTextBox = "AutomationNameGreenTextBox";
    public const string SR_HelpTextColorSpectrum = "HelpTextColorSpectrum";
    public const string SR_AutomationNameHexTextBox = "AutomationNameHexTextBox";
    public const string SR_ContentHSVComboBoxItem = "ContentHSVComboBoxItem";
    public const string SR_TextHueLabel = "TextHueLabel";
    public const string SR_AutomationNameHueTextBox = "AutomationNameHueTextBox";
    public const string SR_LocalizedControlTypeColorSpectrum = "LocalizedControlTypeColorSpectrum";
    public const string SR_TextRedLabel = "TextRedLabel";
    public const string SR_AutomationNameRedTextBox = "AutomationNameRedTextBox";
    public const string SR_ContentRGBComboBoxItem = "ContentRGBComboBoxItem";
    public const string SR_TextSaturationLabel = "TextSaturationLabel";
    public const string SR_AutomationNameSaturationTextBox = "AutomationNameSaturationTextBox";
    public const string SR_TextValueLabel = "TextValueLabel";
    public const string SR_ValueStringColorSpectrumWithColorName = "ValueStringColorSpectrumWithColorName";
    public const string SR_ValueStringColorSpectrumWithoutColorName = "ValueStringColorSpectrumWithoutColorName";
    public const string SR_ValueStringHueSliderWithColorName = "ValueStringHueSliderWithColorName";
    public const string SR_ValueStringHueSliderWithoutColorName = "ValueStringHueSliderWithoutColorName";
    public const string SR_ValueStringSaturationSliderWithColorName = "ValueStringSaturationSliderWithColorName";
    public const string SR_ValueStringSaturationSliderWithoutColorName = "ValueStringSaturationSliderWithoutColorName";
    public const string SR_ValueStringValueSliderWithColorName = "ValueStringValueSliderWithColorName";
    public const string SR_ValueStringValueSliderWithoutColorName = "ValueStringValueSliderWithoutColorName";
    public const string SR_AutomationNameValueTextBox = "AutomationNameValueTextBox";
    public const string SR_ToolTipStringAlphaSlider = "ToolTipStringAlphaSlider";
    public const string SR_ToolTipStringHueSliderWithColorName = "ToolTipStringHueSliderWithColorName";
    public const string SR_ToolTipStringHueSliderWithoutColorName = "ToolTipStringHueSliderWithoutColorName";
    public const string SR_ToolTipStringSaturationSliderWithColorName = "ToolTipStringSaturationSliderWithColorName";
    public const string SR_ToolTipStringSaturationSliderWithoutColorName = "ToolTipStringSaturationSliderWithoutColorName";
    public const string SR_ToolTipStringValueSliderWithColorName = "ToolTipStringValueSliderWithColorName";
    public const string SR_ToolTipStringValueSliderWithoutColorName = "ToolTipStringValueSliderWithoutColorName";
    public const string SR_AutomationNameMoreButtonCollapsed = "AutomationNameMoreButtonCollapsed";
    public const string SR_AutomationNameMoreButtonExpanded = "AutomationNameMoreButtonExpanded";
    public const string SR_HelpTextMoreButton = "HelpTextMoreButton";
    public const string SR_TextMoreButtonLabelCollapsed = "TextMoreButtonLabelCollapsed";
    public const string SR_TextMoreButtonLabelExpanded = "TextMoreButtonLabelExpanded";
    public const string SR_BadgeItemPlural1 = "BadgeItemPlural1";
    public const string SR_BadgeItemPlural2 = "BadgeItemPlural2";
    public const string SR_BadgeItemPlural3 = "BadgeItemPlural3";
    public const string SR_BadgeItemPlural4 = "BadgeItemPlural4";
    public const string SR_BadgeItemPlural5 = "BadgeItemPlural5";
    public const string SR_BadgeItemPlural6 = "BadgeItemPlural6";
    public const string SR_BadgeItemPlural7 = "BadgeItemPlural7";
    public const string SR_BadgeItemSingular = "BadgeItemSingular";
    public const string SR_BadgeItemTextOverride = "BadgeItemTextOverride";
    public const string SR_BadgeIcon = "BadgeIcon";
    public const string SR_BadgeIconTextOverride = "BadgeIconTextOverride";
    public const string SR_PersonName = "PersonName";
    public const string SR_GroupName = "GroupName";
    public const string SR_CancelDraggingString = "CancelDraggingString";
    public const string SR_DefaultItemString = "DefaultItemString";
    public const string SR_DropIntoNodeString = "DropIntoNodeString";
    public const string SR_FallBackPlaceString = "FallBackPlaceString";
    public const string SR_PagerControlPageTextName = "PagerControlPageText";
    public const string SR_PagerControlPrefixTextName = "PagerControlPrefixText";
    public const string SR_PagerControlSuffixTextName = "PagerControlPrefixText";
    public const string SR_PagerControlFirstPageButtonTextName = "PagerControlFirstPageButtonText";
    public const string SR_PagerControlPreviousPageButtonTextName = "PagerControlPreviousPageButtonText";
    public const string SR_PagerControlNextPageButtonTextName = "PagerControlNextPageButtonText";
    public const string SR_PagerControlLastPageButtonTextName = "PagerControlLastPageButtonText";
    public const string SR_PipsPagerNameText = "PipsPagerNameText";
    public const string SR_PipsPagerNextPageButtonText = "PipsPagerNextPageButtonText";
    public const string SR_PipsPagerPreviousPageButtonText = "PipsPagerPreviousPageButtonText";
    public const string SR_PipsPagerPageText = "PipsPagerPageText";
    public const string SR_PlaceAfterString = "PlaceAfterString";
    public const string SR_PlaceBeforeString = "PlaceBeforeString";
    public const string SR_PlaceBetweenString = "PlaceBetweenString";
    public const string SR_ProgressRingName = "ProgressRingName";
    public const string SR_ProgressRingIndeterminateStatus = "ProgressRingIndeterminateStatus";
    public const string SR_ProgressBarIndeterminateStatus = "ProgressBarIndeterminateStatus";
    public const string SR_ProgressBarPausedStatus = "ProgressBarPausedStatus";
    public const string SR_ProgressBarErrorStatus = "ProgressBarErrorStatus";
    public const string SR_RatingLocalizedControlType = "RatingLocalizedControlType";
    public const string SR_SplitButtonSecondaryButtonName = "SplitButtonSecondaryButtonName";
    public const string SR_ProofingMenuItemLabel = "ProofingMenuItemLabel";
    public const string SR_TimePickerAM = "TimePickerAM";
    public const string SR_TimePickerHour = "TimePickerHour";
    public const string SR_TimePickerMinute = "TimePickerMinute";
    public const string SR_TimePickerSecond = "TimePickerSecond";
    public const string SR_TextCommandLabelCut = "TextCommandLabelCut";
    public const string SR_TextCommandLabelCopy = "TextCommandLabelCopy";
    public const string SR_TextCommandLabelPaste = "TextCommandLabelPaste";
    public const string SR_TextCommandLabelSelectAll = "TextCommandLabelSelectAll";
    public const string SR_TextCommandLabelBold = "TextCommandLabelBold";
    public const string SR_TextCommandLabelItalic = "TextCommandLabelItalic";
    public const string SR_TextCommandLabelUnderline = "TextCommandLabelUnderline";
    public const string SR_TextCommandLabelUndo = "TextCommandLabelUndo";
    public const string SR_TextCommandLabelRedo = "TextCommandLabelRedo";
    public const string SR_TextCommandDescriptionCut = "TextCommandDescriptionCut";
    public const string SR_TextCommandDescriptionCopy = "TextCommandDescriptionCopy";
    public const string SR_TextCommandDescriptionPaste = "TextCommandDescriptionPaste";
    public const string SR_TextCommandDescriptionSelectAll = "TextCommandDescriptionSelectAll";
    public const string SR_TextCommandDescriptionBold = "TextCommandDescriptionBold";
    public const string SR_TextCommandDescriptionItalic = "TextCommandDescriptionItalic";
    public const string SR_TextCommandDescriptionUnderline = "TextCommandDescriptionUnderline";
    public const string SR_TextCommandDescriptionUndo = "TextCommandDescriptionUndo";
    public const string SR_TextCommandDescriptionRedo = "TextCommandDescriptionRedo";
    public const string SR_TextCommandKeyboardAcceleratorKeyCut = "TextCommandKeyboardAcceleratorKeyCut";
    public const string SR_TextCommandKeyboardAcceleratorKeyCopy = "TextCommandKeyboardAcceleratorKeyCopy";
    public const string SR_TextCommandKeyboardAcceleratorKeyPaste = "TextCommandKeyboardAcceleratorKeyPaste";
    public const string SR_TextCommandKeyboardAcceleratorKeySelectAll = "TextCommandKeyboardAcceleratorKeySelectAll";
    public const string SR_TextCommandKeyboardAcceleratorKeyBold = "TextCommandKeyboardAcceleratorKeyBold";
    public const string SR_TextCommandKeyboardAcceleratorKeyItalic = "TextCommandKeyboardAcceleratorKeyItalic";
    public const string SR_TextCommandKeyboardAcceleratorKeyUnderline = "TextCommandKeyboardAcceleratorKeyUnderline";
    public const string SR_TextCommandKeyboardAcceleratorKeyUndo = "TextCommandKeyboardAcceleratorKeyUndo";
    public const string SR_TextCommandKeyboardAcceleratorKeyRedo = "TextCommandKeyboardAcceleratorKeyRedo";
    public const string SR_TeachingTipAlternateCloseButtonName = "TeachingTipAlternateCloseButtonName";
    public const string SR_TeachingTipAlternateCloseButtonTooltip = "TeachingTipAlternateCloseButtonTooltip";
    public const string SR_TeachingTipCustomLandmarkName = "TeachingTipCustomLandmarkName";
    public const string SR_TeachingTipNotification = "TeachingTipNotification";
    public const string SR_TeachingTipNotificationWithoutAppName = "TeachingTipNotificationWithoutAppName";
    public const string SR_TabViewAddButtonName = "TabViewAddButtonName";
    public const string SR_TabViewAddButtonTooltip = "TabViewAddButtonTooltip";
    public const string SR_TabViewCloseButtonName = "TabViewCloseButtonName";
    public const string SR_TabViewCloseButtonTooltip = "TabViewCloseButtonTooltip";
    public const string SR_TabViewCloseButtonTooltipWithKA = "TabViewCloseButtonTooltipWithKA";
    public const string SR_TabViewScrollDecreaseButtonTooltip = "TabViewScrollDecreaseButtonTooltip";
    public const string SR_TabViewScrollIncreaseButtonTooltip = "TabViewScrollIncreaseButtonTooltip";
    public const string SR_NumberBoxUpSpinButtonName = "NumberBoxUpSpinButtonName";
    public const string SR_NumberBoxDownSpinButtonName = "NumberBoxDownSpinButtonName";
    public const string SR_ExpanderDefaultControlName = "ExpanderDefaultControlName";

    public const string SR_InfoBarCloseButtonName = "InfoBarCloseButtonName";
    public const string SR_InfoBarOpenedNotification = "InfoBarOpenedNotification";
    public const string SR_InfoBarClosedNotification = "InfoBarClosedNotification";
    public const string SR_InfoBarCustomLandmarkName = "InfoBarCustomLandmarkName";
    public const string SR_InfoBarCloseButtonTooltip = "InfoBarCloseButtonTooltip";
    public const string SR_InfoBarSeverityInformationalName = "InfoBarSeverityInformationalName";
    public const string SR_InfoBarSeveritySuccessName = "InfoBarSeveritySuccessName";
    public const string SR_InfoBarSeverityWarningName = "InfoBarSeverityWarningName";
    public const string SR_InfoBarSeverityErrorName = "InfoBarSeverityErrorName";
    public const string SR_InfoBarIconSeverityInformationalName = "InfoBarIconSeverityInformationalName";
    public const string SR_InfoBarIconSeveritySuccessName = "InfoBarIconSeveritySuccessName";
    public const string SR_InfoBarIconSeverityWarningName = "InfoBarIconSeverityWarningName";
    public const string SR_InfoBarIconSeverityErrorName = "InfoBarIconSeverityErrorName";

    public const string IR_NoiseAsset_256X256_PNG = "NoiseAsset_256X256_PNG";

    #endregion

    private readonly Type? _controlType;
    private readonly string? _baseName;
    private readonly Assembly? _assembly;
    private ResourceManager _resourceManager;

    public ResourceAccessor(Type controlType)
    {
        _controlType = controlType ?? throw new ArgumentNullException(nameof(controlType));
    }

    public ResourceAccessor(string baseName, Assembly assembly)
    {
        _baseName = baseName;
        _assembly = assembly;
    }

    public string GetLocalizedStringResource(string resourceName)
    {
        try
        {
            if (_resourceManager is null)
            {
                _resourceManager = CreateResourceManager();
            }

            return _resourceManager.GetString(resourceName);
        }
        catch
        {
            // TODO: Check
            /*return Strings.ResourceManager.GetString(resourceName) ?? resourceName;*/
            return string.Empty;
        }
    }

    private ResourceManager CreateResourceManager()
    {
        var baseName = _baseName;
        var assembly = _assembly;

        if (_controlType != null)
        {
            assembly = _controlType.Assembly;
            var assemblyName = assembly.GetName().Name;

            var controlName = _controlType.Name;
            if (assemblyName != null)
            {
                baseName = $"{assemblyName}.{controlName}.Strings.Resources";
            }
        }

        return new ResourceManager(baseName, assembly);
    }
}

internal class ItemsRepeaterElementPreparedRevoker : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs>>
{
    public ItemsRepeaterElementPreparedRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler) : base(source, handler)
    {
    }

    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> handler)
    {
        source.ElementPrepared -= handler;
    }
}

internal class ItemsRepeaterElementClearingRevoker : EventRevoker<ItemsRepeater, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs>>
{
    public ItemsRepeaterElementClearingRevoker(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler) : base(source, handler)
    {
    }

    protected override void AddHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing += handler;
    }

    protected override void RemoveHandler(ItemsRepeater source, TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> handler)
    {
        source.ElementClearing -= handler;
    }
}

internal sealed class FlyoutBaseClosingEventArgs : EventArgs
{
    internal FlyoutBaseClosingEventArgs()
    {
    }

    public bool Cancel
    {
        get => false;
        set => throw new NotImplementedException();
    }
}
