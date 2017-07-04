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
Imports scg = System.Collections.Generic
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Serialization

   Public MustInherit Class SerializationHelper
      Friend Const csElementName As String = "eb99185431584dcfb65c838e36421f09"

      Public Shared Function ToStream(Of T As {New, sxs.IXmlSerializable})(instance As T) As System.IO.Stream
         Return New Generic.SerializationHelper(Of T)(instance)
      End Function

      Public Shared Function ToStream(Of T As {New, sxs.IXmlSerializable})(instance As T, attributeOverrides As sxs.XmlAttributeOverrides) As System.IO.Stream
         Return New Generic.SerializationHelper(Of T)(instance, attributeOverrides)
      End Function

      Public Shared Function ToXmlDocument(Of T As {New, sxs.IXmlSerializable})(instance As T) As sx.XmlDocument
         Return New Generic.SerializationHelper(Of T)(instance)
      End Function

      Public Shared Function ToXmlDocument(Of T As {New, sxs.IXmlSerializable})(instance As T, attributeOverrides As sxs.XmlAttributeOverrides) As sx.XmlDocument
         Return New Generic.SerializationHelper(Of T)(instance, attributeOverrides)
      End Function
   End Class

   Namespace Generic
      ''' <summary>
      ''' Creates Xml-results using cmis-specified prefixes in the root
      ''' </summary>
      ''' <typeparam name="T"></typeparam>
      ''' <remarks>
      ''' Classes that implement the System.Xml.Serialization.IXmlSerializable interface will not
      ''' be serialized with a prefix for the root node. To achieve that a prefix specified by cmis
      ''' is used for the root node, this helper class is created.
      ''' </remarks>
      <Serializable(), System.Xml.Serialization.XmlRoot(ElementName:=SerializationHelper.csElementName)>
      Public Class SerializationHelper(Of T As sxs.IXmlSerializable)
         Inherits SerializationHelper

#Region "Constructors"
         ''' <summary>
         ''' Scans T for specified elementname and namespace
         ''' </summary>
         ''' <remarks></remarks>
         Shared Sub New()
            With GetType(T).GetXmlRootAttribute(exactNonNullResult:=True)
               _elementName = .ElementName
               _namespace = .Namespace
            End With

            If String.IsNullOrEmpty(_elementName) Then _elementName = GetType(T).Name
         End Sub

         Public Sub New()
         End Sub
         Public Sub New(item As T)
            Me.Item = item
         End Sub
         Public Sub New(item As T, attributeOverrides As sxs.XmlAttributeOverrides)
            Me.Item = item
            _attributeOverrides = attributeOverrides
         End Sub
#End Region

         Private ReadOnly _attributeOverrides As sxs.XmlAttributeOverrides
         Private Shared ReadOnly _defaultAttributeOverrides As New sxs.XmlAttributeOverrides()

         Private Shared _elementName As String
         ''' <summary>
         ''' Returns the default elementname for T-Type
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private ReadOnly Property ElementName As String
            Get
               Dim attrs As sxs.XmlAttributes = If(_attributeOverrides Is Nothing, Nothing, _attributeOverrides.Item(GetType(T)))
               Return If(attrs Is Nothing OrElse attrs.XmlRoot Is Nothing,
                         _elementName, If(attrs.XmlRoot.ElementName, _elementName))
            End Get
         End Property

         Public Property Item As T

         Private Shared _namespace As String
         ''' <summary>
         ''' Returns the default namespace for T-Type
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks></remarks>
         Private ReadOnly Property [Namespace] As String
            Get
               Dim attrs As sxs.XmlAttributes = If(_attributeOverrides Is Nothing, Nothing, _attributeOverrides.Item(GetType(T)))
               Return If(attrs Is Nothing OrElse attrs.XmlRoot Is Nothing,
                         _namespace, If(attrs.XmlRoot.Namespace, _namespace))
            End Get
         End Property

         Private Shared _serializers As New scg.Dictionary(Of sxs.XmlAttributeOverrides, sxs.XmlSerializer)
         Private ReadOnly Property Serializer() As sxs.XmlSerializer
            Get
               Dim attributeOverrides As sxs.XmlAttributeOverrides = If(_attributeOverrides, _defaultAttributeOverrides)

               SyncLock _serializers
                  If _serializers.ContainsKey(attributeOverrides) Then
                     Return _serializers(attributeOverrides)
                  Else
                     Try
                        'define namespace and elementname for property 'Item'
                        If attributeOverrides.Item(GetType(SerializationHelper(Of T)), "Item") Is Nothing Then
                           Dim attr As New sxs.XmlElementAttribute(ElementName) With {.Namespace = Me.Namespace}
                           Dim attrs As New sxs.XmlAttributes

                           attrs.XmlElements.Add(attr)
                           attributeOverrides.Add(GetType(SerializationHelper(Of T)), "Item", attrs)
                        End If
                     Catch
                     End Try

                     Dim retVal As New sxs.XmlSerializer(GetType(SerializationHelper(Of T)), attributeOverrides)

                     _serializers.Add(attributeOverrides, retVal)
                     Return retVal
                  End If
               End SyncLock
            End Get
         End Property

         Public Shared Widening Operator CType(value As SerializationHelper(Of T)) As System.IO.Stream
            Dim xmlDoc As sx.XmlDocument = value
            Return New System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(xmlDoc.OuterXml)) With {.Position = 0}
         End Operator

         Public Shared Widening Operator CType(value As SerializationHelper(Of T)) As System.Xml.XmlDocument
            Dim retVal As New Xml.XmlDocument

            If value IsNot Nothing Then
               'specified namespaces
               Dim namespaces As New Xml.Serialization.XmlSerializerNamespaces
               For Each de As KeyValuePair(Of sx.XmlQualifiedName, String) In CmisNamespaces(New String() {})
                  namespaces.Add(de.Key.Name, de.Value)
               Next

               Using ms As New IO.MemoryStream
                  Using sw As New IO.StreamWriter(ms)
                     Using writer As sx.XmlWriter = sx.XmlWriter.Create(sw)
                        If value._attributeOverrides Is Nothing Then
                           value.Serializer.Serialize(writer, value, namespaces)
                        Else
                           'the serialization will use the attributeOverrides-instance
                           Using attributeOverrides As New XmlAttributeOverrides(writer, value._attributeOverrides)
                              value.Serializer.Serialize(writer, value, namespaces)
                           End Using
                        End If
                        ms.Position = 0
                        retVal.Load(ms)
                     End Using
                  End Using

                  'replace root with 'Item'-node
                  Dim node As Xml.XmlNode = retVal.SelectSingleNode(csElementName)
                  retVal.RemoveChild(node)
                  retVal.AppendChild(node.FirstChild)
               End Using
            End If

            Return retVal
         End Operator
      End Class
   End Namespace
End Namespace