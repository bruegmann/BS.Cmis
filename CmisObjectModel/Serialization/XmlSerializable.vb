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
Imports sc = System.ComponentModel
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Serialization
   ''' <summary>
   ''' Baseclass for all Cmis-Types
   ''' </summary>
   ''' <remarks>
   ''' Attributes.JavaScriptObjectResolver(GetType(JSON.Serialization.Generic.DefaultObjectResolver(Of Core.cmisObjectType)), "{"""":""T""}")
   ''' declares a fallback mechanism for Cmis-Types to allow JavaScript-serialization. If there is no other ObjectResolver defined for a
   ''' type then the current type is used for the generic typeargument of DefaultObjectResolver(Of T) (empty string """" is mapped to ""T"")
   ''' </remarks>
   <Attributes.JavaScriptConverter(GetType(JSON.Serialization.Generic.XmlSerializerConverter(Of CmisObjectModel.Core.cmisObjectType)), "{"""":""TSerializable""}"),
    Attributes.JavaScriptObjectResolver(GetType(JSON.Serialization.Generic.DefaultObjectResolver(Of Core.cmisObjectType)), "{"""":""T""}")>
   Public MustInherit Class XmlSerializable
      Implements sc.INotifyPropertyChanged, sxs.IXmlSerializable

      Protected Sub New()
      End Sub
      Protected Sub New(initClassSupported As Boolean?)
         If initClassSupported.HasValue AndAlso initClassSupported.Value Then InitClass()
      End Sub
      Protected Overridable Sub InitClass()
      End Sub

      ''' <summary>
      ''' Creates a new instance
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Overridable Function Copy() As XmlSerializable
         Dim retVal As XmlSerializable = CType(Me.GetType().GetConstructor(New Type() {}).Invoke(New Object() {}), XmlSerializable)
         CopyPropertiesTo(retVal)
         Return retVal
      End Function
      ''' <summary>
      ''' Copies the serializable properties to destination-object
      ''' </summary>
      ''' <remarks>destination and current instance MUST be of the same type</remarks>
      Protected Overridable Sub CopyPropertiesTo(destination As XmlSerializable)
         Dim currentType As Type = Me.GetType()
         Dim objectType As Type = GetType(Object)
         Dim stringType As Type = GetType(String)
         Dim xmlSerializableType As Type = GetType(XmlSerializable)
         Dim propertyNames As New System.Collections.Generic.HashSet(Of String)
         Dim emptyIndex As Object() = New Object() {}

         'To support possible property overloads correct the algorithm starts with the currentType (that is the type of the new destination-object)
         'and then handles the properties of the baseclasses until XmlSerializable-Type is reached.
         'Copied properties are primitives properties, enums, strings and properties of type XmlSerializable or derived from it, furthermore arrays which
         'elementtypes accords to the same list.
         Do
            For Each pi As System.Reflection.PropertyInfo In currentType.GetProperties(Reflection.BindingFlags.DeclaredOnly Or Reflection.BindingFlags.Public Or Reflection.BindingFlags.Instance)
               Dim propertyType As Type = pi.PropertyType
               Dim isArray As Boolean = propertyType.IsArray
               Dim elementType As Type = If(isArray, propertyType.GetElementType, propertyType)
               Dim isXmlSerializable As Boolean = xmlSerializableType.IsAssignableFrom(elementType)
               Dim indexParameters As Reflection.ParameterInfo() = pi.GetIndexParameters()

               If pi.CanRead AndAlso pi.CanWrite AndAlso
                  (indexParameters Is Nothing OrElse indexParameters.Length = 0) AndAlso
                  propertyNames.Add(pi.Name) AndAlso
                  (elementType.IsEnum OrElse elementType.IsPrimitive OrElse elementType Is stringType OrElse isXmlSerializable) Then
                  Dim value As Object = pi.GetValue(Me, emptyIndex)

                  If value Is Nothing Then
                     pi.SetValue(destination, value, emptyIndex)
                  ElseIf isArray Then
                     With CType(value, Array)
                        Dim length As Integer = .Length
                        Dim newArray As Array = Array.CreateInstance(elementType, length)

                        If isXmlSerializable Then
                           For index As Integer = 0 To length - 1
                              Dim element As XmlSerializable = CType(.GetValue(index), XmlSerializable)
                              If element IsNot Nothing Then newArray.SetValue(element.Copy(), index)
                           Next
                        Else
                           For index As Integer = 0 To length - 1
                              newArray.SetValue(.GetValue(index), index)
                           Next
                        End If
                        pi.SetValue(destination, newArray, emptyIndex)
                     End With
                  ElseIf isXmlSerializable Then
                     pi.SetValue(destination, CType(value, XmlSerializable).Copy(), emptyIndex)
                  Else
                     pi.SetValue(destination, value, emptyIndex)
                  End If
               End If
            Next
            If currentType Is xmlSerializableType Then
               Exit Sub
            Else
               currentType = currentType.BaseType
            End If
         Loop
      End Sub

