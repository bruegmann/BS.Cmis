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
Imports cac = CmisObjectModel.Attributes.CmisTypeInfoAttribute
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Extensions.Data
   <sxs.XmlRoot("converterDefinition", Namespace:=Constants.Namespaces.com),
    Attributes.CmisTypeInfo("com:converterDefinition", Nothing, "converterDefinition"),
    Attributes.JavaScriptConverter(GetType(JSON.Extensions.Data.ConverterDefinitionConverter))>
   Public Class ConverterDefinition
      Inherits Extension

      Public Sub New()
      End Sub

      Public Sub New(propertyDefinitionId As String, ParamArray items As ConverterDefinitionItem())
         If items IsNot Nothing AndAlso items.Length > 0 Then
            _items.AddRange(items)
         End If
         _propertyDefinitionId = propertyDefinitionId
      End Sub

      Public Sub New(propertyDefinitionId As String, nullValueMapping As String, ParamArray items As ConverterDefinitionItem())
         Me.New(propertyDefinitionId, items)
         _nullValueMapping = nullValueMapping
      End Sub

      Public Sub New(propertyDefinitionId As String, nullValueMapping As String, converterIdentifier As String, ParamArray items As ConverterDefinitionItem())
         Me.New(propertyDefinitionId, nullValueMapping, items)
         _converterIdentifier = converterIdentifier
      End Sub

#Region "Helper classes"
      ''' <summary>
      ''' Base class to create a PropertyValueConverter
      ''' </summary>
      ''' <remarks></remarks>
      Private MustInherit Class ConverterFactory

#Region "Constructors"
         Private Shared _factories As New Dictionary(Of Type, Dictionary(Of Type, ConverterFactory))
         Public Shared Function CreateInstance(localType As Type, remoteType As Type) As ConverterFactory
            SyncLock _factories
               Dim factories As Dictionary(Of Type, ConverterFactory)

               If _factories.ContainsKey(localType) Then
                  factories = _factories(localType)
               Else
                  factories = New Dictionary(Of Type, ConverterFactory)
                  _factories.Add(localType, factories)
               End If
               If factories.ContainsKey(remoteType) Then
                  Return factories(remoteType)
               Else
                  Dim genericTypeDefinition As Type = GetType(ConverterFactory(Of String, String)).GetGenericTypeDefinition
                  Dim genericType As Type = genericTypeDefinition.MakeGenericType(localType, remoteType)
                  Dim ci As System.Reflection.ConstructorInfo = genericType.GetConstructor(New Type() {})
                  Dim retVal As ConverterFactory = CType(ci.Invoke(New Object() {}), ConverterFactory)

                  factories.Add(remoteType, retVal)
                  Return retVal
               End If
            End SyncLock
         End Function
#End Region

#Region "Helper classes"
         ''' <summary>
         ''' Factory for typesafe converters
         ''' </summary>
         ''' <typeparam name="TLocal"></typeparam>
         ''' <typeparam name="TRemote"></typeparam>
         ''' <remarks></remarks>
         Private Class ConverterFactory(Of TLocal, TRemote)
            Inherits ConverterFactory

            ''' <summary>
            ''' Creates a typesafe converter
            ''' </summary>
            ''' <param name="Definition"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Overrides Function CreateConverter(definition As ConverterDefinition) As CmisObjectModel.Data.PropertyValueConverter
               Dim map As New Dictionary(Of TLocal, TRemote)
               Dim nullValueMapping As CmisObjectModel.Common.Generic.Nullable(Of TRemote)

               If Not String.IsNullOrEmpty(definition._nullValueMapping) Then
                  nullValueMapping = New CmisObjectModel.Common.Generic.Nullable(Of TRemote)(CType(CObj(definition._nullValueMapping), TRemote))
               End If
               For Each item As ConverterDefinitionItem In definition._items
                  Try
                     map.Add(CType(CObj(item.Key), TLocal), CType(CObj(item.Value), TRemote))
                  Catch
                  End Try
               Next

               Return New CmisObjectModel.Data.Generic.PropertyValueConverter(Of TLocal, TRemote)(map, nullValueMapping)
            End Function
         End Class
#End Region

         Public MustOverride Function CreateConverter(definition As ConverterDefinition) As CmisObjectModel.Data.PropertyValueConverter
      End Class
