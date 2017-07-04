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

Namespace CmisObjectModel.Core
   ''' <summary>
   ''' </summary>
   ''' <remarks>
   ''' see cmisExtensionFeatureType
   ''' in http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/schema/CMIS-Core.xsd
   ''' </remarks>
   Public Class cmisExtensionFeatureType
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
      Private Shared _setter As New Dictionary(Of String, Action(Of cmisExtensionFeatureType, String)) From {
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
         _id = Read(reader, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         _url = Read(reader, attributeOverrides, "url", Constants.Namespaces.cmis, _url)
         _commonName = Read(reader, attributeOverrides, "commonName", Constants.Namespaces.cmis, _commonName)
         _versionLabel = Read(reader, attributeOverrides, "versionLabel", Constants.Namespaces.cmis, _versionLabel)
         _description = Read(reader, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         _featureDatas = ReadArray(Of Core.cmisExtensionFeatureKeyValuePair)(reader, attributeOverrides, "featureData", Constants.Namespaces.cmis, AddressOf GenericXmlSerializableFactory(Of Core.cmisExtensionFeatureKeyValuePair))
      End Sub

      ''' <summary>
      ''' Serialization of properties
      ''' </summary>
      ''' <param name="writer"></param>
      ''' <remarks></remarks>
      Protected Overrides Sub WriteXmlCore(writer As sx.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
         WriteElement(writer, attributeOverrides, "id", Constants.Namespaces.cmis, _id)
         If Not String.IsNullOrEmpty(_url) Then WriteElement(writer, attributeOverrides, "url", Constants.Namespaces.cmis, _url)
         If Not String.IsNullOrEmpty(_commonName) Then WriteElement(writer, attributeOverrides, "commonName", Constants.Namespaces.cmis, _commonName)
         If Not String.IsNullOrEmpty(_versionLabel) Then WriteElement(writer, attributeOverrides, "versionLabel", Constants.Namespaces.cmis, _versionLabel)
         If Not String.IsNullOrEmpty(_description) Then WriteElement(writer, attributeOverrides, "description", Constants.Namespaces.cmis, _description)
         WriteArray(writer, attributeOverrides, "featureData", Constants.Namespaces.cmis, _featureDatas)
      End Sub
#End Region

      Protected _commonName As String
      Public Overridable Property CommonName As String
         Get
            Return _commonName
         End Get
         Set(value As String)
            If _commonName <> value Then
               Dim oldValue As String = _commonName
               _commonName = value
               OnPropertyChanged("CommonName", value, oldValue)
            End If
         End Set
      End Property 'CommonName

      Protected _description As String
      Public Overridable Property Description As String
         Get
            Return _description
         End Get
         Set(value As String)
            If _description <> value Then
               Dim oldValue As String = _description
               _description = value
               OnPropertyChanged("Description", value, oldValue)
            End If
         End Set
      End Property 'Description

      Protected _featureDatas As Core.cmisExtensionFeatureKeyValuePair()
      Public Overridable Property FeatureDatas As Core.cmisExtensionFeatureKeyValuePair()
         Get
            Return _featureDatas
         End Get
         Set(value As Core.cmisExtensionFeatureKeyValuePair())
            If value IsNot _featureDatas Then
               Dim oldValue As Core.cmisExtensionFeatureKeyValuePair() = _featureDatas
               _featureDatas = value
               OnPropertyChanged("FeatureDatas", value, oldValue)
            End If
         End Set
      End Property 'FeatureDatas

      Protected _id As String
      Public Overridable Property Id As String
         Get
            Return _id
         End Get
         Set(value As String)
            If _id <> value Then
               Dim oldValue As String = _id
               _id = value
               OnPropertyChanged("Id", value, oldValue)
            End If
         End Set
      End Property 'Id

      Protected _url As String
      Public Overridable Property Url As String
         Get
            Return _url
         End Get
         Set(value As String)
            If _url <> value Then
               Dim oldValue As String = _url
               _url = value
               OnPropertyChanged("Url", value, oldValue)
            End If
         End Set
      End Property 'Url

      Protected _versionLabel As String
      Public Overridable Property VersionLabel As String
         Get
            Return _versionLabel
         End Get
         Set(value As String)
            If _versionLabel <> value Then
               Dim oldValue As String = _versionLabel
               _versionLabel = value
               OnPropertyChanged("VersionLabel", value, oldValue)
            End If
         End Set
      End Property 'VersionLabel

   End Class
End Namespace