#Region "INotifyPropertyChanged"
      ''' <summary>
      ''' AddHandler for specified propertyName
      ''' </summary>
      Public Overridable Sub [AddHandler](handler As sc.PropertyChangedEventHandler, propertyName As String)
         Dim propertyChangedHandler As sc.PropertyChangedEventHandler

         If String.IsNullOrEmpty(propertyName) Then propertyName = "*"
         propertyChangedHandler = Me.PropertyChangedHandler(propertyName)
         If propertyChangedHandler Is Nothing Then
            Me.PropertyChangedHandler(propertyName) = handler
         Else
            Me.PropertyChangedHandler(propertyName) = CType(System.Delegate.Combine(propertyChangedHandler, handler), sc.PropertyChangedEventHandler)
         End If
      End Sub
      ''' <summary>
      ''' AddHandler for specified propertyNames
      ''' </summary>
      Public Overridable Sub [AddHandler](handler As sc.PropertyChangedEventHandler, ParamArray propertyNames As String())
         If propertyNames Is Nothing OrElse propertyNames.Length = 0 Then
            Me.AddHandler(handler, "*")
         Else
            For Each propertyName As String In propertyNames
               Me.AddHandler(handler, propertyName)
            Next
         End If
      End Sub

      Protected Overridable Sub OnPropertyChanged(propertyName As String)
         OnPropertyChanged(New sc.PropertyChangedEventArgs(propertyName))
      End Sub
      Protected Overridable Sub OnPropertyChanged(Of TProperty)(propertyName As String, newValue As TProperty, oldValue As TProperty)
         OnPropertyChanged(propertyName.ToPropertyChangedEventArgs(newValue, oldValue))
      End Sub
      Protected Overridable Sub OnPropertyChanged(e As sc.PropertyChangedEventArgs)
         RaiseEvent PropertyChanged(Me, e)
      End Sub

      Public Custom Event PropertyChanged As sc.PropertyChangedEventHandler Implements sc.INotifyPropertyChanged.PropertyChanged
         AddHandler(value As sc.PropertyChangedEventHandler)
            Me.AddHandler(value, "*")
         End AddHandler
         RemoveHandler(value As sc.PropertyChangedEventHandler)
            Me.RemoveHandler(value, "*")
         End RemoveHandler
         RaiseEvent(sender As Object, e As sc.PropertyChangedEventArgs)
            For Each propertyName As String In New String() {If(e.PropertyName, String.Empty), "*"}
               Dim propertyChangedHandler As sc.PropertyChangedEventHandler = Me.PropertyChangedHandler(propertyName)

               If propertyChangedHandler IsNot Nothing Then
                  For Each handler As System.ComponentModel.PropertyChangedEventHandler In propertyChangedHandler.GetInvocationList()
                     Try
                        handler.Invoke(sender, e)
                     Catch ex As Exception
                     End Try
                  Next
               End If
            Next
         End RaiseEvent
      End Event

      Protected _propertyChangedHandlers As New Dictionary(Of String, sc.PropertyChangedEventHandler)
      Protected Property PropertyChangedHandler(propertyName As String) As sc.PropertyChangedEventHandler
         Get
            Return If(_propertyChangedHandlers.ContainsKey(propertyName), _propertyChangedHandlers(propertyName), Nothing)
         End Get
         Set(value As sc.PropertyChangedEventHandler)
            _propertyChangedHandlers.Remove(propertyName)
            If value IsNot Nothing Then
               _propertyChangedHandlers.Add(propertyName, value)
            End If
         End Set
      End Property

      ''' <summary>
      ''' RemoveHandler for specified propertyName
      ''' </summary>
      Public Overridable Sub [RemoveHandler](handler As sc.PropertyChangedEventHandler, propertyName As String)
         Dim propertyChangedHandler As sc.PropertyChangedEventHandler

         If String.IsNullOrEmpty(propertyName) Then propertyName = "*"
         propertyChangedHandler = Me.PropertyChangedHandler(propertyName)
         If Not propertyChangedHandler Is Nothing Then
            propertyChangedHandler = CType(System.Delegate.Remove(propertyChangedHandler, handler), sc.PropertyChangedEventHandler)
            If Not propertyChangedHandler Is Nothing Then
               Dim delegates As [Delegate]() = propertyChangedHandler.GetInvocationList()
               If delegates Is Nothing OrElse delegates.Length = 0 Then
                  Me.PropertyChangedHandler(propertyName) = Nothing
               Else
                  Me.PropertyChangedHandler(propertyName) = propertyChangedHandler
               End If
            End If
         End If
      End Sub
      ''' <summary>
      ''' RemoveHandler for specified propertyNames
      ''' </summary>
      Public Overridable Sub [RemoveHandler](handler As sc.PropertyChangedEventHandler, ParamArray propertyNames As String())
         If propertyNames Is Nothing OrElse propertyNames.Length = 0 Then
            Me.RemoveHandler(handler, "*")
         Else
            For Each propertyName As String In propertyNames
               Me.RemoveHandler(handler, propertyName)
            Next
         End If
      End Sub
