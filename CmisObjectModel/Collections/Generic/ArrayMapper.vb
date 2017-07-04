'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'*
'* Contact: opensource<at>patorg.de
'* 
'* CmisObjectModelLibrary is a VB.NET implementation of the Content Management Interoperability Services (CMIS) standard
'*
'* This file is part of CmisObjectModelLibrary.
'* 
'* This library is free software; you can redistribute it and/or
'* modify it under the terms of the GNU Lesser General Public
'* License as published by the Free Software Foundation; either
'* version 3.0 of the License, or (at your option) any later version.
'*
'* This library is distributed in the hope that it will be useful,
'* but WITHOUT ANY WARRANTY; without even the implied warranty of
'* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
'* Lesser General Public License for more details.
'*
'* You should have received a copy of the GNU Lesser General Public
'* License along with this library (lgpl.txt).
'* If not, see <http://www.gnu.org/licenses/lgpl.txt>.
'***********************************************************************************************************************
Imports CmisObjectModel.Common
Imports CmisObjectModel.Constants
Imports cjs = CmisObjectModel.JSON.Serialization

Namespace CmisObjectModel.Collections.Generic
   ''' <summary>
   ''' Allows to access the elements of an array of XmlSerializable-instances via a customizable key
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapper(Of TOwner As Serialization.XmlSerializable, TItem As Serialization.XmlSerializable)
      Inherits ArrayMapper(Of TOwner, TItem, String)

      Public Sub New(owner As TOwner,
                     arrayPropertyName As String, getArray As Func(Of TItem()),
                     keyPropertyName As String, getKey As Func(Of TItem, String))
         MyBase.New(owner, New Common.Generic.DynamicProperty(Of TItem())(getArray, arrayPropertyName),
                    New Common.Generic.DynamicProperty(Of TItem, String)(getKey, keyPropertyName))
      End Sub

      ''' <summary>
      ''' Maps a new item
      ''' </summary>
      Protected Overrides Function Add(value As ArrayMapperItem(Of TItem, String)) As Boolean
         If MyBase.Add(value) Then
            Dim key As String = If(value.Key, String.Empty).ToLowerInvariant()
            If Not _mapIgnoreCase.ContainsKey(key) Then _mapIgnoreCase.Add(key, value)
            Return True
         Else
            Return False
         End If
      End Function

      Private _mapIgnoreCase As New Dictionary(Of String, ArrayMapperItem(Of TItem, String))

      Default Public Overloads ReadOnly Property Item(key As String, ignoreCase As Boolean) As TItem
         Get
            If key Is Nothing Then
               Return Nothing
            ElseIf ignoreCase Then
               'build maps
               If _items Is Nothing Then Refresh()
               key = key.ToLowerInvariant()
               Return If(_mapIgnoreCase.ContainsKey(key), _mapIgnoreCase(key).Item, Nothing)
            Else
               Return MyBase.Item(key)
            End If
         End Get
      End Property

      ''' <summary>
      ''' Removes item
      ''' </summary>
      Protected Overrides Sub Remove(item As TItem)
         Dim key As String = If(_keyProperty.Value(item), String.Empty).ToLowerInvariant()

         If _mapIgnoreCase.ContainsKey(key) AndAlso _mapIgnoreCase(key).Item Is item Then _mapIgnoreCase.Remove(key)
         MyBase.Remove(item)
      End Sub

      ''' <summary>
      ''' Ensures that the maps will be rebuild before the next access on array items
      ''' </summary>
      ''' <remarks></remarks>
      Protected Overrides Sub Reset()
         MyBase.Reset()
         _mapIgnoreCase.Clear()
      End Sub

   End Class

   ''' <summary>
   ''' Allows to access the elements of an array of XmlSerializable-instances via a customizable key
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapper(Of TOwner As Serialization.XmlSerializable,
                               TItem As Serialization.XmlSerializable,
                               TKey)
      Inherits ArrayMapperBase(Of TOwner, TItem, TKey, ArrayMapperItem(Of TItem, TKey))

      Public Sub New(owner As TOwner,
                     arrayProperty As Common.Generic.DynamicProperty(Of TItem()),
                     keyProperty As Common.Generic.DynamicProperty(Of TItem, TKey))
         MyBase.New(owner, arrayProperty, keyProperty)
      End Sub

      ''' <summary>
      ''' Creates an ArrayMapperItem suitable for item
      ''' </summary>
      Protected Overrides Function CreateArrayMapperItem(item As TItem, index As Integer) As ArrayMapperItem(Of TItem, TKey)
         Return New ArrayMapperItem(Of TItem, TKey)(item, _keyProperty.Value(item), index)
      End Function
   End Class

   ''' <summary>
   ''' Allows to access the elements of an array of XmlSerializable-instances via a customizable key
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapper(Of TOwner As Serialization.XmlSerializable, TItem As {Serialization.XmlSerializable, New}, TKey, TValue)
      Inherits ArrayMapper(Of TOwner, TItem, TKey, TValue, JSON.Serialization.Generic.DefaultObjectResolver(Of TItem))

      Public Sub New(owner As TOwner,
                     arrayProperty As Common.Generic.DynamicProperty(Of TItem()),
                     keyProperty As Common.Generic.DynamicProperty(Of TItem, TKey),
                     valueProperty As Common.Generic.DynamicProperty(Of TItem, TValue))
         MyBase.New(owner, arrayProperty, keyProperty, valueProperty)
      End Sub
   End Class

   ''' <summary>
   ''' Allows to access the elements of an array of XmlSerializable-instances via a customizable key
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapper(Of TOwner As Serialization.XmlSerializable, TItem As Serialization.XmlSerializable, TKey, TValue,
                               TObjectResolver As {JSON.Serialization.Generic.ObjectResolver(Of TItem), New})
      Inherits ArrayMapperBase(Of TOwner, TItem, TKey, ArrayMapperItem(Of TItem, TKey, TValue))

      Public Sub New(owner As TOwner,
                     arrayProperty As Common.Generic.DynamicProperty(Of TItem()),
                     keyProperty As Common.Generic.DynamicProperty(Of TItem, TKey),
                     valueProperty As Common.Generic.DynamicProperty(Of TItem, TValue))
         MyBase.New(owner, arrayProperty, keyProperty)
         _valueProperty = valueProperty
         _objectObserver = New TObjectResolver()
      End Sub

#Region "IJavaSerializationProvider"
      ''' <summary>
      ''' Loads data from serialized java-map
      ''' </summary>
      Public Overrides Function JavaImport(source As Object, serializer As cjs.JavaScriptSerializer) As Object
         Dim javaScriptConverter As cjs.JavaScriptConverter = If(serializer Is Nothing, Nothing, serializer.GetJavaScriptConverter(GetType(TValue)))

         If javaScriptConverter Is Nothing Then
            If TypeOf source Is IDictionary(Of TKey, TValue) Then
               Load(CType(source, IDictionary(Of TKey, TValue)))
            Else
               Reset()
            End If
         ElseIf TryConvertDictionary(Of TKey, IDictionary(Of String, Object))(source) Then
            Dim data As New Dictionary(Of TKey, TValue)

            For Each de As KeyValuePair(Of TKey, IDictionary(Of String, Object)) In CType(source, IDictionary(Of TKey, IDictionary(Of String, Object)))
               data.Add(de.Key, CType(javaScriptConverter.Deserialize(de.Value, GetType(TValue), serializer), TValue))
            Next
            Load(data)
         Else
            Reset()
         End If

         Return _items
      End Function

      ''' <summary>
      ''' Serializes the content as a java-map
      ''' </summary>
      Public Overrides Function JavaExport(obj As Object, serializer As cjs.JavaScriptSerializer) As Object
         Dim javaScriptConverter As cjs.JavaScriptConverter = If(serializer Is Nothing, Nothing, serializer.GetJavaScriptConverter(GetType(TValue)))

         If _items Is Nothing Then Refresh()
         If _map.Count = 0 Then
            Return Nothing
         ElseIf javaScriptConverter Is Nothing Then
            Dim valueType As Type = GetType(TValue)
            If valueType Is GetType(DateTime) Then
               Return _map.ToDictionary(Of TKey, Long)(Function(de) de.Key,
                                                       Function(de) CType(JavaExportDateTime(de.Value.Value), Long))
            ElseIf valueType Is GetType(DateTime()) Then
               Return _map.ToDictionary(Of TKey, Long())(Function(de) de.Key,
                                                         Function(de) CType(JavaExportDateTimeArray(de.Value.Value), Long()))
            ElseIf valueType Is GetType(DateTimeOffset) Then
               Return _map.ToDictionary(Of TKey, Long)(Function(de) de.Key,
                                                       Function(de) CType(JavaExportDateTimeOffset(de.Value.Value), Long))
            ElseIf valueType Is GetType(DateTimeOffset()) Then
               Return _map.ToDictionary(Of TKey, Long())(Function(de) de.Key,
                                                         Function(de) CType(JavaExportDateTimeOffsetArray(de.Value.Value), Long()))
            ElseIf valueType Is GetType(String()) Then
               Return _map.ToDictionary(Of TKey, String())(Function(de) de.Key,
                                                           Function(de) CType(JavaExportStringArray(de.Value.Value), String()))
            Else
               Return _map.ToDictionary(Of TKey, TValue)(Function(de) de.Key,
                                                         Function(de)
                                                            Dim currentValueType As Type = If(de.Value.Value Is Nothing, valueType, de.Value.Value.GetType())
                                                            If currentValueType Is GetType(DateTime) Then
                                                               Return CType(JavaExportDateTime(de.Value.Value), TValue)
                                                            ElseIf currentValueType Is GetType(DateTime()) Then
                                                               Return CType(JavaExportDateTimeArray(de.Value.Value), TValue)
                                                            ElseIf currentValueType Is GetType(DateTimeOffset) Then
                                                               Return CType(JavaExportDateTimeOffset(de.Value.Value), TValue)
                                                            ElseIf currentValueType Is GetType(DateTimeOffset()) Then
                                                               Return CType(JavaExportDateTimeOffsetArray(de.Value.Value), TValue)
                                                            ElseIf currentValueType Is GetType(String()) Then
                                                               Return CType(JavaExportStringArray(de.Value.Value), TValue)
                                                            Else
                                                               Return de.Value.Value
                                                            End If
                                                         End Function)
            End If
         Else
            Return _map.ToDictionary(Of TKey, IDictionary(Of String, Object))(Function(de) de.Key, Function(de) javaScriptConverter.Serialize(de.Value.Value, serializer))
         End If
      End Function

      ''' <summary>
      ''' Support for DateTime objects
      ''' </summary>
      Private Function JavaExportDateTime(value As Object) As Object
         Return CType(value, DateTime).ToJSONTime()
      End Function
      ''' <summary>
      ''' Support for DateTime arrays
      ''' </summary>
      Private Function JavaExportDateTimeArray(values As Object) As Object
         Return (From value As Object In CType(values, IEnumerable)
                 Select JavaExportDateTime(value)).ToArray()
      End Function
      ''' <summary>
      ''' Support for DateTimeOffset objects
      ''' </summary>
      Private Function JavaExportDateTimeOffset(value As Object) As Object
         Return CType(value, DateTimeOffset).DateTime.ToJSONTime()
      End Function
      ''' <summary>
      ''' Support for DateTimeOffset arrays
      ''' </summary>
      Private Function JavaExportDateTimeOffsetArray(values As Object) As Object
         Return (From value As Object In CType(values, IEnumerable)
                 Select JavaExportDateTimeOffset(value)).ToArray()
      End Function
      ''' <summary>
      ''' Support for String arrays (converts null elements to empty elements)
      ''' </summary>
      Private Function JavaExportStringArray(values As Object) As Object
         Return (From value As Object In CType(values, IEnumerable)
                 Select If(CStr(value), String.Empty)).ToArray()
      End Function
