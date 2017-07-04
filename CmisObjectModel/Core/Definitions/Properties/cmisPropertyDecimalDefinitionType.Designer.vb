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
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Core.Definitions.Properties
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisPropertyDecimalDefinitionType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisPropertyDecimalDefinitionType
      Inherits Core.Definitions.Properties.Generic.cmisPropertyDefinitionType(Of Decimal, Core.Choices.cmisChoiceDecimal, Core.Properties.cmisPropertyDecimal)

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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisPropertyDecimalDefinitionType, String)) From {
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
         MyBase.ReadXmlCore(reader, attributeOverrides)
         _defaultValue = Read(Of Core.Properties.cmisPropertyDecimal)(reader, attributeOverrides, "defaultValue", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Properties.cmisPropertyDecimal))
         _maxValue = Read(reader, attributeOverrides, "maxValue", Constants.Namespaces.cmis, _maxValue)
         _minValue = Read(reader, attributeOverrides, "minValue", Constants.Namespaces.cmis, _minValue)
         _precision = ReadOptionalEnum(reader, attributeOverrides, "precision", Constants.Namespaces.cmis, _precision)
         _choices = ReadArray(Of Core.Choices.cmisChoiceDecimal)(reader, attributeOverrides, "choice", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.Choices.cmisChoiceDecimal))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         MyBase.WriteXmlCore(writer, attributeOverrides)
         WriteElement(writer, attributeOverrides, "defaultValue", Constants.Namespaces.cmis, _defaultValue)
         If _maxValue.HasValue Then WriteElement(writer, attributeOverrides, "maxValue", Constants.Namespaces.cmis, Convert(_maxValue))
         If _minValue.HasValue Then WriteElement(writer, attributeOverrides, "minValue", Constants.Namespaces.cmis, Convert(_minValue))
         If _precision.HasValue Then WriteElement(writer, attributeOverrides, "precision", Constants.Namespaces.cmis, _precision.Value.GetName())
         WriteArray(writer, attributeOverrides, "choice", Constants.Namespaces.cmis, _choices)
      End Sub
#End Region

      Protected _maxValue As Decimal?
      Public Overridable Property MaxValue As Decimal?
         Get
            Return _maxValue
         End Get
         Set(value As Decimal?)
            If Not _maxValue.Equals(value) Then
               Dim oldValue As Decimal? = _maxValue
               _maxValue = value
               OnPropertyChanged("MaxValue", value, oldValue)
            End If
         End Set
      End Property 'MaxValue

      Protected _minValue As Decimal?
      Public Overridable Property MinValue As Decimal?
         Get
            Return _minValue
         End Get
         Set(value As Decimal?)
            If Not _minValue.Equals(value) Then
               Dim oldValue As Decimal? = _minValue
               _minValue = value
               OnPropertyChanged("MinValue", value, oldValue)
            End If
         End Set
      End Property 'MinValue

      Protected _precision As Core.enumDecimalPrecision?
      Public Overridable Property Precision As Core.enumDecimalPrecision?
         Get
            Return _precision
         End Get
         Set(value As Core.enumDecimalPrecision?)
            If Not _precision.Equals(value) Then
               Dim oldValue As Core.enumDecimalPrecision? = _precision
               _precision = value
               OnPropertyChanged("Precision", value, oldValue)
            End If
         End Set
      End Property 'Precision

   End Class
End Namespace