#End Region

#Region "IXmlSerializable"
      Public Overridable Function GetSchema() As System.Xml.Schema.XmlSchema Implements sxs.IXmlSerializable.GetSchema
         Return Nothing
      End Function

      ''' <summary>
      ''' Reads the next ElementString from the XmlReader
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                          nodeName As String, defaultValue As String) As String
         Return ReadCore(reader, attributeOverrides, nodeName, defaultValue, Function() reader.ReadElementString())
      End Function
      ''' <summary>
      ''' Read the next ElementString from the XmlReader
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                          nodeName As String, [namespace] As String, defaultValue As String) As String
         Return ReadCore(reader, attributeOverrides, nodeName, [namespace], defaultValue, Function() reader.ReadElementString())
      End Function

      ''' <summary>
      ''' Reads the next primitive property from the XmlReader
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(Of T)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                nodeName As String, defaultValue As T) As T
         Return ReadCore(reader, attributeOverrides, nodeName, defaultValue, Function() ConvertBack(reader.ReadElementString(), defaultValue))
      End Function
      ''' <summary>
      ''' Read the next primitive property from the XmlReader
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(Of TResult)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                      nodeName As String, [namespace] As String, defaultValue As TResult) As TResult
         Return ReadCore(reader, attributeOverrides, nodeName, [namespace], defaultValue, Function() ConvertBack(reader.ReadElementString(), defaultValue))
      End Function

      ''' <summary>
      ''' Reads the next serializable property from the XmlReader
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(Of TResult As Serialization.XmlSerializable)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                                       nodeName As String,
                                                                                       factory As Func(Of sx.XmlReader, TResult)) As TResult
         Return ReadCore(Of TResult)(reader, attributeOverrides, nodeName, Nothing, Function() factory.Invoke(reader))
      End Function
      ''' <summary>
      ''' Read the next serializable property from the XmlReader
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function Read(Of TResult As Serialization.XmlSerializable)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                                       nodeName As String, [namespace] As String,
                                                                                       factory As Func(Of sx.XmlReader, TResult)) As TResult
         Return ReadCore(Of TResult)(reader, attributeOverrides, nodeName, [namespace], Nothing, Function() factory.Invoke(reader))
      End Function

      ''' <summary>
      ''' Reads the next array of primitives or strings from the XmlReader
      ''' </summary>
      ''' <typeparam name="TElement"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadArray(Of TElement)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                            nodeName As String) As TElement()
         Dim result As New List(Of TElement)

         While String.Compare(nodeName, GetCurrentStartElementLocalName(reader), True) = 0
            Try
               result.Add(ConvertBack(Of TElement)(reader.ReadElementString(), Nothing))
            Catch
            End Try
         End While

         Return If(result.Count = 0, Nothing, result.ToArray())
      End Function
      ''' <summary>
      ''' Read the next array of primitives or strings from the XmlReader
      ''' </summary>
      ''' <typeparam name="TElement"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadArray(Of TElement)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                            nodeName As String, [namespace] As String) As TElement()
         Dim result As New List(Of TElement)

         If attributeOverrides IsNot Nothing Then
            Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
            Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
            If xmlRootAttribute IsNot Nothing AndAlso xmlRootAttribute.Namespace IsNot Nothing Then [namespace] = xmlRootAttribute.Namespace
            If xmlElementAttribute IsNot Nothing Then
               nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
               If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
            End If
         End If
         While String.Compare(nodeName, GetCurrentStartElementLocalName(reader), True) = 0 AndAlso
               (String.IsNullOrEmpty([namespace]) OrElse String.Compare(reader.NamespaceURI, [namespace], True) = 0)
            Try
               result.Add(ConvertBack(Of TElement)(reader.ReadElementString(), Nothing))
            Catch
            End Try
         End While

         Return If(result.Count = 0, Nothing, result.ToArray())
      End Function

      ''' <summary>
      ''' Reads the next array of serializables from the XmlReader
      ''' </summary>
      ''' <typeparam name="TElement"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadArray(Of TElement As Serialization.XmlSerializable)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                                             nodeName As String,
                                                                                             factory As Func(Of sx.XmlReader, TElement)) As TElement()
         Dim result As New List(Of TElement)

         Do
            'wenn Factory Nothing zurückgibt, ist das Ende des Arrays erreicht
            Dim factoryResult As TElement = If(nodeName = "" AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
                                               String.Compare(GetCurrentStartElementLocalName(reader), nodeName, True) = 0,
                                               factory.Invoke(reader), Nothing)

            If factoryResult IsNot Nothing Then
               result.Add(factoryResult)
            ElseIf result.Count = 0 Then
               Return Nothing
            Else
               Return result.ToArray()
            End If
         Loop
      End Function
      ''' <summary>
      ''' Read the next array of serializables from the XmlReader
      ''' </summary>
      ''' <typeparam name="TElement"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadArray(Of TElement As Serialization.XmlSerializable)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                                             nodeName As String, [namespace] As String,
                                                                                             factory As Func(Of sx.XmlReader, TElement)) As TElement()
         Dim result As New List(Of TElement)

         If attributeOverrides IsNot Nothing Then
            Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
            Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
            If xmlRootAttribute IsNot Nothing AndAlso xmlRootAttribute.Namespace IsNot Nothing Then [namespace] = xmlRootAttribute.Namespace
            If xmlElementAttribute IsNot Nothing Then
               nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
               If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
            End If
         End If

         Do
            'wenn Factory Nothing zurückgibt, ist das Ende des Arrays erreicht
            Dim factoryResult As TElement = If(String.IsNullOrEmpty(nodeName) AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
                                               String.Compare(GetCurrentStartElementLocalName(reader), nodeName, True) = 0 AndAlso
                                               (String.IsNullOrEmpty([namespace]) OrElse String.Compare(reader.NamespaceURI, [namespace], True) = 0),
                                               factory.Invoke(reader), Nothing)

            If factoryResult IsNot Nothing Then
               result.Add(factoryResult)
            ElseIf result.Count = 0 Then
               Return Nothing
            Else
               Return result.ToArray()
            End If
         Loop
      End Function

      ''' <summary>
      ''' Read properties represented in attributes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected MustOverride Sub ReadAttributes(reader As sx.XmlReader)

      ''' <summary>
      ''' Embeds the getResult-call within a try-catch-block and checks the current nodename of the
      ''' reader object if the name matches the given nodeName
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="defaultValue"></param>
      ''' <param name="itemFactory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadCore(Of TResult)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                          nodeName As String,
                                                          defaultValue As TResult, itemFactory As Func(Of TResult)) As TResult
         Try
            If String.IsNullOrEmpty(nodeName) AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
               String.Compare(nodeName, GetCurrentStartElementLocalName(reader), True) = 0 Then
               Return itemFactory.Invoke()
            Else
               Return defaultValue
            End If
         Catch
            Return defaultValue
         End Try
      End Function
      ''' <summary>
      ''' Embeds the getResult-call within a try-catch-block and checks the current nodename of the
      ''' reader object if the name matches the given nodeName and namespace
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="defaultValue"></param>
      ''' <param name="itemFactory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadCore(Of TResult)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                          nodeName As String, [namespace] As String,
                                                          defaultValue As TResult, itemFactory As Func(Of TResult)) As TResult
         Try
            If attributeOverrides IsNot Nothing Then
               Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
               Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
               If xmlRootAttribute IsNot Nothing AndAlso xmlRootAttribute.Namespace IsNot Nothing Then [namespace] = xmlRootAttribute.Namespace
               If xmlElementAttribute IsNot Nothing Then
                  nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
                  If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
               End If
            End If
            If String.IsNullOrEmpty(nodeName) AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
               String.Compare(nodeName, GetCurrentStartElementLocalName(reader), True) = 0 AndAlso
               (String.IsNullOrEmpty([namespace]) OrElse String.Compare(reader.NamespaceURI, [namespace], True) = 0) Then
               Return itemFactory.Invoke()
            Else
               Return defaultValue
            End If
         Catch
            Return defaultValue
         End Try
      End Function

      ''' <summary>
      ''' Reads the next enum from XmlReader
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadEnum(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                     nodeName As String, defaultValue As TEnum) As TEnum
         Return ReadCore(reader, attributeOverrides, nodeName, defaultValue,
                         Function()
                            Dim value As TEnum
                            Return If(TryParse(reader.ReadElementString(), value, True), value, defaultValue)
                         End Function)
      End Function
      ''' <summary>
      ''' Read the next enum from XmlReader
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadEnum(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                     nodeName As String, [namespace] As String, defaultValue As TEnum) As TEnum
         Return ReadCore(reader, attributeOverrides, nodeName, [namespace], defaultValue,
                         Function()
                            Dim value As TEnum
                            Return If(TryParse(reader.ReadElementString(), value, True), value, defaultValue)
                         End Function)
      End Function

      ''' <summary>
      ''' Reads the next array of enums
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadEnumArray(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                          nodeName As String) As TEnum()
         Dim result As New List(Of TEnum)
         Dim value As TEnum

         While String.IsNullOrEmpty(nodeName) AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
            String.Compare(GetCurrentStartElementLocalName(reader), nodeName, True) = 0
            If TryParse(reader.ReadElementString(), value, True) Then result.Add(value)
         End While

         Return If(result.Count = 0, Nothing, result.ToArray())
      End Function
      ''' <summary>
      ''' Read the next array of enums
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadEnumArray(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                          nodeName As String, [namespace] As String) As TEnum()
         Dim result As New List(Of TEnum)
         Dim value As TEnum

         If attributeOverrides IsNot Nothing Then
            Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
            Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
            If xmlRootAttribute IsNot Nothing AndAlso xmlRootAttribute.Namespace IsNot Nothing Then [namespace] = xmlRootAttribute.Namespace
            If xmlElementAttribute IsNot Nothing Then
               nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
               If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
            End If
         End If

         While String.IsNullOrEmpty(nodeName) AndAlso reader.MoveToContent = Xml.XmlNodeType.Element OrElse
               String.Compare(GetCurrentStartElementLocalName(reader), nodeName, True) = 0 AndAlso
               (String.IsNullOrEmpty([namespace]) OrElse String.Compare(reader.NamespaceURI, [namespace], True) = 0)
            If TryParse(reader.ReadElementString(), value, True) Then result.Add(value)
         End While

         Return If(result.Count = 0, Nothing, result.ToArray())
      End Function

      ''' <summary>
      ''' Reads the next enum from XmlReader
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadOptionalEnum(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                             nodeName As String, defaultValue As TEnum?) As TEnum?
         Return ReadCore(reader, attributeOverrides, nodeName, defaultValue,
                         Function()
                            Dim value As TEnum
                            Return If(TryParse(reader.ReadElementString(), value, True), value, defaultValue)
                         End Function)
      End Function
      ''' <summary>
      ''' Read the next enum from XmlReader
      ''' </summary>
      ''' <typeparam name="TEnum"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="defaultValue"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Protected Overridable Function ReadOptionalEnum(Of TEnum As Structure)(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides,
                                                                             nodeName As String, [namespace] As String, defaultValue As TEnum?) As TEnum?
         Return ReadCore(reader, attributeOverrides, nodeName, [namespace], defaultValue,
                         Function()
                            Dim value As TEnum
                            Return If(TryParse(reader.ReadElementString(), value, True), value, defaultValue)
                         End Function)
      End Function

      ''' <summary>
      ''' Deserialize properties
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Public Overridable Sub ReadXml(reader As sx.XmlReader) Implements sxs.IXmlSerializable.ReadXml
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         ReadAttributes(reader)
         'open tag of current instance
         reader.ReadStartElement()
         If Not isEmptyElement Then
            'give derived classes the chance to deserialize their properties
            ReadXmlCore(reader, XmlAttributeOverrides.GetInstance(reader))
            'skip next child nodes including the EndElement belongs to reader.ReadStartElement()
            reader.ReadToEndElement(True)
         End If
      End Sub

      ''' <summary>
      ''' Permits derived classes to read their properties
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected MustOverride Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides)
      Protected Overridable Function ReadXmlCoreFuzzy(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides, callReadXmlCoreFuzzy2 As Boolean) As Boolean
         Return False
      End Function
      Protected Overridable Function ReadXmlCoreFuzzy2(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides, callReadOuterXml As Boolean) As Boolean
         Return False
      End Function

      ''' <summary>
      ''' Serializes values
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="values"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteArray(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                           nodeName As String, [namespace] As String, values As String())
         If values IsNot Nothing Then
            If attributeOverrides IsNot Nothing Then
               Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
               Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
               If Not (xmlRootAttribute Is Nothing OrElse String.IsNullOrEmpty([namespace])) Then [namespace] = xmlRootAttribute.Namespace
               If xmlElementAttribute IsNot Nothing Then
                  nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
                  If Not String.IsNullOrEmpty([namespace]) Then If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
               End If
            End If
            If String.IsNullOrEmpty([namespace]) Then
               For Each value As String In values
                  writer.WriteElementString(nodeName, value)
               Next
            Else
               For Each value As String In values
                  writer.WriteElementString(nodeName, [namespace], value)
               Next
            End If
         End If
      End Sub
      ''' <summary>
      ''' Serializes values
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="values"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteArray(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                           nodeName As String, [namespace] As String, values As XmlSerializable())
         If values IsNot Nothing Then
            For Each value As XmlSerializable In values
               WriteElement(writer, attributeOverrides, nodeName, [namespace], value)
            Next
         End If
      End Sub
      ''' <summary>
      ''' Serializes values
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="values"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteArray(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                           nodeName As String, [namespace] As String, values As Extensions.Extension())
         If values IsNot Nothing Then
            For Each value As Extensions.Extension In values
               If value IsNot Nothing Then
                  Dim xmlRoot As sxs.XmlRootAttribute = value.GetXmlRootAttribute()

                  If xmlRoot Is Nothing Then
                     WriteElement(writer, attributeOverrides, nodeName, [namespace], value)
                  Else
                     WriteElement(writer, attributeOverrides, If(xmlRoot.ElementName, nodeName), If(xmlRoot.Namespace, [namespace]), value)
                  End If
               End If
            Next
         End If
      End Sub
      ''' <summary>
      ''' Serializes values
      ''' </summary>
      ''' <typeparam name="TItem"></typeparam>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="values"></param>
      ''' <remarks>Detects type of values and delegates the call to a specific WriteArray() method</remarks>
      Protected Overridable Sub WriteArray(Of TItem)(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                                     nodeName As String, [namespace] As String, values As TItem())
         If values IsNot Nothing Then
            If GetType(Extensions.Extension).IsAssignableFrom(GetType(TItem)) Then
               WriteArray(writer, attributeOverrides, nodeName, [namespace],
                          (From item As TItem In values
                           Let serializable As Extensions.Extension = CType(CObj(item), Extensions.Extension)
                           Select serializable).ToArray)
            ElseIf GetType(XmlSerializable).IsAssignableFrom(GetType(TItem)) Then
               WriteArray(writer, attributeOverrides, nodeName, [namespace],
                          (From item As TItem In values
                           Let serializable As XmlSerializable = CType(CObj(item), XmlSerializable)
                           Select serializable).ToArray)
            ElseIf GetType(TItem).IsEnum Then
               WriteArray(writer, attributeOverrides, nodeName, [namespace],
                          (From item As TItem In values
                           Select Common.GetName(CType(CObj(item), System.Enum))).ToArray)
            Else
               WriteArray(writer, attributeOverrides, nodeName, [namespace],
                          (From item As TItem In values
                           Select Convert(item)).ToArray)
            End If
         End If
      End Sub

      ''' <summary>
      ''' Serializes value as attribute
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="value"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteAttribute(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                               nodeName As String, [namespace] As String, value As String)
         If attributeOverrides IsNot Nothing Then
            Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
            Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
            If Not (xmlRootAttribute Is Nothing OrElse String.IsNullOrEmpty([namespace])) Then [namespace] = xmlRootAttribute.Namespace
            If xmlElementAttribute IsNot Nothing Then
               nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
               If Not String.IsNullOrEmpty([namespace]) Then If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
            End If
         End If
         If String.IsNullOrEmpty([namespace]) Then
            writer.WriteAttributeString(nodeName, value)
         Else
            writer.WriteAttributeString(nodeName, [namespace], value)
         End If
      End Sub
      ''' <summary>
      ''' Serializes value as element
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="value"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteElement(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                             nodeName As String, [namespace] As String, value As String)
         If attributeOverrides IsNot Nothing Then
            Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
            Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
            If Not (xmlRootAttribute Is Nothing OrElse String.IsNullOrEmpty([namespace])) Then [namespace] = xmlRootAttribute.Namespace
            If xmlElementAttribute IsNot Nothing Then
               nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
               If Not String.IsNullOrEmpty([namespace]) Then If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
            End If
         End If
         If String.IsNullOrEmpty([namespace]) Then
            writer.WriteElementString(nodeName, value)
         Else
            writer.WriteElementString(nodeName, [namespace], value)
         End If
      End Sub
      ''' <summary>
      ''' Serializes value as element
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName">If this parameter is not set the system determines the nodeName from the XmlRootAttribute of the value-class</param>
      ''' <param name="namespace"></param>
      ''' <param name="value"></param>
      ''' <remarks></remarks>
      Protected Overridable Sub WriteElement(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides,
                                             nodeName As String, [namespace] As String, value As XmlSerializable)
         If value IsNot Nothing Then
            If attributeOverrides IsNot Nothing Then
               Dim xmlRootAttribute As sxs.XmlRootAttribute = attributeOverrides.XmlRoot(Me.GetType)
               Dim xmlElementAttribute As sxs.XmlElementAttribute = If(String.IsNullOrEmpty(nodeName), Nothing, attributeOverrides.XmlElement(Me.GetType(), nodeName))
               If Not (xmlRootAttribute Is Nothing OrElse String.IsNullOrEmpty([namespace])) Then [namespace] = xmlRootAttribute.Namespace
               If xmlElementAttribute IsNot Nothing Then
                  nodeName = xmlElementAttribute.ElementName.NVL(nodeName)
                  If Not String.IsNullOrEmpty([namespace]) Then If xmlElementAttribute.Namespace IsNot Nothing Then [namespace] = xmlElementAttribute.Namespace
               End If
            End If
            'nodeName must be read from the XmlRootAttribute of the value-instance
            If String.IsNullOrEmpty(nodeName) Then nodeName = value.GetXmlRootAttribute(exactNonNullResult:=True).ElementName
            If String.IsNullOrEmpty([namespace]) Then
               writer.WriteStartElement(nodeName)
            Else
               writer.WriteStartElement(nodeName, [namespace])
            End If
            value.WriteXmlCore(writer, attributeOverrides)
            writer.WriteEndElement()
         End If
      End Sub

      ''' <summary>
      ''' Serialization of this instance
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Public Overridable Sub WriteXml(writer As sx.XmlWriter) Implements sxs.IXmlSerializable.WriteXml
         WriteXmlCore(writer, XmlAttributeOverrides.GetInstance(writer))
      End Sub
      Protected MustOverride Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides)
