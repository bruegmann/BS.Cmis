'***********************************************************************************************************************
'* Project: CmisObjectModelLibrary
'* Copyright (c) 2014, Brügmann Software GmbH, Papenburg, All rights reserved
'* Author: auto-generated code
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
Imports CmisObjectModel.Core.Properties
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Collections
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisPropertiesType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   <System.CodeDom.Compiler.GeneratedCode("CmisXsdConverter", "1.0.0.0")>
   Partial Public Class cmisPropertiesType
      Inherits Serialization.XmlSerializable

      Public Sub New()
      End Sub
      ''' <summary>
      ''' this constructor is only used if derived classes from this class needs an InitClass()-call
      ''' </summary>
      ''' <param name="initClassSupported"></param>
      ''' <remarks></remarks>
      Protected Sub New(initClassSupported As Boolean?)
         MyBase.New(initClassSupported)
      End Sub

#Region "IXmlSerializable"
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisPropertiesType, String)) From {
         } '_setter

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

      ''' <summary>
      ''' Deserialization of all properties stored in subnodes
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub ReadXmlCore(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
         _properties = New HashSet(Of cmisProperty)(ReadArray(Of Core.Properties.cmisProperty)(reader, attributeOverrides, Nothing, AddressOf Core.Properties.cmisProperty.CreateInstance), New cmisPropertyEqualityComparer())
         _extensions = ReadArray(Of CmisObjectModel.Extensions.Extension)(reader, attributeOverrides, Nothing, AddressOf CmisObjectModel.Extensions.Extension.CreateInstance)
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmis, _properties.ToArray())
         WriteArray(writer, attributeOverrides, Nothing, Constants.Namespaces.cmis, _extensions)
      End Sub
#End Region

      Protected _extensions As CmisObjectModel.Extensions.Extension()
      Public Overridable Property Extensions As CmisObjectModel.Extensions.Extension()
         Get
            Return _extensions
         End Get
         Set(value As CmisObjectModel.Extensions.Extension())
            If value IsNot _extensions Then
               Dim oldValue As CmisObjectModel.Extensions.Extension() = _extensions
               _extensions = value
               OnPropertyChanged("Extensions", value, oldValue)
            End If
         End Set
      End Property 'Extensions
      Private _equalityComparer As New cmisPropertyEqualityComparer()
      Protected _properties As New HashSet(Of Core.Properties.cmisProperty)(_equalityComparer)
      Public Overridable Property Properties As Core.Properties.cmisProperty()
         Get
            Return _properties.ToArray()
         End Get
         Set(value As Core.Properties.cmisProperty())
            If value Is Nothing Then
               Dim oldValue As Properties.cmisProperty() = _properties.ToArray()
               _properties.Clear()
               OnPropertyChanged("Properties", _properties.ToArray(), oldValue)
            End If
            If _properties.Count <> value.Count OrElse Not _properties.All(Function(e) value.Contains(e)) Then
               Dim oldValue As Core.Properties.cmisProperty() = _properties.ToArray()
               _properties = New HashSet(Of cmisProperty)(value, _equalityComparer)
               OnPropertyChanged("Properties", _properties.ToArray(), oldValue)
            End If
         End Set
      End Property 'Properties

      Public Function AddProperty(p As Properties.cmisProperty) As Boolean
         If p Is Nothing Then Return False
         Dim oldValue As Properties.cmisProperty() = _properties.ToArray()
         If _properties.Add(p) Then
            OnPropertyChanged("Properties", _properties.ToArray(), oldValue)
            Return True
         End If
         Return False
      End Function

      Public Function RemoveProperty(p As Properties.cmisProperty) As Boolean
         If p Is Nothing Then Return False
         Dim oldValue As Properties.cmisProperty() = _properties.ToArray()
         If _properties.Remove(p) Then
            OnPropertyChanged("Properties", _properties.ToArray(), oldValue)
            Return True
         End If
         Return False
      End Function

      Public Function Contains(p As Properties.cmisProperty) As Boolean
         Return _properties.Contains(p)
      End Function
      Private Shared _defaultProperty As Properties.cmisProperty = New cmisPropertyString()
      Public Function GetByPropertyDefinitionId(propertyDefinitionId As String) As Properties.cmisProperty
         SyncLock _defaultProperty
            _defaultProperty.PropertyDefinitionId = propertyDefinitionId
            Dim p As Properties.cmisProperty = Nothing
            _properties.TryGetValue(_defaultProperty, p)
            Return p
         End SyncLock
      End Function
      Public Sub Clear()
         Dim oldValue As Properties.cmisProperty() = _properties.ToArray()
         _properties.Clear()
         OnPropertyChanged("Properties", _properties.ToArray(), oldValue)
      End Sub


#Region "helper classes"
      Private Class cmisPropertyEqualityComparer
         Implements IEqualityComparer(Of Core.Properties.cmisProperty)

         Public Shadows Function Equals(x As cmisProperty, y As cmisProperty) As Boolean Implements IEqualityComparer(Of cmisProperty).Equals
            Return x IsNot Nothing AndAlso y IsNot Nothing AndAlso x.PropertyDefinitionId.Equals(y.PropertyDefinitionId)
         End Function

         Public Shadows Function GetHashCode(obj As cmisProperty) As Integer Implements IEqualityComparer(Of cmisProperty).GetHashCode
            Return CInt(obj?.PropertyDefinitionId.GetHashCode())
         End Function
      End Class
#End Region
   End Class
End Namespace