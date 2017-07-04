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
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_integer = "Integer" OrElse xs_integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#End If

Namespace CmisObjectModel.Messaging.Requests
   Partial Public Class createItem

#Region "missing property support (defined in 2.2.4.6 createItem, but not defined in CMIS-Messaging.xsd)"
      ''' <summary>
      ''' Bridge the gap between createItem-definition as function (see 2.2.4.6 createItem) and the createItem-definition
      ''' in CMIS-Messaging.xsd
      ''' </summary>
      ''' <remarks></remarks>
      Public Class createItemExtensionType
         Inherits cmisExtensionType

#Region "IXmlSerializable"
         Protected Overrides Sub ReadXmlCore(reader As System.Xml.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides)
            MyBase.ReadXmlCore(reader, attributeOverrides)
            _policies = ReadArray(Of String)(reader, attributeOverrides, "policies", Constants.Namespaces.cmism)
         End Sub

         Protected Overrides Sub WriteXmlCore(writer As System.Xml.XmlWriter, attributeOverrides As Serialization.XmlAttributeOverrides)
            MyBase.WriteXmlCore(writer, attributeOverrides)
            WriteArray(writer, attributeOverrides, "policies", Constants.Namespaces.cmism, _policies)
         End Sub
#End Region

         Protected _policies As String()
         ''' <summary>
         ''' Bridge the gap between createItem-definition as function (see 2.2.4.6 createItem) and the createItem-definition
         ''' in CMIS-Messaging.xsd
         ''' </summary>
         ''' <value></value>
         ''' <returns></returns>
         ''' <remarks>As soon as this property will cause a compiler error the complete region has to be removed
         ''' including the methods Read() and WriteElement()</remarks>
         Public Overridable Property Policies As String()
            Get
               Return _policies
            End Get
            Set(value As String())
               If value IsNot _policies Then
                  Dim oldValue As String() = _policies
                  _policies = value
                  OnPropertyChanged("Policies", value, oldValue)
               End If
            End Set
         End Property 'Policies
      End Class

      ''' <summary>
      ''' Deserialization of _extension
      ''' </summary>
      ''' <typeparam name="TResult"></typeparam>
      ''' <param name="reader"></param>
      ''' <param name="attributeOverrides"></param>
      ''' <param name="nodeName"></param>
      ''' <param name="namespace"></param>
      ''' <param name="factory"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Private Overloads Function Read(Of TResult As cmisExtensionType)(reader As sx.XmlReader, attributeOverrides As Serialization.XmlAttributeOverrides,
                                                                       nodeName As String, [namespace] As String,
                                                                       factory As Func(Of sx.XmlReader, cmisExtensionType)) As cmisExtensionType
         Return MyBase.Read(Of createItemExtensionType)(reader, attributeOverrides, nodeName, [namespace], AddressOf GenericXmlSerializableFactory(Of createItemExtensionType))
      End Function

      ''' <summary>
      ''' Bridge the gap between createItem-definition as function (see 2.2.4.6 createItem) and the createItem-definition
      ''' in CMIS-Messaging.xsd
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks>As soon as this property will cause a compiler error the complete region has to be removed
      ''' including the methods Read() and WriteElement() and the class createItemExtensionType and the CType-
      ''' Operator should use _policies-field instead of Policies-property</remarks>
      Public Overridable Property Policies As String()
         Get
            Return If(TypeOf _extension Is createItemExtensionType, CType(_extension, createItemExtensionType).Policies, Nothing)
         End Get
         Set(value As String())
            Dim oldValue As String() = Me.Policies

            If value IsNot oldValue Then
               If value Is Nothing Then
                  Extension = Nothing
               Else
                  Extension = New createItemExtensionType With {.Policies = value}
               End If
               OnPropertyChanged("Policies", value, oldValue)
            End If
         End Set
      End Property 'Policies
#End Region

      ''' <summary>
      ''' Reads transmitted parameters from queryString
      ''' </summary>
      ''' <param name="repositoryId"></param>
      ''' <remarks></remarks>
      Public Overrides Sub ReadQueryString(repositoryId As String)
         Dim requestParams As System.Collections.Specialized.NameValueCollection = If(ssw.WebOperationContext.Current Is Nothing, Nothing,
                                                                                      ssw.WebOperationContext.Current.IncomingRequest.UriTemplateMatch.QueryParameters)

         MyBase.ReadQueryString(repositoryId)

         If requestParams IsNot Nothing Then
            _repositoryId = Read(repositoryId, _repositoryId)
            _folderId = Read(requestParams("folderId"), _folderId)
         End If
      End Sub

      ''' <summary>
      ''' Wraps the request-parameters of the createPolicy-Service
      ''' </summary>
      ''' <param name="value"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shared Widening Operator CType(value As createItem) As AtomPub.AtomEntry
         If value Is Nothing OrElse value._properties Is Nothing Then
            Return Nothing
         Else
            Dim cmisraObject As New Core.cmisObjectType(value._properties.Properties)
            Dim policies As String() = value.Policies

            'missing property Policies defined in 2.2.4.6 createItem, but not defined in CMIS-Messaging.xsd
            If policies IsNot Nothing AndAlso policies.Length > 0 Then
               cmisraObject.PolicyIds = New Core.Collections.cmisListOfIdsType() With {.Ids = policies}
            End If

            Return New AtomPub.AtomEntry(cmisraObject)
         End If
      End Operator

   End Class
End Namespace