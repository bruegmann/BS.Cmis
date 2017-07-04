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
Imports ccg = CmisObjectModel.Common.Generic
Imports cmr = CmisObjectModel.Messaging.Requests
Imports sn = System.Net
Imports ss = System.ServiceModel
Imports ssc = System.Security.Cryptography
Imports ssw = System.ServiceModel.Web
Imports sx = System.Xml
Imports sxs = System.Xml.Serialization
'depends on the chosen interpretation of the xs:integer expression in a xsd-file
#If xs_Integer = "Int32" OrElse xs_Integer = "Integer" OrElse xs_Integer = "Single" Then
Imports xs_Integer = System.Int32
#Else
Imports xs_Integer = System.Int64
#If Not xs_HttpRequestAddRange64 Then
#Const HttpRequestAddRangeShortened = True
#End If
#End If

Namespace CmisObjectModel.Client
   ''' <summary>
   ''' Simplifies requests to cmis folder
   ''' </summary>
   ''' <remarks></remarks>
   Public Class CmisFolder
      Inherits CmisObject

#Region "Constructors"
      Public Sub New(cmisObject As Core.cmisObjectType,
                     client As Contracts.ICmisClient, repositoryInfo As Core.cmisRepositoryInfoType)
         MyBase.New(cmisObject, client, repositoryInfo)
      End Sub
#End Region

#Region "Predefined properties"
      Public Overridable Property AllowedChildObjectTypeIds As ccg.Nullable(Of String())
         Get
            Return _cmisObject.AllowedChildObjectTypeIds
         End Get
         Set(value As ccg.Nullable(Of String()))
            _cmisObject.AllowedChildObjectTypeIds = value
         End Set
      End Property

      Public Overridable Property ParentId As ccg.Nullable(Of String)
         Get
            Return _cmisObject.ParentId
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.ParentId = value
         End Set
      End Property

      Public Overridable Property Path As ccg.Nullable(Of String)
         Get
            Return _cmisObject.Path
         End Get
         Set(value As ccg.Nullable(Of String))
            _cmisObject.Path = value
         End Set
      End Property
#End Region

#Region "Navigation"
      ''' <summary>
      ''' Gets the list of documents that are checked out in the current folder that the user has access to
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function GetCheckedOutDocs(Optional maxItems As xs_Integer? = Nothing,
                                                Optional skipCount As xs_Integer? = Nothing,
                                                Optional orderBy As String = Nothing,
                                                Optional filter As String = Nothing,
                                                Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                                Optional renditionFilter As String = Nothing,
                                                Optional includeAllowableActions As Boolean? = Nothing) As Generic.ItemList(Of CmisObject)
         Return MyBase.GetCheckedOutDocs(_cmisObject.ObjectId, maxItems, skipCount, orderBy, filter, includeRelationships, renditionFilter, includeAllowableActions)
      End Function

      ''' <summary>
      ''' Gets the list of child objects contained in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetChildren(Optional maxItems As xs_Integer? = Nothing,
                                  Optional skipCount As xs_Integer? = Nothing,
                                  Optional orderBy As String = Nothing,
                                  Optional filter As String = Nothing,
                                  Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                  Optional renditionFilter As String = Nothing,
                                  Optional includeAllowableActions As Boolean? = Nothing,
                                  Optional includePathSegment As Boolean = False) As Generic.ItemList(Of CmisObject)
         With _client.GetChildren(New cmr.getChildren() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                              .MaxItems = maxItems, .SkipCount = skipCount, .OrderBy = orderBy, .Filter = filter,
                                                              .IncludeRelationships = includeRelationships, .RenditionFilter = renditionFilter,
                                                              .IncludeAllowableActions = includeAllowableActions,
                                                              .IncludePathSegment = includePathSegment})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Convert(.Response.Objects)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the set of descendant objects containded in the current folder or any of its child-folders
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetDescendants(Optional depth As xs_Integer? = Nothing,
                                     Optional filter As String = Nothing,
                                     Optional includeAllowableActions As Boolean? = Nothing,
                                     Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                                     Optional renditionFilter As String = Nothing,
                                     Optional includePathSegment As Boolean = False) As Generic.ItemContainer(Of CmisObject)()
         With _client.GetDescendants(New cmr.getDescendants() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                                    .Depth = depth, .Filter = filter, .IncludeAllowableActions = includeAllowableActions,
                                                                    .IncludeRelationships = includeRelationships,
                                                                    .RenditionFilter = renditionFilter,
                                                                    .IncludePathSegment = includePathSegment})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Transform(.Response.Objects, New List(Of Generic.ItemContainer(Of CmisObject))).ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the parent folder object for the current folder object
      ''' </summary>
      ''' <param name="filter"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetParent(Optional filter As String = Nothing) As CmisFolder
         With _client.GetFolderParent(New cmr.getFolderParent() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId, .Filter = filter})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(CreateCmisObject(.Response.Object), CmisFolder)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets the set of descendant folder objects contained in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function GetTree(Optional depth As xs_Integer? = Nothing,
                              Optional filter As String = Nothing,
                              Optional includeAllowableActions As Boolean? = Nothing,
                              Optional includeRelationships As Core.enumIncludeRelationships? = Nothing,
                              Optional renditionFilter As String = Nothing,
                              Optional includePathSegment As Boolean = False) As Generic.ItemContainer(Of CmisObject)()
         With _client.GetFolderTree(New cmr.getFolderTree() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                                  .Depth = depth, .Filter = filter, .IncludeAllowableActions = includeAllowableActions,
                                                                  .IncludeRelationships = includeRelationships, .RenditionFilter = renditionFilter,
                                                                  .IncludePathSegment = includePathSegment})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return Transform(.Response.Objects, New List(Of Generic.ItemContainer(Of CmisObject))).ToArray()
            Else
               Return Nothing
            End If
         End With
      End Function
