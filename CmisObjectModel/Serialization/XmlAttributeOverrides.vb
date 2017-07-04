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
   ''' Customization of the ReadXml() and WriteXml() in XmlSerializable-instances
   ''' </summary>
   ''' <remarks>Using-block required</remarks>
   Public Class XmlAttributeOverrides
      Implements IDisposable

#Region "Constructors"
      Public Sub New(reader As sx.XmlReader)
         Me.New(reader, Nothing)
      End Sub
      Public Sub New(reader As sx.XmlReader, attributeOverrides As sxs.XmlAttributeOverrides)
         SyncLock _syncObject
            If Not (reader Is Nothing OrElse _instances.ContainsKey(reader)) Then
               _key = reader
               _instances.Add(reader, Me)
            End If
         End SyncLock
         _innerAttributeOverrides = If(attributeOverrides, New sxs.XmlAttributeOverrides())
      End Sub
      Public Sub New(writer As sx.XmlWriter)
         Me.New(writer, Nothing)
      End Sub
      Public Sub New(writer As sx.XmlWriter, attributeOverrides As sxs.XmlAttributeOverrides)
         SyncLock _syncObject
            If Not (writer Is Nothing OrElse _instances.ContainsKey(writer)) Then
               _key = writer
               _instances.Add(writer, Me)
            End If
         End SyncLock
         _innerAttributeOverrides = If(attributeOverrides, New sxs.XmlAttributeOverrides())
      End Sub

      ''' <summary>
      ''' Returns the XmlAttributeOverrides-instance assigned to the reader-instance if exists otherwise null
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetInstance(reader As sx.XmlReader) As XmlAttributeOverrides
         Return GetInstance(CObj(reader))
      End Function
      ''' <summary>
      ''' Returns the XmlAttributeOverrides-instance assigned to the writer-instance if exists otherwise null
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Function GetInstance(writer As sx.XmlWriter) As XmlAttributeOverrides
         Return GetInstance(CObj(writer))
      End Function
      ''' <summary>
      ''' Returns the XmlAttributeOverrides-instance assigned to the key-instance if exists otherwise null
      ''' </summary>
      ''' <param name="key"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Shared Function GetInstance(key As Object) As XmlAttributeOverrides
         SyncLock _syncObject
            Return If(key IsNot Nothing AndAlso _instances.ContainsKey(key), _instances(key), Nothing)
         End SyncLock
      End Function
#End Region

#Region "IDisposable Support"
      Private _isDisposed As Boolean
      Public ReadOnly Property IsDisposed As Boolean
         Get
            Return _isDisposed
         End Get
      End Property

      ' IDisposable
      Protected Overridable Sub Dispose(disposing As Boolean)
         If Not Me._isDisposed Then
            If disposing Then
               SyncLock _syncObject
                  If _key IsNot Nothing Then _instances.Remove(_key)
                  _key = Nothing
                  Me._isDisposed = True
               End SyncLock
            Else
               Me._isDisposed = True
            End If
         End If
      End Sub

      Public Sub Dispose() Implements IDisposable.Dispose
         Dispose(True)
         GC.SuppressFinalize(Me)
      End Sub
#End Region

      Private _innerAttributeOverrides As sxs.XmlAttributeOverrides
      Private Shared _instances As New Dictionary(Of Object, XmlAttributeOverrides)
      Private _key As Object
      Private Shared _syncObject As New Object

      Public Property XmlElement(type As Type, memberName As String) As sxs.XmlElementAttribute
         Get
            Dim attributes As sxs.XmlAttributes = _innerAttributeOverrides.Item(type, memberName)
            Dim elementAttributes As sxs.XmlElementAttributes = If(attributes Is Nothing, Nothing, attributes.XmlElements)
            Return If(elementAttributes Is Nothing OrElse elementAttributes.Count = 0, Nothing, elementAttributes.Item(0))
         End Get
         Set(value As sxs.XmlElementAttribute)
            Dim attributes As sxs.XmlAttributes = _innerAttributeOverrides.Item(type, memberName)

            If attributes Is Nothing Then
               attributes = New sxs.XmlAttributes()
               _innerAttributeOverrides.Add(type, memberName, attributes)
            Else
               attributes.XmlElements.Clear()
            End If
            If value IsNot Nothing Then attributes.XmlElements.Add(value)
         End Set
      End Property

      Public Property XmlRoot(type As Type) As sxs.XmlRootAttribute
         Get
            Dim attributes As sxs.XmlAttributes = _innerAttributeOverrides.Item(type)
            Return If(attributes Is Nothing, Nothing, attributes.XmlRoot)
         End Get
         Set(value As sxs.XmlRootAttribute)
            Dim attributes As sxs.XmlAttributes = _innerAttributeOverrides.Item(type)

            If attributes Is Nothing Then
               attributes = New sxs.XmlAttributes()
               attributes.XmlRoot = value
               _innerAttributeOverrides.Add(type, attributes)
            Else
               attributes.XmlRoot = value
            End If
         End Set
      End Property

      Public Shared Widening Operator CType(value As XmlAttributeOverrides) As sxs.XmlAttributeOverrides
         Return If(value Is Nothing, Nothing, value._innerAttributeOverrides)
      End Operator
   End Class
End Namespace