#End Region

      Protected _extendedProperties As Dictionary(Of String, Object)
      ''' <summary>
      ''' ExtendProperty - support
      ''' </summary>
      ''' <remarks></remarks>
      Public Overridable ReadOnly Property ExtendedProperties(Optional ensureInstance As Boolean = True) As Dictionary(Of String, Object)
         Get
            If _extendedProperties Is Nothing AndAlso ensureInstance Then _extendedProperties = New Dictionary(Of String, Object)
            Return _extendedProperties
         End Get
      End Property

      ''' <summary>
      ''' Creates a new serializable-instance and initializes it via ReadXml()
      ''' </summary>
      ''' <typeparam name="TClass"></typeparam>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GenericXmlSerializableFactory(Of TClass As {New, Serialization.XmlSerializable})(reader As sx.XmlReader) As TClass
         Dim retVal As New TClass
         retVal.ReadXml(reader)
         Return retVal
      End Function

   End Class

   ''' <summary>
   ''' XmlSerializable class with aspects before and after ReadXmlCore() and WriteXmlCore()
   ''' </summary>
   ''' <remarks></remarks>
   Public MustInherit Class XmlSerializableWithIOAspects
      Inherits XmlSerializable

#Region "Constructors"
      Protected Sub New()
         MyBase.New()
      End Sub
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub
#End Region

      Protected MustOverride Sub BeginReadXmlCore(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides)
      Protected MustOverride Sub BeginWriteXmlCore(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides)
      Protected MustOverride Sub EndReadXmlCore(reader As sx.XmlReader, attributeOverrides As XmlAttributeOverrides)
      Protected MustOverride Sub EndWriteXmlCore(writer As sx.XmlWriter, attributeOverrides As XmlAttributeOverrides)

      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         Dim isEmptyElement As Boolean

         reader.MoveToContent()
         isEmptyElement = reader.IsEmptyElement
         ReadAttributes(reader)
         'open tag of current instance
         reader.ReadStartElement()
         If Not isEmptyElement Then
            Dim attributeOverrides As XmlAttributeOverrides = XmlAttributeOverrides.GetInstance(reader)

            BeginReadXmlCore(reader, attributeOverrides)
            'give derived classes the chance to deserialize their properties
            ReadXmlCore(reader, attributeOverrides)
            EndReadXmlCore(reader, attributeOverrides)
            'skip next child nodes including the EndElement belongs to reader.ReadStartElement()
            reader.ReadToEndElement(True)
         End If
      End Sub

      Public Overrides Sub WriteXml(writer As System.Xml.XmlWriter)
         Dim attributeOverrides As XmlAttributeOverrides = XmlAttributeOverrides.GetInstance(writer)
         BeginWriteXmlCore(writer, attributeOverrides)
         WriteXmlCore(writer, attributeOverrides)
         EndWriteXmlCore(writer, attributeOverrides)
      End Sub
   End Class
End Namespace