#End Region

      Protected Overrides Function CreateArrayMapperItem(item As TItem, index As Integer) As ArrayMapperItem(Of TItem, TKey, TValue)
         Return New ArrayMapperItem(Of TItem, TKey, TValue)(item, _keyProperty.Value(item), _valueProperty.Value(item), index)
      End Function

      ''' <summary>
      ''' Replaces the current content of the arraymapper with data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <remarks></remarks>
      Public Overridable Overloads Sub Load(data As IDictionary(Of TKey, TValue))
         If _arrayProperty.CanWrite AndAlso
            _keyProperty.CanWrite AndAlso
            _valueProperty.CanWrite Then
            Try
               _modifierThread = System.Threading.Thread.CurrentThread
               Reset()
               If data IsNot Nothing Then
                  Dim items As New List(Of TItem)(data.Count)

                  For Each de As KeyValuePair(Of TKey, TValue) In data
                     Dim item As TItem = _objectObserver.CreateInstance(de.Value)

                     _keyProperty.Value(item) = de.Key
                     _valueProperty.Value(item) = de.Value
                     items.Add(item)
                  Next
                  _items = items.ToArray()
               End If
               _arrayProperty.Value = _items
            Finally
               _modifierThread = Nothing
            End Try
         End If
      End Sub

      ''' <summary>
      ''' Mounts key/value-pair to the collection. If the specified key is already in the
      ''' collection, item replaces that entry, otherwise a new entry is copied to the collection
      ''' </summary>
      ''' <param name="key"></param>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Overloads Function Mount(key As TKey, value As TValue) As Boolean
         If _arrayProperty.CanWrite AndAlso
            key IsNot Nothing AndAlso _keyProperty.CanWrite AndAlso
            _valueProperty.CanWrite Then
            Dim item As TItem = MyBase.Item(key)

            If item Is Nothing Then
               item = _objectObserver.CreateInstance(value)
               _keyProperty.Value(item) = key
               _valueProperty.Value(item) = value

               Return MyBase.Mount(item)
            Else
               _keyProperty.Value(item) = key
               _valueProperty.Value(item) = value

               Return True
            End If
         Else
            Return False
         End If
      End Function

      Protected _objectObserver As TObjectResolver

      Default Public Overridable Shadows Property Value(index As Integer) As TValue
         Get
            If _items Is Nothing Then Refresh()
            If _items(index) Is Nothing Then
               Return Nothing
            Else
               Return Value(_keyProperty.Value(_items(index)))
            End If
         End Get
         Set(value As TValue)
            If _items Is Nothing Then Refresh()
            If _valueProperty.CanWrite AndAlso _items(index) IsNot Nothing Then
               _valueProperty.Value(_items(index)) = value
            End If
         End Set
      End Property
      Default Public Overridable Shadows ReadOnly Property Value(key As TKey) As TValue
         Get
            If _items Is Nothing Then Refresh()
            Return If(key IsNot Nothing AndAlso _map.ContainsKey(key), _map(key).Value, Nothing)
         End Get
      End Property

      Protected _valueProperty As Common.Generic.DynamicProperty(Of TItem, TValue)
   End Class

   ''' <summary>
   ''' Allows to access the elements of an array of XmlSerializable-instances via a customizable key
   ''' </summary>
   ''' <remarks></remarks>
   <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
   Public MustInherit Class ArrayMapperBase(Of TOwner As Serialization.XmlSerializable,
                                                TItem As Serialization.XmlSerializable,
                                                TKey, TArrayMapperItem As ArrayMapperItem(Of TItem, TKey))
      Implements Contracts.IJavaSerializationProvider

      Public Sub New(owner As TOwner,
                     arrayProperty As Common.Generic.DynamicProperty(Of TItem()),
                     keyProperty As Common.Generic.DynamicProperty(Of TItem, TKey))
         _owner = owner
         _arrayProperty = arrayProperty
         If _owner IsNot Nothing AndAlso _arrayProperty IsNot Nothing Then _owner.AddHandler(AddressOf _xmlSerializable_PropertyChanged, _arrayProperty.PropertyName)
         _keyProperty = keyProperty
      End Sub

#Region "IJavaSerializationProvider"
      ''' <summary>
      ''' Loads data from serialized java-map
      ''' </summary>
      Public Overridable Function JavaImport(source As Object, serializer As cjs.JavaScriptSerializer) As Object Implements Contracts.IJavaSerializationProvider.JavaImport
         Dim javaScriptConverter As cjs.JavaScriptConverter = If(serializer Is Nothing, Nothing, serializer.GetJavaScriptConverter(GetType(TItem)))

         If javaScriptConverter IsNot Nothing AndAlso
            TryConvertDictionary(Of TKey, IDictionary(Of String, Object))(source) Then
            Dim data As New Dictionary(Of TKey, TItem)

            For Each de As KeyValuePair(Of TKey, IDictionary(Of String, Object)) In CType(source, IDictionary(Of TKey, IDictionary(Of String, Object)))
               data.Add(de.Key, CType(javaScriptConverter.Deserialize(de.Value, GetType(TItem), serializer), TItem))
            Next
            Load(data)
         Else
            Reset()
         End If

         Return _items
      End Function

      ''' <summary>
      ''' Serializes the content as a java-map
      ''' </summary>
      Public Overridable Function JavaExport(obj As Object, serializer As cjs.JavaScriptSerializer) As Object Implements Contracts.IJavaSerializationProvider.JavaExport
         Dim javaScriptConverter As cjs.JavaScriptConverter = If(serializer Is Nothing, Nothing, serializer.GetJavaScriptConverter(GetType(TItem)))

         If javaScriptConverter Is Nothing Then
            Return Nothing
         Else
            If _items Is Nothing Then Refresh()
            Return _map.ToDictionary(Of TKey, IDictionary(Of String, Object))(Function(de) de.Key, Function(de) javaScriptConverter.Serialize(de.Value.Item, serializer))
         End If
      End Function
#End Region

      ''' <summary>
      ''' Maps a new item
      ''' </summary>
      Protected Overridable Function Add(value As TArrayMapperItem) As Boolean
         Dim key As TKey = If(value Is Nothing, Nothing, value.Key)

         If Not (key Is Nothing OrElse _map.ContainsKey(key)) Then
            _map.Add(key, value)
            Return True
         Else
            Return False
         End If
      End Function

      Protected _arrayProperty As Common.Generic.DynamicProperty(Of TItem())

      Public ReadOnly Property Count As Integer
         Get
            If _items Is Nothing Then Refresh()
            Return _items.Length
         End Get
      End Property

      ''' <summary>
      ''' Creates an ArrayMapperItem suitable for item
      ''' </summary>
      Protected MustOverride Function CreateArrayMapperItem(item As TItem, index As Integer) As TArrayMapperItem

      ''' <summary>
      ''' Returns the index of the item corresponding with key
      ''' </summary>
      Public ReadOnly Property IndexOf(key As TKey) As Integer
         Get
            If _items Is Nothing Then Refresh()
            Return If(key IsNot Nothing AndAlso _map.ContainsKey(key), _map(key).Index, -1)
         End Get
      End Property

      Protected _items As TItem()
      Public ReadOnly Property Items As TItem()
         Get
            Return _items
         End Get
      End Property

      Default Public Overridable Property Item(index As Integer) As TItem
         Get
            If _items Is Nothing Then Refresh()
            Return _items(index)
         End Get
         Set(value As TItem)
            Dim oldValue As TItem

            If _items Is Nothing Then Refresh()
            oldValue = _items(index)
            If _arrayProperty.CanWrite AndAlso
               oldValue IsNot value Then
               Try
                  Dim length As Integer = _items.Length
                  Dim items As TItem() = CType(Array.CreateInstance(GetType(TItem), _items.Length), TItem())
                  Array.Copy(_items, items, length)

                  'replace item
                  _modifierThread = System.Threading.Thread.CurrentThread
                  items(index) = value
                  If oldValue IsNot Nothing Then Remove(oldValue)
                  If value IsNot Nothing Then Add(CreateArrayMapperItem(value, index))
                  _arrayProperty.Value = items
                  _items = items
               Finally
                  _modifierThread = Nothing
               End Try
            End If
         End Set
      End Property
      Default Public Overridable ReadOnly Property Item(key As TKey) As TItem
         Get
            If _items Is Nothing Then Refresh()
            Return If(key IsNot Nothing AndAlso _map.ContainsKey(key), _map(key).Item, Nothing)
         End Get
      End Property

      Protected _keyProperty As Common.Generic.DynamicProperty(Of TItem, TKey)

      Public ReadOnly Property Keys As TKey()
         Get
            Return _map.Keys.ToArray()
         End Get
      End Property

      ''' <summary>
      ''' Replaces the current content of the arraymapper with data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <remarks></remarks>
      Public Overridable Sub Load(data As IDictionary(Of TKey, TItem))
         If _arrayProperty.CanWrite AndAlso
            _keyProperty.CanWrite Then
            Try
               _modifierThread = System.Threading.Thread.CurrentThread
               Reset()
               If data IsNot Nothing Then
                  Dim items As New List(Of TItem)(data.Count)

                  For Each de As KeyValuePair(Of TKey, TItem) In data
                     _keyProperty.Value(de.Value) = de.Key
                     items.Add(de.Value)
                  Next
                  _items = items.ToArray()
               End If
               _arrayProperty.Value = _items
            Finally
               _modifierThread = Nothing
            End Try
         End If
      End Sub
      ''' <summary>
      ''' Replaces the current content of the arraymapper with data
      ''' </summary>
      ''' <param name="data"></param>
      ''' <remarks></remarks>
      Public Overridable Sub Load(data As IList(Of TItem))
         If _arrayProperty.CanWrite Then
            Try
               _modifierThread = System.Threading.Thread.CurrentThread
               Reset()
               If data IsNot Nothing Then _items = data.ToArray()
               _arrayProperty.Value = _items
            Finally
               _modifierThread = Nothing
            End Try
         End If
      End Sub

      Protected _map As New Dictionary(Of TKey, TArrayMapperItem)
      Protected _modifierThread As System.Threading.Thread

      ''' <summary>
      ''' Mounts item to the collection. If the key specified by item is already in the
      ''' collection, item replaces that entry, otherwise a new entry is copied to the collection
      ''' </summary>
      Public Overridable Function Mount(item As TItem) As Boolean
         If _items Is Nothing Then Refresh()
         If _arrayProperty.CanWrite AndAlso item IsNot Nothing Then
            Dim key As TKey = _keyProperty.Value(item)
            Dim oldValue As TArrayMapperItem = If(key IsNot Nothing AndAlso _map.ContainsKey(key), _map(key), Nothing)

            If oldValue Is Nothing Then
               'add a new item
               Try
                  Dim length As Integer = _items.Length
                  Dim items As TItem() = CType(Array.CreateInstance(GetType(TItem), length + 1), TItem())

                  If length > 0 Then Array.Copy(_items, items, length)
                  items(length) = item
                  _modifierThread = System.Threading.Thread.CurrentThread
                  Add(CreateArrayMapperItem(item, length))
                  _arrayProperty.Value = items
                  _items = items
               Finally
                  _modifierThread = Nothing
               End Try
            Else
               'replacement
               Me.Item(oldValue.Index) = item
            End If

            Return True
         Else
            Return False
         End If
      End Function

      Protected Overridable Sub _xmlSerializable_PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs)
         'ignore property changed events if caused by an action within this instance
         If System.Threading.Thread.CurrentThread IsNot _modifierThread Then
            Reset()
         End If
      End Sub

      Protected _owner As TOwner
      Public ReadOnly Property Owner As TOwner
         Get
            Return _owner
         End Get
      End Property

      ''' <summary>
      ''' Rebuilds the maps
      ''' </summary>
      ''' <remarks></remarks>
      Protected Overridable Sub Refresh()
         Reset()
         'get the current items
         _items = If(_arrayProperty.Value, New TItem() {})

         For index As Integer = 0 To _items.Length - 1
            Dim item As TItem = _items(index)

            If item IsNot Nothing Then
               Add(CreateArrayMapperItem(item, index))
               If _keyProperty IsNot Nothing Then
                  item.AddHandler(AddressOf _xmlSerializable_PropertyChanged, _keyProperty.PropertyName)
               End If
            End If
         Next
      End Sub

      ''' <summary>
      ''' Removes item
      ''' </summary>
      Protected Overridable Sub Remove(item As TItem)
         Dim key As TKey = _keyProperty.Value(item)

         If key IsNot Nothing AndAlso _map.ContainsKey(key) AndAlso _map(key).Item Is item Then _map.Remove(key)
         If _keyProperty IsNot Nothing Then
            item.RemoveHandler(AddressOf _xmlSerializable_PropertyChanged, _keyProperty.PropertyName)
         End If
      End Sub

      ''' <summary>
      ''' Ensures that the maps will be rebuild before the next access on array items
      ''' </summary>
      ''' <remarks></remarks>
      Protected Overridable Sub Reset()
         'remove items from event handler
         If _items IsNot Nothing Then
            If _keyProperty IsNot Nothing Then
               Dim propertyName As String = _keyProperty.PropertyName

               For Each item As TItem In _items
                  item.RemoveHandler(AddressOf _xmlSerializable_PropertyChanged, propertyName)
               Next
            End If
            _items = Nothing
            _map.Clear()
         End If
      End Sub

   End Class

   ''' <summary>
   ''' Item of an ArrayMapper-instance
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapperItem(Of TItem As Serialization.XmlSerializable, TKey)
      Public Sub New(item As TItem, key As TKey, index As Integer)
         Me.Item = item
         Me.Key = key
         Me.Index = index
      End Sub

      Public ReadOnly Index As Integer
      Public ReadOnly Item As TItem
      Public ReadOnly Key As TKey
   End Class

   ''' <summary>
   ''' Item of an ArrayMapper-instance
   ''' </summary>
   ''' <remarks></remarks>
   Public Class ArrayMapperItem(Of TItem As Serialization.XmlSerializable, TKey, TValue)
      Inherits ArrayMapperItem(Of TItem, TKey)

      Public Sub New(item As TItem, key As TKey, value As TValue, index As Integer)
         MyBase.New(item, key, index)
         Me.Value = value
      End Sub

      Public ReadOnly Value As TValue
   End Class
End Namespace