#End Region

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of ConverterDefinition, String)) From {
         {"converteridentifier", AddressOf SetConverterIdentifier},
         {"propertydefinitionid", AddressOf SetPropertyDefinitionId}} '_setter

      ''' <summary>
      ''' Deserialization of all properties stored in attributes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadAttributes(reader As System.Xml.XmlReader)
         'at least one property is serialized in an attribute-value
         If _setter.Count > 0 Then
            For attributeIndex As Integer = 0 To reader.AttributeCount - 1
               reader.MoveToAttribute(attributeIndex)
               'attribute name
               Dim key As String = reader.Name.ToLowerInvariant
               If _setter.ContainsKey(key) Then _setter(key).Invoke(Me, reader.GetAttribute(attributeIndex))
            Next
         End If
      End Sub

      Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _localType = ReadOptionalEnum(reader, attributeOverrides, "localType", _localType)
         _remoteType = ReadOptionalEnum(reader, attributeOverrides, "remoteType", _remoteType)
         _nullValueMapping = Read(reader, attributeOverrides, "nullValueMapping", Nothing)

         Dim items As ConverterDefinitionItem() = ReadArray(Of ConverterDefinitionItem)(reader, attributeOverrides, "item", Constants.Namespaces.com, AddressOf GenericXmlSerializableFactory(Of ConverterDefinitionItem))
         If items Is Nothing OrElse items.Length = 0 Then
            _items = New List(Of ConverterDefinitionItem)
         Else
            _items = New List(Of ConverterDefinitionItem)(items)
         End If
      End Sub

      Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         If Not String.IsNullOrEmpty(_converterIdentifier) Then WriteAttribute(writer, attributeOverrides, "converterIdentifier", Nothing, _converterIdentifier)
         WriteAttribute(writer, attributeOverrides, "propertyDefinitionId", Nothing, _propertyDefinitionId)
         If _localType.HasValue Then WriteElement(writer, attributeOverrides, "localType", Constants.Namespaces.com, _localType.Value.GetName())
         If _remoteType.HasValue Then WriteElement(writer, attributeOverrides, "remoteType", Constants.Namespaces.com, _remoteType.Value.GetName())
         If Not String.IsNullOrEmpty(_nullValueMapping) Then WriteElement(writer, attributeOverrides, "nullValueMapping", Constants.Namespaces.com, _nullValueMapping)
         If _items.Count > 0 Then WriteArray(writer, attributeOverrides, "item", Constants.Namespaces.com, _items.ToArray())
      End Sub
#End Region

#Region "Converter by identifier"
      Private Shared _converterFactories As New Dictionary(Of String, Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter))

      ''' <summary>
      ''' Returns a registered factory if available, otherwise null
      ''' </summary>
      ''' <param name="instance"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function GetStoredFactory(instance As ConverterDefinition) As Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter)
         SyncLock _converterFactories
            If Not String.IsNullOrEmpty(instance._converterIdentifier) AndAlso _converterFactories.ContainsKey(instance._converterIdentifier) Then
               Return _converterFactories(instance._converterIdentifier)
            Else
               Return Nothing
            End If
         End SyncLock
      End Function

      ''' <summary>
      ''' Registers a factory for converterIdentifier
      ''' </summary>
      ''' <param name="converterIdentifier"></param>
      ''' <param name="factory"></param>
      ''' <remarks></remarks>
      Public Shared Sub RegisterConverterFactory(converterIdentifier As String, factory As Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter))
         SyncLock _converterFactories
            If Not (String.IsNullOrEmpty(converterIdentifier) OrElse _converterFactories.ContainsKey(converterIdentifier)) Then
               _converterFactories.Add(converterIdentifier, factory)
            End If
         End SyncLock
      End Sub

      ''' <summary>
      ''' Unregisters a factory for converterIdentifier
      ''' </summary>
      ''' <param name="converterIdentifier"></param>
      ''' <param name="factory"></param>
      ''' <remarks></remarks>
      Public Shared Sub UnRegisterConverterFactory(converterIdentifier As String, factory As Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter))
         SyncLock _converterFactories
            Dim storedFactory As Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter) =
               If(String.IsNullOrEmpty(converterIdentifier) OrElse Not _converterFactories.ContainsKey(converterIdentifier),
                  Nothing, _converterFactories(converterIdentifier))
            If storedFactory IsNot Nothing AndAlso factory IsNot Nothing AndAlso
               storedFactory.Target Is factory.Target AndAlso
               storedFactory.Method.MethodHandle.Value.Equals(factory.Method.MethodHandle.Value) Then
               _converterFactories.Remove(converterIdentifier)
            End If
         End SyncLock
      End Sub
