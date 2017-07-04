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
Imports sss = System.ServiceModel.Syndication
Imports sxl = System.Xml.Linq
Imports sxs = System.Xml.Serialization

Namespace CmisObjectModel.Core
   Partial Public Class cmisRepositoryInfoType

      Protected Overrides Sub InitClass()
         MyBase.InitClass()
         _capabilities = New cmisRepositoryCapabilitiesType
         _cmisVersionSupported = "1.1"
      End Sub

#Region "links"
      ''' <summary>
      ''' Creates a list of links for a cmisRepositoryInfoType-instance
      ''' </summary>
      ''' <typeparam name="TLink"></typeparam>
      ''' <param name="baseUri"></param>
      ''' <param name="elementFactory"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
      ''' 3.7.1 HTTP GET
      ''' </remarks>
      Protected Function GetLinks(Of TLink)(baseUri As Uri,
                                            elementFactory As AtomPub.Factory.GenericDelegates(Of Uri, TLink).CreateLinkDelegate) As List(Of TLink)
         Dim retVal As New List(Of TLink) From {
            elementFactory(New Uri(baseUri, ServiceURIs.TypeDescendantsUri(ServiceURIs.enumTypeDescendantsUri.none).ReplaceUri("repositoryId", _repositoryId)),
                           LinkRelationshipTypes.TypeDescendants, MediaTypes.Feed, Nothing, Nothing)}

         If _capabilities IsNot Nothing Then
            If _capabilities.CapabilityGetFolderTree Then
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.FolderTreeUri(ServiceURIs.enumFolderTreeUri.folderId).ReplaceUri("repositoryId", _repositoryId, "folderId", _rootFolderId)),
                                         LinkRelationshipTypes.FolderTree, MediaTypes.Tree, Nothing, Nothing))
            End If
            If _capabilities.CapabilityGetDescendants Then
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.DescendantsUri(ServiceURIs.enumDescendantsUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _rootFolderId)),
                                         LinkRelationshipTypes.RootDescendants, MediaTypes.Tree, _repositoryId, Nothing))
            End If
            If _capabilities.CapabilityChanges <> enumCapabilityChanges.none Then
               retVal.Add(elementFactory(New Uri(baseUri, ServiceURIs.ChangesUri(ServiceURIs.enumChangesUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                         LinkRelationshipTypes.Changes, MediaTypes.Feed, Nothing, Nothing))
            End If
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Creates a list of links for a cmisRepositoryInfoType-instance
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetLinks(baseUri As Uri) As List(Of AtomPub.AtomLink)
         Return GetLinks(Of AtomPub.AtomLink)(baseUri,
                                              Function(uri, relationshipType, mediaType, id, renditionKind) New AtomPub.AtomLink(uri, relationshipType, mediaType, id, renditionKind))
      End Function
      ''' <summary>
      ''' Creates a list of links for a cmisRepositoryInfoType-instance
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <param name="ns"></param>
      ''' <param name="elementName"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetLinks(baseUri As Uri,
                               ns As sxl.XNamespace, elementName As String) As List(Of sxl.XElement)
         With New AtomPub.Factory.XElementBuilder(ns, elementName)
            Return GetLinks(Of sxl.XElement)(baseUri,
                                             AddressOf .CreateXElement)
         End With
      End Function
#End Region

      ''' <summary>
      ''' Returns the uritemplates to
      ''' - get an object by id
      ''' - get an object by path
      ''' - get a type by id
      ''' - request a query (if the repository supports queries)
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
      ''' 3.7.1.1 URI Templates
      '''</remarks>
      Public Function GetUriTemplates(baseUri As Uri) As List(Of RestAtom.cmisUriTemplateType)
         Dim retVal As New List(Of RestAtom.cmisUriTemplateType) From {
            New RestAtom.cmisUriTemplateType(New Uri(baseUri, ServiceURIs.ObjectUri(ServiceURIs.enumObjectUri.getObjectById).ReplaceUri("repositoryId", RepositoryId)),
                                             UriTemplates.ObjectById, MediaTypes.Entry),
            New RestAtom.cmisUriTemplateType(New Uri(baseUri, ServiceURIs.GetObjectByPath.ReplaceUri("repositoryId", RepositoryId)),
                                             UriTemplates.ObjectByPath, MediaTypes.Entry),
            New RestAtom.cmisUriTemplateType(New Uri(baseUri, ServiceURIs.TypeUri(ServiceURIs.enumTypeUri.typeId).ReplaceUri("repositoryId", RepositoryId)),
                                             UriTemplates.TypeById, MediaTypes.Entry)}

         If _capabilities IsNot Nothing AndAlso _capabilities.CapabilityQuery <> enumCapabilityQuery.none Then
            retVal.Add(New RestAtom.cmisUriTemplateType(New Uri(baseUri, ServiceURIs.QueryUri(ServiceURIs.enumQueryUri.query).ReplaceUri("repositoryId", RepositoryId)),
                                                        UriTemplates.Query, MediaTypes.Feed))
         End If

         Return retVal
      End Function

      ''' <summary>
      ''' Returns the collections supported by the repository
      ''' </summary>
      ''' <param name="baseUri"></param>
      ''' <returns></returns>
      ''' <remarks>
      ''' see http://docs.oasis-open.org/cmis/CMIS/v1.1/cs01/CMIS-v1.1-cs01.html
      ''' 3.7.1 HTTP GET
      ''' Accepts defined in 3.8
      '''</remarks>
      Public Function GetCollectionInfos(baseUri As Uri) As List(Of AtomPub.AtomCollectionInfo)
         Dim retVal As New List(Of AtomPub.AtomCollectionInfo)
         Dim collectionInfo As AtomPub.AtomCollectionInfo
         Dim accepts As String()

         'root
         collectionInfo = New AtomPub.AtomCollectionInfo("Root Collection",
                                                         New Uri(baseUri, ServiceURIs.ChildrenUri(ServiceURIs.enumChildrenUri.folderId).ReplaceUri("repositoryId", _repositoryId, "id", _rootFolderId)),
                                                         CollectionInfos.Root, MediaTypes.Entry, MediaTypes.Request)
         retVal.Add(collectionInfo)
         'query
         If _capabilities IsNot Nothing AndAlso _capabilities.CapabilityQuery <> enumCapabilityQuery.none Then
            collectionInfo = New AtomPub.AtomCollectionInfo("Query Collection",
                                                            New Uri(baseUri, ServiceURIs.QueryUri(ServiceURIs.enumQueryUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                            CollectionInfos.Query, MediaTypes.Query, MediaTypes.Request)
            retVal.Add(collectionInfo)
         End If
         'checkedOut
         If _capabilities IsNot Nothing AndAlso _capabilities.CapabilityPWCUpdatable Then
            collectionInfo = New AtomPub.AtomCollectionInfo("Checked Out Collection",
                                                            New Uri(baseUri, ServiceURIs.CheckedOutUri(ServiceURIs.enumCheckedOutUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                            CollectionInfos.CheckedOut, MediaTypes.Entry, MediaTypes.Request)
            retVal.Add(collectionInfo)
         End If
         'unfiling
         If _capabilities IsNot Nothing AndAlso _capabilities.CapabilityUnfiling Then
            collectionInfo = New AtomPub.AtomCollectionInfo("Unfiled collection",
                                                            New Uri(baseUri, ServiceURIs.UnfiledUri(ServiceURIs.enumUnfiledUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                            CollectionInfos.Unfiled, MediaTypes.Entry, MediaTypes.Request)
            retVal.Add(collectionInfo)
         End If
         'type children
         accepts = If(_capabilities IsNot Nothing AndAlso _capabilities.CapabilityNewTypeSettableAttributes IsNot Nothing,
                      New String() {MediaTypes.Entry}, Nothing)
         collectionInfo = New AtomPub.AtomCollectionInfo("Types Collection",
                                                         New Uri(baseUri, ServiceURIs.TypesUri(ServiceURIs.enumTypesUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                         CollectionInfos.Types, accepts)
         retVal.Add(collectionInfo)
         'BulkUpdates
         If _capabilities IsNot Nothing AndAlso _capabilities.CapabilityBulkUpdatable Then
            collectionInfo = New AtomPub.AtomCollectionInfo("Updates Collection",
                                                            New Uri(baseUri, ServiceURIs.BulkUpdateProperties.ReplaceUri("repositoryId", _repositoryId)),
                                                            CollectionInfos.Update, MediaTypes.Entry)
         End If
         'relationships (not directly defined in 3.7.1, but listed in 3.9.1)
         collectionInfo = New AtomPub.AtomCollectionInfo("Relationships Collection",
                                                         New Uri(baseUri, ServiceURIs.RelationshipsUri(ServiceURIs.enumRelationshipsUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                         CollectionInfos.Relationships, MediaTypes.Entry, MediaTypes.Request)
         'policies (not directly defined in 3.7.1, but listed in 3.9.3)
         collectionInfo = New AtomPub.AtomCollectionInfo("Policies Collection",
                                                         New Uri(baseUri, ServiceURIs.PoliciesUri(ServiceURIs.enumPoliciesUri.none).ReplaceUri("repositoryId", _repositoryId)),
                                                         CollectionInfos.Policies, MediaTypes.Entry)

         Return retVal
      End Function

      Public Shared Operator +(arg1 As cmisRepositoryInfoType, arg2 As Contracts.ICmisClient) As Client.CmisRepository
         Return New Client.CmisRepository(arg1, arg2)
      End Operator
      Public Shared Operator +(arg1 As Contracts.ICmisClient, arg2 As cmisRepositoryInfoType) As Client.CmisRepository
         Return New Client.CmisRepository(arg2, arg1)
      End Operator

   End Class
End Namespace