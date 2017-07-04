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
Imports ca = CmisObjectModel.AtomPub
Imports ccdp = CmisObjectModel.Core.Definitions.Properties
Imports ccdt = CmisObjectModel.Core.Definitions.Types
Imports ccg = CmisObjectModel.Common.Generic
Imports ccg1 = CmisObjectModel.Collections.Generic
Imports sss = System.ServiceModel.Syndication
Imports sxl = System.Xml.Linq
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core.Definitions.Types
   <Attributes.JavaScriptObjectResolver(GetType(JSON.Serialization.CmisTypeDefinitionResolver)),
    Attributes.JavaScriptConverterAttribute(GetType(JSON.Core.Definitions.Types.Generic.cmisTypeDefinitionTypeConverter(Of ccdt.cmisTypeDefinitionType)),
                                            "{"""":""TTypeDefinition""}")>
   Partial Public Class cmisTypeDefinitionType

#Region "Constructors"
      Protected Sub New(id As String, localName As String, displayName As String, queryName As String,
                        ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName)
         _propertyDefinitions = propertyDefinitions
      End Sub
      Protected Sub New(id As String, localName As String, displayName As String, queryName As String,
                        parentId As String,
                        ParamArray propertyDefinitions As Core.Definitions.Properties.cmisPropertyDefinitionType())
         MyBase.New(id, localName, displayName, queryName)
         _propertyDefinitions = propertyDefinitions
         _parentId = parentId
      End Sub

      ''' <summary>
      ''' Creates a new instance suitable for the current node of the reader
      ''' </summary>
      ''' <param name="reader"></param>
      ''' <returns></returns>
      ''' <remarks>Node "baseId" is responsible for type of returned TypeDefinition-instance</remarks>
      Public Overloads Shared Function CreateInstance(reader As System.Xml.XmlReader) As cmisTypeDefinitionType
         reader.MoveToContent()
         'support for xsi:type-attribute
         If reader.MoveToAttribute("type", Namespaces.w3instance) Then
            Dim retVal As cmisTypeDefinitionType = CreateInstance(Of cmisTypeDefinitionType)(reader.GetAttribute("type", Namespaces.w3instance))

            reader.MoveToContent()
            If retVal IsNot Nothing Then
               retVal.ReadXml(reader)
               Return retVal
            End If
         End If
         Return CreateInstance(Of cmisTypeDefinitionType)(reader, "baseId")
      End Function
#End Region

      Public Overridable ReadOnly Property BaseId As Core.enumBaseObjectTypeIds
         Get
            Return _baseId
         End Get
      End Property
      Protected MustOverride ReadOnly Property _baseId As Core.enumBaseObjectTypeIds

      Protected _propertyDefinitionsAsReadOnly As New ccg1.ArrayMapper(Of cmisTypeDefinitionType, ccdp.cmisPropertyDefinitionType)(Me,
                                                                                                                                   "PropertyDefinitions", Function() _propertyDefinitions,
                                                                                                                                   "Id", Function(propertyDefinition) propertyDefinition.Id)
      Protected MustOverride Overrides Sub InitClass()
      Protected Sub MyBaseInitClass()
         MyBase.InitClass()
      End Sub

      ''' <summary>
      ''' Access to PropertyDefinitions via index or Id
      ''' </summary>
      ''' <value></value>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public ReadOnly Property PropertyDefinitionsAsReadOnly As ccg1.ArrayMapper(Of cmisTypeDefinitionType, ccdp.cmisPropertyDefinitionType)
         Get
            Return _propertyDefinitionsAsReadOnly
         End Get
      End Property

      Protected MustOverride Function GetCmisTypeName() As String

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns></returns>
      ''' <remarks>The given propertyDefinitionIds handled casesensitive, if there is
      ''' none at all, all properties of this instance will be returned</remarks>
      Public Function GetPropertyDefinitions(ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisPropertyDefinitionType)
         Return GetPropertyDefinitions(enumKeySyntax.original, propertyDefinitionIds)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="ignoreCase">If True each propertyDefinitionId is compared case insensitive</param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns>Dictionary of all existing propertyDefinitions specified by propertyDefinitionsIds.
      ''' Notice: if ignoreCase is set to True, then the keys of the returned dictionary are lowercase</returns>
      ''' <remarks>If there are no propertyDefinitionIds defined, all properties of this instance will be returned</remarks>
      Public Function GetPropertyDefinitions(ignoreCase As Boolean, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisPropertyDefinitionType)
         Return GetPropertyDefinitions(If(ignoreCase, enumKeySyntax.lowerCase, enumKeySyntax.original), propertyDefinitionIds)
      End Function

      ''' <summary>
      ''' Returns the properties specified by the given propertyDefinitionIds
      ''' </summary>
      ''' <param name="keySyntax">If the lowerCase-bit is set each propertyDefinitionId is compared case insensitive</param>
      ''' <param name="propertyDefinitionIds"></param>
      ''' <returns>Dictionary of all existing propertyDefinitions specified by propertyDefinitionsIds.
      ''' Notice: if keySyntax is set to lowerCase, then the keys of the returned dictionary are lowercase</returns>
      ''' <remarks>If there are no propertyDefinitionIds defined, all properties of this instance will be returned</remarks>
      Public Function GetPropertyDefinitions(keySyntax As enumKeySyntax, ParamArray propertyDefinitionIds As String()) As Dictionary(Of String, Properties.cmisPropertyDefinitionType)
         Dim retVal As New Dictionary(Of String, Properties.cmisPropertyDefinitionType)
         Dim verifyIds As New HashSet(Of String)
         Dim ignoreCase As Boolean = ((keySyntax And enumKeySyntax.searchIgnoreCase) = enumKeySyntax.searchIgnoreCase)
         Dim lowerCase As Boolean = ((keySyntax And enumKeySyntax.lowerCase) = enumKeySyntax.lowerCase)
         Dim originalCase As Boolean = ((keySyntax And enumKeySyntax.original) = enumKeySyntax.original)

         'collect requested propertyDefinitionIds
         If propertyDefinitionIds Is Nothing OrElse propertyDefinitionIds.Length = 0 Then
            verifyIds.Add("*")
         Else
            For Each propertyDefinitionId As String In propertyDefinitionIds
               If propertyDefinitionId Is Nothing Then propertyDefinitionId = ""
               If ignoreCase Then propertyDefinitionId = propertyDefinitionId.ToLowerInvariant()
               verifyIds.Add(propertyDefinitionId)
            Next
         End If
         'collect requested properties
         If _propertyDefinitions IsNot Nothing Then
            For Each pd As Properties.cmisPropertyDefinitionType In _propertyDefinitions
               Dim originalName As String = pd.Id
               Dim name As String

               If originalName Is Nothing Then
                  name = ""
                  originalName = ""
               ElseIf ignoreCase Then
                  name = originalName.ToLowerInvariant()
               Else
                  name = originalName
               End If
               If (verifyIds.Contains(name) OrElse verifyIds.Contains("*")) Then
                  If lowerCase AndAlso Not retVal.ContainsKey(name) Then retVal.Add(name, pd)
                  If originalCase AndAlso Not retVal.ContainsKey(originalName) Then retVal.Add(originalName, pd)
               End If
            Next
         End If

         Return retVal
      End Function

#Region "IXmlSerialization"
      Public Overrides Sub ReadXml(reader As System.Xml.XmlReader)
         MyBase.ReadXml(reader)
         If _propertyDefinitions IsNot Nothing Then
            'support properties extension (alfresco)
            For Each propertyDefinition As Core.Definitions.Properties.cmisPropertyDefinitionType In _propertyDefinitions
               propertyDefinition.ExtendedProperties.Add(Constants.ExtendedProperties.DeclaringType, _id)
            Next
         End If
      End Sub

      Public Overrides Sub WriteXml(writer As System.Xml.XmlWriter)
         Dim typeName As String = GetCmisTypeName()
         If Not String.IsNullOrEmpty(typeName) Then WriteAttribute(writer, Nothing, "type", Namespaces.w3instance, typeName)
         MyBase.WriteXml(writer)
      End Sub
#End Region

#Region "Links of type definition"
      ''' <summary>
      ''' Creates a list of links for a typedefinition-instance
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="repositoryId"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
      ''' 3.11.1.1 HTTP GET
      ''' </remarks>
      Protected Overridable Function GetLinks(Of TLink)(baseUri As Uri, repositoryId As String,
                                                        elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Dim retVal As New List(Of TLink) From {
            elementFactory(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _id)),
                           LinkRelationshipTypes.Self, MediaTypes.Entry, _id, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.GetRepositoryInfo.ReplaceUri("repositoryId", repositoryId)),
                           LinkRelationshipTypes.Service, MediaTypes.Service, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _baseId.GetName())),
                           LinkRelationshipTypes.DescribedBy, MediaTypes.Entry, _baseId.GetName(), Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.TypesUri(ServiceURIs.enumTypesUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _id)),
                           LinkRelationshipTypes.Down, MediaTypes.Feed, Nothing, Nothing),
            elementFactory(New Uri(baseUri, ServiceURIs.TypeDescendantsUri(ServiceURIs.enumTypeDescendantsUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _id)),
                           LinkRelationshipTypes.Down, MediaTypes.Tree, Nothing, Nothing)}

         If Not String.IsNullOrEmpty(_parentId) Then
            retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", repositoryId, "id", _parentId)),
                                      LinkRelationshipTypes.Up, MediaTypes.Entry, _parentId, Nothing))
         End If

         Return retVal
      End Function
      Public Function GetLinks(baseUri As Uri, repositoryId As String) As List(Of AtomPub.AtomLink)
         Return GetLinks(Of AtomPub.AtomLink)(baseUri, repositoryId,
                                              Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function
      Public Function GetLinks(baseUri As Uri, repositoryId As String,
                               ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetLinks(Of sxl.XElement)(baseUri, repositoryId,
                                             AddressOf .CreateXElement)
         End With
      End Function
#End Region

      Public Shared Operator +(arg1 As cmisTypeDefinitionType, arg2 As Contracts.ICmisClient) As Client.CmisType.PreStage
         Return New Client.CmisType.PreStage(arg2, arg1)
      End Operator
      Public Shared Operator +(arg1 As Contracts.ICmisClient, arg2 As cmisTypeDefinitionType) As Client.CmisType.PreStage
         Return New Client.CmisType.PreStage(arg1, arg2)
      End Operator

      Public Shared Narrowing Operator CType(value As ca.AtomEntry) As cmisTypeDefinitionType
         Return If(value Is Nothing, Nothing, value.Type)
      End Operator

   End Class
End Namespace