#End Region

      ''' <summary>
      ''' Adds a new item to the items collection
      ''' </summary>
      ''' <param name="item"></param>
      ''' <remarks></remarks>
      Public Sub Add(item As ConverterDefinitionItem)
         If item IsNot Nothing Then
            _items.Add(item)
            OnPropertyChanged("Items")
         End If
      End Sub

      Private _converterIdentifier As String
      Public Property ConverterIdentifier As String
         Get
            Return _converterIdentifier
         End Get
         Set(value As String)
            If _converterIdentifier <> value Then
               Dim oldValue As String = _converterIdentifier
               _converterIdentifier = value
               OnPropertyChanged("ConverterIdentifier", value, oldValue)
            End If
         End Set
      End Property 'ConverterIdentifier
      Private Shared Sub SetConverterIdentifier(instance As ConverterDefinition, value As String)
         instance._converterIdentifier = value
      End Sub

      ''' <summary>
      ''' Creates a converter depending on local and remote type
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateConverter() As CmisObjectModel.Data.PropertyValueConverter
         Dim factory As Func(Of ConverterDefinitionItem(), CmisObjectModel.Data.PropertyValueConverter) = GetStoredFactory(Me)
         Return If(factory Is Nothing,
                   ConverterFactory.CreateInstance(GetLocalType(), GetRemoteType()).CreateConverter(Me),
                   factory(_items.ToArray()))
      End Function

      Public Function GetLocalType() As Type
         Return Me.GetType(_localType)
      End Function
      Public Function GetRemoteType() As Type
         Return Me.GetType(_remoteType)
      End Function
      Private Overloads Function [GetType](type As CmisObjectModel.Common.enumConverterSupportedTypes?) As Type
         Select Case If(type.HasValue, type.Value, CmisObjectModel.Common.enumConverterSupportedTypes.string)
            Case enumConverterSupportedTypes.boolean
               Return GetType(Boolean)
            Case enumConverterSupportedTypes.decimal
               Return If(CmisObjectModel.Common.DecimalRepresentation = enumDecimalRepresentation.decimal, GetType(Decimal), GetType(Double))
            Case enumConverterSupportedTypes.integer
               Return GetType(xs_Integer)
            Case Else
               Return GetType(String)
         End Select
      End Function

      Private _items As New List(Of ConverterDefinitionItem)
      Public Property Items As ConverterDefinitionItem()
         Get
            Return _items.ToArray()
         End Get
         Private Set(value As ConverterDefinitionItem())
            _items.Clear()
            If value IsNot Nothing AndAlso value.Length > 0 Then _items.AddRange(value)
            OnPropertyChanged("Items")
         End Set
      End Property 'Items

      Private _localType As CmisObjectModel.Common.enumConverterSupportedTypes?
      Public Property LocalType As CmisObjectModel.Common.enumConverterSupportedTypes?
         Get
            Return _localType
         End Get
         Set(value As CmisObjectModel.Common.enumConverterSupportedTypes?)
            If Not _localType.Equals(value) Then
               Dim oldValue As CmisObjectModel.Common.enumConverterSupportedTypes? = _localType
               _localType = value
               OnPropertyChanged("LocalType", value, oldValue)
            End If
         End Set
      End Property 'LocalType

      Private _nullValueMapping As String
      Public Property NullValueMapping As String
         Get
            Return _nullValueMapping
         End Get
         Set(value As String)
            If _nullValueMapping <> value Then
               Dim oldValue As String = _nullValueMapping
               _nullValueMapping = value
               OnPropertyChanged("NullValueMapping", value, oldValue)
            End If
         End Set
      End Property 'NullValueMapping

      Protected _propertyDefinitionId As String
      Public Overridable Property PropertyDefinitionId As String
         Get
            Return _propertyDefinitionId
         End Get
         Set(value As String)
            If _propertyDefinitionId <> value Then
               Dim oldValue As String = _propertyDefinitionId
               _propertyDefinitionId = value
               OnPropertyChanged("PropertyDefinitionId", value, oldValue)
            End If
         End Set
      End Property 'PropertyDefinitionId
      Private Shared Sub SetPropertyDefinitionId(instance As ConverterDefinition, value As String)
         instance._propertyDefinitionId = value
      End Sub

      Private _remoteType As CmisObjectModel.Common.enumConverterSupportedTypes?
      Public Property RemoteType As CmisObjectModel.Common.enumConverterSupportedTypes?
         Get
            Return _remoteType
         End Get
         Set(value As CmisObjectModel.Common.enumConverterSupportedTypes?)
            If Not _remoteType.Equals(value) Then
               Dim oldValue As CmisObjectModel.Common.enumConverterSupportedTypes? = _remoteType
               _remoteType = value
               OnPropertyChanged("RemoteType", value, oldValue)
            End If
         End Set
      End Property 'RemoteType

      ''' <summary>
      ''' Removes the specified item from the items collection
      ''' </summary>
      ''' <param name="item"></param>
      ''' <remarks></remarks>
      Public Sub RemoveItem(item As ConverterDefinitionItem)
         If item IsNot Nothing AndAlso _items.Remove(item) Then OnPropertyChanged("Items")
      End Sub
   End Class
End Namespace