#End Region

#Region "Object"
      ''' <summary>
      ''' Creates a document object of the specified type (given by the cmis:objectTypeId property) in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function CreateDocument(properties As Core.Collections.cmisPropertiesType,
                                             Optional contentStream As Messaging.cmisContentStreamType = Nothing,
                                             Optional versioningState As Core.enumVersioningState? = Nothing,
                                             Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                             Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                             Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         Return MyBase.CreateDocument(properties, _cmisObject.ObjectId, contentStream, versioningState, policies, addACEs, removeACEs)
      End Function

      ''' <summary>
      ''' Creates a document object as a copy of the given source document in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Shadows Function CreateDocumentFromSource(sourceId As String,
                                                       Optional properties As Core.Collections.cmisPropertiesType = Nothing,
                                                       Optional versioningState As Core.enumVersioningState? = Nothing,
                                                       Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                                       Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                                       Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisDocument
         Return MyBase.CreateDocumentFromSource(sourceId, properties, _cmisObject.ObjectId, versioningState, policies, addACEs, removeACEs)
      End Function

      ''' <summary>
      ''' Creates a folder object of the specified type (given by the cmis:objectTypeId property) in the current folder
      ''' </summary>
      ''' <remarks></remarks>
      Public Function CreateFolder(properties As Core.Collections.cmisPropertiesType,
                                   Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                   Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                   Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisFolder
         With _client.CreateFolder(New cmr.createFolder() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                                .Properties = properties, .Policies = policies, .AddACEs = addACEs, .RemoveACEs = removeACEs})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(GetObject(.Response.ObjectId), CmisFolder)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a item object of the specified type (given by the cmis:objectTypeId property) in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreateItem(properties As Core.Collections.cmisPropertiesType,
                                 Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                 Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                 Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisObject
         With _client.CreateItem(New cmr.createItem() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                            .Properties = properties, .Policies = policies,
                                                            .AddACEs = addACEs, .RemoveACEs = removeACEs})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return GetObject(.Response.ObjectId)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Creates a policy object of the specified type (given by the cmis:objectTypeId property) in the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function CreatePolicy(properties As Core.Collections.cmisPropertiesType,
                                   Optional policies As Core.Collections.cmisListOfIdsType = Nothing,
                                   Optional addACEs As Core.Security.cmisAccessControlListType = Nothing,
                                   Optional removeACEs As Core.Security.cmisAccessControlListType = Nothing) As CmisPolicy
         With _client.CreatePolicy(New cmr.createPolicy() With {.RepositoryId = _repositoryInfo.RepositoryId, .Properties = properties,
                                                                .FolderId = _cmisObject.ObjectId, .Policies = policies,
                                                                .AddACEs = addACEs, .RemoveACEs = removeACEs})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return TryCast(GetObject(.Response.ObjectId), CmisPolicy)
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Deletes the current folder object and all of its child- and descendant-objects
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function DeleteTree(Optional allVersions As Boolean = True,
                                 Optional unfileObjects As Core.enumUnfileObject = Core.enumUnfileObject.delete,
                                 Optional continueOnFailure As Boolean = False) As Messaging.failedToDelete
         With _client.DeleteTree(New cmr.deleteTree() With {.RepositoryId = _repositoryInfo.RepositoryId, .FolderId = _cmisObject.ObjectId,
                                                            .AllVersions = allVersions, .UnfileObjects = unfileObjects,
                                                            .ContinueOnFailure = continueOnFailure})
            _lastException = .Exception
            If _lastException Is Nothing Then
               Return .Response.FailedToDelete
            Else
               Return Nothing
            End If
         End With
      End Function

      ''' <summary>
      ''' Gets a rendition stream for a specified rendition of the current folder object
      ''' </summary>
      ''' <param name="streamId"></param>
      ''' <param name="offset"></param>
      ''' <param name="length"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
#If HttpRequestAddRangeShortened Then
      Public Shadows Function GetContentStream(streamId As String,
                                               Optional offset As Integer? = Nothing,
                                               Optional length As Integer? = Nothing) As Messaging.cmisContentStreamType
#Else
      Public Shadows Function GetContentStream(streamId As String,
                                               Optional offset As xs_Integer? = Nothing,
                                               Optional length As xs_Integer? = Nothing) As Messaging.cmisContentStreamType
#End If
         Return MyBase.GetContentStream(streamId, offset, length)
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from one folder to the current folder
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function MoveObjectFrom(objectId As String, sourceFolderId As String) As CmisObject
         Return MyBase.MoveObject(objectId, _cmisObject.ObjectId, sourceFolderId)
      End Function

      ''' <summary>
      ''' Moves the specified file-able object from the current folder to another folder
      ''' </summary>
      ''' <remarks></remarks>
      Public Shadows Function MoveObjectTo(objectId As String, targetFolderId As String) As CmisObject
         Return MyBase.MoveObject(objectId, targetFolderId, _cmisObject.ObjectId)
      End Function
#End Region

#Region "Multi"
      ''' <summary>
      ''' Adds an existing fileable non-folder object to the current folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function AddObject(objectId As String, Optional allVersions As Boolean = True) As Boolean
         With _client.AddObjectToFolder(New cmr.addObjectToFolder() With {.RepositoryId = _repositoryInfo.RepositoryId, .ObjectId = objectId,
                                                                          .FolderId = _cmisObject.ObjectId, .AllVersions = allVersions})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function

      ''' <summary>
      ''' Folders are non-filable objects, therefore this method should not be used
      ''' </summary>
      ''' <param name="folderId"></param>
      ''' <param name="allVersions"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Overrides Function AddObjectToFolder(folderId As String, Optional allVersions As Boolean = True) As Boolean
         Return False
      End Function

      ''' <summary>
      ''' Removes an existing fileable non-folder object from a folder
      ''' </summary>
      ''' <returns></returns>
      ''' <remarks></remarks>
      Public Function RemoveObject(objectId As String) As Boolean
         With _client.RemoveObjectFromFolder(New cmr.removeObjectFromFolder() With {.RepositoryId = _repositoryInfo.RepositoryId,
                                                                                    .ObjectId = objectId, .FolderId = _cmisObject.ObjectId})
            _lastException = .Exception
            Return _lastException Is Nothing
         End With
      End Function

      ''' <summary>
      ''' Folders are non-filable objects, therefore this method should not be used
      ''' </summary>
      ''' <param name="folderId"></param>
      ''' <returns></returns>
      ''' <remarks></remarks>
      <System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)>
      Public Overrides Function RemoveObjectFromFolder(Optional folderId As String = Nothing) As Boolean
         Return False
      End Function
#End Region

      ''' <summary>
      ''' Transforms the cmisObjectInFolderContainerType()-structure into a List(Of ItemContainer(Of CmisObject))-structure
      ''' </summary>
      ''' <param name="source"></param>
      ''' <param name="result"></param>
      ''' <remarks></remarks>
      Private Function Transform(source As Messaging.cmisObjectInFolderContainerType(),
                                 result As List(Of Generic.ItemContainer(Of CmisObject))) As List(Of Generic.ItemContainer(Of CmisObject))
         result.Clear()
         If source IsNot Nothing Then
            For Each objectInFolderContainer As Messaging.cmisObjectInFolderContainerType In source
               Dim objectInFolder As Messaging.cmisObjectInFolderType = If(objectInFolderContainer Is Nothing, Nothing, objectInFolderContainer.ObjectInFolder)

               If objectInFolder IsNot Nothing Then
                  Dim cmisObject As CmisObject = CreateCmisObject(objectInFolder.Object)
                  Dim container As New Generic.ItemContainer(Of CmisObject)(cmisObject)

                  cmisObject.PathSegment = objectInFolder.PathSegment
                  result.Add(container)
                  Transform(objectInFolderContainer.Children, container.Children)
               End If
            Next
         End If

         Return result
      End Function

   End Class
End Namespace